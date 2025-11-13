using Newtonsoft.Json;
using System;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using PostNamazu.Attributes;
using PostNamazu.Models;
using PostNamazu.Common.Localization;
using Triggernometry.PluginBridges.BridgeNamazu;

#pragma warning disable CS0649 // 从未对字段赋值，字段将一直保持其默认值

namespace PostNamazu.Actions
{
    internal class Mark : NamazuModule
    {
        private delegate IntPtr MarkingDelegate(long a1, uint markingTypeOrder, long id);
        private unsafe delegate void LocalMarkingDelegate(MarkingController* controller, uint markingTypeOrder, long id, uint a4);
        private static MarkingDelegate? _markingDelegate;
        private static LocalMarkingDelegate? _localMarkingDelegate;

        // 本地化字符串定义
        [LocalizationProvider("Mark")]
        private static class Localizations
        {
            [Localized("Could not find actor: {0}", "未能找到实体： {0}")]
            public static readonly string ActorNotFound;

            [Localized("Invalid format for actor marker", "实体标点格式错误")]
            public static readonly string Exception;
        }

        public override void GetOffsets() {
            base.GetOffsets();
            try {
                _markingDelegate = GetSig<MarkingDelegate>("E8 * * * * E8 ? ? ? ? 48 8B CB 48 89 86");
            }
            catch (Exception e) {
                PostNamazu.Log.Error("Failed to initialize _markingDelegate: " + e);
            }
            try {
                _localMarkingDelegate = GetSig<LocalMarkingDelegate>("E8 * * * * 4C 8B C5 8B D7 48 8B CB E8");
            }
            catch (Exception e) {
                PostNamazu.Log.Error("Failed to initialize _localMarkingDelegate: " + e);
            }
        }

        [Command("mark")]
        public void DoMarking(string command)
        {
            CheckBeforeExecution(command);
            var mark = JsonConvert.DeserializeObject<Marking>(command);
            if (mark?.MarkType == null) {
                throw new Exception(L.Get("Mark/Exception"));
            }
            var actor = GetActor(mark.ActorID, mark.Name);
            MarkActor(actor, mark.MarkType.Value, mark.Log, mark.LocalOnly);
        }

        private FFXIV_ACT_Plugin.Common.Models.Combatant GetActor(uint? id, string name)
        {
            if (id is (0xE0000000 or 0xE000000))
            {
                FFXIV_ACT_Plugin.Common.Models.Combatant actor = new()
                {
                    ID = 0xE0000000
                };
                return actor;
            }
            var combatants = FFXIV_ACT_Plugin.DataRepository.GetCombatantList().Where(i => !string.IsNullOrEmpty(i.Name) && i.ID != 0xE0000000);
            return combatants.FirstOrDefault(i => i.ID == id) 
                ?? combatants.FirstOrDefault(i => i.Name == name)
                ?? throw new Exception(L.Get("Mark/ActorNotFound", id?.ToString("X8") ?? name ?? "(null)"));
        }

        private unsafe void MarkActor(FFXIV_ACT_Plugin.Common.Models.Combatant actor, MarkType markingType, bool shouldLog, bool localOnly = false)
        {
            if (shouldLog)
            {
                PluginUI.Log($"Mark: Actor={actor.Name} (0x{actor.ID:X8}), Type={markingType} ({(int)markingType}), LocalOnly={localOnly}");
            }
            GreyMagicMemoryBase.ExecuteWithLock(() =>
            {
                _localMarkingDelegate(MarkingController.Instance(), (uint)(markingType - 1), actor.ID, 0);
                _markingDelegate(0, (uint)(markingType - 1), actor.ID);
            });
        }
    }
}
