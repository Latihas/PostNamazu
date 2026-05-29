using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using Dalamud.Plugin.Services;
using PostNamazu.Common;
using PostNamazu.Common.Localization;

namespace PostNamazu.Actions {
	public abstract class NamazuModule {
		internal static T GetSig<T>(string pattern) =>
			Marshal.GetDelegateForFunctionPointer<T>(DalamudSigScanner.ScanText(
				pattern.Replace('*', '?').Replace("??", "?").Replace("?", "??")));

		protected static PostNamazu PostNamazu => PostNamazu.Plugin;
		protected static FFXIV_ACT_Plugin.FFXIV_ACT_Plugin FFXIV_ACT_Plugin => PostNamazu.FFXIV_ACT_Plugin;
		protected static Process FFXIV => PostNamazu.FFXIV;
		protected static PostNamazuUi PluginUI => PostNamazu.PluginUi;
		private static ISigScanner DalamudSigScanner => PostNamazu.DalamudSigScanner;

		private static bool IsPluginReady => true;


		private bool complaintAboutModuleNotReady;

		[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
		public PostNamazu.StateEnum State {
			get;
			internal set;
		}

		public void Setup() {
			try {
				State = PostNamazu.StateEnum.Waiting;
				GetOffsets();
				State = PostNamazu.StateEnum.Ready;
			} catch (Exception ex) {
				PluginUI.Log(L.Get("PostNamazu/getOffsetsFail", GetType().Name, ex.Message + " \n" + ex.StackTrace));
				State = PostNamazu.StateEnum.Failure;
			}
			//Log("初始化完成");
		}

		protected virtual void GetOffsets() {
		}

		protected static void Log(string msg) {
			PluginUI.Log(msg);
		}

		/// <summary> 检查插件和模组是否准备就绪，若不是则抛出异常，避免在错误的地址调用函数导致游戏崩溃。</summary>
		/// <exception cref="Exception"></exception>
		/// <exception cref="IgnoredException">模组初始化失败的情况下非首次报错，忽略此异常。</exception>
		protected void CheckBeforeExecution() {
			if (!IsPluginReady) {
				throw new Exception(L.Get("PostNamazu/xivProcNotFound"));
			}
			if (State == PostNamazu.StateEnum.NotReady) {
				throw new Exception(L.Get("PostNamazu/moduleNotReady", GetType().Name));
			}
			var count = 0;
			// 在已扫描到游戏进程到模组初始化完成的期间，延迟等待动作直至模组就绪（或就绪失败）
			// ACT 晚于游戏进程启动，且触发器中有进入游戏时立刻执行的指令时，会出现此情况
			while (State == PostNamazu.StateEnum.Waiting) {
				count++;
#if DEBUG
				Log($"{GetType().Name} 模组未就绪，正在等待第 {count} / {Constants.ModuleInitMaxWaitCount} 次…");
#endif
				Thread.Sleep(Constants.ModuleInitWaitInterval);
				if (count <= Constants.ModuleInitMaxWaitCount) continue;
				// 不应进入此分支，进入此分支说明 State 由于程序逻辑问题而错误地保持在 Waiting 状态
				Log($"{GetType().Name} 模组长期未能初始化，已跳过。");
				State = PostNamazu.StateEnum.Failure;
			}
			if (State == PostNamazu.StateEnum.Failure) {
				var noModuleMsg = L.Get("PostNamazu/moduleInitFail", GetType().Name);
				// 对于模组初始化失败的情况，只抛出一次异常，之后忽略
				if (complaintAboutModuleNotReady) {
					throw new IgnoredException(noModuleMsg);
				}
				complaintAboutModuleNotReady = true;
				throw new Exception(noModuleMsg);
			}
			// Ready
			complaintAboutModuleNotReady = false;
		}

		/// <summary> 检查插件和模组是否准备就绪，且指令是否非空，若不是则抛出异常。</summary>
		protected void CheckBeforeExecution(string command) {
			CheckBeforeExecution();
			if (string.IsNullOrWhiteSpace(command))
				throw new Exception(L.Get("PostNamazu/emptyCommand"));
		}


		internal class IgnoredException : Exception {
			internal IgnoredException(string msg) : base(msg) {
			}
		}
	}
}

namespace PostNamazu.Attributes {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class CommandAttribute(string command) : Attribute {
		public string Command { get; } = command;
	}
}