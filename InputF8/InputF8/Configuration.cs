using System;
using System.Diagnostics;
using System.IO;

namespace InputF8 {
	class Configuration {
		internal static string DirectoryPath;
		internal static string CountPath;
		internal static string DurationPath;
		internal static string MousePath;
		internal static string InteractionPath;
		internal static string SettingsPath;

		internal static bool MinimizeToTray = true;
		internal static bool ExitToTray = false;

		internal static string name = "Microsoft Sans Serif";
		internal static int size = 18;	// point size
		internal static int style = 1;  //cast as FontStyle class
		internal static int color = 0xffffff;
		internal static int backColor = 0x000000;

		/// <summary>
		/// Set directory paths and create them if they don't exist
		/// </summary>
		internal static void SetDirectories() {
			DirectoryPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName); // Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"S\SKeys9\"); //@"S\SKeys9\"); //@"S\inputf8\");
			CountPath = Path.Combine(DirectoryPath, "count.log");
			DurationPath = Path.Combine(DirectoryPath, "duration.log"); 
			MousePath = Path.Combine(DirectoryPath, "mousemove2map.log");
			InteractionPath = Path.Combine(DirectoryPath, "mouseinteraction2map.log");
			SettingsPath = Path.Combine(DirectoryPath, "skeys9.ini");

			if (!Directory.Exists(DirectoryPath)) {
				Directory.CreateDirectory(DirectoryPath);
			}
		}

		internal static void LoadSettings() {
			// read from the file
			color = color |= unchecked((int)0xff000000);
			backColor = backColor |= unchecked((int)0xff000000);
		}

		internal static void SaveSettings() {
			// save to the file
		}
	}
}
