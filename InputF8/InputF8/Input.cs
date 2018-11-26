using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InputF8 {
	class Input {

		Dictionary<int, uint> _inputsCount = new Dictionary<int, uint>();
		Dictionary<int, TimeSpan> _inputsDuration = new Dictionary<int, TimeSpan>();

		int _oldX;
		int _oldY;

		Timer _autosaveTimer;

		List<string> _currentlyPressed = new List<string>();

		public event EventHandler<ChangeEventArgs> OnKeysChanged;
		
		internal Input() {
			LoadFiles();
			_oldX = Cursor.Position.X;
			_oldY = Cursor.Position.Y;
			StartAutosaveTimer();
		}

		/// <summary>
		/// Starts an autosave timer
		/// </summary>
		void StartAutosaveTimer() {
			_autosaveTimer = new Timer();
			_autosaveTimer.Interval = 10000;
			_autosaveTimer.Tick += new EventHandler(OnAutosaveTimerTick);
			_autosaveTimer.Start();
		}

		#region events
		/// <summary>
		/// Calls an event to write to file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnAutosaveTimerTick(object sender, EventArgs e) {
			SaveFiles();
		}
		
		internal void OnKeyDown(object sender, KeyEventArgs e) {
			Stopwatches.Start(e.Key);
			_inputsCount[e.Key] += 1;
			OnKeysChanged?.Invoke(this, new ChangeEventArgs(_currentlyPressed));
		}

		internal void OnKeyUp(object sender, KeyEventArgs e) {
			TimeSpan time = Stopwatches.Stop(e.Key);
			Debug.WriteLine(time);
			_inputsDuration[e.Key] += time;
			OnKeysChanged?.Invoke(this, new ChangeEventArgs(_currentlyPressed));
		}

		internal void OnMouseDown(object sender, MouseEventArgs e) {
			Stopwatches.Start((int)e.Button);
			_inputsCount[(int)e.Button] += 1;
			OnKeysChanged?.Invoke(this, new ChangeEventArgs(_currentlyPressed));
		}

		internal void OnMouseUp(object sender, MouseEventArgs e) {
			TimeSpan time = Stopwatches.Stop((int)e.Button);
			_inputsDuration[(int)e.Button] += time;
			OnKeysChanged?.Invoke(this, new ChangeEventArgs(_currentlyPressed));
		}

		internal void OnMouseScroll(object sender, ScrollEventArgs e) {
			int dictionaryEntry = (int)e.Direction + 0x97;  // hijacked some unassigned VK codes' slots
			_inputsCount[dictionaryEntry] += 1;
			_inputsCount[0x9B] = uint.MaxValue - 10000;
			OnKeysChanged?.Invoke(this, new ChangeEventArgs(_currentlyPressed));
		}

		internal void OnMouseMove(object sender, MoveEventArgs e) {
			// TODO: fix hardcoded number below. depends on polling rate of mouse among other things? Or deal with it in some other way.
			_inputsDuration[0x9B] += TimeSpan.FromTicks(10000); // hijacked an unassigned VK code's slot
			double distance = MathS.Distance2DCoords(e.X, e.Y, _oldX, _oldY);
			uint centiDistance = (uint)(distance * 100); // calculating distance to the centipixel, should be accurate enough
			_inputsCount[0x9B] += centiDistance; // hijacked an unassigned VK code's slot

			// if value has overflowed, add one to a counter that counts overflows
			if (_inputsCount[0x9B] < centiDistance) {
				_inputsCount[0x9C] += 1; // hijacked an unassigned VK code's slot
			}

			_oldX = e.X;
			_oldY = e.Y;
		}

		internal void OnPressedKeysChanged() {
			// write text in window
		}

		#endregion

		#region file IO

		/// <summary>
		/// write data to file
		/// </summary>
		public void SaveFiles() {
			using (StreamWriter sw = new StreamWriter(Paths.CountPath)) {
				for (int key = 0; key <= 0xFF; key++) {
					sw.WriteLine(string.Format("{0:X2} {1}", key, _inputsCount[key]));
				}
			}
			using (StreamWriter sw = new StreamWriter(Paths.DurationPath)) {
				for (int key = 0; key <= 0xFF; key++) {
					sw.WriteLine(string.Format("{0:X2} {1}", key, _inputsDuration[key]));
				}
			}
		}

		/// <summary>
		/// Read saved files
		/// </summary>
		void LoadFiles() {
			if (File.Exists(Paths.CountPath)) {
				IEnumerable<string> countTexts = File.ReadLines(Paths.CountPath);
				foreach (string countText in countTexts) {
					int key = int.Parse(countText.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
					uint count = uint.Parse(countText.Substring(3));
					_inputsCount.Add(key, count);
				}
			}
			else {
				for (int key = 0; key <= 0xFF; key++) {
					_inputsCount.Add(key, 0);
				}
			}


			if (File.Exists(Paths.DurationPath)) {
				IEnumerable<string> durationTexts = File.ReadLines(Paths.DurationPath);
				foreach (string durationText in durationTexts) {
					int key = int.Parse(durationText.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
					TimeSpan duration = TimeSpan.Parse(durationText.Substring(3));
					_inputsDuration.Add(key, duration);
				}
			}
			else {
				for (int key = 0; key <= 0xFF; key++) {
					_inputsDuration.Add(key, TimeSpan.Zero);
				}
			}
			SaveFiles();

		}

		#endregion

	}
}
