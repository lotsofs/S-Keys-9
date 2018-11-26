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

			Application.ApplicationExit += ExitProgram;

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

			_hooks.OnKeyDown += _input.OnKeyDown;
			_hooks.OnKeyUp += _input.OnKeyUp;
			_hooks.OnMouseDown += _input.OnMouseDown;
			_hooks.OnMouseUp += _input.OnMouseUp;
			_hooks.OnMouseScroll += _input.OnMouseScroll;
			_hooks.OnMouseMove += _input.OnMouseMove;

			_hooks.EnableHooks();
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

		void ExitProgram(object sender, EventArgs e) {
			_hooks.DisableHooks();
			_input.SaveFiles();
		}
	}
}
