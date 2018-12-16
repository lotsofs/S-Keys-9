using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;

namespace InputF8 {
	public partial class MainForm : Form {
		Hooks _hooks;
		Input _input;

		public MainForm() {
			InitializeComponent();
			Configuration.SetDirectories();
			Configuration.ReadSettings();
			UpdateAppearance();

			Application.ApplicationExit += new EventHandler(ExitProgram);
			AddHooks();
		}

		#region startup

		/// <summary>
		/// Creates the hooks and adds the listeners
		/// </summary>
		void AddHooks() {
			_hooks = new Hooks();
			_input = new Input();

			_input.OnKeysChanged += UpdateText;

			_hooks.OnKeyDown += _input.OnKeyDown;
			_hooks.OnKeyUp += _input.OnKeyUp;
			_hooks.OnMouseDown += _input.OnMouseDown;
			_hooks.OnMouseUp += _input.OnMouseUp;
			_hooks.OnMouseScroll += _input.OnMouseScroll;
			_hooks.OnMouseMove += _input.OnMouseMove;

			_hooks.EnableHooks();
		}

		#endregion

		/// <summary>
		/// updates appearance based on user's selected settigns
		/// </summary>
		void UpdateAppearance() {
			Font font = new Font(Configuration.Name, Configuration.Size, (FontStyle)Configuration.Style);
			DisplayText.Font = font;
			DisplayText.ForeColor = Color.FromArgb(Configuration.Color);
			this.BackColor = Color.FromArgb(Configuration.BackColor);
		}

		void UpdateAppearance(object sender, EventArgs e) {
			UpdateAppearance();
		}

		/// <summary>
		/// updates the text to display currently pressed keys
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UpdateText(object sender, ChangeEventArgs e) {
			if (ToolStripMenuItemDisableDisplay.Checked) {
				return;
			}

			string text = string.Empty;
			foreach (string s in e.ActiveButtons) {
				text += (s + " ");
			}
			text += "\n";
			foreach (string s in e.ScrollCount.Keys) {
				if (e.ScrollCount[s] != 0) {
					text += (s + " " + e.ScrollCount[s] + " ");
				}
			}
			DisplayText.Text = text;
		}

		#region exit procedure

		/// <summary>
		/// Because apparently adding a listener to applicationexit or processexit or whatever works only if the stars align
		/// </summary>
		/// <param name="m"></param>
		protected override void WndProc(ref Message m) {
			if (m.Msg == 0x11) {	// WM_QUERYENDSESSION
				ExitProgram();
			}
			base.WndProc(ref m);
		}

		/// <summary>
		/// Prepares program for exit, saving stuff, closing hooks etc
		/// </summary>
		void ExitProgram() {
			_hooks.DisableHooks();
			_input.SaveFiles();
		}

		void ExitProgram(object sender, EventArgs e) {
			ExitProgram();
		}

		#endregion

		#region form closing/minimizing/tray behavior

		void MoveToTray(bool tray) {
			NotifyIcon.Visible = tray;
			if (tray) {
				this.Hide();
				WindowState = FormWindowState.Minimized;
			}
			else {
				this.Show();
				WindowState = FormWindowState.Normal;
			}
		}

		/// <summary>
		/// Minimize
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainForm_Resize(object sender, EventArgs e) {
			if (WindowState == FormWindowState.Minimized && Configuration.MinimizeToTray) {
				MoveToTray(true);
			}
		}

		/// <summary>
		/// Double click tray icon
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NotifyIcon_DoubleClick(object sender, EventArgs e) {
			MoveToTray(false);
		}

		/// <summary>
		/// Close
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
			if (e.CloseReason == CloseReason.UserClosing && Configuration.ExitToTray) {
				e.Cancel = true;
				MoveToTray(true);
			}
		}

		#endregion

		#region toolstrip menu items

		private void ToolStripMenuItemSettings_Click(object sender, EventArgs e) {
			SettingsForm form = new SettingsForm();
			form.OnSettingsChanged += UpdateAppearance;
			form.Show();
			
			//ColorDialog b = new ColorDialog();
			//b.ShowDialog();
			//string a = b.Color.ToArgb().ToString("X");
			//Debug.WriteLine(a);
			//int c = int.Parse(a, System.Globalization.NumberStyles.HexNumber);
			//this.BackColor = System.Drawing.Color.FromArgb(Configuration.Color);
		}

		private void ToolStripMenuItemDisableDisplay_CheckedChanged(object sender, EventArgs e) {
			if (ToolStripMenuItemDisableDisplay.Checked) {
				DisplayText.Text = " { Display disabled } ";
			}
			else {
				DisplayText.Text = "";
			}
		}

		private void ToolStripItemExit_Click(object sender, EventArgs e) {
			Application.Exit();
		}

		private void ToolStripMenuItemTray_Click(object sender, EventArgs e) {
			MoveToTray(true);
		}

		#endregion
	}
}
