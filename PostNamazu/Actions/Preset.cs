using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Triggernometry.PluginBridges.BridgeNamazu;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using Newtonsoft.Json;
using PostNamazu.Attributes;
using PostNamazu.Common;
using PostNamazu.Common.Localization;
using PostNamazu.Models;

#pragma warning disable CS0649 // 从未对字段赋值，字段将一直保持其默认值

namespace PostNamazu.Actions
{
    internal partial class Preset : NamazuModule
    {
        // 本地化字符串定义
        [LocalizationProvider("Preset")]
        private static class Localizations
        {
            [Localized("Preset and current map ID are both invalid, loading preset failed.", "预设与当前的地图 ID 均不合法，加载预设失败。")]
            public static readonly string MapIdIllegal;
        }

        /// <summary>
        ///     写入预设
        /// </summary>
        /// <param name="waymarks">标点合集对象</param>
        private unsafe void DoInsertPreset(WayMarks waymarks)
        {
            if (waymarks.MapID is > 2000 or 0)
                waymarks.MapID = GameMain.Instance()->CurrentContentFinderConditionId;
            if (waymarks.MapID == 0)
            {
                Log(L.Get("Preset/MapIdIllegal"));
                return;
            }
            var match = SlotRegex().Match(waymarks.Name.Trim());
            ConstructGamePreset(match.Success && int.TryParse(match.Groups[1].Value, out var slotNum) && slotNum is > 0 and <= 30 ? slotNum - 1 : 0, waymarks);
        }

        /// <summary>
        ///     写入预设
        /// </summary>
        /// <param name="waymarksStr">标点合集序列化Json字符串</param>
        [Command("preset")]
        [Command("DoInsertPreset")]
        public void DoInsertPreset(string waymarksStr)
        {
            CheckBeforeExecution(waymarksStr);

            switch (waymarksStr.ToLower())
            {
                default:
                    var waymarks = JsonConvert.DeserializeObject<WayMarks>(waymarksStr);
                    if (waymarks.Log)
                    {
                        WayMarks.SetWaymarkIds(waymarks);
                        PluginUI.Log("Preset: " + waymarks.ToString());
                    }
                    DoInsertPreset(waymarks);
                    break;
            }
        }

        /// <summary>
        ///     构造预设结构，从0号头子的PPR抄来的
        /// </summary>
        /// <param name="waymark">标点</param>
        /// <returns>byte[]预设结构</returns>
        public unsafe void ConstructGamePreset(int index, WayMarks waymarks)
        {
            var newPreset = new FieldMarkerPreset();
            byte activeMask = 0x00;
            var iter = 0;
            foreach (var twaymark in waymarks)
            {
                var p = new GamePresetPoint();
                var waymark = twaymark ?? new Waymark();
                p.X = waymark.Active ? (Int32)(waymark.X * 1000.0) : 0;
                p.Y = waymark.Active ? (Int32)(waymark.Y * 1000.0) : 0;
                p.Z = waymark.Active ? (Int32)(waymark.Z * 1000.0) : 0;
                activeMask >>= 1;
                if (waymark.Active) activeMask |= 0b10000000;
                newPreset.Markers[iter++] = p;
            }
            newPreset.ActiveMarkers = activeMask;
            newPreset.ContentFinderConditionId = waymarks.MapID;
            newPreset.Timestamp = (Int32)new DateTimeOffset(DateTimeOffset.Now.UtcDateTime).ToUnixTimeSeconds();
            GreyMagicMemoryBase.ExecuteWithLock(() =>
            {
                Marshal.StructureToPtr(newPreset,
                                       (IntPtr)FieldMarkerModule.Instance()
                                       + Marshal.OffsetOf<FieldMarkerModule>("_presets")
                                       + index * Marshal.SizeOf<FieldMarkerPreset>(), false);
            });
        }

        [GeneratedRegex(@"^Slot\s*(\d+)$", RegexOptions.IgnoreCase, "zh-CN")]
        private static partial Regex SlotRegex();
    }
}
