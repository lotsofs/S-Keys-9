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

		public static void MouseStart() {
			_mouseStopwatch.Restart();
		}

		public static TimeSpan MouseStop() {
			_mouseStopwatch.Stop();
			return _mouseStopwatch.Elapsed;
		}

		/// <summary>
		/// starts a stopwatch that tracks the duration of a keypress
		/// </summary>
		/// <param name="key"></param>
		public static void Start(int key) {
			if (!_inputsStopwatches.ContainsKey(key)) {
				_inputsStopwatches.Add(key, new Stopwatch());
				_inputsStopwatches[key].Restart();
			}
		}

		/// <summary>
		/// stops a stopwatch that tracks the duration of a keypress
		/// </summary>
		/// <param name="key"></param>
		public static TimeSpan Stop(int key) {
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
