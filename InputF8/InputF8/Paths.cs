using System;
using System.IO;

namespace InputF8 {
	class Paths {
		internal static string DirectoryPath;
		internal static string CountPath;
		internal static string DurationPath;

		/// <summary>
		/// Set directory paths and create them if they don't exist
		/// </summary>
		internal static void SetDirectories() {
			DirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"S\SKeys9\"); //@"S\SKeys9\"); //@"S\inputf8\");
			CountPath = Path.Combine(DirectoryPath, "count.txt");
			DurationPath = Path.Combine(DirectoryPath, "duration.txt");

			if (!Directory.Exists(DirectoryPath)) {
				Directory.CreateDirectory(DirectoryPath);
			}
		}
	}
}
