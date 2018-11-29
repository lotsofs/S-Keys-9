using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputF8 {
	class Stopwatches {

		static Dictionary<int, Stopwatch> _inputsStopwatches = new Dictionary<int, Stopwatch>();
		static Stopwatch _mouseStopwatch = new Stopwatch();

		/// <summary>
		/// starts the stopwatch that tracks how long the mouse isn't moving for
		/// </summary>
		public static void MouseStart() {
			_mouseStopwatch.Restart();
		}

		/// <summary>
		/// stops the stopwatch that tracks how long the mouse isn't moving for
		/// </summary>
		/// <returns></returns>
		public static TimeSpan MouseStop() {
			_mouseStopwatch.Stop();
			return _mouseStopwatch.Elapsed;
		}

		/// <summary>
		/// starts a stopwatch that tracks the duration of a keypress
		/// </summary>
		/// <param name="key">the key the stopwatch is for</param>
		public static void KeyPressStart(int key) {
			if (!_inputsStopwatches.ContainsKey(key)) {
				_inputsStopwatches.Add(key, new Stopwatch());
				_inputsStopwatches[key].Restart();
			}
		}

		/// <summary>
		/// stops a stopwatch that tracks the duration of a keypress
		/// </summary>
		/// <param name="key">the key the stopwatch is for</param>
		public static TimeSpan KeyPressStop(int key) {
			if (_inputsStopwatches.ContainsKey(key)) {
				_inputsStopwatches[key].Stop();
				TimeSpan ts = _inputsStopwatches[key].Elapsed;
				_inputsStopwatches.Remove(key);
				return ts;
			}
			return TimeSpan.Zero;
		}



	}
}
