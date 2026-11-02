namespace SKeys9 {
	partial class CounterForm {
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
			this.Display = new System.Windows.Forms.Label();
			this.ContextMenuStripCounter = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ToolStripMenuItemReset = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockSize = new System.Windows.Forms.ToolStripMenuItem();
			this.ContextMenuStripCounter.SuspendLayout();
			this.SuspendLayout();
			// 
			// Display
			// 
			this.Display.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Display.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Display.ForeColor = System.Drawing.Color.White;
			this.Display.Location = new System.Drawing.Point(12, 9);
			this.Display.Name = "Display";
			this.Display.Size = new System.Drawing.Size(268, 989);
			this.Display.TabIndex = 0;
			this.Display.Paint += new System.Windows.Forms.PaintEventHandler(this.Display_Paint);
			//
			// ContextMenuStripCounter
			//
			this.ContextMenuStripCounter.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemReset,
            this.ToolStripMenuItemLockSize});
			this.ContextMenuStripCounter.Name = "ContextMenuStrip";
			this.ContextMenuStripCounter.Size = new System.Drawing.Size(124, 48);
			//
			// ToolStripMenuItemReset
			//
			this.ToolStripMenuItemReset.Name = "ToolStripMenuItemReset";
			this.ToolStripMenuItemReset.Size = new System.Drawing.Size(123, 22);
			this.ToolStripMenuItemReset.Text = "Reset";
			this.ToolStripMenuItemReset.Click += new System.EventHandler(this.ToolStripMenuItemReset_Click);
			//
			// ToolStripMenuItemLockSize
			//
			this.ToolStripMenuItemLockSize.CheckOnClick = true;
			this.ToolStripMenuItemLockSize.Name = "ToolStripMenuItemLockSize";
			this.ToolStripMenuItemLockSize.Size = new System.Drawing.Size(123, 22);
			this.ToolStripMenuItemLockSize.Text = "Lock Size";
			this.ToolStripMenuItemLockSize.Click += new System.EventHandler(this.ToolStripMenuItemLockSize_Click);
			//
			// CounterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(292, 1007);
			this.ContextMenuStrip = this.ContextMenuStripCounter;
			this.Controls.Add(this.Display);
			this.Name = "CounterForm";
			this.Text = "CounterForm";
			this.ResizeEnd += new System.EventHandler(this.CounterForm_ResizeEnd);
			this.ContextMenuStripCounter.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label Display;
		private System.Windows.Forms.ContextMenuStrip ContextMenuStripCounter;
		private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemReset;
		private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemLockSize;
	}
}