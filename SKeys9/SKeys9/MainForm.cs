using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SKeys9 {
	public partial class MainForm : Form {
		Hooks _hooks;
		Input _input;
		HeldKeyLayout _keyLayout = new HeldKeyLayout();
		ChangeEventArgs _lastChange;
		Timer _keyRemoveTimer;

		CounterForm _counterForm;
		SettingsForm _settingsForm;

		public MainForm() {
			InitializeComponent();
			Configuration.ReadSettings();
			UpdateAppearance();

			Keys.ReadJson();
			AddHooks();
			Application.ApplicationExit += new EventHandler(PrepareProgramForExit); // only once there's something to save
		}

		#region startup

		/// <summary>
		/// Creates the hooks and adds the listeners
		/// </summary>
		void AddHooks() {
			_input = new Input();
			_hooks = new Hooks(); // after the logs load, so a failed start leaves no hook check timer running behind its message

			_keyRemoveTimer = new Timer();
			_keyRemoveTimer.Tick += OnKeyRemoveTimerTick;

			_input.OnKeysChanged += UpdateText;

			_hooks.OnKeyDown += _input.OnButtonDown;
			_hooks.OnKeyUp += _input.OnButtonUp;
			_hooks.OnMouseDown += _input.OnMouseDown;
			_hooks.OnMouseUp += _input.OnMouseUp;
			_hooks.OnMouseScroll += _input.OnMouseScroll;
			_hooks.OnMouseMove += _input.OnMouseMove;
			_hooks.OnHooksReinstalled += _input.OnHooksReinstalled;

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
			_keyLayout.Clear(); // the font may have changed, and the keys were measured with the old one
			if (_lastChange != null) {
				UpdateText(this, _lastChange);
			}
		}

		/// <summary>
		/// updates the text to display currently pressed keys
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void UpdateText(object sender, ChangeEventArgs e) {
			_lastChange = e;
			if (ToolStripMenuItemDisableDisplay.Checked) {
				return;
			}

			_keyLayout.Update(e.ActiveButtons, DisplayText.Font, ClientSize.Width);

			string text = new string('\n', _keyLayout.Rows); // DisplayText_Paint draws the held keys on these lines
			foreach (string s in e.ScrollCount.Keys) {
				if (e.ScrollCount[s] != 0) {
					text += (s + " " + e.ScrollCount[s] + " ");
				}
			}
			DisplayText.Text = text;
			DisplayText.Invalidate();
			DisplayText.Update(); // a title bar press holds up the message loop but not the hooks, so this can't wait for it
			ScheduleKeyRemoval();
		}

		/// <summary>
		/// Sets the timer for the next released key to go. It doesn't run while no key is waiting, so an idle program stays idle
		/// </summary>
		void ScheduleKeyRemoval() {
			_keyRemoveTimer.Stop();
			int? timeLeft = _keyLayout.NextRemoval();
			if (timeLeft != null) {
				_keyRemoveTimer.Interval = System.Math.Max(1, timeLeft.Value);
				_keyRemoveTimer.Start();
			}
		}

		/// <summary>
		/// A released key's time is up
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnKeyRemoveTimerTick(object sender, EventArgs e) {
			_keyRemoveTimer.Stop();
			UpdateText(this, _lastChange);
		}

		/// <summary>
		/// Draws the keys, each at its own spot, over the label's text
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void DisplayText_Paint(object sender, PaintEventArgs e) {
			Color released = Configuration.ReleasedKeyColor();
			foreach (PlacedKey key in _keyLayout.Placed) {
				Point location = new Point(key.X, key.Row * DisplayText.Font.Height);
				Color color = key.ReleasedAt == null ? DisplayText.ForeColor : released;
				TextRenderer.DrawText(e.Graphics, key.Name, DisplayText.Font, location, color, TextFormatFlags.NoPrefix);
			}
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
			_input.FinishQueuedMouseMovements();
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
			if (_settingsForm != null && _settingsForm.IsHandleCreated) return;
			_settingsForm = new SettingsForm();
			_settingsForm.OnSettingsChanged += UpdateAppearance;
			if (_counterForm != null && _counterForm.IsHandleCreated) {
				_settingsForm.OnSettingsChanged += _counterForm.UpdateAppearance;
			}
			_settingsForm.Show();
		}

		private void ToolStripMenuItemAbout_Click(object sender, EventArgs e) {
			MessageBox.Show(String.Format(
				"S Keys\nV{0}\n\nMade and maintained by:\nLotsOfS\ngithub.com/lotsofs\n\nSpecial thanks to:\nFatalis",
				Application.ProductVersion
			));
		}

		//-------

		private void ToolStripMenuItemDisableDisplay_CheckedChanged(object sender, EventArgs e) {
			_keyLayout.Clear();
			if (ToolStripMenuItemDisableDisplay.Checked) {
				DisplayText.Text = " { Display disabled } ";
			}
			else {
				DisplayText.Text = "";
			}
		}

		private void ToolStripMenuItemCounterDisplay_Click(object sender, EventArgs e) {
			if (_counterForm != null && _counterForm.IsHandleCreated ) return;
			_counterForm = new CounterForm(_input);
			if (_settingsForm != null && _settingsForm.IsHandleCreated) {
				_settingsForm.OnSettingsChanged += _counterForm.UpdateAppearance;
			}
			_counterForm.Show();
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
