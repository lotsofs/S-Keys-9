using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SKeys9 {
	public partial class CounterForm : Form {
		Dictionary<string, int> _inputsCount = new Dictionary<string, int>();
		List<string> _previousHeld = new List<string>();
		Dictionary<string, int> _previousScroll = new Dictionary<string, int>();
		List<string> _entries = new List<string>();
		int _lineHeight;

		public CounterForm(Input input) {
			InitializeComponent();
			input.OnKeysChanged += ChangeCount;
			ApplySizeLock();
			UpdateAppearance();
		}

		/// <summary>
		/// updates appearance based on user's selected settings
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void UpdateAppearance(object sender = null, EventArgs e = null) {
			Font font = new Font(Configuration.CounterName, Configuration.CounterSize, (FontStyle)Configuration.CounterStyle);
			Display.Font = font;
			_lineHeight = TextRenderer.MeasureText("A", font).Height; // taller than font.Height, which would clip the entries
			Display.ForeColor = Color.FromArgb(Configuration.CounterColor);
			this.BackColor = Color.FromArgb(Configuration.CounterBackColor);
			if (WindowState == FormWindowState.Normal) {
				ClientSize = new Size(Configuration.CounterWindowWidth, Configuration.CounterWindowHeight);
			}
			Display.Invalidate();
		}

		/// <summary>
		/// Locks or unlocks the window's size. Windows won't let this border be resized,
		/// but it's a different thickness, so this keeps the inside of the window the same size
		/// </summary>
		void ApplySizeLock() {
			Size inside = ClientSize;
			ToolStripMenuItemLockSize.Checked = Configuration.CounterSizeLocked;
			MaximizeBox = !Configuration.CounterSizeLocked;
			FormBorderStyle = Configuration.CounterSizeLocked ? FormBorderStyle.FixedSingle : FormBorderStyle.Sizable;
			ClientSize = inside;
		}

		/// <summary>
		/// Count input, then display this in a box
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void ChangeCount(object sender, ChangeEventArgs e) {
			CountButtons(e.ActiveButtons);
			CountScrolls(e.ScrollCount);
			UpdateText();
		}

		/// <summary>
		/// count buttons
		/// </summary>
		/// <param name="buttons"></param>
		private void CountButtons(List<string> buttons) {
			foreach (string ab in buttons) {
				if (!_previousHeld.Contains(ab)) {
					S.Dictionaries.IncrementValue(_inputsCount, ab, 1);
				}
			}
			_previousHeld.Clear();
			_previousHeld.AddRange(buttons);
		}

		/// <summary>
		/// Count scrolls
		/// </summary>
		/// <param name="scrolls"></param>
		private void CountScrolls(Dictionary<string, int> scrolls) {
			foreach (KeyValuePair<string, int> kvp in scrolls) {
				if (!_previousScroll.ContainsKey(kvp.Key)) {
					S.Dictionaries.IncrementValue(_previousScroll, kvp.Key, 0);
				}
				int diff = kvp.Value - _previousScroll[kvp.Key];
				if (diff == 0) {
					continue;
				}
				if (diff > 0) {
					S.Dictionaries.IncrementValue(_inputsCount, kvp.Key, diff);
				}
				_previousScroll[kvp.Key] = kvp.Value;
			}
		}

		/// <summary>
		/// Update text in the window
		/// </summary>
		private void UpdateText() {
			_entries = _inputsCount.OrderByDescending(pair => pair.Value).Select(pair => pair.Key + ": " + pair.Value).ToList();
			Display.Invalidate();
		}

		/// <summary>
		/// Draws the list in columns of equal width. Each entry is drawn by itself, because text of several
		/// lines only gets an ellipsis on its last one. A list that's too long for the window fills it in
		/// order, so only the end of the list is left out, and the last spot says how many entries that is
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Display_Paint(object sender, PaintEventArgs e) {
			int columns = Configuration.CounterColumns;
			int rowsThatFit = Math.Max(Display.ClientSize.Height / _lineHeight, 1);
			int rows = Math.Min((_entries.Count + columns - 1) / columns, rowsThatFit);
			int width = Display.ClientSize.Width / columns;
			int shown = Math.Min(_entries.Count, rows * columns);
			for (int i = 0; i < shown; i++) {
				int column = Configuration.CounterFillAcross ? i % columns : i / rows;
				int row = Configuration.CounterFillAcross ? i / columns : i % rows;
				Rectangle cell = new Rectangle(column * width, row * _lineHeight, width, _lineHeight);
				string text = i == shown - 1 && shown < _entries.Count ? "... (+" + (_entries.Count - i) + ")" : _entries[i];
				TextRenderer.DrawText(e.Graphics, text, Display.Font, cell, Display.ForeColor, TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
			}
		}

		/// <summary>
		/// Saves the size the window was just given. This also runs after a plain move
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void CounterForm_ResizeEnd(object sender, EventArgs e) {
			bool sameSize = ClientSize.Width == Configuration.CounterWindowWidth && ClientSize.Height == Configuration.CounterWindowHeight;
			if (WindowState != FormWindowState.Normal || sameSize) {
				return;
			}
			Configuration.CounterWindowWidth = ClientSize.Width;
			Configuration.CounterWindowHeight = ClientSize.Height;
			Configuration.SaveCounterWindow();
		}

		private void ToolStripMenuItemReset_Click(object sender, EventArgs e) {
			_inputsCount.Clear();
			UpdateText();
		}

		private void ToolStripMenuItemLockSize_Click(object sender, EventArgs e) {
			WindowState = FormWindowState.Normal; // a locked window has no button to come back from being maximised
			Configuration.CounterSizeLocked = ToolStripMenuItemLockSize.Checked;
			ApplySizeLock();
			Configuration.SaveCounterWindow();
		}
	}
}
