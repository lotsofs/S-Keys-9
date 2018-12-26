using System;
using System.Drawing;
using System.Windows.Forms;

namespace SKeys9 {
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
			ForeColorPreview.BackColor = _foreColor;
			_backColor = Color.FromArgb(Configuration.BackColor);
			BackColorPreview.BackColor = _backColor;
		}

		/// <summary>
		/// Opens a prompt to select a font and sets the font
		/// </summary>
		void SelectFont() {
			FontDialog.ShowDialog();
			_font = FontDialog.Font;
			Configuration.Name = _font.Name;
			Configuration.Style = (int)_font.Style;
			Configuration.Size = _font.Size;
			FontLabel.Text = String.Format("{0} {2}pt {1}", _font.Name, _font.Style, _font.Size);
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}

		/// <summary>
		/// Open a prompt to select a color
		/// </summary>
		/// <param name="color">Color the prompt shows by default</param>
		/// <returns></returns>
		Color SelectColor(Color color) {
			ColorDialog.Color = color;
			ColorDialog.ShowDialog();
			color = ColorDialog.Color;
			Configuration.Color = color.ToArgb();
			OnSettingsChanged?.Invoke(this, new EventArgs());
			return color;
		}

		#region events for buttons

		private void FontButton_Click(object sender, EventArgs e) {
			SelectFont();
		}

		private void ButtonForeColor_Click(object sender, EventArgs e) {
			_foreColor = SelectColor(_foreColor);
			ForeColorPreview.BackColor = _foreColor;
		}

		private void ButtonBackColor_Click(object sender, EventArgs e) {
			_backColor = SelectColor(_backColor);
			BackColorPreview.BackColor = _backColor;
		}

		private void MinimizeToTrayCheckBox_CheckedChanged(object sender, EventArgs e) {
			Configuration.MinimizeToTray = MinimizeToTrayCheckBox.Checked;
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}

		private void ExitToTrayCheckBox_CheckedChanged(object sender, EventArgs e) {
			Configuration.ExitToTray = ExitToTrayCheckBox.Checked;
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}

		#endregion

		#region confirmation buttons events

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

		#endregion

	}
}
