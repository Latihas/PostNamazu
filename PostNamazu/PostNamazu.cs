using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
	private PluginIntegrationManager _integrationManager;
	private HttpServer? _httpServer;
	internal Process FFXIV;
	internal FFXIV_ACT_Plugin.FFXIV_ACT_Plugin FFXIV_ACT_Plugin;
	public static IDalamudPluginInterface DalamudPluginInterface;
	[SuppressMessage("ReSharper", "NotAccessedField.Global")]
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

	public StateEnum State { get; private set; } = StateEnum.Waiting;

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
		Plugin.State = StateEnum.Waiting;

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

	public T? GetModuleInstance<T>() where T : NamazuModule =>
		Modules.FirstOrDefault(m => m is T) as T;

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
	private void OnException(Exception ex) =>
		ExceptionHandler.HandleHttpServerException(ex, _httpServer?.Port ?? -1, PluginUi,
			() => PluginUi.ButtonStart.Enabled = true,
			() => PluginUi.ButtonStop.Enabled = false);

	internal void Attach() {
		try {
			PluginUi.Log(L.Get("PostNamazu/xivProcInject", FFXIV.Id));
			State = StateEnum.Ready;
			LogACT("Attached");
			foreach (var m in Modules) m.Setup();
			LogACT("ModulesInitialized");
		} catch (Exception ex) {
			PluginUi.Log(L.Get("PostNamazu/xivProcInjectFailWithError", FFXIV.Id, ex.Message + " \n" + ex.StackTrace));
			FFXIV = null;
			State = StateEnum.Failure;
		}
	}

	private static FFXIV_ACT_Plugin.FFXIV_ACT_Plugin GetFFXIVPlugin() => ActGlobals.oFormActMain.FfxivPlugin
	                                                                     ?? throw new Exception(L.Get("PostNamazu/parserNotFound"));

	public unsafe bool IsCN => Framework.Instance()->ClientLanguage == 4;

	internal static void LogACT(string msg) {
		ActGlobals.oFormActMain.ParseRawLogLine($"00|{DateTime.Now:O}|FFFF|{Constants.PluginName}|{msg}|0000000000000000");
	}

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
		} catch (Exception ex) {
			ExceptionHandler.HandleActionExecutionException(ex, command, PluginUi);
		}
	}

	/// <summary>
	///     设置指令与对应的方法
	/// </summary>
	/// <param name="command">指令类型</param>
	/// <param name="action">对应指令的方法委托</param>
	public void SetAction(string command, HandlerDelegate action) => CmdBind[command] = action;

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
	public void ClearAction() => CmdBind.Clear();
}