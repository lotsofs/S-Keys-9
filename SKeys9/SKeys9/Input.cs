using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace SKeys9 {
	public class Input {
		Dictionary<int, uint> _inputsCount = new Dictionary<int, uint>();
		Dictionary<int, TimeSpan> _inputsDuration = new Dictionary<int, TimeSpan>();

		Dictionary<int, TimeSpan> _mousePos = new Dictionary<int, TimeSpan>();
		Dictionary<int, int> _mouseInteractions = new Dictionary<int, int>();
		readonly object _statsLock = new object(); // the mouse move thread shares everything above except _mouseInteractions

		Stopwatches _stopwatches = new Stopwatches();
		System.Windows.Forms.Timer _autosaveTimer;
		System.Windows.Forms.Timer _scrollRemoveTimer;

		int _oldMouseX;
		int _oldMouseY;
		uint _oldMouseTime;
		int _logMovementChanges;

		List<string> _currentlyPressed = new List<string>();
		Dictionary<string, int> _currentScrollCount = new Dictionary<string, int> {
			{ Keys.KeyNames[0x97], 0 }, 
			{ Keys.KeyNames[0x98], 0 }, 
			{ Keys.KeyNames[0x99], 0 }, 
			{ Keys.KeyNames[0x9A], 0 }, 
		};

		public event EventHandler<ChangeEventArgs> OnKeysChanged;

		ConcurrentQueue<MoveEventArgs> _mouseLocationQueue = new ConcurrentQueue<MoveEventArgs>();
		AutoResetEvent _mouseMoveQueued = new AutoResetEvent(false);
		Thread _mouseMoveThread;

		internal Input() {
			LoadFiles();
			StartAutosaveTimer();
			StartScrollRemoveTimer();
			StartMouseMovementProcessing();
		}

		#region statistics shared with the mouse move thread

		/// <summary>
		/// Adds to a key's count
		/// </summary>
		/// <param name="key">the key</param>
		void AddToCount(int key) {
			lock (_statsLock) {
				S.Dictionaries.IncrementValue(_inputsCount, key, 1);
			}
		}

		/// <summary>
		/// Adds to a key's total duration
		/// </summary>
		/// <param name="key">the key</param>
		/// <param name="time">the time to add</param>
		void AddToDuration(int key, TimeSpan time) {
			lock (_statsLock) {
				S.Dictionaries.IncrementValue(_inputsDuration, key, time);
			}
		}

		#endregion

		#region mouse scrollwheel counter

		/// <summary>
		/// Starts a timer that resets scroll wheel count back to 0 and removes it from the display
		/// </summary>
		void StartScrollRemoveTimer() {
			_scrollRemoveTimer = new System.Windows.Forms.Timer();
			_scrollRemoveTimer.Interval = 300;
			_scrollRemoveTimer.Tick += ResetScrolls;
		}

		/// <summary>
		/// reset scroll wheel counters to 0
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void ResetScrolls(object sender, EventArgs e) {
			_scrollRemoveTimer.Stop();
			_currentScrollCount["ScrUp"] = 0;
			_currentScrollCount["ScrDn"] = 0;
			_currentScrollCount["ScrLeft"] = 0;
			_currentScrollCount["ScrRight"] = 0;
			OnKeysChanged?.Invoke(this, new ChangeEventArgs(_currentlyPressed, _currentScrollCount));
		}
		
		/// <summary>
		/// Detects mouse scroll
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnMouseScroll(object sender, ScrollEventArgs e) {
			int dictionaryEntry = (int)e.Direction + 0x97;  // hijacked some unassigned VK codes' slots
			AddToCount(dictionaryEntry);
			AddScroll(dictionaryEntry);
		}

		/// <summary>
		/// Increase scroll wheel counter by 1
		/// </summary>
		/// <param name="directionId"></param>
		void AddScroll(int directionId) {
			string scrollDirectionName;
			if (Keys.KeyNames.ContainsKey(directionId)) {
				scrollDirectionName = Keys.KeyNames[directionId];
			}
			else {
				scrollDirectionName = ("?" + directionId);
			}
			S.Dictionaries.IncrementValue(_currentScrollCount, scrollDirectionName, 1);
			_scrollRemoveTimer.Stop();
			_scrollRemoveTimer.Start();
			OnKeysChanged?.Invoke(this, new ChangeEventArgs(_currentlyPressed, _currentScrollCount));
		}

		#endregion

		#region Button Press Detection

		/// <summary>
		/// Marks a key's pressed status
		/// </summary>
		/// <param name="key">the key</param>
		/// <param name="pressed">is it pressed?</param>
		void SetButtonAsPressed(int key, bool pressed) {
			string keyName;
			keyName = Keys.KeyNames.ContainsKey(key) ? Keys.KeyNames[key] : ("?" + key);
		
			if (pressed) {
				_currentlyPressed.Add(keyName);
			}
			else {
				_currentlyPressed.Remove(keyName);
			}
			OnKeysChanged?.Invoke(this, new ChangeEventArgs(_currentlyPressed, _currentScrollCount));
		}

		/// <summary>
		/// key down
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnButtonDown(object sender, KeyEventArgs e) {
			if (Configuration.LogButtons) {
				_stopwatches.KeyPressStart(e.Key);
				AddToCount(e.Key);
			}
			SetButtonAsPressed(e.Key, true);
		}

		/// <summary>
		/// key up
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnButtonUp(object sender, KeyEventArgs e) {
			if (Configuration.LogButtons) {
				TimeSpan time = _stopwatches.KeyPressStop(e.Key);
				AddToDuration(e.Key, time);
			}
			SetButtonAsPressed(e.Key, false);
		}

		/// <summary>
		/// mouse down
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnMouseDown(object sender, MouseEventArgs e) {
			if (Configuration.LogButtons) {
				_stopwatches.KeyPressStart((int)e.Button);
				AddToCount((int)e.Button);
			}
			if (Configuration.LogClicks) {
				int coordsInt = S.Bits.CombineInt(S.Math.FloorDivide(e.X, 10), S.Math.FloorDivide(e.Y, 10));
				S.Dictionaries.IncrementValue(_mouseInteractions, coordsInt, 1);
			}
			SetButtonAsPressed((int)e.Button, true);
		}

		/// <summary>
		/// mouse up
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnMouseUp(object sender, MouseEventArgs e) {
			if (Configuration.LogButtons) {
				TimeSpan time = _stopwatches.KeyPressStop((int)e.Button);
				AddToDuration((int)e.Button, time);
			}
			//if (Configuration.LogClicks) {
			//	int coordsInt = MathS.CombineInt(e.X, e.Y);
			//	MathS.AddValueToDictionaryValue(_mouseInteractions, coordsInt, 1);
			//}
			SetButtonAsPressed((int)e.Button, false);
		}
		
		#endregion

		#region hook health

		/// <summary>
		/// The hooks were reinstalled because one of them missed input
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnHooksReinstalled(object sender, EventArgs e) {
			if (Configuration.LogButtons) {
				AddToCount(0x9D); // hijacked an unassigned VK code's slot
			}
		}

		#endregion

		#region mouse movement stuff

		/// <summary>
		/// Creates a thread to process mouse movement data
		/// </summary>
		void StartMouseMovementProcessing() {
			_oldMouseX = Cursor.Position.X;
			_oldMouseY = Cursor.Position.Y;
			_oldMouseTime = unchecked((uint)Environment.TickCount); // same clock as the hook's timestamps
			_logMovementChanges = Configuration.LogMovementChanges;

			_mouseMoveThread = new Thread(ProcessQueuedMouseMovements);
			_mouseMoveThread.IsBackground = true;
			//_mouseMoveThread.Name = "S Keys 9 BG Thread, where does this show up?";
			_mouseMoveThread.Start();
		}

		/// <summary>
		/// mouse move
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnMouseMove(object sender, MoveEventArgs e) {
			if (Configuration.LogMovement) {
				_mouseLocationQueue.Enqueue(e);
				_mouseMoveQueued.Set();
			}
		}

		/// <summary>
		/// Sleeps until there are mouse movements to be processed.
		/// Runs until the process ends, because a shutdown that was asked for can still be cancelled
		/// </summary>
		void ProcessQueuedMouseMovements() {
			while (true) {
				_mouseMoveQueued.WaitOne();
				Thread.Sleep(50); // let moves pile up, so a moving mouse doesn't wake this thread for each one
				FinishQueuedMouseMovements();
			}
		}

		/// <summary>
		/// Processes everything in the queue. Each move leaves the queue inside the lock,
		/// so once this returns on the UI thread, none is left for a save to miss
		/// </summary>
		internal void FinishQueuedMouseMovements() {
			while (true) {
				lock (_statsLock) {
					MoveEventArgs result;
					if (!_mouseLocationQueue.TryDequeue(out result)) {
						return;
					}
					MouseMoveProcessing(result);
				}
			}
		}

		/// <summary>
		/// Process mouse movement
		/// </summary>
		/// <param name="e"></param>
		void MouseMoveProcessing(MoveEventArgs e) {
			if (!Configuration.LogMovement) {
				return;
			}

			S.Dictionaries.IncrementValue(_inputsDuration, 0x9B, TimeSpan.FromTicks(1)); // hijacked an unassigned VK code's slot

			// logging was off since the previous move, so the time and distance from it mean nothing
			if (_logMovementChanges != Configuration.LogMovementChanges) {
				_logMovementChanges = Configuration.LogMovementChanges;
				RememberMouseMove(e);
				return;
			}

			double distance = S.Math.Distance2D(e.X, e.Y, _oldMouseX, _oldMouseY);
			uint centiDistance = (uint)(distance * 100); // calculating distance to the centipixel, should be accurate enough
			S.Dictionaries.IncrementValue(_inputsCount, 0x9B, centiDistance); // hijacked an unassigned VK code's slot
			
			// if value has overflowed, add one to a counter that counts overflows
			if (_inputsCount[0x9B] < centiDistance) {
				S.Dictionaries.IncrementValue(_inputsCount, 0x9C, 1); // hijacked an unassigned VK code's slot
			}

			// the time since the previous move is how long the mouse rested in the previous cell
			// signed so that it survives the timestamp wrapping, and a timestamp running backwards adds nothing
			int restMilliseconds = System.Math.Max(0, unchecked((int)(e.Time - _oldMouseTime)));
			int coordsInt = S.Bits.CombineInt(S.Math.FloorDivide(_oldMouseX, 100), S.Math.FloorDivide(_oldMouseY, 100));
			S.Dictionaries.IncrementValue(_mousePos, coordsInt, TimeSpan.FromMilliseconds(restMilliseconds));

			RememberMouseMove(e);
		}

		/// <summary>
		/// Keeps a mouse move for the next one to be compared against
		/// </summary>
		/// <param name="e"></param>
		void RememberMouseMove(MoveEventArgs e) {
			_oldMouseX = e.X;
			_oldMouseY = e.Y;
			_oldMouseTime = e.Time;
		}

		#endregion

		#region log data

		/// <summary>
		/// Calls an event to write to file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnAutosaveTimerTick(object sender, EventArgs e) {
			SaveFiles();
		}

		/// <summary>
		/// Starts an autosave timer
		/// </summary>
		void StartAutosaveTimer() {
			_autosaveTimer = new System.Windows.Forms.Timer();
			_autosaveTimer.Interval = 60000;
			_autosaveTimer.Tick += OnAutosaveTimerTick;
			_autosaveTimer.Start();
		}

		/// <summary>
		/// Makes a backup save
		/// </summary>
		public void BackupSave() {
			if (Configuration.LogButtons) {
				File.Copy(Configuration.CountPath, Path.ChangeExtension(Configuration.CountPath, ".log.bak"), true);
				File.Copy(Configuration.DurationPath, Path.ChangeExtension(Configuration.DurationPath, ".log.bak"), true);
			}
			if (Configuration.LogClicks) {
				File.Copy(Configuration.InteractionPath, Path.ChangeExtension(Configuration.InteractionPath, ".log.bak"), true);
			}
			if (Configuration.LogMovement) {
				File.Copy(Configuration.MousePath, Path.ChangeExtension(Configuration.MousePath, ".log.bak"), true);
			}
		}

		/// <summary>
		/// write data to file
		/// </summary>
		public void SaveFiles() {
			Dictionary<int, uint> inputsCount;
			Dictionary<int, TimeSpan> inputsDuration;
			Dictionary<int, TimeSpan> mousePos;
			lock (_statsLock) {
				inputsCount = new Dictionary<int, uint>(_inputsCount);
				inputsDuration = new Dictionary<int, TimeSpan>(_inputsDuration);
				mousePos = new Dictionary<int, TimeSpan>(_mousePos);
			}

			if (Configuration.LogButtons) {
				WriteLog(Configuration.CountPath, sw => {
					for (int key = 0; key <= 0xFF; key++) {
						sw.WriteLine(string.Format("{0:X2} {1}", key, inputsCount[key]));
					}
				});
				WriteLog(Configuration.DurationPath, sw => {
					for (int key = 0; key <= 0xFF; key++) {
						sw.WriteLine(string.Format("{0:X2} {1}", key, inputsDuration[key]));
					}
				});
			}
			if (Configuration.LogMovement) {
				WriteLog(Configuration.MousePath, sw => {
					foreach (int i in mousePos.Keys) {
						sw.WriteLine(string.Format("{0:X8} {1:X}", i, mousePos[i].Ticks));
					}
				});
			}
			if (Configuration.LogClicks) {
				WriteLog(Configuration.InteractionPath, sw => {
					foreach (int i in _mouseInteractions.Keys) {
						sw.WriteLine(string.Format("{0:X8} {1:X}", i, _mouseInteractions[i]));
					}
				});
			}
		}

		/// <summary>
		/// Writes a log to a temporary file and swaps it in, so a save that fails partway can't cut the real file short
		/// </summary>
		/// <param name="path">the log</param>
		/// <param name="write">writes the log's lines</param>
		void WriteLog(string path, Action<StreamWriter> write) {
			string tempPath = path + ".tmp";
			using (StreamWriter sw = new StreamWriter(tempPath)) {
				write(sw);
			}

			if (!File.Exists(path)) {
				File.Move(tempPath, path);
				return;
			}
			try {
				File.Replace(tempPath, path, null);
			}
			catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) {
				// something has the log open. The data is still in memory, so the next save tries again
				Log.Write("Couldn't update " + Path.GetFileName(path) + ": " + e.Message);
			}
		}

		/// <summary>
		/// Read saved files
		/// </summary>
		void LoadFiles() {
			if (File.Exists(Configuration.CountPath)) {
				IEnumerable<string> countTexts = File.ReadLines(Configuration.CountPath);
				foreach (string countText in countTexts) {
					int key = int.Parse(countText.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
					uint count = uint.Parse(countText.Substring(3));
					S.Dictionaries.IncrementValue(_inputsCount, key, count);
				}
			}
			for (int key = 0; key <= 0xFF; key++) {
				if (!_inputsCount.ContainsKey(key)) {
					S.Dictionaries.IncrementValue(_inputsCount, key, 0);
				}
			}

			if (File.Exists(Configuration.DurationPath)) {
				IEnumerable<string> durationTexts = File.ReadLines(Configuration.DurationPath);
				foreach (string durationText in durationTexts) {
					int key = int.Parse(durationText.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
					TimeSpan duration = TimeSpan.Parse(durationText.Substring(3));
					S.Dictionaries.IncrementValue(_inputsDuration, key, duration);
				}
			}
			for (int key = 0; key <= 0xFF; key++) {
				if (!_inputsDuration.ContainsKey(key)) {
					S.Dictionaries.IncrementValue(_inputsDuration, key, TimeSpan.Zero);
				}
			}

			if (File.Exists(Configuration.MousePath)) {
				IEnumerable<string> mouseTexts = File.ReadLines(Configuration.MousePath);
				foreach (string mouseText in mouseTexts) {
					int coord = int.Parse(mouseText.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
					TimeSpan duration = TimeSpan.FromTicks(long.Parse(mouseText.Substring(9), System.Globalization.NumberStyles.HexNumber));
					S.Dictionaries.IncrementValue(_mousePos, coord, duration);
				}
			}

			if (File.Exists(Configuration.InteractionPath)) {
				IEnumerable<string> mouseTexts = File.ReadLines(Configuration.InteractionPath);
				foreach (string mouseText in mouseTexts) {
					int coord = int.Parse(mouseText.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
					int count = int.Parse(mouseText.Substring(9), System.Globalization.NumberStyles.HexNumber);
					S.Dictionaries.IncrementValue(_mouseInteractions, coord, count);
				}
			}

			SaveFiles();
			BackupSave();
		}

		#endregion

	}
}
