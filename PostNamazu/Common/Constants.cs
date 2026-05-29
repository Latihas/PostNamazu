namespace PostNamazu.Common;

/// <summary>
///     项目常量定义
/// </summary>
public static class Constants {
	/// <summary>
	///     模组初始化最大等待次数
	/// </summary>
	public const int ModuleInitMaxWaitCount = 20;

	/// <summary>
	///     模组初始化等待间隔（毫秒）
	/// </summary>
	public const int ModuleInitWaitInterval = 1000;

	/// <summary>
	///     当前频道前缀
	/// </summary>
	public const string CurrentChannelPrefix = "/current ";

	/// <summary>
	///     插件名称
	/// </summary>
	public const string PluginName = "PostNamazu";
}