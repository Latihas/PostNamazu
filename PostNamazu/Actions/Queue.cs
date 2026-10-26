using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PostNamazu.Attributes;
using PostNamazu.Common.Localization;

namespace PostNamazu.Actions;

[SuppressMessage("Performance", "CS0649")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Queue : NamazuModule {
	private static readonly Dictionary<Guid, string> QueuePending = []; //注册qid的队列实例
	private static readonly Lock QueuePendingLock = new();

	// 本地化字符串定义
	[LocalizationProvider("Queue")]
	[SuppressMessage("ReSharper", "UnusedType.Local")]
	private static class Localizations {
		[Localized("Request to interrupt queue: {0}", "要求打断队列：{0}")]
		public static readonly string? Break;

		[Localized("Queue {0} has been requested to interrupt", "队列 {0} 已被要求中断")]
		public static readonly string? Broken;
	}

	[Command("queue")]
	[Command("DoQueueActions")]
	public async void DoQueue(string command) {
		try {
			CheckBeforeExecution(command);
			var actions = JsonConvert.DeserializeObject<QueueAction[]>(command);
			if (actions == null) return;
			var qid = ""; //QueueID，为空时不会受到打断指令的影响
			Guid? registration = null;
			try {
			foreach (var action in actions) {
				await Task.Delay(Math.Max(0, action.D));

				var interrupted = false;
				var isQueueIdAction = action.C.Equals("qid", StringComparison.OrdinalIgnoreCase);
				lock (QueuePendingLock) {
					if (registration.HasValue && !QueuePending.ContainsKey(registration.Value)) {
						interrupted = true;
					} else if (isQueueIdAction) {
						if (registration.HasValue)
							QueuePending.Remove(registration.Value);
						qid = action.P;
						registration = qid == "" ? null : Guid.NewGuid();
						if (registration.HasValue)
							QueuePending.Add(registration.Value, qid); //注册新的qid实例
					}
				}
				if (interrupted) break;
				if (!isQueueIdAction) {
					try {
						PostNamazu.DoAction(action.C, action.P);
					} catch (Exception e) {
						Log(e.ToString());
					}
				}
				lock (QueuePendingLock) {
					if (!registration.HasValue || QueuePending.ContainsKey(registration.Value)) continue;
				}
				Log(L.Get("Queue/Broken", qid));
				break;
			}
			} finally {
				lock (QueuePendingLock) {
					if (registration.HasValue)
						QueuePending.Remove(registration.Value); //执行完毕，移除本队列的注册
				}
			}
		} catch (Exception e) {
			Log(e.ToString());
		}
	}

	[Command("stop")]
	[Command("break")]
	[Command("BreakQueueActions")]
	[SuppressMessage("Performance", "CA1822")]
	public void BreakQueue(string command) {
		Log(L.Get("Queue/Break", command));
		lock (QueuePendingLock) {
			if (command.Equals("all", StringComparison.OrdinalIgnoreCase)) QueuePending.Clear();
			else {
				foreach (var id in QueuePending.Where(entry => Regex.IsMatch(entry.Value, $"^{command}$")).Select(entry => entry.Key).ToArray())
					QueuePending.Remove(id);
			}
		}
	}

	public class QueueAction {
		[JsonProperty] public string C { get; set; } = "";
		[JsonProperty] public string P { get; set; } = "";
		[JsonProperty] public int D { get; set; }
	}
}
