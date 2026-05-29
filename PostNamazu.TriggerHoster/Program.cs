using System;
using System.Collections.Generic;
using Triggernometry.Core;
using TriggernometryProxy;

namespace PostNamazu.TriggerHoster;

public class Program(ProxyPlugin plugin) {
	public Action<string, string> PostNamazuDelegate = null;
	public Action<string> LogDelegate = null;

	private readonly List<int> _triggCallbackId = [];

	public void Init(string[] commands) {
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
			plugin.UnregisterNamedCallback(id);
		_triggCallbackId.Clear();
	}
}