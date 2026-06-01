using System.Diagnostics.CodeAnalysis;
using PostNamazu.Attributes;
using PostNamazu.Common.Localization;

namespace PostNamazu.Actions;

[SuppressMessage("Performance", "CS0649")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Command : NormalCommand {
	// 本地化字符串定义
	[LocalizationProvider("Command")]
	[SuppressMessage("ReSharper", "UnusedType.Local")]
	private static class Localizations {
		[Localized("To avoid sending wrong text to public channels, only commands starting with \"/\" are permitted. Add the prefix \"{0}\" to post to the current channel.",
			"为防止误操作导致错误文本发送至公共频道，仅允许以 \"/\" 开头的指令。如需发送至当前频道，请加前缀 \"{0}\"。")]
		public static readonly string NoChannelError;
	}

	/// <summary>
	///     执行给出的文本指令
	/// </summary>
	/// <param name="command">文本指令</param>
	[Command("command")]
	[Command("DoTextCommand")]
	public void DoTextCommand(string command) {
		DoNormalTextCommand(command);
	}
}