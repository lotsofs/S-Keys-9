using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace InputF8 {
	class Input {

		Dictionary<int, uint> _inputsCount = new Dictionary<int, uint>();
		Dictionary<int, TimeSpan> _inputsDuration = new Dictionary<int, TimeSpan>();

		Dictionary<int, TimeSpan> _mousePos = new Dictionary<int, TimeSpan>();
		Dictionary<int, int> _mouseInteractions = new Dictionary<int, int>();

		int _oldX;
		int _oldY;

		System.Windows.Forms.Timer _autosaveTimer;

		List<string> _currentlyPressed = new List<string>();

		public event EventHandler<ChangeEventArgs> OnKeysChanged;

		ConcurrentQueue<MoveEventArgs> queue = new ConcurrentQueue<MoveEventArgs>();
		Thread mouseMoveThread;

		internal Input() {
			LoadFiles();
			_oldX = Cursor.Position.X;
			_oldY = Cursor.Position.Y;
			StartAutosaveTimer();

			mouseMoveThread = new Thread(() => ProcessedQueuedMouseMovements(queue));
			mouseMoveThread.IsBackground = true;
			mouseMoveThread.Name = "S Keys 9 BG Thread, where does this show up?";
			mouseMoveThread.Start();
		}

		/// <summary>
		/// Starts an autosave timer
		/// </summary>
		void StartAutosaveTimer() {
			_autosaveTimer = new System.Windows.Forms.Timer();
			_autosaveTimer.Interval = 60000;
			_autosaveTimer.Tick += new EventHandler(OnAutosaveTimerTick);
			_autosaveTimer.Start();
		}

		void SetKeyAsPressed(int key, bool pressed) {
			string keyName;
			if (Keys.keyNames.ContainsKey(key)) {
				keyName = Keys.keyNames[key];
			}
			else {
				keyName = ("?" + key);
			}
			if (pressed) {
				_currentlyPressed.Add(keyName);
			}
			else {
				_currentlyPressed.Remove(keyName);
			}
			OnKeysChanged?.Invoke(this, new ChangeEventArgs(_currentlyPressed));
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
			MathS.AddValueToDictionaryValue(_inputsCount, e.Key, 1);
			SetKeyAsPressed(e.Key, true);
		}

		/// <summary>
		/// key up
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnKeyUp(object sender, KeyEventArgs e) {
			TimeSpan time = Stopwatches.Stop(e.Key);
			MathS.AddValueToDictionaryValue(_inputsDuration, e.Key, time);
			SetKeyAsPressed(e.Key, false);
		}

		/// <summary>
		/// mouse down
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnMouseDown(object sender, MouseEventArgs e) {
			Stopwatches.Start((int)e.Button);
			MathS.AddValueToDictionaryValue(_inputsCount, (int)e.Button, 1);
			int coordsInt = MathS.CombineInt(e.X / 10, e.Y / 10);
			MathS.AddValueToDictionaryValue(_mouseInteractions, coordsInt, 1);
			SetKeyAsPressed((int)e.Button, true);
		}

		/// <summary>
		/// mouse up
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnMouseUp(object sender, MouseEventArgs e) {
			TimeSpan time = Stopwatches.Stop((int)e.Button);
			MathS.AddValueToDictionaryValue(_inputsDuration, (int)e.Button, time);
			//int coordsInt = MathS.CombineInt(e.X, e.Y);
			//MathS.AddValueToDictionaryValue(_mouseInteractions, coordsInt, 1);
			SetKeyAsPressed((int)e.Button, false);
		}

		/// <summary>
		/// mouse scroll
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnMouseScroll(object sender, ScrollEventArgs e) {
			int dictionaryEntry = (int)e.Direction + 0x97;  // hijacked some unassigned VK codes' slots
			MathS.AddValueToDictionaryValue(_inputsCount, dictionaryEntry, 1);
			OnKeysChanged?.Invoke(this, new ChangeEventArgs(_currentlyPressed));
		}

		/// <summary>
		/// mouse move
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnMouseMove(object sender, MoveEventArgs e) {
			queue.Enqueue(e);
		}

		internal void OnPressedKeysChanged() {
			// write text in window
		}

		#endregion

		#region mouse movement stuff

		void ProcessedQueuedMouseMovements(ConcurrentQueue<MoveEventArgs> movementQueue) {
			while (true) {
				if (movementQueue.Count == 0) {
					Thread.Sleep(50);
					continue;
				}
				MoveEventArgs result;
				if (movementQueue.TryDequeue(out result)) {
					MouseMoveProcessing(result);
				}
			}
		}

		void MouseMoveProcessing(MoveEventArgs e) {
			MathS.AddValueToDictionaryValue(_inputsDuration, 0x9B, TimeSpan.FromTicks(1)); // hijacked an unassigned VK code's slot
			double distance = MathS.Distance2DCoords(e.X, e.Y, _oldX, _oldY);
			uint centiDistance = (uint)(distance * 100); // calculating distance to the centipixel, should be accurate enough
			MathS.AddValueToDictionaryValue(_inputsCount, 0x9B, centiDistance); // hijacked an unassigned VK code's slot
																				// if value has overflowed, add one to a counter that counts overflows
			if (_inputsCount[0x9B] < centiDistance) {
				MathS.AddValueToDictionaryValue(_inputsCount, 0x9C, 1); // hijacked an unassigned VK code's slot
			}

			int coordsInt = MathS.CombineInt(e.X / 100, e.X / 100);
			MathS.AddValueToDictionaryValue(_mousePos, coordsInt, Stopwatches.MouseStop());
			Stopwatches.MouseStart();

			_oldX = e.X;
			_oldY = e.Y;
		}

		#endregion


		#region file IO

		public void BackupSave() {
			File.Copy(Paths.CountPath, Path.ChangeExtension(Paths.CountPath, ".log.bak"), true);
			File.Copy(Paths.DurationPath, Path.ChangeExtension(Paths.DurationPath, ".log.bak"), true);
			File.Copy(Paths.MousePath, Path.ChangeExtension(Paths.MousePath, ".log.bak"), true);
			File.Copy(Paths.InteractionPath, Path.ChangeExtension(Paths.InteractionPath, ".log.bak"), true);
		}
		
		/// <summary>
		/// write data to file
		/// </summary>
		public void SaveFiles() {
			//await Task.Delay(1);
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
					MathS.AddValueToDictionaryValue(_inputsCount, key, count);
				}
			}
			for (int key = 0; key <= 0xFF; key++) {
				if (!_inputsCount.ContainsKey(key)) {
					MathS.AddValueToDictionaryValue(_inputsCount, key, 0);
				}
			}

			if (File.Exists(Paths.DurationPath)) {
				IEnumerable<string> durationTexts = File.ReadLines(Paths.DurationPath);
				foreach (string durationText in durationTexts) {
					int key = int.Parse(durationText.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
					TimeSpan duration = TimeSpan.Parse(durationText.Substring(3));
					MathS.AddValueToDictionaryValue(_inputsDuration, key, duration);
				}
			}
			for (int key = 0; key <= 0xFF; key++) {
				if (!_inputsDuration.ContainsKey(key)) {
					MathS.AddValueToDictionaryValue(_inputsDuration, key, TimeSpan.Zero);
				}
			}

			if (File.Exists(Paths.MousePath)) {
				IEnumerable<string> mouseTexts = File.ReadLines(Paths.MousePath);
				foreach (string mouseText in mouseTexts) {
					int coord = int.Parse(mouseText.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
					TimeSpan duration = TimeSpan.FromTicks(long.Parse(mouseText.Substring(9), System.Globalization.NumberStyles.HexNumber));
					MathS.AddValueToDictionaryValue(_mousePos, coord, duration);
				}
			}

			if (File.Exists(Paths.InteractionPath)) {
				IEnumerable<string> mouseTexts = File.ReadLines(Paths.InteractionPath);
				foreach (string mouseText in mouseTexts) {
					int coord = int.Parse(mouseText.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
					int count = int.Parse(mouseText.Substring(9), System.Globalization.NumberStyles.HexNumber);
					MathS.AddValueToDictionaryValue(_mouseInteractions, coord, count);
				}
			}

			SaveFiles();
			BackupSave();
		}

		#endregion

	}
}
