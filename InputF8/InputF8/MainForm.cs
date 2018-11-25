using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace InputF8 {
	public partial class MainForm : Form {
		KeyPresses _keyPresses;

		Dictionary<int, int> _keyPressCount = new Dictionary<int, int>();
		Dictionary<int, TimeSpan> _keyPressDuration = new Dictionary<int, TimeSpan>();
		Dictionary<int, Stopwatch> _keyPressStopwatches = new Dictionary<int, Stopwatch>();

		Timer _autosaveTimer;

		string _directoryPath;
		string _countPath;
		string _durationPath;

		Hooks test = new Hooks();

		public MainForm() {
			InitializeComponent();
			test.EnableHooks();

			Directories();
			ReadFromFile();
			AutosaveTimer();
			KeyPresses();
		}

		/// <summary>
		/// Start reading keypresses
		/// </summary>
		void KeyPresses() {
			_keyPresses = new KeyPresses();
			_keyPresses.KeyDownEvent += OnKeyDown;
			_keyPresses.KeyUpEvent += OnKeyUp;
			_keyPresses.KeyChangeEvent += OnPressedKeysChanged;
			_keyPresses.ReadKeys();
		}

		/// <summary>
		/// Set directory paths and create them
		/// </summary>
		void Directories() {
			_directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"S\inputf8\");
			_countPath = Path.Combine(_directoryPath, "count.txt");
			_durationPath = Path.Combine(_directoryPath, "duration.txt");

			if (!Directory.Exists(_directoryPath)) {
				Directory.CreateDirectory(_directoryPath);
			}
		}

		/// <summary>
		/// Starts an autosave timer
		/// </summary>
		void AutosaveTimer() {
			_autosaveTimer = new Timer();
			_autosaveTimer.Interval = 10000;
			_autosaveTimer.Tick += new EventHandler(OnAutosaveTimerTick);
			_autosaveTimer.Start();
		}

		#region events

		/// <summary>
		/// key pressed
		/// </summary>
		/// <param name="key"></param>
		void OnKeyDown(int key) {
			_keyPressCount[key] += 1;
			StartStopwatch(key);
		}

		/// <summary>
		/// key released
		/// </summary>
		/// <param name="key"></param>
		void OnKeyUp(int key) {
			StopStopwatch(key);
		}

		void OnPressedKeysChanged() {
			// write text in window
		}

		/// <summary>
		/// Calls an event to write to file
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnAutosaveTimerTick(object sender, EventArgs e) {
			WriteToFile();
		}

		#endregion

		#region stopwatches

		/// <summary>
		/// starts a stopwatch that tracks the duration of a keypress
		/// </summary>
		/// <param name="key"></param>
		void StartStopwatch(int key) {
			_keyPressStopwatches.Add(key, new Stopwatch());
			_keyPressStopwatches[key].Restart();
		}

		/// <summary>
		/// stops a stopwatch that tracks the duration of a keypress
		/// </summary>
		/// <param name="key"></param>
		void StopStopwatch(int key) {
			_keyPressStopwatches[key].Stop();
			TimeSpan ts = _keyPressStopwatches[key].Elapsed;
			_keyPressDuration[key] += ts;
			_keyPressStopwatches.Remove(key);
		}

		#endregion

		#region file IO

		/// <summary>
		/// write data to file
		/// </summary>
		void WriteToFile() {
			using (StreamWriter sw = new StreamWriter(_countPath)) {
				for (int key = 0; key <= 0xFF; key++) {
					sw.WriteLine(string.Format("{0:X2} {1}", key, _keyPressCount[key]));
				}
			}
			using (StreamWriter sw = new StreamWriter(_durationPath)) {
				for (int key = 0; key <= 0xFF; key++) {
					sw.WriteLine(string.Format("{0:X2} {1}", key, _keyPressDuration[key]));
				}
			}
		}

		/// <summary>
		/// Read saved files
		/// </summary>
		void ReadFromFile() {
			if (File.Exists(_countPath)) {
				IEnumerable<string> countTexts = File.ReadLines(_countPath);
				foreach (string countText in countTexts) {
					int key = int.Parse(countText.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
					int count = int.Parse(countText.Substring(3));
					_keyPressCount.Add(key, count);
				}
			}
			else {
				for (int key = 0; key <= 0xFF; key++) {
					_keyPressCount.Add(key, 0);
				}
			}


			if (File.Exists(_durationPath)) {
				IEnumerable<string> durationTexts = File.ReadLines(_durationPath);
				foreach (string durationText in durationTexts) {
					int key = int.Parse(durationText.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
					TimeSpan duration = TimeSpan.Parse(durationText.Substring(3));
					_keyPressDuration.Add(key, duration);
				}
			}
			else {
				for (int key = 0; key <= 0xFF; key++) {
					_keyPressDuration.Add(key, TimeSpan.Zero);
				}
			}
			WriteToFile();

		}

		#endregion

	}
}
