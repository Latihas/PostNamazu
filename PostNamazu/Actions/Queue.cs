using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PostNamazu.Attributes;
using PostNamazu.Common.Localization;

namespace PostNamazu.Actions;

[SuppressMessage("Performance", "CS0649")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Queue : NamazuModule {
	private static readonly List<string> QueuePending = []; //注册qid的队列

	// 本地化字符串定义
	[LocalizationProvider("Queue")] [SuppressMessage("ReSharper", "UnusedType.Local")]
	private static class Localizations {
		[Localized("Request to interrupt queue: {0}", "要求打断队列：{0}")]
		public static readonly string Break;

		[Localized("Queue {0} has been requested to interrupt", "队列 {0} 已被要求中断")]
		public static readonly string Broken;
	}

	[Command("queue")]
	[Command("DoQueueActions")]
	public async void DoQueue(string command) {
		try {
			CheckBeforeExecution(command);
			var actions = JsonConvert.DeserializeObject<QueueAction[]>(command);
			if (actions == null) return;
			var qid = ""; //QueueID，为空时不会受到打断指令的影响
			foreach (var action in actions) {
				await Task.Run(async () => {
					await Task.Delay(action.D);

					if (qid != "" && !QueuePending.Contains(qid)) //注册了qid，但是对应qid已经被移除(stop)的场合
						return; //打断
					switch (action.C.ToLower()) {
						case "qid":
							if (qid != "")
								QueuePending.Remove(qid); //如果之前已经有旧的qid，则先移除旧的qid
							qid = action.P;
							if (qid != "")
								QueuePending.Add(qid); //注册新的qid
							break;
						default:
							try {
								PostNamazu.DoAction(action.C, action.P);
							} catch (Exception e) {
								Log(e.ToString());
							}
							break;
					}
				});
				if (qid == "" || QueuePending.Contains(qid)) continue;
				Log(L.Get("Queue/Broken", qid));
				break;
			}
			if (qid != "")
				QueuePending.Remove(qid); //执行完毕，移除队列
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
		if (command.ToLower() == "all") QueuePending.Clear();
		else QueuePending.RemoveAll(qid => Regex.IsMatch(qid, $"^{command}$"));
	}

	public class QueueAction {
		[JsonProperty] public string C { get; set; }
		[JsonProperty] public string P { get; set; }
		[JsonProperty] public int D { get; set; }
	}
}