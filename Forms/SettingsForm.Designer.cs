namespace MonitorPresetManager.Forms
{
    partial class SettingsForm
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
            this.groupBoxStartup = new GroupBox();
            this.labelCurrentStatus = new Label();
            this.checkBoxStartMinimized = new CheckBox();
            this.checkBoxStartWithWindows = new CheckBox();
            this.labelDescription = new Label();
            this.panelButtons = new Panel();
            this.buttonApply = new Button();
            this.buttonCancel = new Button();
            this.buttonOK = new Button();
            this.groupBoxStartup.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxStartup
            // 
            this.groupBoxStartup.Controls.Add(this.labelCurrentStatus);
            this.groupBoxStartup.Controls.Add(this.checkBoxStartMinimized);
            this.groupBoxStartup.Controls.Add(this.checkBoxStartWithWindows);
            this.groupBoxStartup.Controls.Add(this.labelDescription);
            this.groupBoxStartup.Location = new Point(12, 12);
            this.groupBoxStartup.Name = "groupBoxStartup";
            this.groupBoxStartup.Size = new Size(360, 150);
            this.groupBoxStartup.TabIndex = 0;
            this.groupBoxStartup.TabStop = false;
            this.groupBoxStartup.Text = "Startup Settings";
            // 
            // labelDescription
            // 
            this.labelDescription.Location = new Point(6, 25);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new Size(348, 30);
            this.labelDescription.TabIndex = 0;
            this.labelDescription.Text = "Configure how Monitor Preset Manager starts with Windows.";
            // 
            // checkBoxStartWithWindows
            // 
            this.checkBoxStartWithWindows.AutoSize = true;
            this.checkBoxStartWithWindows.Location = new Point(6, 65);
            this.checkBoxStartWithWindows.Name = "checkBoxStartWithWindows";
            this.checkBoxStartWithWindows.Size = new Size(180, 19);
            this.checkBoxStartWithWindows.TabIndex = 1;
            this.checkBoxStartWithWindows.Text = "Start with Windows";
            this.checkBoxStartWithWindows.UseVisualStyleBackColor = true;
            // 
            // checkBoxStartMinimized
            // 
            this.checkBoxStartMinimized.AutoSize = true;
            this.checkBoxStartMinimized.Location = new Point(25, 90);
            this.checkBoxStartMinimized.Name = "checkBoxStartMinimized";
            this.checkBoxStartMinimized.Size = new Size(180, 19);
            this.checkBoxStartMinimized.TabIndex = 2;
            this.checkBoxStartMinimized.Text = "Start minimized to system tray";
            this.checkBoxStartMinimized.UseVisualStyleBackColor = true;
            // 
            // labelCurrentStatus
            // 
            this.labelCurrentStatus.AutoSize = true;
            this.labelCurrentStatus.Location = new Point(6, 120);
            this.labelCurrentStatus.Name = "labelCurrentStatus";
            this.labelCurrentStatus.Size = new Size(85, 15);
            this.labelCurrentStatus.TabIndex = 3;
            this.labelCurrentStatus.Text = "Status: Unknown";
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.buttonApply);
            this.panelButtons.Controls.Add(this.buttonCancel);
            this.panelButtons.Controls.Add(this.buttonOK);
            this.panelButtons.Dock = DockStyle.Bottom;
            this.panelButtons.Location = new Point(0, 220);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new Size(384, 50);
            this.panelButtons.TabIndex = 1;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new Point(135, 15);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new Size(75, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new Point(220, 15);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new Point(305, 15);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new Size(75, 23);
            this.buttonApply.TabIndex = 2;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(384, 270);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.groupBoxStartup);
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.groupBoxStartup.ResumeLayout(false);
            this.groupBoxStartup.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBoxStartup;
        private Label labelDescription;
        private CheckBox checkBoxStartWithWindows;
        private CheckBox checkBoxStartMinimized;
        private Label labelCurrentStatus;
        private Panel panelButtons;
        private Button buttonOK;
        private Button buttonCancel;
        private Button buttonApply;
    }
}