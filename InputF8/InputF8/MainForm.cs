using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;

namespace InputF8 {
	public partial class MainForm : Form {
		Hooks _hooks;
		Input _input;

		public MainForm() {
			InitializeComponent();
			Application.ApplicationExit += new EventHandler(ExitProgram);

			Paths.SetDirectories();
			AddHooks();
		}

		#region startup
		
		/// <summary>
		/// Creates the hooks and adds the listeners
		/// </summary>
		void AddHooks() {
			_hooks = new Hooks();
			_input = new Input();

			_input.OnKeysChanged += ChangeText;

			_hooks.OnKeyDown += _input.OnKeyDown;
			_hooks.OnKeyUp += _input.OnKeyUp;
			_hooks.OnMouseDown += _input.OnMouseDown;
			_hooks.OnMouseUp += _input.OnMouseUp;
			_hooks.OnMouseScroll += _input.OnMouseScroll;
			_hooks.OnMouseMove += _input.OnMouseMove;

			_hooks.EnableHooks();
		}


		#endregion

		private void ChangeText(object sender, ChangeEventArgs e) {
			string text = string.Empty;
			foreach (string s in e.ActiveButtons) {
				text += (s + " ");
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
				ExitProgramPrep();
			}
			base.WndProc(ref m);
		}

		/// <summary>
		/// Prepares program for exit, saving stuff, closing hooks etc
		/// </summary>
		void ExitProgramPrep() {
			_hooks.DisableHooks();
			_input.SaveFiles();
		}

		void ExitProgram(object sender, EventArgs e) {
			_hooks.DisableHooks();
			_input.SaveFiles();
		}

		#endregion

		#region form interactions

		private void MainForm_Resize(object sender, EventArgs e) {
			if (WindowState == FormWindowState.Minimized) {
				NotifyIcon.Visible = true;
				this.Hide();
			}
			else {
				NotifyIcon.Visible = false;
			}
		}

		private void NotifyIcon_DoubleClick(object sender, EventArgs e) {
			this.Show();
			WindowState = FormWindowState.Normal;
		}

		#endregion

		private void ToolStripMenuItemSettings_Click(object sender, EventArgs e) {

		}
	}
}
