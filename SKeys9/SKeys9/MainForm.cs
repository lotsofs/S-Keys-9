using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SKeys9 {
	public partial class MainForm : Form {
		Hooks _hooks;
		Input _input;

		public MainForm() {
			InitializeComponent();
			Configuration.SetDirectories();
			Configuration.ReadSettings();
			UpdateAppearance();

			Application.ApplicationExit += new EventHandler(PrepareProgramForExit);
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

			_hooks.OnKeyDown += _input.OnButtonDown;
			_hooks.OnKeyUp += _input.OnButtonUp;
			_hooks.OnMouseDown += _input.OnMouseDown;
			_hooks.OnMouseUp += _input.OnMouseUp;
			_hooks.OnMouseScroll += _input.OnMouseScroll;
			_hooks.OnMouseMove += _input.OnMouseMove;

			_hooks.EnableHooks();
		}

		#endregion

		#region display

		/// <summary>
		/// updates appearance based on user's selected settings
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void UpdateAppearance(object sender = null, EventArgs e = null) {
			Font font = new Font(Configuration.Name, Configuration.Size, (FontStyle)Configuration.Style);
			DisplayText.Font = font;
			DisplayText.ForeColor = Color.FromArgb(Configuration.Color);
			this.BackColor = Color.FromArgb(Configuration.BackColor);
		}

		/// <summary>
		/// updates the text to display currently pressed keys
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void UpdateText(object sender, ChangeEventArgs e) {
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

		#endregion

		#region exit procedure

		/// <summary>
		/// Because apparently adding a listener to applicationexit or processexit or whatever works only if the stars align
		/// </summary>
		/// <param name="m"></param>
		protected override void WndProc(ref Message m) {
			if (m.Msg == 0x11) {    // WM_QUERYENDSESSION
				PrepareProgramForExit();
			}
			base.WndProc(ref m);
		}

		/// <summary>
		/// Prepares program for exit, saving stuff, closing hooks etc
		/// </summary>
		void PrepareProgramForExit(object sender = null, EventArgs e = null) {
			_hooks.DisableHooks();
			_input.SaveFiles();
		}

		#endregion

		#region form closing/minimizing/tray behavior

		/// <summary>
		/// Moves the tool to the tray instead of the taskbar
		/// </summary>
		/// <param name="tray">Whether to move or unmove</param>
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
		void MainForm_Resize(object sender, EventArgs e) {
			if (WindowState == FormWindowState.Minimized && Configuration.MinimizeToTray) {
				MoveToTray(true);
			}
		}

		/// <summary>
		/// Double click tray icon
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void NotifyIcon_DoubleClick(object sender, EventArgs e) {
			MoveToTray(false);
		}

		/// <summary>
		/// Close
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
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
		}

		private void ToolStripMenuItemAbout_Click(object sender, EventArgs e) {
			MessageBox.Show(String.Format(
				"S Keys\nV{0}\n\nMade by:\nLotsOfS\ngithub.com/lotsofs\n\nSpecial thanks to:\nFatalis",
				Application.ProductVersion
			));
		}

		//-------

		private void ToolStripMenuItemDisableDisplay_CheckedChanged(object sender, EventArgs e) {
			if (ToolStripMenuItemDisableDisplay.Checked) {
				DisplayText.Text = " { Display disabled } ";
			}
			else {
				DisplayText.Text = "";
			}
		}

		private void ToolStripMenuItemCounterDisplay_Click(object sender, EventArgs e) {
			CounterForm form = new CounterForm(_input);
			form.Show();
		}

		//-------

		private void ToolStripMenuItemTray_Click(object sender, EventArgs e) {
			MoveToTray(true);
		}

		private void ToolStripItemExit_Click(object sender, EventArgs e) {
			Application.Exit();
		}
		
		#endregion

	}
}
