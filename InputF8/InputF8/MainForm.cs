using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace InputF8 {
	public partial class MainForm : Form {
		KeyPresses keyPresses;

		Dictionary<int, int> keyPressCount = new Dictionary<int, int>();
		Dictionary<int, TimeSpan> keyPressDuration = new Dictionary<int, TimeSpan>();
		Dictionary<int, Stopwatch> keyPressStopwatches = new Dictionary<int, Stopwatch>();

		// write text to file

		public MainForm() {
			InitializeComponent();

			foreach (int key in Keys.keyNames.Keys) {
				keyPressCount.Add(key, 0);
				keyPressDuration.Add(key, TimeSpan.Zero);
			}

			keyPresses = new KeyPresses();
			keyPresses.KeyDownEvent += OnKeyDown;
			keyPresses.KeyUpEvent += OnKeyUp;
			keyPresses.KeyChangeEvent += OnPressedKeysChanged;
			keyPresses.ReadKeys();
		}

		void OnKeyDown(int key) {
			keyPressCount[key] += 1;
			keyPressStopwatches.Add(key, new Stopwatch());
			keyPressStopwatches[key].Start();
		}

		void OnKeyUp(int key) {
			keyPressStopwatches[key].Stop();
			TimeSpan ts = keyPressStopwatches[key].Elapsed;
			keyPressDuration[key] += ts;
			keyPressStopwatches.Remove(key);
			
			//if (key == 0x91) {
			//	string asd = "";
			//	foreach (int keybah in keyPressCount.Keys) {
			//		asd += string.Format("{0}: {1}\n", Keys.keyNames[keybah], keyPressDuration[keybah]);
			//	}
			//	MessageBox.Show(asd);
			//	foreach (int keybah in keyPressCount.Keys) {
			//		asd += string.Format("{0}: {1}\n", Keys.keyNames[keybah], keyPressCount[keybah]);
			//	}
			//	MessageBox.Show(asd);
			//}
		}

		void OnPressedKeysChanged() {
			// write text in window
		}
	}
}
