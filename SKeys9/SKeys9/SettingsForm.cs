using System;
using System.Drawing;
using System.Windows.Forms;

namespace SKeys9 {
	public partial class SettingsForm : Form {
		public event EventHandler OnSettingsChanged;
		Font _font;
		Font _counterFont;
		Color _foreColor;
		Color _backColor;
		Color _counterForeColor;
		Color _counterBackColor;

		public SettingsForm() {
			InitializeComponent();

			MinimizeToTrayCheckBox.Checked = Configuration.MinimizeToTray;
			ExitToTrayCheckBox.Checked = Configuration.ExitToTray;

			LogButtonsCheckbox.Checked = Configuration.LogButtons;
			LogClicksCheckbox.Checked = Configuration.LogClicks;
			LogMovementCheckbox.Checked = Configuration.LogMovement;

			_font = new Font(Configuration.Name, Configuration.Size, (FontStyle)Configuration.Style);
			FontDialog.Font = _font;
			FontLabel.Text = String.Format("{0} {2}pt {1}", _font.Name, _font.Style, _font.Size);

			_foreColor = Color.FromArgb(Configuration.Color);
			ForeColorPreview.BackColor = _foreColor;
			_backColor = Color.FromArgb(Configuration.BackColor);
			BackColorPreview.BackColor = _backColor;

			_counterFont = new Font(Configuration.CounterName, Configuration.CounterSize, (FontStyle)Configuration.CounterStyle);
			labelCounterFont.Text = String.Format("{0} {2}pt {1}", _counterFont.Name, _counterFont.Style, _counterFont.Size);

			_counterForeColor = Color.FromArgb(Configuration.CounterColor);
			panelCounterTextCol.BackColor = _counterForeColor;
			_counterBackColor = Color.FromArgb(Configuration.CounterBackColor);
			panelCounterBGCol.BackColor = _counterBackColor;
		}

		/// <summary>
		/// Opens a prompt to select a font and sets the font
		/// </summary>
		void SelectFont() {
			FontDialog.Font = _font;
			FontDialog.ShowDialog();
			_font = FontDialog.Font;
			Configuration.Name = _font.Name;
			Configuration.Style = (int)_font.Style;
			Configuration.Size = _font.Size;
			FontLabel.Text = String.Format("{0} {2}pt {1}", _font.Name, _font.Style, _font.Size);
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}

		void SelectCounterFormFont() {
			FontDialog.Font = _counterFont;
			FontDialog.ShowDialog();
			_counterFont = FontDialog.Font;
			Configuration.CounterName = _counterFont.Name;
			Configuration.CounterStyle = (int)_counterFont.Style;
			Configuration.CounterSize = _counterFont.Size;
			labelCounterFont.Text = String.Format("{0} {2}pt {1}", _counterFont.Name, _counterFont.Style, _counterFont.Size);
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
			return color;
		}

		#region events for buttons

		private void FontButton_Click(object sender, EventArgs e) {
			SelectFont();
		}

		private void buttonCounterFont_Click(object sender, EventArgs e) {
			SelectCounterFormFont();
		}

		private void ButtonForeColor_Click(object sender, EventArgs e) {
			_foreColor = SelectColor(_foreColor);
			Configuration.Color = _foreColor.ToArgb();
			ForeColorPreview.BackColor = _foreColor;
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}

		private void ButtonBackColor_Click(object sender, EventArgs e) {
			_backColor = SelectColor(_backColor);
			Configuration.BackColor = _backColor.ToArgb();
			BackColorPreview.BackColor = _backColor;
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}

		private void buttonCounterTextCol_Click(object sender, EventArgs e) {
			_counterForeColor = SelectColor(_counterForeColor);
			Configuration.CounterColor = _counterForeColor.ToArgb();
			panelCounterTextCol.BackColor = _counterForeColor;
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}

		private void buttonCounterBGCol_Click(object sender, EventArgs e) {
			_counterBackColor = SelectColor(_counterBackColor);
			Configuration.CounterBackColor = _counterBackColor.ToArgb();
			panelCounterBGCol.BackColor = _counterBackColor;
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}

		private void MinimizeToTrayCheckBox_CheckedChanged(object sender, EventArgs e) {
			Configuration.MinimizeToTray = MinimizeToTrayCheckBox.Checked;
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}

		private void ExitToTrayCheckBox_CheckedChanged(object sender, EventArgs e) {
			Configuration.ExitToTray = ExitToTrayCheckBox.Checked;
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}
		
		private void LogButtonsCheckbox_CheckedChanged(object sender, EventArgs e) {
			Configuration.LogButtons = LogButtonsCheckbox.Checked;
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}

		private void LogClicksCheckbox_CheckedChanged(object sender, EventArgs e) {
			Configuration.LogClicks = LogClicksCheckbox.Checked;
			OnSettingsChanged?.Invoke(this, new EventArgs());
		}

		private void LogMovementCheckbox_CheckedChanged(object sender, EventArgs e) {
			Configuration.LogMovement = LogMovementCheckbox.Checked;
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
