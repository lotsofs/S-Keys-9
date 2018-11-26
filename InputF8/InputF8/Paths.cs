using System;
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
			DirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"S\SKeys9\"); //@"S\SKeys9\"); //@"S\inputf8\");
			CountPath = Path.Combine(DirectoryPath, "count.log");
			DurationPath = Path.Combine(DirectoryPath, "duration.log");
			MousePath = Path.Combine(DirectoryPath, "mousemap.log");
			InteractionPath = Path.Combine(DirectoryPath, "mouseinteractionmap.log");

			if (!Directory.Exists(DirectoryPath)) {
				Directory.CreateDirectory(DirectoryPath);
			}
		}
	}
}
