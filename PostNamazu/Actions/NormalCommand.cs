using System;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using PostNamazu.Attributes;
using PostNamazu.Common;
using PostNamazu.Common.Localization;
#pragma warning disable CS0649 // 从未对字段赋值，字段将一直保持其默认值

namespace PostNamazu.Actions
{
    internal class NormalCommand : NamazuModule
    {
        private unsafe delegate IntPtr ProcessChatBoxDelegate(UIModule* module, Utf8String* message, IntPtr a3, byte a4);
        private static ProcessChatBoxDelegate? _processChatBox;

        // 本地化字符串定义
        [LocalizationProvider("NormalCommand")]
        private static class Localizations
        {
            [Localized("To avoid sending wrong text to public channels, only commands starting with \"/\" are permitted. Add the prefix \"{0}\" to post to the current channel.",
                       "为防止误操作导致错误文本发送至公共频道，仅允许以 \"/\" 开头的指令。如需发送至当前频道，请加前缀 \"{0}\"。")]
            public static readonly string NoChannelError;
        }

        public override void GetOffsets()
        {
            base.GetOffsets();
            try {
                _processChatBox = GetSig<ProcessChatBoxDelegate>("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 48 8B F2 48 8B F9 45 84 C9");
            }
            catch (Exception e) {
                PostNamazu.Log.Error("Failed to initialize _processChatBox: " + e);
            }
        }

        void CheckChannel(ref string command)
        {
            if (!command.StartsWith("/"))
            {
                throw new ArgumentException(L.Get("NormalCommand/NoChannelError", Constants.CurrentChannelPrefix));
            }
            if (command.StartsWith(Constants.CurrentChannelPrefix))
            {
                command = command.Substring(Constants.CurrentChannelPrefix.Length);
            }
        }

        /// <summary>
        ///     执行给出的文本指令
        /// </summary>
        /// <param name="command">文本指令</param>
        [Command("normalcommand")] [Command("DoNormalTextCommand")]
        public unsafe void DoNormalTextCommand(string command)
        {
            CheckBeforeExecution(command);
            CheckChannel(ref command);
            PluginUI.Log(command);
            fixed (byte* ptr = (ReadOnlySpan<byte>)Encoding.UTF8.GetBytes(command))
				_processChatBox!(UIModule.Instance(), Utf8String.FromSequence(ptr), IntPtr.Zero, 0);
        }
    }
}
