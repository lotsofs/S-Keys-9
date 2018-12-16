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

		public SettingsForm() {
			InitializeComponent();
		}

		private void FontButton_Click(object sender, EventArgs e) {
			FontDialog.ShowDialog();
			Font newFont = FontDialog.Font;
			Configuration.Name = newFont.Name;
			Configuration.Style = (int)newFont.Style;
			Configuration.Size = newFont.Size;
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
	}
}
