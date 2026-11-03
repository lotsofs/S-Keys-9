using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SKeys9 {
	public class Input {
		Dictionary<int, uint> _inputsCount = new Dictionary<int, uint>();
		Dictionary<int, TimeSpan> _inputsDuration = new Dictionary<int, TimeSpan>();

		Dictionary<int, TimeSpan> _mousePos = new Dictionary<int, TimeSpan>();
		Dictionary<int, int> _mouseInteractions = new Dictionary<int, int>();

		// whether each logging group changed since it was last saved. True at first, so the save after loading writes every log
		bool _buttonsChanged = true;
		bool _movementChanged = true;
		bool _clicksChanged = true;
		readonly object _statsLock = new object(); // the mouse move thread shares everything above except _mouseInteractions, and a failed save sets the flags again

		HashSet<string> _damagedLogs = new HashSet<string>(); // logs that had lines that couldn't be read, so their backups are kept

		Stopwatches _stopwatches = new Stopwatches();
		System.Windows.Forms.Timer _autosaveTimer;
		Task _saving; // the autosave that's writing, if any. Only the UI thread touches this
		System.Windows.Forms.Timer _scrollRemoveTimer;

		int _oldMouseX;
		int _oldMouseY;
		uint _oldMouseTime;
		int _logMovementChanges;

		List<string> _currentlyPressed = new List<string>();
		Dictionary<string, int> _currentScrollCount = new Dictionary<string, int>();

		public event EventHandler<ChangeEventArgs> OnKeysChanged;

		ConcurrentQueue<MoveEventArgs> _mouseLocationQueue = new ConcurrentQueue<MoveEventArgs>();
		AutoResetEvent _mouseMoveQueued = new AutoResetEvent(false);
		Thread _mouseMoveThread;

		internal Input() {
			LoadFiles();
			StartAutosaveTimer();
			AddScrollCounters();
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
				_buttonsChanged = true;
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
				_buttonsChanged = true;
			}
		}

		#endregion

		#region mouse scrollwheel counter

		/// <summary>
		/// Adds a counter for each scroll direction, in display order. Directions with the same name share one
		/// </summary>
		void AddScrollCounters() {
			for (int direction = 0x97; direction <= 0x9A; direction++) {
				_currentScrollCount[Keys.KeyNames[direction]] = 0;
			}
		}

		/// <summary>
		/// Starts a timer that resets scroll wheel count back to 0 and removes it from the display
		/// </summary>
		void StartScrollRemoveTimer() {
			_scrollRemoveTimer = new System.Windows.Forms.Timer();
			_scrollRemoveTimer.Tick += ResetScrolls;
		}

		/// <summary>
		/// reset scroll wheel counters to 0
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void ResetScrolls(object sender, EventArgs e) {
			_scrollRemoveTimer.Stop();
			// zeroed, not removed, because CounterForm compares each counter with its previous value
			foreach (string direction in new List<string>(_currentScrollCount.Keys)) {
				_currentScrollCount[direction] = 0;
			}
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
			_scrollRemoveTimer.Interval = Configuration.ClearDelay;
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
			// stopped with logging off too, or a press while logging is back on would time from this one
			TimeSpan time = _stopwatches.KeyPressStop(e.Key, e.LateBy);
			if (Configuration.LogButtons) {
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
				lock (_statsLock) {
					_clicksChanged = true;
				}
			}
			SetButtonAsPressed((int)e.Button, true);
		}

		/// <summary>
		/// mouse up
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnMouseUp(object sender, MouseEventArgs e) {
			// stopped with logging off too, or a press while logging is back on would time from this one
			TimeSpan time = _stopwatches.KeyPressStop((int)e.Button, e.LateBy);
			if (Configuration.LogButtons) {
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
			_buttonsChanged = true; // count.log and duration.log hold the movement totals

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
			_movementChanged = true;

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
		/// Copies of the statistics for one save. A group that isn't being saved is null
		/// </summary>
		class LogSnapshot {
			public Dictionary<int, uint> Count; // count and duration are saved together, like their setting
			public Dictionary<int, TimeSpan> Duration;
			public List<KeyValuePair<int, TimeSpan>> MousePos;
			public List<KeyValuePair<int, int>> MouseInteractions;
		}

		/// <summary>
		/// Saves the logs in the background
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnAutosaveTimerTick(object sender, EventArgs e) {
			SaveFilesInBackground();
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
				BackupLog(Configuration.CountPath);
				BackupLog(Configuration.DurationPath);
			}
			if (Configuration.LogClicks) {
				BackupLog(Configuration.InteractionPath);
			}
			if (Configuration.LogMovement) {
				BackupLog(Configuration.MousePath);
			}
		}

		/// <summary>
		/// Copies a log to its backup, unless lines of it couldn't be read. Then the backup is the last copy that may still have them
		/// </summary>
		/// <param name="path">the log</param>
		void BackupLog(string path) {
			if (!_damagedLogs.Contains(path)) {
				File.Copy(path, Path.ChangeExtension(path, ".log.bak"), true);
			}
		}

		/// <summary>
		/// Saves the logs on another thread, so the hooks don't wait for the disk. The statistics are copied first, on this thread.
		/// Does nothing while the previous save is still writing. What changed since then stays marked, so the next save gets it
		/// </summary>
		void SaveFilesInBackground() {
			if (_saving != null && !_saving.IsCompleted) {
				return;
			}
			LogSnapshot snapshot = TakeSnapshot();
			if (snapshot == null) {
				return;
			}
			_saving = Task.Run(() => {
				try {
					WriteSnapshot(snapshot);
				}
				catch (Exception e) {
					// nothing would report an error left in the task
					Log.Write("Couldn't save the logs: " + e);
					MarkUnsaved(snapshot.Count != null, snapshot.MousePos != null, snapshot.MouseInteractions != null);
				}
			});
		}

		/// <summary>
		/// Saves the logs on this thread, once a save in the background is done, so the two never write the same file
		/// </summary>
		public void SaveFiles() {
			_saving?.Wait();
			LogSnapshot snapshot = TakeSnapshot();
			if (snapshot != null) {
				WriteSnapshot(snapshot);
			}
		}

		/// <summary>
		/// Copies each group of statistics that's being logged and changed since it was last saved, and marks it as saved
		/// </summary>
		/// <returns>the copies, or null if there's nothing to save</returns>
		LogSnapshot TakeSnapshot() {
			LogSnapshot snapshot = new LogSnapshot();
			lock (_statsLock) {
				if (Configuration.LogButtons && _buttonsChanged) {
					snapshot.Count = new Dictionary<int, uint>(_inputsCount);
					snapshot.Duration = new Dictionary<int, TimeSpan>(_inputsDuration);
					_buttonsChanged = false;
				}
				// lists, because copying a map's entries is cheaper than building a new map from them
				if (Configuration.LogMovement && _movementChanged) {
					snapshot.MousePos = new List<KeyValuePair<int, TimeSpan>>(_mousePos);
					_movementChanged = false;
				}
				if (Configuration.LogClicks && _clicksChanged) {
					snapshot.MouseInteractions = new List<KeyValuePair<int, int>>(_mouseInteractions);
					_clicksChanged = false;
				}
			}
			if (snapshot.Count == null && snapshot.MousePos == null && snapshot.MouseInteractions == null) {
				return null;
			}
			return snapshot;
		}

		/// <summary>
		/// Writes the logs a snapshot holds. A group that couldn't be written is marked as changed, so the next save tries it again
		/// </summary>
		/// <param name="snapshot">the copies to write</param>
		void WriteSnapshot(LogSnapshot snapshot) {
			bool buttonsSaved = true;
			bool movementSaved = true;
			bool clicksSaved = true;
			if (snapshot.Count != null) {
				buttonsSaved = WriteLog(Configuration.CountPath, sw => {
					for (int key = 0; key <= 0xFF; key++) {
						sw.WriteLine(string.Format("{0:X2} {1}", key, snapshot.Count[key]));
					}
				});
				buttonsSaved &= WriteLog(Configuration.DurationPath, sw => {
					for (int key = 0; key <= 0xFF; key++) {
						sw.WriteLine(string.Format("{0:X2} {1}", key, snapshot.Duration[key]));
					}
				});
			}
			if (snapshot.MousePos != null) {
				movementSaved = WriteLog(Configuration.MousePath, sw => {
					foreach (KeyValuePair<int, TimeSpan> cell in snapshot.MousePos) {
						sw.WriteLine(string.Format("{0:X8} {1:X}", cell.Key, cell.Value.Ticks));
					}
				});
			}
			if (snapshot.MouseInteractions != null) {
				clicksSaved = WriteLog(Configuration.InteractionPath, sw => {
					foreach (KeyValuePair<int, int> cell in snapshot.MouseInteractions) {
						sw.WriteLine(string.Format("{0:X8} {1:X}", cell.Key, cell.Value));
					}
				});
			}
			MarkUnsaved(!buttonsSaved, !movementSaved, !clicksSaved);
		}

		/// <summary>
		/// Marks groups of statistics as changed, because their save failed
		/// </summary>
		/// <param name="buttons">count and duration</param>
		/// <param name="movement">the movement map</param>
		/// <param name="clicks">the click map</param>
		void MarkUnsaved(bool buttons, bool movement, bool clicks) {
			lock (_statsLock) {
				_buttonsChanged |= buttons;
				_movementChanged |= movement;
				_clicksChanged |= clicks;
			}
		}

		/// <summary>
		/// Writes a log to a temporary file and swaps it in, so a save that fails partway can't cut the real file short
		/// </summary>
		/// <param name="path">the log</param>
		/// <param name="write">writes the log's lines</param>
		/// <returns>whether it was written. If not, the data is still in memory for the next save</returns>
		bool WriteLog(string path, Action<StreamWriter> write) {
			string tempPath = path + ".tmp";
			try {
				using (StreamWriter sw = new StreamWriter(tempPath)) {
					write(sw);
				}
				if (!File.Exists(path)) {
					File.Move(tempPath, path);
				}
				else {
					File.Replace(tempPath, path, null);
				}
				return true;
			}
			catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) {
				// such as something having the log open, or a read-only file
				Log.Write("Couldn't update " + Path.GetFileName(path) + ": " + e.Message);
				return false;
			}
		}

		/// <summary>
		/// Read saved files
		/// </summary>
		void LoadFiles() {
			ReadLog(Configuration.CountPath, line => {
				string keyText, countText;
				int key;
				uint count;
				if (!SplitLogLine(line, out keyText, out countText) || !ParseKeyCode(keyText, out key) || !uint.TryParse(countText, out count)) {
					return false;
				}
				S.Dictionaries.IncrementValue(_inputsCount, key, count);
				return true;
			});
			for (int key = 0; key <= 0xFF; key++) {
				if (!_inputsCount.ContainsKey(key)) {
					S.Dictionaries.IncrementValue(_inputsCount, key, 0);
				}
			}

			ReadLog(Configuration.DurationPath, line => {
				string keyText, durationText;
				int key;
				TimeSpan duration;
				if (!SplitLogLine(line, out keyText, out durationText) || !ParseKeyCode(keyText, out key) || !TimeSpan.TryParse(durationText, out duration)) {
					return false;
				}
				S.Dictionaries.IncrementValue(_inputsDuration, key, duration);
				return true;
			});
			for (int key = 0; key <= 0xFF; key++) {
				if (!_inputsDuration.ContainsKey(key)) {
					S.Dictionaries.IncrementValue(_inputsDuration, key, TimeSpan.Zero);
				}
			}

			ReadLog(Configuration.MousePath, line => {
				string coordText, ticksText;
				int coord;
				long ticks;
				if (!SplitLogLine(line, out coordText, out ticksText) || !ParseHex(coordText, out coord) || !long.TryParse(ticksText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ticks)) {
					return false;
				}
				S.Dictionaries.IncrementValue(_mousePos, coord, TimeSpan.FromTicks(ticks));
				return true;
			});

			ReadLog(Configuration.InteractionPath, line => {
				string coordText, countText;
				int coord;
				int count;
				if (!SplitLogLine(line, out coordText, out countText) || !ParseHex(coordText, out coord) || !ParseHex(countText, out count)) {
					return false;
				}
				S.Dictionaries.IncrementValue(_mouseInteractions, coord, count);
				return true;
			});

			SaveFiles();
			BackupSave();
		}

		/// <summary>
		/// Reads a log line by line. A line that can't be read is skipped, and the file as it was is kept with .bad added to its name,
		/// because the save after loading replaces it with only the lines that could be read
		/// </summary>
		/// <param name="path">the log</param>
		/// <param name="readLine">adds one line to the statistics, and returns whether it could</param>
		void ReadLog(string path, Func<string, bool> readLine) {
			if (!File.Exists(path)) {
				return;
			}
			int lineNumber = 0;
			int badLines = 0;
			int firstBadLine = 0;
			foreach (string line in File.ReadLines(path)) {
				lineNumber++;
				if (string.IsNullOrWhiteSpace(line)) {
					continue; // holds nothing to lose
				}
				if (!readLine(line)) {
					badLines++;
					if (firstBadLine == 0) {
						firstBadLine = lineNumber;
					}
				}
			}
			if (badLines == 0) {
				return;
			}

			_damagedLogs.Add(path);
			string name = Path.GetFileName(path);
			string kept;
			try {
				File.Copy(path, path + ".bad", true);
				kept = string.Format("The file as it was is kept as {0}.bad.", name);
			}
			catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) {
				kept = string.Format("Couldn't keep the file as it was as {0}.bad: {1}", name, e.Message);
			}
			Log.Write(string.Format(
				"Skipped {0} of the lines in {1}, which couldn't be read. The first was line {2}. {3} {1}.bak wasn't replaced",
				badLines, name, firstBadLine, kept
			));
		}

		/// <summary>
		/// Splits a log line into its key and its value, which a space separates
		/// </summary>
		/// <param name="line">the line</param>
		/// <param name="key">the text before the first space</param>
		/// <param name="value">the text after it</param>
		/// <returns>whether the line has a space</returns>
		static bool SplitLogLine(string line, out string key, out string value) {
			int space = line.IndexOf(' ');
			if (space < 0) {
				key = null;
				value = null;
				return false;
			}
			key = line.Substring(0, space);
			value = line.Substring(space + 1);
			return true;
		}

		/// <summary>
		/// Reads a hex number. Its bits are kept as they are, so FFFFFFFF is -1
		/// </summary>
		/// <param name="text">the hex digits</param>
		/// <param name="value">the number</param>
		/// <returns>whether it could be read</returns>
		static bool ParseHex(string text, out int value) {
			return int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
		}

		/// <summary>
		/// Reads a key code, 00 to FF in hex
		/// </summary>
		/// <param name="text">the hex digits</param>
		/// <param name="key">the key code</param>
		/// <returns>whether it's a key code</returns>
		static bool ParseKeyCode(string text, out int key) {
			return ParseHex(text, out key) && key >= 0 && key <= 0xFF;
		}

		#endregion

	}
}
