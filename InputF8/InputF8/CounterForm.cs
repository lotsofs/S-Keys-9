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

		public CounterForm(Input input) {
			InitializeComponent();
			input.OnKeysChanged += ChangeCount;
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
			StringBuilder sb = new StringBuilder();
			foreach (KeyValuePair<string, int> kvp in _inputsCount.OrderByDescending(pair => pair.Value)) {
				sb.Append(kvp.Key);
				sb.Append(": ");
				sb.Append(kvp.Value);
				sb.Append("\n");
			}
			Display.Text = sb.ToString();
		}

		private void ToolStripMenuItemReset_Click(object sender, EventArgs e) {
			_inputsCount.Clear();
			Display.Text = string.Empty;
		}
	}
}
