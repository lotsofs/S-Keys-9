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

		Dictionary<int, TimeSpan> _mousePos = new Dictionary<int, TimeSpan>();
		Dictionary<int, int> _mouseInteractions = new Dictionary<int, int>();

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
		
		/// <summary>
		/// key down
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnKeyDown(object sender, KeyEventArgs e) {
			Stopwatches.Start(e.Key);
			_inputsCount[e.Key] += 1;
			OnKeysChanged?.Invoke(this, new ChangeEventArgs(_currentlyPressed));
		}

		/// <summary>
		/// key up
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnKeyUp(object sender, KeyEventArgs e) {
			TimeSpan time = Stopwatches.Stop(e.Key);
			_inputsDuration[e.Key] += time;
			OnKeysChanged?.Invoke(this, new ChangeEventArgs(_currentlyPressed));
		}

		/// <summary>
		/// mouse down
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnMouseDown(object sender, MouseEventArgs e) {
			Stopwatches.Start((int)e.Button);
			_inputsCount[(int)e.Button] += 1;
			OnKeysChanged?.Invoke(this, new ChangeEventArgs(_currentlyPressed));

			int coordsInt = MathS.CombineInt(e.X, e.Y);
			if (_mouseInteractions.ContainsKey(coordsInt)) {
				_mouseInteractions[coordsInt] += 1;
			}
			else {
				_mouseInteractions.Add(coordsInt, 1);
			}
		}

		/// <summary>
		/// mouse up
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnMouseUp(object sender, MouseEventArgs e) {
			TimeSpan time = Stopwatches.Stop((int)e.Button);
			_inputsDuration[(int)e.Button] += time;
			OnKeysChanged?.Invoke(this, new ChangeEventArgs(_currentlyPressed));

			int coordsInt = MathS.CombineInt(e.X, e.Y);
			if (_mouseInteractions.ContainsKey(coordsInt)) {
				_mouseInteractions[coordsInt] += 1;
			}
			else {
				_mouseInteractions.Add(coordsInt, 1);
			}
		}

		/// <summary>
		/// mouse scroll
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnMouseScroll(object sender, ScrollEventArgs e) {
			int dictionaryEntry = (int)e.Direction + 0x97;  // hijacked some unassigned VK codes' slots
			_inputsCount[dictionaryEntry] += 1;
			OnKeysChanged?.Invoke(this, new ChangeEventArgs(_currentlyPressed));
		}

		/// <summary>
		/// mouse move
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

			int coordsInt = MathS.CombineInt(e.X, e.Y);
			if (_mousePos.ContainsKey(coordsInt)) {
				_mousePos[coordsInt] += Stopwatches.MouseStop();
			}
			else {
				_mousePos.Add(coordsInt, Stopwatches.MouseStop());
			}
			Stopwatches.MouseStart();

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
			// TODO: Make backup saves
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
			using (StreamWriter sw = new StreamWriter(Paths.MousePath)) {
				foreach (int i in _mousePos.Keys) {
					sw.WriteLine(string.Format("{0:X8} {1:X}", i, _mousePos[i].Ticks));
				}
			}
			using (StreamWriter sw = new StreamWriter(Paths.InteractionPath)) {
				foreach (int i in _mouseInteractions.Keys) {
					sw.WriteLine(string.Format("{0:X8} {1:X}", i, _mouseInteractions[i]));
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
			// TODO: Validate contents, make sure there's 256 entries
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

			if (File.Exists(Paths.MousePath)) {
				IEnumerable<string> mouseTexts = File.ReadLines(Paths.MousePath);
				foreach (string mouseText in mouseTexts) {
					int coord = int.Parse(mouseText.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
					TimeSpan duration = TimeSpan.FromTicks(long.Parse(mouseText.Substring(9), System.Globalization.NumberStyles.HexNumber));
					_mousePos.Add(coord, duration);
				}
			}

			if (File.Exists(Paths.InteractionPath)) {
				IEnumerable<string> mouseTexts = File.ReadLines(Paths.InteractionPath);
				foreach (string mouseText in mouseTexts) {
					int coord = int.Parse(mouseText.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
					int count = int.Parse(mouseText.Substring(9), System.Globalization.NumberStyles.HexNumber);
					_mouseInteractions.Add(coord, count);
				}
			}

			SaveFiles();
		}

		#endregion

	}
}
