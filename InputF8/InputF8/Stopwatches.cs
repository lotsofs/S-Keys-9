using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SKeys9 {
	class Stopwatches {
		Dictionary<int, Stopwatch> _inputsStopwatches = new Dictionary<int, Stopwatch>();
		Stopwatch _mouseStopwatch = new Stopwatch();

		#region stopwatch for mouse tracking

		/// <summary>
		/// starts the stopwatch that tracks how long the mouse isn't moving for
		/// </summary>
		internal void MouseStart() {
			_mouseStopwatch.Restart();
		}

		/// <summary>
		/// stops the stopwatch that tracks how long the mouse isn't moving for
		/// </summary>
		/// <returns></returns>
		internal TimeSpan MouseStop() {
			_mouseStopwatch.Stop();
			return _mouseStopwatch.Elapsed;
		}

		#endregion

		#region stopwatch for keys

		/// <summary>
		/// starts a stopwatch that tracks the duration of a keypress
		/// </summary>
		/// <param name="key">the key the stopwatch is for</param>
		internal void KeyPressStart(int key) {
			if (!_inputsStopwatches.ContainsKey(key)) {
				_inputsStopwatches.Add(key, new Stopwatch());
				_inputsStopwatches[key].Restart();
			}
		}

		/// <summary>
		/// stops a stopwatch that tracks the duration of a keypress
		/// </summary>
		/// <param name="key">the key the stopwatch is for</param>
		internal TimeSpan KeyPressStop(int key) {
			if (_inputsStopwatches.ContainsKey(key)) {
				_inputsStopwatches[key].Stop();
				TimeSpan ts = _inputsStopwatches[key].Elapsed;
				_inputsStopwatches.Remove(key);
				return ts;
			}
			return TimeSpan.Zero;
		}

		#endregion 
	}
}
