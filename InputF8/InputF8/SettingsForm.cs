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

namespace InputF8 {
	public partial class SettingsForm : Form {

		public event EventHandler OnSettingsChanged;
		Font _font;
		Color _foreColor;
		Color _backColor;

		public SettingsForm() {
			InitializeComponent();

			MinimizeToTrayCheckBox.Checked = Configuration.MinimizeToTray;
			ExitToTrayCheckBox.Checked = Configuration.ExitToTray;

			_font = new Font(Configuration.Name, Configuration.Size, (FontStyle)Configuration.Style);
			FontDialog.Font = _font;
			FontLabel.Text = String.Format("{0} {2}pt {1}", _font.Name, _font.Style, _font.Size);

			_foreColor = Color.FromArgb(Configuration.Color);
			ForeColorDialog.Color = _foreColor;
			ForeColorPreview.BackColor = _foreColor;
			_backColor = Color.FromArgb(Configuration.BackColor);
			BackColorDialog.Color = _backColor;
			BackColorPreview.BackColor = _backColor;
		}

		private void FontButton_Click(object sender, EventArgs e) {
			FontDialog.ShowDialog();
			_font = FontDialog.Font;
			Configuration.Name = _font.Name;
			Configuration.Style = (int)_font.Style;
			Configuration.Size = _font.Size;
			FontLabel.Text = String.Format("{0} {2}pt {1}", _font.Name, _font.Style, _font.Size);
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}

		private void ButtonForeColor_Click(object sender, EventArgs e) {
			ForeColorDialog.ShowDialog();
			_foreColor = ForeColorDialog.Color;
			Configuration.Color = _foreColor.ToArgb();
			ForeColorPreview.BackColor = _foreColor;
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}

		private void ButtonBackColor_Click(object sender, EventArgs e) {
			BackColorDialog.ShowDialog();
			_backColor = BackColorDialog.Color;
			Configuration.BackColor = _backColor.ToArgb();
			BackColorPreview.BackColor = _backColor;
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}

		private void OkButton_Click(object sender, EventArgs e) {
			Configuration.ApplySettings();
			OnSettingsChanged?.Invoke(this, new EventArgs());
			this.Close();
		}

		private void AbortButton_Click(object sender, EventArgs e) {
			Configuration.ReadSettings();
			OnSettingsChanged?.Invoke(this, new EventArgs());
			this.Close();
		}

		private void MinimizeToTrayCheckBox_CheckedChanged(object sender, EventArgs e) {
			Configuration.MinimizeToTray = MinimizeToTrayCheckBox.Checked;
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}

		private void ExitToTrayCheckBox_CheckedChanged(object sender, EventArgs e) {
			Configuration.ExitToTray = ExitToTrayCheckBox.Checked;
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}
	}
}
