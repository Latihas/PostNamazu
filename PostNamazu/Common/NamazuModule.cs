using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using PostNamazu.Common.Localization;

namespace PostNamazu.Actions {
	public abstract class NamazuModule {
		internal static T GetSig<T>(string pattern) =>
			Marshal.GetDelegateForFunctionPointer<T>(PostNamazu.DalamudSigScanner.ScanText(
				pattern.Replace('*', '?').Replace("??", "?").Replace("?", "??")));

		protected static PostNamazu PostNamazu => PostNamazu.Plugin;
		protected static Process FFXIV => PostNamazu.FFXIV;
		protected static PostNamazuUi PluginUI => PostNamazu.PluginUi;


		[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
		[SuppressMessage("Performance", "CA1822")]
		public PostNamazu.StateEnum State => PostNamazu.StateEnum.Ready;

		public void Setup() {
			try {
				GetOffsets();
			} catch (Exception ex) {
				PluginUI.Log(L.Get("PostNamazu/getOffsetsFail", GetType().Name, ex.Message + " \n" + ex.StackTrace));
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
		/// <summary> 检查插件和模组是否准备就绪，且指令是否非空，若不是则抛出异常。</summary>
		protected static void CheckBeforeExecution(string command) {
			if (string.IsNullOrWhiteSpace(command))
				throw new Exception(L.Get("PostNamazu/emptyCommand"));
		}
	}
}

namespace PostNamazu.Attributes {
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class CommandAttribute(string command) : Attribute {
		public string Command { get; } = command;
	}
}