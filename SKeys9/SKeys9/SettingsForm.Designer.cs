namespace SKeys9 {
	partial class SettingsForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
			this.MinimizeToTrayCheckBox = new System.Windows.Forms.CheckBox();
			this.ExitToTrayCheckBox = new System.Windows.Forms.CheckBox();
			this.FontButton = new System.Windows.Forms.Button();
			this.FontDialog = new System.Windows.Forms.FontDialog();
			this.OkButton = new System.Windows.Forms.Button();
			this.ÁbortButton = new System.Windows.Forms.Button();
			this.FontLabel = new System.Windows.Forms.Label();
			this.GroupBoxFont = new System.Windows.Forms.GroupBox();
			this.GroupBoxWindow = new System.Windows.Forms.GroupBox();
			this.GroupBoxColor = new System.Windows.Forms.GroupBox();
			this.BackColorPreview = new System.Windows.Forms.Panel();
			this.ForeColorPreview = new System.Windows.Forms.Panel();
			this.ButtonBackColor = new System.Windows.Forms.Button();
			this.ButtonForeColor = new System.Windows.Forms.Button();
			this.ColorDialog = new System.Windows.Forms.ColorDialog();
			this.BackColorDialog = new System.Windows.Forms.ColorDialog();
			this.GroupBoxFeatures = new System.Windows.Forms.GroupBox();
			this.LogMovementCheckbox = new System.Windows.Forms.CheckBox();
			this.LogClicksCheckbox = new System.Windows.Forms.CheckBox();
			this.LogButtonsCheckbox = new System.Windows.Forms.CheckBox();
			this.groupBoxCounter = new System.Windows.Forms.GroupBox();
			this.buttonCounterFont = new System.Windows.Forms.Button();
			this.labelCounterFont = new System.Windows.Forms.Label();
			this.panelCounterBGCol = new System.Windows.Forms.Panel();
			this.panelCounterTextCol = new System.Windows.Forms.Panel();
			this.buttonCounterBGCol = new System.Windows.Forms.Button();
			this.buttonCounterTextCol = new System.Windows.Forms.Button();
			this.GroupBoxFont.SuspendLayout();
			this.GroupBoxWindow.SuspendLayout();
			this.GroupBoxColor.SuspendLayout();
			this.GroupBoxFeatures.SuspendLayout();
			this.groupBoxCounter.SuspendLayout();
			this.SuspendLayout();
			// 
			// MinimizeToTrayCheckBox
			// 
			this.MinimizeToTrayCheckBox.AutoSize = true;
			this.MinimizeToTrayCheckBox.Checked = true;
			this.MinimizeToTrayCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.MinimizeToTrayCheckBox.Location = new System.Drawing.Point(6, 19);
			this.MinimizeToTrayCheckBox.Name = "MinimizeToTrayCheckBox";
			this.MinimizeToTrayCheckBox.Size = new System.Drawing.Size(106, 17);
			this.MinimizeToTrayCheckBox.TabIndex = 2;
			this.MinimizeToTrayCheckBox.Text = "Minimize To Tray";
			this.MinimizeToTrayCheckBox.UseVisualStyleBackColor = true;
			this.MinimizeToTrayCheckBox.CheckedChanged += new System.EventHandler(this.MinimizeToTrayCheckBox_CheckedChanged);
			// 
			// ExitToTrayCheckBox
			// 
			this.ExitToTrayCheckBox.AutoSize = true;
			this.ExitToTrayCheckBox.Location = new System.Drawing.Point(6, 42);
			this.ExitToTrayCheckBox.Name = "ExitToTrayCheckBox";
			this.ExitToTrayCheckBox.Size = new System.Drawing.Size(83, 17);
			this.ExitToTrayCheckBox.TabIndex = 3;
			this.ExitToTrayCheckBox.Text = "Exit To Tray";
			this.ExitToTrayCheckBox.UseVisualStyleBackColor = true;
			this.ExitToTrayCheckBox.CheckedChanged += new System.EventHandler(this.ExitToTrayCheckBox_CheckedChanged);
			// 
			// FontButton
			// 
			this.FontButton.Location = new System.Drawing.Point(6, 19);
			this.FontButton.Name = "FontButton";
			this.FontButton.Size = new System.Drawing.Size(188, 23);
			this.FontButton.TabIndex = 4;
			this.FontButton.Text = "Change Font";
			this.FontButton.UseVisualStyleBackColor = true;
			this.FontButton.Click += new System.EventHandler(this.FontButton_Click);
			// 
			// FontDialog
			// 
			this.FontDialog.AllowScriptChange = false;
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.Location = new System.Drawing.Point(424, 12);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = new System.Drawing.Size(75, 23);
			this.OkButton.TabIndex = 5;
			this.OkButton.Text = "OK";
			this.OkButton.UseVisualStyleBackColor = true;
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// ÁbortButton
			// 
			this.ÁbortButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ÁbortButton.Location = new System.Drawing.Point(424, 41);
			this.ÁbortButton.Name = "ÁbortButton";
			this.ÁbortButton.Size = new System.Drawing.Size(75, 23);
			this.ÁbortButton.TabIndex = 6;
			this.ÁbortButton.Text = "Cancel";
			this.ÁbortButton.UseVisualStyleBackColor = true;
			this.ÁbortButton.Click += new System.EventHandler(this.AbortButton_Click);
			// 
			// FontLabel
			// 
			this.FontLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.FontLabel.Location = new System.Drawing.Point(6, 45);
			this.FontLabel.Name = "FontLabel";
			this.FontLabel.Size = new System.Drawing.Size(188, 58);
			this.FontLabel.TabIndex = 7;
			this.FontLabel.Text = "Font Name xxpt Style";
			// 
			// GroupBoxFont
			// 
			this.GroupBoxFont.Controls.Add(this.FontButton);
			this.GroupBoxFont.Controls.Add(this.FontLabel);
			this.GroupBoxFont.Location = new System.Drawing.Point(12, 84);
			this.GroupBoxFont.Name = "GroupBoxFont";
			this.GroupBoxFont.Size = new System.Drawing.Size(200, 106);
			this.GroupBoxFont.TabIndex = 8;
			this.GroupBoxFont.TabStop = false;
			this.GroupBoxFont.Text = "Font";
			// 
			// GroupBoxWindow
			// 
			this.GroupBoxWindow.Controls.Add(this.MinimizeToTrayCheckBox);
			this.GroupBoxWindow.Controls.Add(this.ExitToTrayCheckBox);
			this.GroupBoxWindow.Location = new System.Drawing.Point(12, 12);
			this.GroupBoxWindow.Name = "GroupBoxWindow";
			this.GroupBoxWindow.Size = new System.Drawing.Size(200, 66);
			this.GroupBoxWindow.TabIndex = 9;
			this.GroupBoxWindow.TabStop = false;
			this.GroupBoxWindow.Text = "Window";
			// 
			// GroupBoxColor
			// 
			this.GroupBoxColor.Controls.Add(this.BackColorPreview);
			this.GroupBoxColor.Controls.Add(this.ForeColorPreview);
			this.GroupBoxColor.Controls.Add(this.ButtonBackColor);
			this.GroupBoxColor.Controls.Add(this.ButtonForeColor);
			this.GroupBoxColor.Location = new System.Drawing.Point(218, 12);
			this.GroupBoxColor.Name = "GroupBoxColor";
			this.GroupBoxColor.Size = new System.Drawing.Size(200, 77);
			this.GroupBoxColor.TabIndex = 10;
			this.GroupBoxColor.TabStop = false;
			this.GroupBoxColor.Text = "Color";
			// 
			// BackColorPreview
			// 
			this.BackColorPreview.BackColor = System.Drawing.Color.Black;
			this.BackColorPreview.Location = new System.Drawing.Point(113, 48);
			this.BackColorPreview.Name = "BackColorPreview";
			this.BackColorPreview.Size = new System.Drawing.Size(81, 23);
			this.BackColorPreview.TabIndex = 3;
			// 
			// ForeColorPreview
			// 
			this.ForeColorPreview.BackColor = System.Drawing.Color.White;
			this.ForeColorPreview.Location = new System.Drawing.Point(113, 19);
			this.ForeColorPreview.Name = "ForeColorPreview";
			this.ForeColorPreview.Size = new System.Drawing.Size(81, 23);
			this.ForeColorPreview.TabIndex = 2;
			// 
			// ButtonBackColor
			// 
			this.ButtonBackColor.Location = new System.Drawing.Point(7, 48);
			this.ButtonBackColor.Name = "ButtonBackColor";
			this.ButtonBackColor.Size = new System.Drawing.Size(100, 23);
			this.ButtonBackColor.TabIndex = 1;
			this.ButtonBackColor.Text = "Background Color";
			this.ButtonBackColor.UseVisualStyleBackColor = true;
			this.ButtonBackColor.Click += new System.EventHandler(this.ButtonBackColor_Click);
			// 
			// ButtonForeColor
			// 
			this.ButtonForeColor.Location = new System.Drawing.Point(7, 19);
			this.ButtonForeColor.Name = "ButtonForeColor";
			this.ButtonForeColor.Size = new System.Drawing.Size(100, 23);
			this.ButtonForeColor.TabIndex = 0;
			this.ButtonForeColor.Text = "Text Color";
			this.ButtonForeColor.UseVisualStyleBackColor = true;
			this.ButtonForeColor.Click += new System.EventHandler(this.ButtonForeColor_Click);
			// 
			// GroupBoxFeatures
			// 
			this.GroupBoxFeatures.Controls.Add(this.LogMovementCheckbox);
			this.GroupBoxFeatures.Controls.Add(this.LogClicksCheckbox);
			this.GroupBoxFeatures.Controls.Add(this.LogButtonsCheckbox);
			this.GroupBoxFeatures.Location = new System.Drawing.Point(218, 96);
			this.GroupBoxFeatures.Name = "GroupBoxFeatures";
			this.GroupBoxFeatures.Size = new System.Drawing.Size(200, 94);
			this.GroupBoxFeatures.TabIndex = 11;
			this.GroupBoxFeatures.TabStop = false;
			this.GroupBoxFeatures.Text = "Logging";
			// 
			// LogMovementCheckbox
			// 
			this.LogMovementCheckbox.AutoSize = true;
			this.LogMovementCheckbox.Location = new System.Drawing.Point(6, 65);
			this.LogMovementCheckbox.Name = "LogMovementCheckbox";
			this.LogMovementCheckbox.Size = new System.Drawing.Size(111, 17);
			this.LogMovementCheckbox.TabIndex = 6;
			this.LogMovementCheckbox.Text = "Mouse Movement";
			this.LogMovementCheckbox.UseVisualStyleBackColor = true;
			this.LogMovementCheckbox.CheckedChanged += new System.EventHandler(this.LogMovementCheckbox_CheckedChanged);
			// 
			// LogClicksCheckbox
			// 
			this.LogClicksCheckbox.AutoSize = true;
			this.LogClicksCheckbox.Location = new System.Drawing.Point(6, 42);
			this.LogClicksCheckbox.Name = "LogClicksCheckbox";
			this.LogClicksCheckbox.Size = new System.Drawing.Size(143, 17);
			this.LogClicksCheckbox.TabIndex = 5;
			this.LogClicksCheckbox.Text = "Mouse Click Coordinates";
			this.LogClicksCheckbox.UseVisualStyleBackColor = true;
			this.LogClicksCheckbox.CheckedChanged += new System.EventHandler(this.LogClicksCheckbox_CheckedChanged);
			// 
			// LogButtonsCheckbox
			// 
			this.LogButtonsCheckbox.AutoSize = true;
			this.LogButtonsCheckbox.Location = new System.Drawing.Point(6, 19);
			this.LogButtonsCheckbox.Name = "LogButtonsCheckbox";
			this.LogButtonsCheckbox.Size = new System.Drawing.Size(97, 17);
			this.LogButtonsCheckbox.TabIndex = 4;
			this.LogButtonsCheckbox.Text = "Button Presses";
			this.LogButtonsCheckbox.UseVisualStyleBackColor = true;
			this.LogButtonsCheckbox.CheckedChanged += new System.EventHandler(this.LogButtonsCheckbox_CheckedChanged);
			// 
			// groupBoxCounter
			// 
			this.groupBoxCounter.Controls.Add(this.panelCounterBGCol);
			this.groupBoxCounter.Controls.Add(this.labelCounterFont);
			this.groupBoxCounter.Controls.Add(this.panelCounterTextCol);
			this.groupBoxCounter.Controls.Add(this.buttonCounterFont);
			this.groupBoxCounter.Controls.Add(this.buttonCounterBGCol);
			this.groupBoxCounter.Controls.Add(this.buttonCounterTextCol);
			this.groupBoxCounter.Location = new System.Drawing.Point(12, 196);
			this.groupBoxCounter.Name = "groupBoxCounter";
			this.groupBoxCounter.Size = new System.Drawing.Size(406, 78);
			this.groupBoxCounter.TabIndex = 12;
			this.groupBoxCounter.TabStop = false;
			this.groupBoxCounter.Text = "Counter Display";
			// 
			// buttonCounterFont
			// 
			this.buttonCounterFont.Location = new System.Drawing.Point(6, 19);
			this.buttonCounterFont.Name = "buttonCounterFont";
			this.buttonCounterFont.Size = new System.Drawing.Size(188, 23);
			this.buttonCounterFont.TabIndex = 8;
			this.buttonCounterFont.Text = "Change Font";
			this.buttonCounterFont.UseVisualStyleBackColor = true;
			this.buttonCounterFont.Click += new System.EventHandler(this.buttonCounterFont_Click);
			// 
			// labelCounterFont
			// 
			this.labelCounterFont.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.labelCounterFont.Location = new System.Drawing.Point(6, 45);
			this.labelCounterFont.Name = "labelCounterFont";
			this.labelCounterFont.Size = new System.Drawing.Size(188, 26);
			this.labelCounterFont.TabIndex = 8;
			this.labelCounterFont.Text = "Font Name xxpt Style";
			// 
			// panelCounterBGCol
			// 
			this.panelCounterBGCol.BackColor = System.Drawing.Color.Black;
			this.panelCounterBGCol.Location = new System.Drawing.Point(319, 48);
			this.panelCounterBGCol.Name = "panelCounterBGCol";
			this.panelCounterBGCol.Size = new System.Drawing.Size(81, 23);
			this.panelCounterBGCol.TabIndex = 7;
			// 
			// panelCounterTextCol
			// 
			this.panelCounterTextCol.BackColor = System.Drawing.Color.White;
			this.panelCounterTextCol.Location = new System.Drawing.Point(319, 19);
			this.panelCounterTextCol.Name = "panelCounterTextCol";
			this.panelCounterTextCol.Size = new System.Drawing.Size(81, 23);
			this.panelCounterTextCol.TabIndex = 6;
			// 
			// buttonCounterBGCol
			// 
			this.buttonCounterBGCol.Location = new System.Drawing.Point(213, 48);
			this.buttonCounterBGCol.Name = "buttonCounterBGCol";
			this.buttonCounterBGCol.Size = new System.Drawing.Size(100, 23);
			this.buttonCounterBGCol.TabIndex = 5;
			this.buttonCounterBGCol.Text = "Background Color";
			this.buttonCounterBGCol.UseVisualStyleBackColor = true;
			this.buttonCounterBGCol.Click += new System.EventHandler(this.buttonCounterBGCol_Click);
			// 
			// buttonCounterTextCol
			// 
			this.buttonCounterTextCol.Location = new System.Drawing.Point(213, 19);
			this.buttonCounterTextCol.Name = "buttonCounterTextCol";
			this.buttonCounterTextCol.Size = new System.Drawing.Size(100, 23);
			this.buttonCounterTextCol.TabIndex = 4;
			this.buttonCounterTextCol.Text = "Text Color";
			this.buttonCounterTextCol.UseVisualStyleBackColor = true;
			this.buttonCounterTextCol.Click += new System.EventHandler(this.buttonCounterTextCol_Click);
			// 
			// SettingsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(511, 285);
			this.Controls.Add(this.groupBoxCounter);
			this.Controls.Add(this.GroupBoxFeatures);
			this.Controls.Add(this.GroupBoxColor);
			this.Controls.Add(this.GroupBoxWindow);
			this.Controls.Add(this.GroupBoxFont);
			this.Controls.Add(this.ÁbortButton);
			this.Controls.Add(this.OkButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "SettingsForm";
			this.Text = "Settings";
			this.GroupBoxFont.ResumeLayout(false);
			this.GroupBoxWindow.ResumeLayout(false);
			this.GroupBoxWindow.PerformLayout();
			this.GroupBoxColor.ResumeLayout(false);
			this.GroupBoxFeatures.ResumeLayout(false);
			this.GroupBoxFeatures.PerformLayout();
			this.groupBoxCounter.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.CheckBox MinimizeToTrayCheckBox;
		private System.Windows.Forms.CheckBox ExitToTrayCheckBox;
		private System.Windows.Forms.Button FontButton;
		private System.Windows.Forms.FontDialog FontDialog;
		private System.Windows.Forms.Button OkButton;
		private System.Windows.Forms.Button ÁbortButton;
		private System.Windows.Forms.Label FontLabel;
		private System.Windows.Forms.GroupBox GroupBoxFont;
		private System.Windows.Forms.GroupBox GroupBoxWindow;
		private System.Windows.Forms.GroupBox GroupBoxColor;
		private System.Windows.Forms.Panel BackColorPreview;
		private System.Windows.Forms.Panel ForeColorPreview;
		private System.Windows.Forms.Button ButtonBackColor;
		private System.Windows.Forms.Button ButtonForeColor;
		private System.Windows.Forms.ColorDialog ColorDialog;
		private System.Windows.Forms.ColorDialog BackColorDialog;
		private System.Windows.Forms.GroupBox GroupBoxFeatures;
		private System.Windows.Forms.CheckBox LogMovementCheckbox;
		private System.Windows.Forms.CheckBox LogClicksCheckbox;
		private System.Windows.Forms.CheckBox LogButtonsCheckbox;
		private System.Windows.Forms.GroupBox groupBoxCounter;
		private System.Windows.Forms.Panel panelCounterBGCol;
		private System.Windows.Forms.Label labelCounterFont;
		private System.Windows.Forms.Panel panelCounterTextCol;
		private System.Windows.Forms.Button buttonCounterFont;
		private System.Windows.Forms.Button buttonCounterBGCol;
		private System.Windows.Forms.Button buttonCounterTextCol;
	}
}