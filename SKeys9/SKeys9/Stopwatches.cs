using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SKeys9 {
	class Stopwatches {
		Dictionary<int, Stopwatch> _inputsStopwatches = new Dictionary<int, Stopwatch>();

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
