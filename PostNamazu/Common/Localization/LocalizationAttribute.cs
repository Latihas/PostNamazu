using System;

namespace PostNamazu.Common.Localization;

/// <summary>
///     标记一个可本地化的字符串
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class LocalizedAttribute(string english, string chinese) : Attribute {
	public string English { get; } = english;
	public string Chinese { get; } = chinese;
}

/// <summary>
///     标记一个包含本地化字符串的类
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class LocalizationProviderAttribute(string? prefix = null) : Attribute {
	public string? Prefix { get; } = prefix;
}