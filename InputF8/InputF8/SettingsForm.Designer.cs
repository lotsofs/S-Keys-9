namespace InputF8 {
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
			this.MinimizeToTrayCheckBox = new System.Windows.Forms.CheckBox();
			this.ExitToTrayCheckBox = new System.Windows.Forms.CheckBox();
			this.FontButton = new System.Windows.Forms.Button();
			this.FontDialog = new System.Windows.Forms.FontDialog();
			this.SuspendLayout();
			// 
			// MinimizeToTrayCheckBox
			// 
			this.MinimizeToTrayCheckBox.AutoSize = true;
			this.MinimizeToTrayCheckBox.Location = new System.Drawing.Point(12, 12);
			this.MinimizeToTrayCheckBox.Name = "MinimizeToTrayCheckBox";
			this.MinimizeToTrayCheckBox.Size = new System.Drawing.Size(106, 17);
			this.MinimizeToTrayCheckBox.TabIndex = 2;
			this.MinimizeToTrayCheckBox.Text = "Minimize To Tray";
			this.MinimizeToTrayCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExitToTrayCheckBox
			// 
			this.ExitToTrayCheckBox.AutoSize = true;
			this.ExitToTrayCheckBox.Location = new System.Drawing.Point(12, 35);
			this.ExitToTrayCheckBox.Name = "ExitToTrayCheckBox";
			this.ExitToTrayCheckBox.Size = new System.Drawing.Size(83, 17);
			this.ExitToTrayCheckBox.TabIndex = 3;
			this.ExitToTrayCheckBox.Text = "Exit To Tray";
			this.ExitToTrayCheckBox.UseVisualStyleBackColor = true;
			// 
			// FontButton
			// 
			this.FontButton.Location = new System.Drawing.Point(12, 59);
			this.FontButton.Name = "FontButton";
			this.FontButton.Size = new System.Drawing.Size(118, 23);
			this.FontButton.TabIndex = 4;
			this.FontButton.Text = "Change Font";
			this.FontButton.UseVisualStyleBackColor = true;
			this.FontButton.Click += new System.EventHandler(this.FontButton_Click);
			// 
			// FontDialog
			// 
			this.FontDialog.AllowScriptChange = false;
			// 
			// SettingsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.FontButton);
			this.Controls.Add(this.ExitToTrayCheckBox);
			this.Controls.Add(this.MinimizeToTrayCheckBox);
			this.Name = "SettingsForm";
			this.Text = "SettingsForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.CheckBox MinimizeToTrayCheckBox;
		private System.Windows.Forms.CheckBox ExitToTrayCheckBox;
		private System.Windows.Forms.Button FontButton;
		private System.Windows.Forms.FontDialog FontDialog;
	}
}