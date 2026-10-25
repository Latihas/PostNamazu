using System;
using System.Linq;
using RainbowMage.OverlayPlugin;

namespace PostNamazu.OverlayHoster;

public class Program : IOverlayAddonV2 {
	public readonly Action<string, string> PostNamazuDelegate;
	private readonly EventSource eventSource;

	public Program(Action<string, string> postNamazuDelegate) {
		PostNamazuDelegate = postNamazuDelegate;
		var container = Registry.GetContainer();
		var registry = container.Resolve<Registry>();
		if (registry.EventSources.FirstOrDefault(p => p?.Name == "鲶鱼精邮差") is not EventSource e) {
			eventSource = new EventSource(container);
			registry.StartEventSource(eventSource);
		} else eventSource = e;
		eventSource.PostNamazuDelegate = PostNamazuDelegate;
	}

	public void Init() {
	}

	public void DeInit() => eventSource.PostNamazuDelegate = null;
}