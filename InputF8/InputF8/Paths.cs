using System;
using System.Diagnostics;
using System.IO;

namespace InputF8 {
	class Paths {
		internal static string DirectoryPath;
		internal static string CountPath;
		internal static string DurationPath;
		internal static string MousePath;
		internal static string InteractionPath;

		/// <summary>
		/// Set directory paths and create them if they don't exist
		/// </summary>
		internal static void SetDirectories() {
			DirectoryPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName); // Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"S\SKeys9\"); //@"S\SKeys9\"); //@"S\inputf8\");
			CountPath = Path.Combine(DirectoryPath, "count.log");
			DurationPath = Path.Combine(DirectoryPath, "duration.log"); 
			MousePath = Path.Combine(DirectoryPath, "mousemove2map.log");
			InteractionPath = Path.Combine(DirectoryPath, "mouseinteraction2map.log");

			if (!Directory.Exists(DirectoryPath)) {
				Directory.CreateDirectory(DirectoryPath);
			}
		}
	}
}
