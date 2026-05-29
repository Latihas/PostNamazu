using System;
using System.Linq;
using RainbowMage.OverlayPlugin;

namespace PostNamazu.OverlayHoster;

public class Program : IOverlayAddonV2 {
	public Action<string, string> PostNamazuDelegate;
	private EventSource eventSource;
	
	public void Init() {
		var container = Registry.GetContainer();
		var registry = container.Resolve<Registry>();
		if (registry.EventSources.FirstOrDefault(p => p.Name == "鲶鱼精邮差") is not EventSource e) {
			eventSource = new EventSource(container);
			registry.StartEventSource(eventSource);
		} else eventSource = e;
		eventSource.PostNamazuDelegate = PostNamazuDelegate;
	}

	public void DeInit() {
		eventSource.PostNamazuDelegate = null;
	}
}