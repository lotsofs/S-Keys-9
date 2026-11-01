using System;
using System.Diagnostics;
using System.IO;

namespace SKeys9 {
	static class Log {
		static readonly object _fileLock = new object();

		/// <summary>
		/// Adds a timestamped line to debuglog.log. Never throws, because it also runs while the app is already failing
		/// </summary>
		/// <param name="message">what happened</param>
		internal static void Write(string message) {
			try {
				string line = string.Format("{0:yyyy-MM-dd HH:mm:ss} {1}{2}", DateTime.Now, message, Environment.NewLine);
				lock (_fileLock) {
					File.AppendAllText(Configuration.DebugLogPath, line);
				}
			}
			catch (Exception e) {
				Debug.WriteLine(e);
			}
		}
	}
}
