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
			this.FilePathTextBox = new System.Windows.Forms.TextBox();
			this.FilePathBrowseButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// FilePathTextBox
			// 
			this.FilePathTextBox.Location = new System.Drawing.Point(36, 41);
			this.FilePathTextBox.Name = "FilePathTextBox";
			this.FilePathTextBox.Size = new System.Drawing.Size(360, 20);
			this.FilePathTextBox.TabIndex = 0;
			// 
			// FilePathBrowseButton
			// 
			this.FilePathBrowseButton.Location = new System.Drawing.Point(402, 39);
			this.FilePathBrowseButton.Name = "FilePathBrowseButton";
			this.FilePathBrowseButton.Size = new System.Drawing.Size(75, 23);
			this.FilePathBrowseButton.TabIndex = 1;
			this.FilePathBrowseButton.Text = "Browse";
			this.FilePathBrowseButton.UseVisualStyleBackColor = true;
			// 
			// SettingsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.FilePathBrowseButton);
			this.Controls.Add(this.FilePathTextBox);
			this.Name = "SettingsForm";
			this.Text = "SettingsForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox FilePathTextBox;
		private System.Windows.Forms.Button FilePathBrowseButton;
	}
}