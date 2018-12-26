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

		Stopwatches _stopwatches = new Stopwatches();
		System.Windows.Forms.Timer _autosaveTimer;
		System.Windows.Forms.Timer _scrollRemoveTimer;

		int _oldMouseX;
		int _oldMouseY;

		List<string> _currentlyPressed = new List<string>();
		Dictionary<string, int> _currentScrollCount = new Dictionary<string, int> {
			{ Keys.KeyNames[0x97], 0 }, 
			{ Keys.KeyNames[0x98], 0 }, 
			{ Keys.KeyNames[0x99], 0 }, 
			{ Keys.KeyNames[0x9A], 0 }, 
		};

		public event EventHandler<ChangeEventArgs> OnKeysChanged;

		ConcurrentQueue<MoveEventArgs> _mouseLocationQueue = new ConcurrentQueue<MoveEventArgs>();
		Thread _mouseMoveThread;

		internal Input() {
			LoadFiles();
			StartAutosaveTimer();
			StartScrollRemoveTimer();
			StartMouseMovementProcessing();
		}

		#region mouse scrollwheel counter

		/// <summary>
		/// Starts a timer that resets scroll wheel count back to 0 and removes it from the display
		/// </summary>
		void StartScrollRemoveTimer() {
			_scrollRemoveTimer = new System.Windows.Forms.Timer();
			_scrollRemoveTimer.Interval = 300; // TODO: Make this not hardcoded
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
			S.Dictionaries.IncrementValue(_inputsCount, dictionaryEntry, 1);
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
				S.Dictionaries.IncrementValue(_inputsCount, e.Key, 1);
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
				S.Dictionaries.IncrementValue(_inputsDuration, e.Key, time);
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
				S.Dictionaries.IncrementValue(_inputsCount, (int)e.Button, 1);
			}
			if (Configuration.LogClicks) {
				int coordsInt = S.Bits.CombineInt(e.X / 10, e.Y / 10);
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
				S.Dictionaries.IncrementValue(_inputsDuration, (int)e.Button, time);
			}
			//if (Configuration.LogClicks) {
			//	int coordsInt = MathS.CombineInt(e.X, e.Y);
			//	MathS.AddValueToDictionaryValue(_mouseInteractions, coordsInt, 1);
			//}
			SetButtonAsPressed((int)e.Button, false);
		}
		
		#endregion

		#region mouse movement stuff

		/// <summary>
		/// Creates a thread to process mouse movement data
		/// </summary>
		void StartMouseMovementProcessing() {
			// Todo: Add a way to stop this
			_oldMouseX = Cursor.Position.X;
			_oldMouseY = Cursor.Position.Y;

			_mouseMoveThread = new Thread(() => ProcessQueuedMouseMovements(_mouseLocationQueue));
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
			}
		}

		/// <summary>
		/// Looks if there are mouse movements to be processed
		/// </summary>
		/// <param name="movementQueue"></param>
		void ProcessQueuedMouseMovements(ConcurrentQueue<MoveEventArgs> movementQueue) {
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

		/// <summary>
		/// Process mouse movement
		/// </summary>
		/// <param name="e"></param>
		void MouseMoveProcessing(MoveEventArgs e) {
			if (!Configuration.LogMovement) {
				return;
			}

			S.Dictionaries.IncrementValue(_inputsDuration, 0x9B, TimeSpan.FromTicks(1)); // hijacked an unassigned VK code's slot
			double distance = S.Math.Distance2D(e.X, e.Y, _oldMouseX, _oldMouseY);
			uint centiDistance = (uint)(distance * 100); // calculating distance to the centipixel, should be accurate enough
			S.Dictionaries.IncrementValue(_inputsCount, 0x9B, centiDistance); // hijacked an unassigned VK code's slot
			
			// if value has overflowed, add one to a counter that counts overflows
			if (_inputsCount[0x9B] < centiDistance) {
				S.Dictionaries.IncrementValue(_inputsCount, 0x9C, 1); // hijacked an unassigned VK code's slot
			}

			int coordsInt = S.Bits.CombineInt(e.X / 100, e.X / 100);
			S.Dictionaries.IncrementValue(_mousePos, coordsInt, _stopwatches.MouseStop());   // TODO: This adds the new position of the mouse when really it should be the previous position added
			_stopwatches.MouseStart();

			_oldMouseX = e.X;
			_oldMouseY = e.Y;
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
			try {
				if (Configuration.LogButtons) {
					using (StreamWriter sw = new StreamWriter(Configuration.CountPath)) {
						for (int key = 0; key <= 0xFF; key++) {
							sw.WriteLine(string.Format("{0:X2} {1}", key, _inputsCount[key]));
						}
					}
					using (StreamWriter sw = new StreamWriter(Configuration.DurationPath)) {
						for (int key = 0; key <= 0xFF; key++) {
							sw.WriteLine(string.Format("{0:X2} {1}", key, _inputsDuration[key]));
						}
					}
				}
				if (Configuration.LogMovement) {
					using (StreamWriter sw = new StreamWriter(Configuration.MousePath)) {
						foreach (int i in _mousePos.Keys) {
							sw.WriteLine(string.Format("{0:X8} {1:X}", i, _mousePos[i].Ticks));
						}
					}
				}
				if (Configuration.LogClicks) {
					using (StreamWriter sw = new StreamWriter(Configuration.InteractionPath)) {
						foreach (int i in _mouseInteractions.Keys) {
							sw.WriteLine(string.Format("{0:X8} {1:X}", i, _mouseInteractions[i]));
						}
					}
				}
			}
			catch (InvalidOperationException) {
				Debug.WriteLine("Collection modified, cancelling save");
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
