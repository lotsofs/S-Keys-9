using System;
using System.Collections.Generic;
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

		internal static string Name = "Microsoft Sans Serif";
		internal static float Size = 18;	// point size
		internal static int Style = 1;  //cast as FontStyle class
		internal static int Color = unchecked((int)0xffffffff);
		internal static int BackColor = unchecked((int)0xff000000);

		static Dictionary<string, string> serializableSettings = new Dictionary<string, string>();

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

		internal static void ReadSettings() {
			// read from the file
			if (!File.Exists(Configuration.SettingsPath)) {
				return;
			}
			IEnumerable<string> settings = File.ReadLines(Configuration.SettingsPath);
			foreach (string setting in settings) {
				for (int i = 0; i < setting.Length; i++) {
					if (setting[i] == '=') {
						MathS.AddStringToDictionary(serializableSettings, setting.Substring(0, i), setting.Substring(i + 1));
						break;
					}
				}
			}
			LoadSettings();
			SaveSettings();
		}

		internal static void LoadSettings() {
			foreach (string setting in serializableSettings.Keys) {
				switch (setting) {
					default:
						break;
					case "Name":
						Configuration.Name = serializableSettings[setting];
						break;
					case "Size":
						Configuration.Size = int.Parse(serializableSettings[setting]);
						break;
					case "Style":
						Configuration.Style = int.Parse(serializableSettings[setting]);
						break;
					case "Color":
						int rIndex = serializableSettings[setting].Length - 6;
						Configuration.Color = unchecked(int.Parse(serializableSettings[setting].Substring(rIndex), System.Globalization.NumberStyles.HexNumber));
						Configuration.Color |= unchecked((int)0xff000000);
						break;
					case "BackColor":
						int rIndexB = serializableSettings[setting].Length - 6;
						Configuration.BackColor = unchecked(int.Parse(serializableSettings[setting].Substring(rIndexB), System.Globalization.NumberStyles.HexNumber));
						Configuration.BackColor |= unchecked((int)0xff000000);
						break;
					case "MinimizeToTray":
						Debug.WriteLine(serializableSettings[setting]);
						Configuration.MinimizeToTray = bool.Parse(serializableSettings[setting]);
						break;
					case "ExitToTray":
						Configuration.ExitToTray = bool.Parse(serializableSettings[setting]);
						break;
				}
			}
		}

		internal static void ApplySettings() {
			MathS.AddStringToDictionary(serializableSettings, "Name", Configuration.Name);
			MathS.AddStringToDictionary(serializableSettings, "Size", Configuration.Size.ToString());
			MathS.AddStringToDictionary(serializableSettings, "Style", Configuration.Style.ToString());
			MathS.AddStringToDictionary(serializableSettings, "Color", Configuration.Color.ToString("X6").Substring(2));
			MathS.AddStringToDictionary(serializableSettings, "BackColor", Configuration.BackColor.ToString("X6").Substring(2));
			MathS.AddStringToDictionary(serializableSettings, "MinimizeToTray", Configuration.MinimizeToTray.ToString());
			MathS.AddStringToDictionary(serializableSettings, "ExitToTray", Configuration.ExitToTray.ToString());
			SaveSettings();
		}

		internal static void SaveSettings() {
			using (StreamWriter sw = new StreamWriter(Configuration.SettingsPath)) {
				foreach (string setting in serializableSettings.Keys) {
					sw.WriteLine(string.Format("{0}={1}", setting, serializableSettings[setting]));
				}
			}
		}
	}
}
