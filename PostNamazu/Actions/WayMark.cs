using System;
using System.Linq;
using System.Runtime.InteropServices;
using Advanced_Combat_Tracker;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Newtonsoft.Json;
using PostNamazu.Attributes;
using PostNamazu.Common.Localization;
using PostNamazu.Models;
using RainbowMage.OverlayPlugin;
using RainbowMage.OverlayPlugin.MemoryProcessors.InCombat;
using Triggernometry.PluginBridges.BridgeNamazu;

#pragma warning disable CS0649 // 从未对字段赋值，字段将一直保持其默认值

namespace PostNamazu.Actions;

public class WayMark : NamazuModule {
    private WayMarks tempMarks; //暂存场地标点

    private delegate IntPtr ExecuteCommandDelegate(int a1, int a2, int a3, int a4, int a5);

    private static ExecuteCommandDelegate _ExecuteCommandDelegate;

    // 本地化字符串定义
    [LocalizationProvider("WayMark")]
    private static class Localizations {
        [Localized("Waymarks: cache restored", "场地标点: 已本地清除所有标点。")]
        public static readonly string Clear;

        [Localized("Waymarks: cache restored", "场地标点: 已公开清除所有标点。")]
        public static readonly string ClearPublic;

        [Localized("Waymarks: Failed to obtain InCombat status from OverlayPlugin: \n{0}",
            "从 OverlayPlugin 获取战斗状态失败：\n{0}")]
        public static readonly string GetInCombatFail;

        [Localized("Waymarks: Currently in combat, unable to place public waymarks.",
            "场地标点: 当前处于战斗状态，无法公开标点。")]
        public static readonly string InCombat;

        [Localized("Waymarks: cache restored", "场地标点: 已本地恢复暂存的标点。")]
        public static readonly string Load;

        [Localized("Waymarks: Local mark: \n{0}", "场地标点: 本地标记 \n{0}")]
        public static readonly string Local;

        [Localized("Waymarks: Public mark: \n{0}", "场地标点: 公开标记 \n{0}")]
        public static readonly string Public;

        [Localized("Waymarks: cache cleared", "场地标点: 已清除暂存的标点。")]
        public static readonly string Reset;

        [Localized("Waymarks: current waymarks saved to cache", "场地标点: 已暂存当前标点。")]
        public static readonly string Save;

        [Localized("Waymarks: Exception occurred when saving waymarks: \n{0}",
            "场地标点: 保存标记错误：\n{0}")]
        public static readonly string SaveException;
    }

    public override void GetOffsets() {
        base.GetOffsets();
        try {
            _ExecuteCommandDelegate = GetSig<ExecuteCommandDelegate>("E8 * * * * 48 83 C4 ?? C3 CC CC CC CC CC CC CC CC CC CC CC CC 48 83 EC ?? 45 0F B6 C0");
        }
        catch (Exception e) {
            PostNamazu.Log.Error("Failed to initialize _ExecuteCommandDelegate: " + e);
        }
    }

    /// <summary>
    ///     场地标点
    /// </summary>
    /// <param name="waymarks">标点合集对象</param>
    internal void DoWaymarks(WayMarks waymarks) {
        WriteWaymark(waymarks.A, 0);
        WriteWaymark(waymarks.B, 1);
        WriteWaymark(waymarks.C, 2);
        WriteWaymark(waymarks.D, 3);
        WriteWaymark(waymarks.One, 4);
        WriteWaymark(waymarks.Two, 5);
        WriteWaymark(waymarks.Three, 6);
        WriteWaymark(waymarks.Four, 7);
    }

    /// <summary>
    ///     场地标点
    /// </summary>
    /// <param name="waymarksStr">标点合集序列化Json字符串</param>
    [Command("place")]
    [Command("DoWaymarks")]
    public void DoWaymarks(string waymarksStr) {
        CheckBeforeExecution(waymarksStr);

        switch (waymarksStr.ToLower().Trim()) {
            case "save":
            case "backup":
                SaveWaymark();
                break;
            case "load":
            case "restore":
                LoadWaymark();
                break;
            case "reset":
                tempMarks = null;
                PluginUI.Log(L.Get("WayMark/Reset"));
                break;
            case "clear":
                DoWaymarks(new WayMarks {
                    A = new Waymark(),
                    B = new Waymark(),
                    C = new Waymark(),
                    D = new Waymark(),
                    One = new Waymark(),
                    Two = new Waymark(),
                    Three = new Waymark(),
                    Four = new Waymark()
                });
                PluginUI.Log(L.Get("WayMark/Clear"));
                break;
            case "public":
                if (GetInCombat()) {
                    Log(L.Get("WayMark/InCombat"));
                    return;
                }
                Public(ReadCurrentWaymarks());
                break;
            default:
                var waymarks = JsonConvert.DeserializeObject<WayMarks>(waymarksStr);
                if (waymarks.LocalOnly) {
                    DoWaymarks(waymarks);
                }
                else {
                    if (GetInCombat()) {
                        if (waymarks.Log) Log(L.Get("WayMark/InCombat"));
                        return;
                    }
                    Public(waymarks);
                }
                if (waymarks.Log) {
                    WayMarks.SetWaymarkIds(waymarks);
                    Log(L.Get(waymarks.LocalOnly ? "WayMark/Local" : "WayMark/Public", waymarks.ToString()));
                }
                break;
        }
    }

    /// <summary>
    ///     暂存当前标点
    /// </summary>
    public void SaveWaymark() {
        tempMarks = new WayMarks();

        try {
            tempMarks = ReadCurrentWaymarks();
            PluginUI.Log(L.Get("WayMark/Save"));
        }
        catch (Exception ex) {
            throw new Exception(L.Get("WayMark/SaveException", ex.Message));
        }
    }

    public unsafe WayMarks ReadCurrentWaymarks() {
        CheckBeforeExecution();
        var m = MarkingController.Instance()->FieldMarkers;
        var waymarks = new WayMarks {
            A = ReadWaymark(m[0], WaymarkID.A),
            B = ReadWaymark(m[1], WaymarkID.B),
            C = ReadWaymark(m[2], WaymarkID.C),
            D = ReadWaymark(m[3], WaymarkID.D),
            One = ReadWaymark(m[4], WaymarkID.One),
            Two = ReadWaymark(m[5], WaymarkID.Two),
            Three = ReadWaymark(m[6], WaymarkID.Three),
            Four = ReadWaymark(m[7], WaymarkID.Four)
        };
        return waymarks;

        static Waymark ReadWaymark(FieldMarker marker, WaymarkID id) {
            return new Waymark {
                Marker = marker,
                ID = id
            };
        }
    }

    /// <summary>
    ///     恢复暂存标点
    /// </summary>
    public void LoadWaymark() {
        if (tempMarks == null)
            return;
        DoWaymarks(tempMarks);
        PluginUI.Log(L.Get("WayMark/Load"));
    }

    /// <summary>
    ///     写入指定标点
    /// </summary>
    /// <param name="waymark">标点</param>
    /// <param name="id">ID</param>
    private unsafe void WriteWaymark(Waymark waymark, int id = -1) {
        if (waymark == null)
            return;

        var wId = id == -1 ? (byte)waymark.ID : id;
        if (wId is < 0 or >= 8) PostNamazu.Log.Error("ID必须在0-7范围内");
        GreyMagicMemoryBase.ExecuteWithLock(() => {
            Marshal.StructureToPtr(waymark.Marker,
                (IntPtr)MarkingController.Instance()
                + Marshal.OffsetOf<MarkingController>("_fieldMarkers")
                + id * Marshal.SizeOf<FieldMarker>(), false);
        });
    }

    /// <summary> 将指定标点标记为公开标点。 </summary>
    /// <param name="waymarks">标点，传入 null 时清空标点，单个标点为 null 时忽略。</param>
    public void Public(WayMarks waymarks) {
        if (waymarks == null || waymarks.All(waymark => waymark?.Active == false)) {
            // clear all
            ExecuteCommand(313);
            if (waymarks == null) {
                Log(L.Get("WayMark/ClearPublic"));
            }
        }
        else {
            var idx = -1;
            foreach (var waymark in waymarks) {
                idx++;
                if (waymark == null) continue;
                if (waymark.Active) {
                    // mark single
                    ExecuteCommand(317, idx, IntEncode(waymark.X), IntEncode(waymark.Y), IntEncode(waymark.Z));
                }
                else {
                    // clear single
                    ExecuteCommand(318, idx);
                }
            }
        }
    }

    internal static int IntEncode(float x) => (int)(x * 1000);

    // 统一使用 uint 调用此内部函数（参数常用于传入 id 等，uint 相比于 int 更合理）
    // 防止 GreyMagic 多次调用时参数类型不一致报错
    private void ExecuteCommand(int command, int a1 = 0, int a2 = 0, int a3 = 0, int a4 = 0)
        => _ExecuteCommandDelegate(command, a1, a2, a3, a4);

    public bool GetInCombat() {
        try {
            var op = ActGlobals.oFormActMain.ActPlugins
                .First(x => x.pluginObj.GetType() == typeof(PluginLoader))
                .pluginObj as PluginLoader;

            var pluginMain = op!.pluginMain;
            var container = pluginMain._container;
            var inCombatMemoryManager = container.Resolve<IInCombatMemory>();
            return inCombatMemoryManager.GetInCombat();
        }
        catch (Exception ex) {
            Log(L.Get("WayMark/GetInCombatFail", ex.ToString()));
            return false;
        }
    }
}