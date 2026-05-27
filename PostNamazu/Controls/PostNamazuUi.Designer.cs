using System.Windows.Forms;

namespace PostNamazu;

partial class PostNamazuUi
{
	/// <summary> 
	/// 必需的设计器变量。
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary> 
	/// 清理所有正在使用的资源。
	/// </summary>
	/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing && (components != null))
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	#region 组件设计器生成的代码

	/// <summary> 
	/// 设计器支持所需的方法 - 不要修改
	/// 使用代码编辑器修改此方法的内容。
	/// </summary>
	public void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.CheckAutoStart = new CheckBox();
		this.ButtonStop = new Button();
		this.ButtonStart = new Button();
		this.TextPort = new NumericUpDown();
		this.lbPort = new Label();
		this.mainGroupBox = new GroupBox();
		this.mainPanel = new Panel();
		this.mainTable = new TableLayoutPanel();
		this.leftTable = new TableLayoutPanel();
		this.rightTable = new TableLayoutPanel();
		this.tableHttpRow1 = new TableLayoutPanel();
		this.tableHttpRow3 = new TableLayoutPanel();
		this.lstMessages = new ListBox();
		this.flowLayoutActions = new FlowLayoutPanel();
		this.ButtonClearMessage = new Button();
		this.ButtonCopySelection = new Button();
		this.ButtonCopyProblematic = new Button();
		this.grpHttp = new GroupBox();
		this.grpEnabledCmd = new GroupBox();
		this.grpLang = new GroupBox();
		this.radioButtonEN = new RadioButton();
		this.radioButtonCN = new RadioButton();
		this.grpWaymarks = new GroupBox();
		this.tableWaymarks = new TableLayoutPanel();
		this.btnWaymarksImport = new Button();
		this.btnWaymarksExport = new Button();
		this.logTip = new ToolTip(this.components);
		((System.ComponentModel.ISupportInitialize)(this.TextPort)).BeginInit();
		this.mainGroupBox.SuspendLayout();
		this.mainTable.SuspendLayout();
		this.tableHttpRow1.SuspendLayout();
		this.tableHttpRow3.SuspendLayout();
		this.grpHttp.SuspendLayout();
		this.grpEnabledCmd.SuspendLayout();
		this.grpLang.SuspendLayout();
		this.tableWaymarks.SuspendLayout();
		this.grpWaymarks.SuspendLayout();
		this.flowLayoutActions.SuspendLayout();
		this.mainPanel.SuspendLayout();
		this.SuspendLayout();
		// 
		// mainPanel
		// 
		this.mainPanel.Controls.Add(this.mainGroupBox);
		this.mainPanel.Dock = DockStyle.Fill;
		this.mainPanel.Margin = new Padding(8);
		this.mainPanel.Name = "mainPanel";
		this.mainPanel.Padding = new Padding(8);
		// 
		// mainGroupBox
		// 
		this.mainGroupBox.Controls.Add(this.mainTable);
		this.mainGroupBox.Dock = DockStyle.Fill;

		this.mainGroupBox.Margin = new Padding(6);
		this.mainGroupBox.Name = "mainGroupBox";
		this.mainGroupBox.Padding = new Padding(8);
		this.mainGroupBox.Text = "鲶鱼精邮差";
		// 
		// mainTable
		// 
		this.mainTable.AutoSize = true;
		this.mainTable.ColumnCount = 2;
		this.mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
		this.mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
		this.mainTable.Controls.Add(this.leftTable, 0, 0);
		this.mainTable.Controls.Add(this.rightTable, 1, 0);
		this.mainTable.Dock = DockStyle.Fill;
		this.mainTable.Name = "mainTable";
		this.mainTable.RowCount = 1;
		this.mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
		// 
		// leftTable
		// 
		this.leftTable.AutoSize = true;
		this.leftTable.ColumnCount = 1;
		this.leftTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            
		this.leftTable.Controls.Add(this.grpEnabledCmd, 0, 0);
		this.leftTable.Controls.Add(this.grpHttp, 0, 1);
		this.leftTable.Controls.Add(this.grpLang, 0, 2);
		this.leftTable.Controls.Add(this.grpWaymarks, 0, 3);
		this.leftTable.Dock = DockStyle.Fill;
		this.leftTable.Name = "leftTable";
		this.leftTable.RowCount = 4;

		this.leftTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
		this.leftTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		this.leftTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		this.leftTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		// 
		// rightTable
		// 
		this.rightTable.ColumnCount = 3;
		this.rightTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
		this.rightTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
		this.rightTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
		this.rightTable.Controls.Add(this.lstMessages, 0, 0);
		this.rightTable.Controls.Add(this.ButtonCopySelection, 0, 1);
		this.rightTable.Controls.Add(this.ButtonCopyProblematic, 1, 1);
		this.rightTable.Controls.Add(this.ButtonClearMessage, 2, 1);
		this.rightTable.Dock = DockStyle.Fill;
		this.rightTable.Name = "rightTable";
		this.rightTable.RowCount = 2;
		this.rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
		this.rightTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
		// 
		// grpHttp
		// 
		this.grpHttp.AutoSize = true;
		//this.grpHttp.Controls.Add(this.tableHttp);
		this.grpHttp.Controls.Add(this.tableHttpRow1);
		this.grpHttp.Controls.Add(this.TextPort);
		this.grpHttp.Controls.Add(this.tableHttpRow3);
		this.grpHttp.Dock = DockStyle.Fill;
		this.grpHttp.Margin = new Padding(0, 0, 4, 8);
		this.grpHttp.Name = "grpHttp";
		this.grpHttp.Padding = new Padding(6, 4, 6, 6);
		this.grpHttp.Text = "HTTP";
		// 
		// tableHttpRow1
		// 
		this.tableHttpRow1.AutoSize = true;
		this.tableHttpRow1.ColumnCount = 2;
		this.tableHttpRow1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
		this.tableHttpRow1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66.67F));
		this.tableHttpRow1.Controls.Add(this.lbPort, 0, 0);
		this.tableHttpRow1.Controls.Add(this.CheckAutoStart, 1, 0);
		this.tableHttpRow1.Dock = DockStyle.Top;
		this.tableHttpRow1.Name = "tableHttpRow1";
		this.tableHttpRow1.RowCount = 1;
		this.tableHttpRow1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		// 
		// tableHttpRow3
		// 
		this.tableHttpRow3.AutoSize = true;
		this.tableHttpRow3.ColumnCount = 2;
		this.tableHttpRow3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
		this.tableHttpRow3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
		this.tableHttpRow3.Controls.Add(this.ButtonStart, 0, 0);
		this.tableHttpRow3.Controls.Add(this.ButtonStop, 1, 0);
		this.tableHttpRow3.Dock = DockStyle.Bottom;
		this.tableHttpRow3.Name = "tableHttpRow3";
		this.tableHttpRow3.RowCount = 1;
		this.tableHttpRow3.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		// 
		// lbPort
		// 
		this.lbPort.Anchor = AnchorStyles.Left;
		this.lbPort.AutoSize = true;
		this.lbPort.Margin = new Padding(0, 0, 4, 0);
		this.lbPort.Name = "lbPort";
		this.lbPort.Text = "端口：";
		// 
		// TextPort
		// 
		this.TextPort.Dock = DockStyle.Bottom;
		this.TextPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0});
		this.TextPort.Name = "TextPort";
		this.TextPort.Size = new System.Drawing.Size(80, 23);
		this.TextPort.TabIndex = 1;
		this.TextPort.Value = new decimal(new int[] { 2019, 0, 0, 0});
		// 
		// ButtonStart
		// 
		this.ButtonStart.Dock = DockStyle.Fill;
		this.ButtonStart.Location = new System.Drawing.Point(2, 2);
		this.ButtonStart.Margin = new Padding(2);
		this.ButtonStart.Name = "ButtonStart";
		this.ButtonStart.Size = new System.Drawing.Size(80, 23);
		this.ButtonStart.TabIndex = 2;
		this.ButtonStart.Text = "开始";
		this.ButtonStart.UseVisualStyleBackColor = true;
		// 
		// ButtonStop
		// 
		this.ButtonStop.Dock = DockStyle.Fill;
		this.ButtonStop.Enabled = false;
		this.ButtonStop.Location = new System.Drawing.Point(86, 2);
		this.ButtonStop.Margin = new Padding(2);
		this.ButtonStop.Name = "ButtonStop";
		this.ButtonStop.Size = new System.Drawing.Size(80, 23);
		this.ButtonStop.TabIndex = 3;
		this.ButtonStop.Text = "停止";
		this.ButtonStop.UseVisualStyleBackColor = true;
		// 
		// CheckAutoStart
		// 
		this.CheckAutoStart.Anchor = AnchorStyles.Right;
		this.CheckAutoStart.AutoSize = true;
		this.CheckAutoStart.Location = new System.Drawing.Point(58, 2);
		this.CheckAutoStart.Margin = new Padding(0, 2, 2, 2);
		this.CheckAutoStart.Name = "CheckAutoStart";
		this.CheckAutoStart.Size = new System.Drawing.Size(96, 19);
		this.CheckAutoStart.TabIndex = 4;
		this.CheckAutoStart.Text = "自动启动监听";
		this.CheckAutoStart.UseVisualStyleBackColor = true;
		// 
		// grpEnabledCmd
		// 
		this.grpEnabledCmd.Controls.Add(this.flowLayoutActions);
		this.grpEnabledCmd.Dock = DockStyle.Fill;
		this.grpEnabledCmd.Margin = new Padding(0, 0, 4, 8);
		this.grpEnabledCmd.Name = "grpEnabledCmd";
		this.grpEnabledCmd.Padding = new Padding(6, 4, 6, 6);
		this.grpEnabledCmd.Text = "启用以下动作";
		// 
		// flowLayoutActions
		// 
		this.flowLayoutActions.AutoScroll = true;
		this.flowLayoutActions.Dock = DockStyle.Fill;
		this.flowLayoutActions.FlowDirection = FlowDirection.TopDown;
		this.flowLayoutActions.Margin = new Padding(0);
		this.flowLayoutActions.Name = "flowLayoutActions";
		this.flowLayoutActions.WrapContents = false;
		// 
		// grpLang
		// 
		this.grpLang.AutoSize = true;
		this.grpLang.Controls.Add(this.radioButtonCN);
		this.grpLang.Controls.Add(this.radioButtonEN);
		this.grpLang.Dock = DockStyle.Fill;
		this.grpLang.Margin = new Padding(0, 0, 4, 8);
		this.grpLang.Name = "grpLang";
		this.grpLang.Padding = new Padding(6, 4, 6, 6);
		this.grpLang.Text = "语言";
		// 
		// radioButtonEN
		// 
		this.radioButtonEN.AutoSize = true;
		this.radioButtonEN.Dock = DockStyle.Top;
		this.radioButtonEN.Name = "radioButtonEN";
		this.radioButtonEN.TabStop = false;
		this.radioButtonEN.Text = " English";
		this.radioButtonEN.UseVisualStyleBackColor = true;
		this.radioButtonEN.CheckedChanged += LanguageRadioButton_CheckedChanged;
		// 
		// radioButtonCN
		// 
		this.radioButtonCN.AutoSize = true;
		this.radioButtonCN.Dock = DockStyle.Top;
		this.radioButtonCN.Name = "radioButtonCN";
		this.radioButtonCN.TabStop = false;
		this.radioButtonCN.Text = " 中文";
		this.radioButtonCN.UseVisualStyleBackColor = true;
		this.radioButtonCN.CheckedChanged += LanguageRadioButton_CheckedChanged;
		// 
		// grpWaymarks
		// 
		this.grpWaymarks.AutoSize = true;
		this.grpWaymarks.Controls.Add(this.tableWaymarks);
		this.grpWaymarks.Dock = DockStyle.Fill;
		this.grpWaymarks.Margin = new Padding(0, 0, 4, 0);
		this.grpWaymarks.Name = "grpWaymarks";
		this.grpWaymarks.Padding = new Padding(6, 4, 6, 6);
		this.grpWaymarks.Text = "场地标点";
		// 
		// tableWaymarks
		// 
		this.tableWaymarks.AutoSize = true;
		this.tableWaymarks.ColumnCount = 2;
		this.tableWaymarks.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
		this.tableWaymarks.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
		this.tableWaymarks.Controls.Add(this.btnWaymarksImport, 0, 0);
		this.tableWaymarks.Controls.Add(this.btnWaymarksExport, 1, 0);
		this.tableWaymarks.Dock = DockStyle.Fill;
		this.tableWaymarks.Margin = new Padding(0);
		this.tableWaymarks.Name = "tableWaymarks";
		this.tableWaymarks.RowCount = 1;
		this.tableWaymarks.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		// 
		// btnWaymarksImport
		// 
		this.btnWaymarksImport.Dock = DockStyle.Fill;
		this.btnWaymarksImport.Location = new System.Drawing.Point(2, 2);
		this.btnWaymarksImport.Margin = new Padding(2);
		this.btnWaymarksImport.Name = "btnWaymarksImport";
		this.btnWaymarksImport.Size = new System.Drawing.Size(92, 23);
		this.btnWaymarksImport.TabStop = false;
		this.btnWaymarksImport.Text = "导入";
		this.btnWaymarksImport.UseVisualStyleBackColor = true;
		this.btnWaymarksImport.Click +=BtnWaymarksImport_Click;
		// 
		// btnWaymarksExport
		// 
		this.btnWaymarksExport.Dock = DockStyle.Fill;
		this.btnWaymarksExport.Location = new System.Drawing.Point(98, 2);
		this.btnWaymarksExport.Margin = new Padding(2);
		this.btnWaymarksExport.Name = "btnWaymarksExport";
		this.btnWaymarksExport.Size = new System.Drawing.Size(92, 23);
		this.btnWaymarksExport.TabStop = false;
		this.btnWaymarksExport.Text = "导出";
		this.btnWaymarksExport.UseVisualStyleBackColor = true;
		this.btnWaymarksExport.Click += BtnWaymarksExport_Click;
		// 
		// lstMessages
		// 
		this.rightTable.SetColumnSpan(this.lstMessages, 3);
		this.lstMessages.BorderStyle = BorderStyle.FixedSingle;
		this.lstMessages.Dock = DockStyle.Fill;

		this.lstMessages.FormattingEnabled = true;
		this.lstMessages.HorizontalScrollbar = true;
		this.lstMessages.ItemHeight = 13;
		this.lstMessages.Location = new System.Drawing.Point(6, 6);
		this.lstMessages.Margin = new Padding(6, 6, 6, 2);
		this.lstMessages.Name = "lstMessages";
		this.lstMessages.SelectionMode = SelectionMode.MultiExtended;
		this.lstMessages.Size = new System.Drawing.Size(408, 357);
		this.lstMessages.TabIndex = 0;
		this.lstMessages.TabStop = false;
		this.lstMessages.MouseMove +=LstMessages_MouseMove;
		// 
		// ButtonClearMessage
		// 
		this.ButtonClearMessage.Dock = DockStyle.Fill;
		this.ButtonClearMessage.Location = new System.Drawing.Point(8, 367);
		this.ButtonClearMessage.Margin = new Padding(2, 2, 4, 2);
		this.ButtonClearMessage.Name = "ButtonClearMessage";
		this.ButtonClearMessage.Size = new System.Drawing.Size(326, 31);
		this.ButtonClearMessage.TabIndex = 1;
		this.ButtonClearMessage.TabStop = false;
		this.ButtonClearMessage.Text = "清空全部日志";
		this.ButtonClearMessage.UseVisualStyleBackColor = true;
		this.ButtonClearMessage.Click += CmdClearMessages_Click;
		// 
		// ButtonCopySelection
		// 
		this.ButtonCopySelection.Dock = DockStyle.Fill;
		this.ButtonCopySelection.Location = new System.Drawing.Point(340, 367);
		this.ButtonCopySelection.Margin = new Padding(2);
		this.ButtonCopySelection.Name = "ButtonCopySelection";
		this.ButtonCopySelection.Size = new System.Drawing.Size(86, 31);
		this.ButtonCopySelection.TabIndex = 2;
		this.ButtonCopySelection.TabStop = false;
		this.ButtonCopySelection.Text = "复制选中日志";
		this.ButtonCopySelection.UseVisualStyleBackColor = true;
		this.ButtonCopySelection.Click += this.CmdCopySelection_Click;
		// 
		// ButtonCopyProblematic
		// 
		this.ButtonCopyProblematic.Dock = DockStyle.Fill;
		this.ButtonCopyProblematic.Location = new System.Drawing.Point(430, 367);
		this.ButtonCopyProblematic.Margin = new Padding(2);
		this.ButtonCopyProblematic.Name = "ButtonCopyProblematic";
		this.ButtonCopyProblematic.Size = new System.Drawing.Size(86, 31);
		this.ButtonCopyProblematic.TabIndex = 3;
		this.ButtonCopyProblematic.TabStop = false;
		this.ButtonCopyProblematic.Text = "复制全部日志";
		this.ButtonCopyProblematic.UseVisualStyleBackColor = true;
		this.ButtonCopyProblematic.Click += CmdCopyProblematic_Click;
		// 
		// logTip
		// 
		this.logTip.AutoPopDelay = 32767;
		this.logTip.InitialDelay = 200;
		this.logTip.ReshowDelay = 200;
		this.logTip.ShowAlways = true;
		// 
		// PostNamazuUi
		// 
		this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
		this.AutoScaleMode = AutoScaleMode.Font;
		this.Controls.Add(this.mainPanel);
		this.Dock = DockStyle.Fill;
		this.DoubleBuffered = true;
		this.Margin = new Padding(2);
		this.Name = "PostNamazuUi";
		this.Size = new System.Drawing.Size(625, 373);
		this.TextPort.EndInit();
		this.mainGroupBox.ResumeLayout(false);
		this.mainGroupBox.PerformLayout();
		this.flowLayoutActions.ResumeLayout(false);
		this.flowLayoutActions.PerformLayout();
		this.mainTable.ResumeLayout(false);
		this.mainTable.PerformLayout();
		this.tableHttpRow1.ResumeLayout(false);
		this.tableHttpRow1.PerformLayout();
		this.tableHttpRow3.ResumeLayout(false);
		this.tableWaymarks.ResumeLayout(false);
		this.tableWaymarks.PerformLayout();
		this.grpWaymarks.ResumeLayout(false);
		this.grpWaymarks.PerformLayout();
		this.grpHttp.ResumeLayout(false);
		this.grpHttp.PerformLayout();
		this.grpEnabledCmd.ResumeLayout(false);
		this.grpEnabledCmd.PerformLayout();
		this.grpLang.ResumeLayout(false);
		this.grpLang.PerformLayout();
		this.mainPanel.ResumeLayout(false);
		this.mainPanel.PerformLayout();
		this.ResumeLayout(false);
	}

	#endregion

	public CheckBox CheckAutoStart;
	public Button ButtonStop;
	public Button ButtonStart;
	public NumericUpDown TextPort;
	public Label lbPort;
	public GroupBox mainGroupBox;
	public Button ButtonCopySelection;
	public Button ButtonCopyProblematic;
	public Button ButtonClearMessage;
	public GroupBox grpHttp;
	public GroupBox grpEnabledCmd;
	public GroupBox grpLang;
	public GroupBox grpWaymarks;
	private TableLayoutPanel tableWaymarks;
	public Button btnWaymarksImport;
	public Button btnWaymarksExport;
	private ToolTip logTip;
	public FlowLayoutPanel flowLayoutActions;
	private Panel mainPanel;
	private TableLayoutPanel mainTable;
	private TableLayoutPanel leftTable;
	private TableLayoutPanel rightTable;
	private TableLayoutPanel tableHttpRow1;
	private TableLayoutPanel tableHttpRow3;
	private RadioButton radioButtonEN;
	private RadioButton radioButtonCN;
	public ListBox lstMessages;
}