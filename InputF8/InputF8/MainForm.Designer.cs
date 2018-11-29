namespace InputF8 {
	partial class MainForm {
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.ContextMenuStripMain = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ToolStripMenuItemSettings = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemDisableDisplay = new System.Windows.Forms.ToolStripMenuItem();
			this.DisplayText = new System.Windows.Forms.Label();
			this.ContextMenuStripMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// NotifyIcon
			// 
			this.NotifyIcon.ContextMenuStrip = this.ContextMenuStripMain;
			this.NotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("NotifyIcon.Icon")));
			this.NotifyIcon.Text = "S Keys 9";
			this.NotifyIcon.DoubleClick += new System.EventHandler(this.NotifyIcon_DoubleClick);
			// 
			// ContextMenuStripMain
			// 
			this.ContextMenuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemSettings,
            this.ToolStripMenuItemAbout,
            this.ToolStripMenuItemDisableDisplay});
			this.ContextMenuStripMain.Name = "ContextMenuStripMain";
			this.ContextMenuStripMain.Size = new System.Drawing.Size(181, 92);
			// 
			// ToolStripMenuItemSettings
			// 
			this.ToolStripMenuItemSettings.Name = "ToolStripMenuItemSettings";
			this.ToolStripMenuItemSettings.Size = new System.Drawing.Size(180, 22);
			this.ToolStripMenuItemSettings.Text = "Settings";
			this.ToolStripMenuItemSettings.Click += new System.EventHandler(this.ToolStripMenuItemSettings_Click);
			// 
			// ToolStripMenuItemAbout
			// 
			this.ToolStripMenuItemAbout.Name = "ToolStripMenuItemAbout";
			this.ToolStripMenuItemAbout.Size = new System.Drawing.Size(180, 22);
			this.ToolStripMenuItemAbout.Text = "About";
			// 
			// ToolStripMenuItemDisableDisplay
			// 
			this.ToolStripMenuItemDisableDisplay.CheckOnClick = true;
			this.ToolStripMenuItemDisableDisplay.Name = "ToolStripMenuItemDisableDisplay";
			this.ToolStripMenuItemDisableDisplay.Size = new System.Drawing.Size(180, 22);
			this.ToolStripMenuItemDisableDisplay.Text = "Disable Display";
			this.ToolStripMenuItemDisableDisplay.CheckedChanged += new System.EventHandler(this.ToolStripMenuItemDisableDisplay_CheckedChanged);
			// 
			// DisplayText
			// 
			this.DisplayText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DisplayText.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DisplayText.ForeColor = System.Drawing.Color.White;
			this.DisplayText.Location = new System.Drawing.Point(0, 0);
			this.DisplayText.Margin = new System.Windows.Forms.Padding(0);
			this.DisplayText.Name = "DisplayText";
			this.DisplayText.Size = new System.Drawing.Size(767, 160);
			this.DisplayText.TabIndex = 1;
			this.DisplayText.Text = "{ S Keys 9 } \r\n{ Right click for options }";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(353, 83);
			this.ContextMenuStrip = this.ContextMenuStripMain;
			this.Controls.Add(this.DisplayText);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MainForm";
			this.Text = "S Keys 9";
			this.Resize += new System.EventHandler(this.MainForm_Resize);
			this.ContextMenuStripMain.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NotifyIcon NotifyIcon;
		private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemSettings;
		private System.Windows.Forms.ContextMenuStrip ContextMenuStripMain;
		private System.Windows.Forms.Label DisplayText;
		private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemAbout;
		private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemDisableDisplay;
	}
}

