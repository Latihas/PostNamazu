using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace PostNamazu.Common.Localization;

/// <summary>
///     Module本地化基类，提供简洁的本地化定义方式
/// </summary>
[SuppressMessage("ReSharper", "UnusedType.Global")]
public abstract class ModuleLocalization {
	protected ModuleLocalization() {
		RegisterLocalizations();
	}

	/// <summary>
	///     注册当前类中定义的所有本地化字符串
	/// </summary>
	private void RegisterLocalizations() {
		var type = GetType();
		var moduleType = type.DeclaringType;
		var prefix = moduleType?.Name ?? type.Name;

		// 获取所有公共字段
		foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static)) {
			if (field.FieldType != typeof(LocalizedString) || field.GetValue(null) is not LocalizedString value) continue;
			LocalizationManager.Register($"{prefix}/{field.Name}", value.English, value.Chinese);
		}
	}
}

/// <summary>
///     表示一个本地化字符串
/// </summary>
public class LocalizedString(string english, string chinese) {
	public string English { get; } = english;
	public string Chinese { get; } = chinese;

	/// <summary>
	///     隐式转换，方便直接使用
	/// </summary>
	public static implicit operator string(LocalizedString str) => LocalizationManager.CurrentLanguage == Language.CN ? str.Chinese : str.English;
}