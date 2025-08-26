namespace MonitorPresetManager.Forms
{
    partial class HotkeySettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new SplitContainer();
            this.groupBoxPresets = new GroupBox();
            this.listViewPresets = new ListView();
            this.columnHeaderPresetName = new ColumnHeader();
            this.columnHeaderHotkey = new ColumnHeader();
            this.groupBoxHotkeyConfig = new GroupBox();
            this.labelSelectedPreset = new Label();
            this.labelCurrentHotkey = new Label();
            this.groupBoxNewHotkey = new GroupBox();
            this.labelModifiers = new Label();
            this.checkBoxCtrl = new CheckBox();
            this.checkBoxAlt = new CheckBox();
            this.checkBoxShift = new CheckBox();
            this.checkBoxWin = new CheckBox();
            this.labelKey = new Label();
            this.comboBoxKey = new ComboBox();
            this.labelPreview = new Label();
            this.labelHotkeyPreview = new Label();
            this.buttonAssignHotkey = new Button();
            this.buttonRemoveHotkey = new Button();
            this.panelButtons = new Panel();
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBoxPresets.SuspendLayout();
            this.groupBoxHotkeyConfig.SuspendLayout();
            this.groupBoxNewHotkey.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = DockStyle.Fill;
            this.splitContainer1.Location = new Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBoxPresets);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBoxHotkeyConfig);
            this.splitContainer1.Size = new Size(600, 350);
            this.splitContainer1.SplitterDistance = 280;
            this.splitContainer1.TabIndex = 0;
            // 
            // groupBoxPresets
            // 
            this.groupBoxPresets.Controls.Add(this.listViewPresets);
            this.groupBoxPresets.Dock = DockStyle.Fill;
            this.groupBoxPresets.Location = new Point(0, 0);
            this.groupBoxPresets.Name = "groupBoxPresets";
            this.groupBoxPresets.Size = new Size(280, 350);
            this.groupBoxPresets.TabIndex = 0;
            this.groupBoxPresets.TabStop = false;
            this.groupBoxPresets.Text = "Presets";
            // 
            // listViewPresets
            // 
            this.listViewPresets.Columns.AddRange(new ColumnHeader[] {
            this.columnHeaderPresetName,
            this.columnHeaderHotkey});
            this.listViewPresets.Dock = DockStyle.Fill;
            this.listViewPresets.FullRowSelect = true;
            this.listViewPresets.GridLines = true;
            this.listViewPresets.Location = new Point(3, 19);
            this.listViewPresets.MultiSelect = false;
            this.listViewPresets.Name = "listViewPresets";
            this.listViewPresets.Size = new Size(274, 328);
            this.listViewPresets.TabIndex = 0;
            this.listViewPresets.UseCompatibleStateImageBehavior = false;
            this.listViewPresets.View = View.Details;
            // 
            // columnHeaderPresetName
            // 
            this.columnHeaderPresetName.Text = "Preset Name";
            this.columnHeaderPresetName.Width = 150;
            // 
            // columnHeaderHotkey
            // 
            this.columnHeaderHotkey.Text = "Hotkey";
            this.columnHeaderHotkey.Width = 100;
            // 
            // groupBoxHotkeyConfig
            // 
            this.groupBoxHotkeyConfig.Controls.Add(this.buttonRemoveHotkey);
            this.groupBoxHotkeyConfig.Controls.Add(this.buttonAssignHotkey);
            this.groupBoxHotkeyConfig.Controls.Add(this.groupBoxNewHotkey);
            this.groupBoxHotkeyConfig.Controls.Add(this.labelCurrentHotkey);
            this.groupBoxHotkeyConfig.Controls.Add(this.labelSelectedPreset);
            this.groupBoxHotkeyConfig.Dock = DockStyle.Fill;
            this.groupBoxHotkeyConfig.Location = new Point(0, 0);
            this.groupBoxHotkeyConfig.Name = "groupBoxHotkeyConfig";
            this.groupBoxHotkeyConfig.Size = new Size(316, 350);
            this.groupBoxHotkeyConfig.TabIndex = 0;
            this.groupBoxHotkeyConfig.TabStop = false;
            this.groupBoxHotkeyConfig.Text = "Hotkey Configuration";
            // 
            // labelSelectedPreset
            // 
            this.labelSelectedPreset.AutoSize = true;
            this.labelSelectedPreset.Location = new Point(6, 25);
            this.labelSelectedPreset.Name = "labelSelectedPreset";
            this.labelSelectedPreset.Size = new Size(89, 15);
            this.labelSelectedPreset.TabIndex = 0;
            this.labelSelectedPreset.Text = "Selected: None";
            // 
            // labelCurrentHotkey
            // 
            this.labelCurrentHotkey.AutoSize = true;
            this.labelCurrentHotkey.Location = new Point(6, 45);
            this.labelCurrentHotkey.Name = "labelCurrentHotkey";
            this.labelCurrentHotkey.Size = new Size(115, 15);
            this.labelCurrentHotkey.TabIndex = 1;
            this.labelCurrentHotkey.Text = "Current Hotkey: None";
            // 
            // groupBoxNewHotkey
            // 
            this.groupBoxNewHotkey.Controls.Add(this.labelHotkeyPreview);
            this.groupBoxNewHotkey.Controls.Add(this.labelPreview);
            this.groupBoxNewHotkey.Controls.Add(this.comboBoxKey);
            this.groupBoxNewHotkey.Controls.Add(this.labelKey);
            this.groupBoxNewHotkey.Controls.Add(this.checkBoxWin);
            this.groupBoxNewHotkey.Controls.Add(this.checkBoxShift);
            this.groupBoxNewHotkey.Controls.Add(this.checkBoxAlt);
            this.groupBoxNewHotkey.Controls.Add(this.checkBoxCtrl);
            this.groupBoxNewHotkey.Controls.Add(this.labelModifiers);
            this.groupBoxNewHotkey.Location = new Point(6, 75);
            this.groupBoxNewHotkey.Name = "groupBoxNewHotkey";
            this.groupBoxNewHotkey.Size = new Size(304, 180);
            this.groupBoxNewHotkey.TabIndex = 2;
            this.groupBoxNewHotkey.TabStop = false;
            this.groupBoxNewHotkey.Text = "New Hotkey";
            // 
            // labelModifiers
            // 
            this.labelModifiers.AutoSize = true;
            this.labelModifiers.Location = new Point(6, 25);
            this.labelModifiers.Name = "labelModifiers";
            this.labelModifiers.Size = new Size(81, 15);
            this.labelModifiers.TabIndex = 0;
            this.labelModifiers.Text = "Modifier Keys:";
            // 
            // checkBoxCtrl
            // 
            this.checkBoxCtrl.AutoSize = true;
            this.checkBoxCtrl.Location = new Point(6, 45);
            this.checkBoxCtrl.Name = "checkBoxCtrl";
            this.checkBoxCtrl.Size = new Size(45, 19);
            this.checkBoxCtrl.TabIndex = 1;
            this.checkBoxCtrl.Text = "Ctrl";
            this.checkBoxCtrl.UseVisualStyleBackColor = true;
            // 
            // checkBoxAlt
            // 
            this.checkBoxAlt.AutoSize = true;
            this.checkBoxAlt.Location = new Point(57, 45);
            this.checkBoxAlt.Name = "checkBoxAlt";
            this.checkBoxAlt.Size = new Size(40, 19);
            this.checkBoxAlt.TabIndex = 2;
            this.checkBoxAlt.Text = "Alt";
            this.checkBoxAlt.UseVisualStyleBackColor = true;
            // 
            // checkBoxShift
            // 
            this.checkBoxShift.AutoSize = true;
            this.checkBoxShift.Location = new Point(103, 45);
            this.checkBoxShift.Name = "checkBoxShift";
            this.checkBoxShift.Size = new Size(50, 19);
            this.checkBoxShift.TabIndex = 3;
            this.checkBoxShift.Text = "Shift";
            this.checkBoxShift.UseVisualStyleBackColor = true;
            // 
            // checkBoxWin
            // 
            this.checkBoxWin.AutoSize = true;
            this.checkBoxWin.Location = new Point(159, 45);
            this.checkBoxWin.Name = "checkBoxWin";
            this.checkBoxWin.Size = new Size(47, 19);
            this.checkBoxWin.TabIndex = 4;
            this.checkBoxWin.Text = "Win";
            this.checkBoxWin.UseVisualStyleBackColor = true;
            // 
            // labelKey
            // 
            this.labelKey.AutoSize = true;
            this.labelKey.Location = new Point(6, 80);
            this.labelKey.Name = "labelKey";
            this.labelKey.Size = new Size(29, 15);
            this.labelKey.TabIndex = 5;
            this.labelKey.Text = "Key:";
            // 
            // comboBoxKey
            // 
            this.comboBoxKey.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxKey.FormattingEnabled = true;
            this.comboBoxKey.Items.AddRange(new object[] {
            "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12",
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9",
            "NumPad0", "NumPad1", "NumPad2", "NumPad3", "NumPad4", "NumPad5", "NumPad6", "NumPad7", "NumPad8", "NumPad9",
            "Space", "Enter", "Escape", "Tab", "Back", "Delete", "Insert", "Home", "End", "PageUp", "PageDown",
            "Up", "Down", "Left", "Right"});
            this.comboBoxKey.Location = new Point(6, 100);
            this.comboBoxKey.Name = "comboBoxKey";
            this.comboBoxKey.Size = new Size(200, 23);
            this.comboBoxKey.TabIndex = 6;
            // 
            // labelPreview
            // 
            this.labelPreview.AutoSize = true;
            this.labelPreview.Location = new Point(6, 135);
            this.labelPreview.Name = "labelPreview";
            this.labelPreview.Size = new Size(51, 15);
            this.labelPreview.TabIndex = 7;
            this.labelPreview.Text = "Preview:";
            // 
            // labelHotkeyPreview
            // 
            this.labelHotkeyPreview.AutoSize = true;
            this.labelHotkeyPreview.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.labelHotkeyPreview.Location = new Point(6, 155);
            this.labelHotkeyPreview.Name = "labelHotkeyPreview";
            this.labelHotkeyPreview.Size = new Size(38, 15);
            this.labelHotkeyPreview.TabIndex = 8;
            this.labelHotkeyPreview.Text = "None";
            // 
            // buttonAssignHotkey
            // 
            this.buttonAssignHotkey.Location = new Point(6, 270);
            this.buttonAssignHotkey.Name = "buttonAssignHotkey";
            this.buttonAssignHotkey.Size = new Size(100, 30);
            this.buttonAssignHotkey.TabIndex = 3;
            this.buttonAssignHotkey.Text = "Assign Hotkey";
            this.buttonAssignHotkey.UseVisualStyleBackColor = true;
            // 
            // buttonRemoveHotkey
            // 
            this.buttonRemoveHotkey.Location = new Point(120, 270);
            this.buttonRemoveHotkey.Name = "buttonRemoveHotkey";
            this.buttonRemoveHotkey.Size = new Size(100, 30);
            this.buttonRemoveHotkey.TabIndex = 4;
            this.buttonRemoveHotkey.Text = "Remove Hotkey";
            this.buttonRemoveHotkey.UseVisualStyleBackColor = true;
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.buttonCancel);
            this.panelButtons.Controls.Add(this.buttonOK);
            this.panelButtons.Dock = DockStyle.Bottom;
            this.panelButtons.Location = new Point(0, 350);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new Size(600, 50);
            this.panelButtons.TabIndex = 1;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new Point(400, 15);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new Size(75, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new Point(485, 15);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // HotkeySettingsForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(600, 400);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panelButtons);
            this.Name = "HotkeySettingsForm";
            this.Text = "Hotkey Settings";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBoxPresets.ResumeLayout(false);
            this.groupBoxHotkeyConfig.ResumeLayout(false);
            this.groupBoxHotkeyConfig.PerformLayout();
            this.groupBoxNewHotkey.ResumeLayout(false);
            this.groupBoxNewHotkey.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private GroupBox groupBoxPresets;
        private ListView listViewPresets;
        private ColumnHeader columnHeaderPresetName;
        private ColumnHeader columnHeaderHotkey;
        private GroupBox groupBoxHotkeyConfig;
        private Label labelSelectedPreset;
        private Label labelCurrentHotkey;
        private GroupBox groupBoxNewHotkey;
        private Label labelModifiers;
        private CheckBox checkBoxCtrl;
        private CheckBox checkBoxAlt;
        private CheckBox checkBoxShift;
        private CheckBox checkBoxWin;
        private Label labelKey;
        private ComboBox comboBoxKey;
        private Label labelPreview;
        private Label labelHotkeyPreview;
        private Button buttonAssignHotkey;
        private Button buttonRemoveHotkey;
        private Panel panelButtons;
        private Button buttonOK;
        private Button buttonCancel;
    }
}