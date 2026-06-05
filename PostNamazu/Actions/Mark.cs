using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin.Common.Models;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Newtonsoft.Json;
using PostNamazu.Attributes;
using PostNamazu.Common.Localization;
using PostNamazu.Models;

namespace PostNamazu.Actions;

[SuppressMessage("Performance", "CS0649")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Mark : NamazuModule {
	private delegate IntPtr MarkingDelegate(long a1, uint markingTypeOrder, long id);

	private unsafe delegate void LocalMarkingDelegate(MarkingController* controller, uint markingTypeOrder, long id, uint a4);

	private static MarkingDelegate _markingDelegate;
	private static LocalMarkingDelegate _localMarkingDelegate;

	// 本地化字符串定义
	[LocalizationProvider("Mark")] [SuppressMessage("ReSharper", "UnusedType.Local")]
	private static class Localizations {
		[Localized("Could not find actor: {0}", "未能找到实体： {0}")]
		public static readonly string ActorNotFound;

		[Localized("Invalid format for actor marker", "实体标点格式错误")]
		public static readonly string Exception;
	}

	protected override void GetOffsets() {
		base.GetOffsets();
		try {
			_markingDelegate = GetSig<MarkingDelegate>("E8 * * * * E8 ? ? ? ? 48 8B CB 48 89 86");
		} catch (Exception e) {
			PostNamazu.Log.Error("Failed to initialize _markingDelegate: " + e);
		}
		try {
			_localMarkingDelegate = GetSig<LocalMarkingDelegate>("E8 * * * * 4C 8B C5 8B D7 48 8B CB E8");
		} catch (Exception e) {
			PostNamazu.Log.Error("Failed to initialize _localMarkingDelegate: " + e);
		}
	}

	[Command("mark")]
	public void DoMarking(string command) {
		CheckBeforeExecution(command);
		var mark = JsonConvert.DeserializeObject<Marking>(command);
		if (mark?.MarkType == null) {
			throw new Exception(L.Get("Mark/Exception"));
		}
		var actor = GetActor(mark.ActorID, mark.Name);
		MarkActor(actor, mark.MarkType.Value, mark.Log, mark.LocalOnly);
	}

	private static Combatant GetActor(uint? id, string? name) {
		if (id is 0xE0000000 or 0xE000000) {
			Combatant actor = new() {
				ID = 0xE0000000
			};
			return actor;
		}
		var combatants = ActGlobals.oFormActMain.FfxivPlugin.DataRepository.GetCombatantList().Where(i => !string.IsNullOrEmpty(i.Name) && i.ID != 0xE0000000).ToList();
		return combatants.FirstOrDefault(i => i.ID == id)
		       ?? combatants.FirstOrDefault(i => i.Name == name)
		       ?? throw new Exception(L.Get("Mark/ActorNotFound", id?.ToString("X8") ?? name ?? "(null)"));
	}

	private static unsafe void MarkActor(Combatant actor, MarkType markingType, bool shouldLog, bool localOnly = false) {
		if (shouldLog) {
			PluginUI.Log($"Mark: Actor={actor.Name} (0x{actor.ID:X8}), Type={markingType} ({(int)markingType}), LocalOnly={localOnly}");
		}
		PostNamazu.ExecuteWithLock(() => {
			_localMarkingDelegate(MarkingController.Instance(), (uint)(markingType - 1), actor.ID, 0);
			_markingDelegate(0, (uint)(markingType - 1), actor.ID);
		});
	}
}