using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using PostNamazu.Actions;
using PostNamazu.Attributes;
using PostNamazu.Common;
using PostNamazu.Common.Localization;
using SigScanner = PostNamazu.Common.SigScanner;

namespace PostNamazu;

public class PostNamazu : IActPluginV1 {
	public static PostNamazu Plugin;
	public PostNamazuUi PluginUi;
	// private Label _lblStatus; // The status label that appears in ACT's Plugin tab

	private PluginIntegrationManager _integrationManager;

	private HttpServer? _httpServer;

	internal Process FFXIV;
	internal FFXIV_ACT_Plugin.FFXIV_ACT_Plugin FFXIV_ACT_Plugin;
	// public ExternalProcessMemory Memory;
	public static IDalamudPluginInterface DalamudPluginInterface;
	public SigScanner SigScanner;
	internal ISigScanner DalamudSigScanner;
	public IPluginLog Log;

	private Dictionary<string, bool> ActionEnabled => PluginUi.ActionEnabled; //直接使用UI控件上的ActionEnabled状态
	private readonly Dictionary<string, HandlerDelegate> CmdBind = new(StringComparer.OrdinalIgnoreCase); //key不区分大小写

	private readonly List<NamazuModule> Modules = [];

	/// <summary> 插件或模组的当前状态。 </summary>
	public enum StateEnum {
		/// <summary> 尚未开始。 </summary>
		NotReady,
		/// <summary> 扫描失败。 </summary>
		Failure,
		/// <summary> 正在启动或尝试扫描。 </summary>
		Waiting,
		/// <summary> 扫描成功，已预备。 </summary>
		Ready
	}

	private StateEnum _state = StateEnum.Waiting;
	public StateEnum State {
		get => _state;
		private set => SetState(value);
	}

	/// <summary>
	///     设置插件状态（供内部类使用）
	/// </summary>
	internal void SetState(StateEnum value) {
		_state = value;
#if DEBUG
		PluginUi.Log($"插件状态变更：{value}");
#endif
	}

	#region Init

	public void InitPlugin(IDalamudPluginInterface dalamudPluginInterface, IPluginLog log, ISigScanner dalamudSigScanner) {
		Plugin = this;
		DalamudPluginInterface = dalamudPluginInterface;
		Log = log;
		DalamudSigScanner = dalamudSigScanner;
		SigScanner = new SigScanner();
		PluginUi = new PostNamazuUi();
		PluginUi.Log(L.Get("PostNamazu/pluginVersion", Assembly.GetExecutingAssembly().GetName().Version));

		FFXIV_ACT_Plugin = GetFFXIVPlugin();

		FFXIV = Plugin.FFXIV_ACT_Plugin.DataRepository.GetCurrentFFXIVProcess();
		Plugin.SetState(StateEnum.Waiting);
	

		// 初始化管理器
		_integrationManager = new PluginIntegrationManager();

		if (PluginUi.AutoStart) ServerStart();
		PluginUi.ButtonStart.Click += ServerStart;
		PluginUi.ButtonStop.Click += ServerStop;

		InitializeActions();
		_integrationManager.InitializeIntegrations();
		Plugin.Attach();
		Log.Info(L.Get("PostNamazu/pluginInit"));
		LogACT("Initialized");
	}

	public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText) {
	}

	public void DeInitPlugin() {
		PluginUi.SaveSettings();
		Detach();
		_integrationManager.DeInitializeIntegrations();
		if (_httpServer != null) ServerStop();
		Plugin = null;
	}

	public delegate void HandlerDelegate(string command);

	/// <summary>
	///     注册命令
	/// </summary>
	public void InitializeActions() {
		foreach (var t in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(NamazuModule)) && !t.IsAbstract)) {
#if DEBUG
			PluginUi.Log($"Initalizing Module: {t.Name}");
#endif
			if (Activator.CreateInstance(t) is not NamazuModule module) continue;
			Modules.Add(module);
			PluginUi.RegisterAction(t.Name);
			var commands = module.GetType().GetMethods().Where(method => method.GetCustomAttributes<CommandAttribute>().Any());
			foreach (var action in commands) {
				var handlerDelegate = (HandlerDelegate)Delegate.CreateDelegate(typeof(HandlerDelegate), module, action);
				foreach (var command in action.GetCustomAttributes<CommandAttribute>()) {
					SetAction(command.Command, handlerDelegate);
#if DEBUG
					PluginUi.Log($"{action.Name}@{command.Command}");
#endif
				}
			}
		}
	}

	public T? GetModuleInstance<T>() where T : NamazuModule {
		return Modules.FirstOrDefault(m => m is T) as T;
	}

	/// <summary>
	///     获取所有命令键（供集成管理器使用）
	/// </summary>
	internal string[] GetCommandKeys() => CmdBind.Keys.ToArray();

	public void ServerStart(object? sender = null, EventArgs? e = null) {
		try {
			_httpServer = new HttpServer((int)PluginUi.TextPort.Value) {
				PostNamazuDelegate = DoAction
			};
			_httpServer.OnException += OnException;

			PluginUi.ButtonStart.Enabled = false;
			PluginUi.ButtonStop.Enabled = true;
			PluginUi.Log(L.Get("PostNamazu/httpStart", _httpServer.Port));
		} catch (Exception ex) {
			OnException(ex);
		}
	}

	public void ServerStop(object? sender = null, EventArgs? e = null) {
		if (_httpServer != null) {
			_httpServer.Stop();
			_httpServer.PostNamazuDelegate = null;
			_httpServer.OnException -= OnException;
		}

		PluginUi.ButtonStart.Enabled = true;
		PluginUi.ButtonStop.Enabled = false;
		PluginUi.Log(L.Get("PostNamazu/httpStop"));
	}

	/// <summary>
	///     委托给HttpServer类的异常处理
	/// </summary>
	/// <param name="ex"></param>
	private void OnException(Exception ex) {
		ExceptionHandler.HandleHttpServerException(ex, _httpServer?.Port ?? -1, PluginUi,
			() => PluginUi.ButtonStart.Enabled = true,
			() => PluginUi.ButtonStop.Enabled = false);
	}

	#endregion

	#region Memory and Process Management

	internal void Attach() {
		try {
			PluginUi.Log(L.Get("PostNamazu/xivProcInject", FFXIV.Id));
			State = StateEnum.Ready;
			LogACT("Attached");
			_isCN = null;
			GetRegion();
			foreach (var m in Modules) m.Setup();
			LogACT("ModulesInitialized");
		} catch (Exception ex) {
			PluginUi.Log(L.Get("PostNamazu/xivProcInjectFailWithError", FFXIV.Id, ex.Message + " \n" + ex.StackTrace));
			FFXIV = null;
			State = StateEnum.Failure;
		}
	}

	internal void Detach() {
		FFXIV = null;
	}

	private static FFXIV_ACT_Plugin.FFXIV_ACT_Plugin GetFFXIVPlugin() => ActGlobals.oFormActMain.FfxivPlugin
	                                                                     ?? throw new Exception(L.Get("PostNamazu/parserNotFound"));

	#endregion

	#region Region Detection

	internal static bool _playerDetected = false;
	internal static bool? _isCN;
	public bool IsCN {
		get {
			if (!_isCN.HasValue) GetRegion();
			return _isCN ?? false;
		}
	}

	private void GetRegion() {
		try {
			GetRegionByMemory();
		} catch (Exception ex) {
			ExceptionHandler.HandleRegionDetectionException(ex, PluginUi);
		}
	}

	private unsafe void GetRegionByMemory() {
		var language = Framework.Instance()->ClientLanguage;
		bool? result = language switch {
			0 or 1 or 2 or 3 => false,
			4 => true,
			_ => null
		};
		if (result.HasValue) {
			_isCN = result;
			PluginUi.Log(_isCN.Value
				? L.Get("PostNamazu/xivDetectMemRegionCN")
				: L.Get("PostNamazu/xivDetectMemRegionGlobal")
			);
		} else _isCN = false; // default
	}

	#endregion

	#region Logging

	internal static void LogACT(string msg) {
		ActGlobals.oFormActMain.ParseRawLogLine($"00|{DateTime.Now:O}|FFFF|{Constants.PluginName}|{msg}|0000000000000000");
	}

	#endregion

	#region Delegate

	/// <summary>
	///     执行指令对应的方法
	/// </summary>
	/// <param name="command"></param>
	/// <param name="payload"></param>
	public void DoAction(string command, string payload) {
		try {
			var reflectedType = GetAction(command).GetMethodInfo().ReflectedType!.Name;

			if (ActionEnabled.TryGetValue(reflectedType, out var value) && value) //不响应没有启用的动作
				GetAction(command)(payload);
			else
				PluginUi.Log(L.Get("PostNamazu/actionIgnored", command, payload));
		} catch (NamazuModule.IgnoredException) {
		} catch (Exception ex) {
			ExceptionHandler.HandleActionExecutionException(ex, command, PluginUi);
		}
	}

	/// <summary>
	///     设置指令与对应的方法
	/// </summary>
	/// <param name="command">指令类型</param>
	/// <param name="action">对应指令的方法委托</param>
	public void SetAction(string command, HandlerDelegate action) {
		CmdBind[command] = action;
	}

	/// <summary>
	///     获取指令对应的方法
	/// </summary>
	/// <param name="command">指令类型</param>
	/// <returns>对应指令的委托方法</returns>
	private HandlerDelegate GetAction(string command) {
		try {
			return CmdBind[command];
		} catch {
			throw new Exception(L.Get("PostNamazu/actionNotFound", command));
		}
	}

	/// <summary>
	///     清空绑定的委托列表
	/// </summary>
	public void ClearAction() {
		CmdBind.Clear();
	}

	#endregion
}