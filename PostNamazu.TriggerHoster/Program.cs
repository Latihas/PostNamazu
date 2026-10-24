using System;
using System.Collections.Generic;
using Triggernometry.Core;
using TriggernometryProxy;

namespace PostNamazu.TriggerHoster;

public class Program {
	public readonly Action<string, string> PostNamazuDelegate;
	public readonly Action<string> LogDelegate;

	private readonly List<int> _triggCallbackId = [];
	private readonly ProxyPlugin _plugin;

	public Program(ProxyPlugin plugin, string[] commands, Action<string, string> postNamazuDelegate, Action<string> logDelegate) {
		_plugin = plugin;
		PostNamazuDelegate = postNamazuDelegate;
		LogDelegate = logDelegate;
		foreach (var command in commands)
			_triggCallbackId.Add(RealPlugin.Instance.RegisterNamedCallback(command, new ProxyPlugin.CustomCallbackDelegate((_, payload) => PostNamazuDelegate(command, payload)), registrant: "PostNamazu"));
		_triggCallbackId.Add(RealPlugin.Instance.RegisterNamedCallback(
			"NamazuLog", new ProxyPlugin.CustomCallbackDelegate((_, log) => LogDelegate(log)), registrant: "PostNamazu")
		);
	}

	public void DeInit() {
		ClearAction();
	}

	private void ClearAction() {
		foreach (var id in _triggCallbackId)
			_plugin.UnregisterNamedCallback(id);
		_triggCallbackId.Clear();
	}
}