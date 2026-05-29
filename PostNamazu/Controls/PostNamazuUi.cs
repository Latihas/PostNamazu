using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using PostNamazu.Actions;
using PostNamazu.Common.Localization;

// ReSharper disable CheckNamespace

namespace PostNamazu;

public partial class PostNamazuUi : UserControl {
	public PostNamazuUi() {
		InitializeComponent();
		LoadSettings();
	}

	public bool AutoStart => CheckAutoStart.Checked;
	private static string SettingsFile => Path.Combine(PostNamazu.DalamudPluginInterface.ConfigDirectory.ToString(), "PostNamazu.config.xml");
	public readonly Dictionary<string, bool> ActionEnabled = new();

	public void RegisterAction(string name) {
		ActionEnabled.TryAdd(name, true);
		CheckBox checkAction = new() {
			Text = name,
			Checked = ActionEnabled[name],
			AutoSize = true
		};
		checkAction.CheckedChanged += CheckBoxActions_CheckedChanged;
		flowLayoutActions.Controls.Add(checkAction);
	}

	private void CheckBoxActions_CheckedChanged(object? sender, EventArgs e) {
		var checkbox = (CheckBox)sender!;
		ActionEnabled[checkbox.Text] = checkbox.Checked;
	}

	public void Log(string log) {
		AddParserMessage(log);
	}

	private void CmdCopyProblematic_Click(object sender, EventArgs e) => CopyLog(true);

	private void CmdCopySelection_Click(object sender, EventArgs e) => CopyLog(false);

	private void CopyLog(bool copyAll) {
		var stringBuilder = new StringBuilder();
		var source = copyAll ? lstMessages.Items.Cast<object>() : lstMessages.SelectedItems.Cast<object>();
		foreach (var item in source)
			stringBuilder.AppendLine((item ?? "").ToString());
		if (stringBuilder.Length == 0) return;
		stringBuilder.Remove(stringBuilder.Length - Environment.NewLine.Length, Environment.NewLine.Length);
		Clipboard.SetText(stringBuilder.ToString());
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
		if (keyData != (Keys.Control | Keys.C)) return base.ProcessCmdKey(ref msg, keyData);
		CopyLog(false);
		return true;
	}

	private void CmdClearMessages_Click(object sender, EventArgs e) {
		lstMessages.Items.Clear();
	}

	private int prevTipIdx = -1;

	private void LstMessages_MouseMove(object sender, MouseEventArgs e) {
		var lb = (ListBox)sender;
		var index = lb.IndexFromPoint(e.Location);
		if (index == prevTipIdx) return;
		if (index != -1)
			logTip.SetToolTip(lb, lb.Items[index].ToString());
		else
			logTip.RemoveAll();
		prevTipIdx = index;
	}

	private void AddParserMessage(string message) {
		try {
			lstMessages?.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
			PostNamazu.Plugin.Log.Info($"[PostNamazu][{DateTime.Now:HH:mm:ss}] {message}");
		} catch (Exception) {
			//
		}
	}

	private void LoadSettings() {
		if (!File.Exists(SettingsFile)) return;
		XmlDocument xdo = new();
		try {
			xdo.Load(SettingsFile);
			var head = xdo.SelectSingleNode("Config");
			TextPort.Text = head?.SelectSingleNode("Port")?.InnerText;
			if (string.IsNullOrEmpty(TextPort.Text))
				TextPort.Text = "2019";
			CheckAutoStart.Checked = bool.Parse(head?.SelectSingleNode("AutoStart")?.InnerText ?? "false");

			var language = head?.SelectSingleNode("Language")?.InnerText;
			if (string.IsNullOrEmpty(language) || !Enum.TryParse(language, out Language currentLang)) {
				currentLang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "zh" ? Language.CN : Language.EN;
			}
			LocalizationManager.CurrentLanguage = currentLang;
			if (currentLang == Language.EN) {
				radioButtonEN.Checked = true;
			} else {
				radioButtonCN.Checked = true;
			}
			TranslateUi();

			var actionList = head?.SelectSingleNode("Actions")?.ChildNodes;
			if (actionList == null) return;
			foreach (XmlNode action in actionList)
				ActionEnabled[action.Name] = bool.Parse(action.InnerText);
		} catch (Exception ex) {
			Log(L.Get("PostNamazu/cfgLoadException", ex.ToString()));
			File.Delete(SettingsFile);
			Log(L.Get("PostNamazu/cfgReset"));
		}
	}

	public void SaveSettings() {
		var fs = new FileStream(SettingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		var xWriter = new XmlTextWriter(fs, Encoding.UTF8) {
			Formatting = Formatting.Indented,
			Indentation = 1,
			IndentChar = '\t'
		};
		xWriter.WriteStartDocument(true);
		xWriter.WriteStartElement("Config"); // <Config>
		xWriter.WriteElementString("Port", TextPort.Text);
		xWriter.WriteElementString("AutoStart", CheckAutoStart.Checked.ToString());
		xWriter.WriteElementString("Language", LocalizationManager.CurrentLanguage.ToString());
		xWriter.WriteStartElement("Actions"); // <Actions>
		foreach (var action in ActionEnabled)
			xWriter.WriteElementString(action.Key, action.Value.ToString());
		xWriter.WriteEndElement(); // </Actions>
		xWriter.WriteEndElement(); // </Config>
		xWriter.WriteEndDocument(); // Tie up loose ends (shouldn't be any)
		xWriter.Flush(); // Flush the file buffer to disk
		xWriter.Close();
	}

	private void LanguageRadioButton_CheckedChanged(object sender, EventArgs e) {
		if (radioButtonEN.Checked) {
			LocalizationManager.CurrentLanguage = Language.EN;
		} else if (radioButtonCN.Checked) {
			LocalizationManager.CurrentLanguage = Language.CN;
		}
		TranslateUi();
	}

	/// <summary> 根据模组状态更新对应动作的颜色。</summary>
	/// <param name="actionName">模组的类名（Type.Name）。</param>
	/// <param name="state"></param>
	internal void UpdateActionColorByState(string actionName, PostNamazu.StateEnum state) {
		if (flowLayoutActions.InvokeRequired) {
			flowLayoutActions.Invoke(() => UpdateActionColorByState(actionName, state));
			return;
		}
		var checkBox = flowLayoutActions.Controls.OfType<CheckBox>().FirstOrDefault(chk => chk.Text == actionName);
		checkBox?.ForeColor = state switch {
			PostNamazu.StateEnum.Failure => Color.FromArgb(180, 45, 30) // red
			,
			PostNamazu.StateEnum.Waiting => Color.FromArgb(150, 105, 0) // yellow
			,
			PostNamazu.StateEnum.Ready => Color.FromArgb(15, 90, 60) // green
			,
			PostNamazu.StateEnum.NotReady => Color.Black, _ => checkBox.ForeColor
		};
	}

	private void TranslateUi() {
		SuspendLayout();
		Parent?.Text = L.Get("PostNamazu/title");
		RecursiveTranslateControls(this);
		ResumeLayout(true);
	}

	private static void RecursiveTranslateControls(Control control) {
		if (!string.IsNullOrEmpty(control.Text)) {
			// 尝试通过控件名称查找翻译
			var key = $"PostNamazu/{control.Name}";
			var text = L.Get(key);
			if (text != $"[{key}]") // 如果找到了翻译（不是返回的默认值）
			{
				control.Text = text;
			}
		}
		foreach (Control child in control.Controls) {
			RecursiveTranslateControls(child);
		}
	}

	private void BtnWaymarksImport_Click(object sender, EventArgs e) {
		var importForm = new ImportWaymarksForm();
		importForm.Show(this);
		importForm.BringToFront();
	}

	private void BtnWaymarksExport_Click(object sender, EventArgs e) {
		try {
			var data = GetCurrentWaymarksString();
			Clipboard.SetText(data);
			MessageBox.Show(L.Get("PostNamazu/exportWaymarks"), "PostNamazu", MessageBoxButtons.OK, MessageBoxIcon.Information);
		} catch (Exception ex) {
			MessageBox.Show(L.Get("PostNamazu/exportWaymarksFail", ex.ToString()), "PostNamazu", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private static string GetCurrentWaymarksString() {
		var waymarks = PostNamazu.Plugin.GetModuleInstance<WayMark>();
		return waymarks == null ? "{}" : waymarks.ReadCurrentWaymarks().ToJsonString();
	}
}