using System;
using System.Windows.Forms;
using PostNamazu.Common.Localization;

namespace PostNamazu.Common;

/// <summary>
///     异常处理管理器
/// </summary>
public static class ExceptionHandler {
	/// <summary>
	///     处理HTTP服务器异常
	/// </summary>
	public static void HandleHttpServerException(Exception ex, int port, PostNamazuUi? ui, Action? enableStartButton, Action? disableStopButton) {
		var errorMessage = L.Get("PostNamazu/httpException", port, ex.Message);
		enableStartButton?.Invoke();
		disableStopButton?.Invoke();
		ui?.Log(errorMessage);
		MessageBox.Show(errorMessage);
	}

	/// <summary>
	///     处理动作执行异常
	/// </summary>
	public static void HandleActionExecutionException(Exception ex, string command, PostNamazuUi? ui) {
		ui?.Log(L.Get("PostNamazu/doActionFail", command, ex.Message + "\n" + ex.StackTrace));
	}
}