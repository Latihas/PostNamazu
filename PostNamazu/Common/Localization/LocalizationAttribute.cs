using System;

namespace PostNamazu.Common.Localization;

/// <summary>
///     标记一个可本地化的字符串
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class LocalizedAttribute : Attribute {
    public string English { get; set; }
    public string Chinese { get; set; }

    public LocalizedAttribute(string english, string chinese) {
        English = english;
        Chinese = chinese;
    }
}

/// <summary>
///     标记一个包含本地化字符串的类
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class LocalizationProviderAttribute : Attribute {
    public string Prefix { get; set; }

    public LocalizationProviderAttribute(string prefix = null) {
        Prefix = prefix;
    }
}