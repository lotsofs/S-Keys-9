using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SKeys9 {
	class Configuration {
		internal static string DirectoryPath;
		internal static string CountPath;
		internal static string DurationPath;
		internal static string MousePath;
		internal static string InteractionPath;
		internal static string SettingsPath;
		internal static string DebugLogPath;
		internal static string KeyNamesPath;

		internal static bool MinimizeToTray = true;
		internal static bool ExitToTray = false;

		internal static bool KeepKeysInPlace = true;
		internal static bool FillKeyGaps = true;
		internal static bool GreyOutReleasedKeys = true;

		internal const int MinClearDelay = 1;
		internal const int MaxClearDelay = 60000;
		internal static int ClearDelay = 300;	// milliseconds that released keys and scroll counters stay
		internal static int FadeOpacity = 50;	// percent of the way from the background colour to the text colour
		internal static bool UseFadeColor = false;
		internal static int FadeColor = unchecked((int)0xff808080);

		internal static bool LogButtons = false;
		internal static bool LogClicks = false;

		static bool _logMovement = false;
		internal static bool LogMovement {
			get {
				return _logMovement;
			}
			set {
				if (value != _logMovement) {
					_logMovement = value;
					LogMovementChanges++;
				}
			}
		}

		/// <summary>
		/// How many times LogMovement has changed, so the mouse move thread can tell that logging was interrupted
		/// </summary>
		internal static int LogMovementChanges {
			get; private set;
		}

		internal static string Name = "Microsoft Sans Serif";
		internal static float Size = 18;	// point size
		internal static int Style = 1;  //cast as FontStyle struct
		internal static int Color = unchecked((int)0xffffffff);
		internal static int BackColor = unchecked((int)0xff000000);

		internal static string CounterName = "Microsoft Sans Serif";
		internal static float CounterSize = 10;    // point size
		internal static int CounterStyle = 1;  //cast as FontStyle struct
		internal static int CounterColor = unchecked((int)0xffffffff);
		internal static int CounterBackColor = unchecked((int)0xff000000);

		internal const int MinCounterColumns = 1;
		internal const int MaxCounterColumns = 10;
		internal static int CounterColumns = 1;
		internal static bool CounterFillAcross = false;

		internal const int MinCounterWindowSize = 1;
		internal const int MaxCounterWindowSize = 10000;
		internal static int CounterWindowWidth = 292;	// pixels inside the window, without its title bar and borders
		internal static int CounterWindowHeight = 1007;
		internal static bool CounterSizeLocked = false;


		static Dictionary<string, string> serializableSettings = new Dictionary<string, string>();

		/// <summary>
		/// Set directory paths and create them if they don't exist
		/// </summary>
		internal static void SetDirectories() {
			DirectoryPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName); // Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"S\SKeys9\"); //@"S\SKeys9\"); //@"S\inputf8\");
			CountPath = Path.Combine(DirectoryPath, "count.log");
			DurationPath = Path.Combine(DirectoryPath, "duration.log"); 
			MousePath = Path.Combine(DirectoryPath, "mousemovemap.log");
			InteractionPath = Path.Combine(DirectoryPath, "mouseinteractionmap.log");
			SettingsPath = Path.Combine(DirectoryPath, "skeys9.ini");
			DebugLogPath = Path.Combine(DirectoryPath, "debuglog.log");
			KeyNamesPath = Path.Combine(DirectoryPath, "KeyNames.json");

			if (!Directory.Exists(DirectoryPath)) {
				Directory.CreateDirectory(DirectoryPath);
			}
		}

		/// <summary>
		/// Read the settings from the file
		/// </summary>
		internal static void ReadSettings() {
			// read from the file
			if (!File.Exists(Configuration.SettingsPath)) {
				return;
			}
			IEnumerable<string> settings = File.ReadLines(Configuration.SettingsPath);
			foreach (string setting in settings) {
				for (int i = 0; i < setting.Length; i++) {
					if (setting[i] == '=') {
						S.Dictionaries.SetValue(serializableSettings, setting.Substring(0, i), setting.Substring(i + 1));
						break;
					}
				}
			}
			LoadSettings();
			SaveSettings(true);
		}

		/// <summary>
		/// Applies loaded settings to the tool. A value that can't be used leaves its setting as it was, and is logged
		/// </summary>
		internal static void LoadSettings() {
			string name = Configuration.Name;
			float size = Configuration.Size;
			int style = Configuration.Style;
			string counterName = Configuration.CounterName;
			float counterSize = Configuration.CounterSize;
			int counterStyle = Configuration.CounterStyle;

			foreach (string setting in serializableSettings.Keys) {
				Debug.WriteLine(setting);
				switch (setting) {
					default:
						break;
					case "Name":
						Configuration.Name = serializableSettings[setting];
						break;
					case "Size":
						Configuration.Size = ReadFloat(setting, Configuration.Size);
						break;
					case "Style":
						Configuration.Style = ReadInt(setting, Configuration.Style);
						break;
					case "Color":
						Configuration.Color = ReadColor(setting, Configuration.Color);
						break;
					case "BackColor":
						Configuration.BackColor = ReadColor(setting, Configuration.BackColor);
						break;
					case "CounterName":
						Configuration.CounterName = serializableSettings[setting];
						break;
					case "CounterSize":
						Configuration.CounterSize = ReadFloat(setting, Configuration.CounterSize);
						break;
					case "CounterStyle":
						Configuration.CounterStyle = ReadInt(setting, Configuration.CounterStyle);
						break;
					case "CounterColor":
						Configuration.CounterColor = ReadColor(setting, Configuration.CounterColor);
						break;
					case "CounterBackColor":
						Configuration.CounterBackColor = ReadColor(setting, Configuration.CounterBackColor);
						break;
					case "CounterColumns":
						Configuration.CounterColumns = Math.Min(Math.Max(ReadInt(setting, Configuration.CounterColumns), MinCounterColumns), MaxCounterColumns);
						break;
					case "CounterFillAcross":
						Configuration.CounterFillAcross = ReadBool(setting, Configuration.CounterFillAcross);
						break;
					case "CounterWindowWidth":
						Configuration.CounterWindowWidth = Math.Min(Math.Max(ReadInt(setting, Configuration.CounterWindowWidth), MinCounterWindowSize), MaxCounterWindowSize);
						break;
					case "CounterWindowHeight":
						Configuration.CounterWindowHeight = Math.Min(Math.Max(ReadInt(setting, Configuration.CounterWindowHeight), MinCounterWindowSize), MaxCounterWindowSize);
						break;
					case "CounterSizeLocked":
						Configuration.CounterSizeLocked = ReadBool(setting, Configuration.CounterSizeLocked);
						break;
					case "MinimizeToTray":
						Configuration.MinimizeToTray = ReadBool(setting, Configuration.MinimizeToTray);
						break;
					case "ExitToTray":
						Configuration.ExitToTray = ReadBool(setting, Configuration.ExitToTray);
						break;
					case "KeepKeysInPlace":
						Configuration.KeepKeysInPlace = ReadBool(setting, Configuration.KeepKeysInPlace);
						break;
					case "FillKeyGaps":
						Configuration.FillKeyGaps = ReadBool(setting, Configuration.FillKeyGaps);
						break;
					case "GreyOutReleasedKeys":
						Configuration.GreyOutReleasedKeys = ReadBool(setting, Configuration.GreyOutReleasedKeys);
						break;
					case "ClearDelay":
						// a timer can't take 0, and the settings window's box can't show anything outside the range
						int clearDelay = ReadInt(setting, Configuration.ClearDelay);
						Configuration.ClearDelay = Math.Min(Math.Max(clearDelay, MinClearDelay), MaxClearDelay);
						break;
					case "FadeOpacity":
						Configuration.FadeOpacity = Math.Min(Math.Max(ReadInt(setting, Configuration.FadeOpacity), 0), 100);
						break;
					case "UseFadeColor":
						Configuration.UseFadeColor = ReadBool(setting, Configuration.UseFadeColor);
						break;
					case "FadeColor":
						Configuration.FadeColor = ReadColor(setting, Configuration.FadeColor);
						break;
					case "LogButtons":
						Configuration.LogButtons = ReadBool(setting, Configuration.LogButtons);
						break;
					case "LogClicks":
						Configuration.LogClicks = ReadBool(setting, Configuration.LogClicks);
						break;
					case "LogMovement":
						Configuration.LogMovement = ReadBool(setting, Configuration.LogMovement);
						break;
				}
			}

			CheckFont("", ref Configuration.Name, ref Configuration.Size, ref Configuration.Style, name, size, style);
			CheckFont("Counter", ref Configuration.CounterName, ref Configuration.CounterSize, ref Configuration.CounterStyle, counterName, counterSize, counterStyle);
		}

		#region reading settings

		/// <summary>
		/// Reads a true or false setting
		/// </summary>
		/// <param name="setting">the setting's name</param>
		/// <param name="current">what it stays if the value can't be read</param>
		/// <returns>the value</returns>
		static bool ReadBool(string setting, bool current) {
			bool value;
			if (bool.TryParse(serializableSettings[setting], out value)) {
				return value;
			}
			LogBadSetting(setting, current.ToString());
			return current;
		}

		/// <summary>
		/// Reads a whole number setting
		/// </summary>
		/// <param name="setting">the setting's name</param>
		/// <param name="current">what it stays if the value can't be read</param>
		/// <returns>the value</returns>
		static int ReadInt(string setting, int current) {
			int value;
			if (int.TryParse(serializableSettings[setting], out value)) {
				return value;
			}
			LogBadSetting(setting, current.ToString());
			return current;
		}

		/// <summary>
		/// Reads a number setting, in the same culture that ApplySettings writes it in
		/// </summary>
		/// <param name="setting">the setting's name</param>
		/// <param name="current">what it stays if the value can't be read</param>
		/// <returns>the value</returns>
		static float ReadFloat(string setting, float current) {
			float value;
			if (float.TryParse(serializableSettings[setting], out value)) {
				return value;
			}
			LogBadSetting(setting, current.ToString());
			return current;
		}

		/// <summary>
		/// Reads a colour setting from its last 6 hex digits, so that a # in front doesn't matter. Alpha is always ff
		/// </summary>
		/// <param name="setting">the setting's name</param>
		/// <param name="current">what it stays if the value can't be read</param>
		/// <returns>the colour</returns>
		static int ReadColor(string setting, int current) {
			string text = serializableSettings[setting];
			int value;
			if (text.Length >= 6 && int.TryParse(text.Substring(text.Length - 6), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out value)) {
				return value | unchecked((int)0xff000000);
			}
			LogBadSetting(setting, current.ToString("X6").Substring(2));
			return current;
		}

		/// <summary>
		/// Logs a setting whose value can't be used
		/// </summary>
		/// <param name="setting">the setting's name</param>
		/// <param name="current">the value it stays, as the file would hold it</param>
		static void LogBadSetting(string setting, string current) {
			Log.Write(string.Format("Couldn't use {0}={1} in skeys9.ini, so it stays {2}", setting, serializableSettings[setting], current));
		}

		/// <summary>
		/// Puts a font's settings back as they were before loading if no font can be made from them,
		/// such as with a size of 0 or a style that the font doesn't have
		/// </summary>
		/// <param name="prefix">what the font's settings start with in the file</param>
		/// <param name="name">the loaded name</param>
		/// <param name="size">the loaded size</param>
		/// <param name="style">the loaded style</param>
		/// <param name="oldName">the name before loading</param>
		/// <param name="oldSize">the size before loading</param>
		/// <param name="oldStyle">the style before loading</param>
		static void CheckFont(string prefix, ref string name, ref float size, ref int style, string oldName, float oldSize, int oldStyle) {
			try {
				using (new System.Drawing.Font(name, size, (System.Drawing.FontStyle)style)) { }
			}
			catch (ArgumentException e) {
				Log.Write(string.Format(
					"Couldn't make a font from {0}Name={1}, {0}Size={2} and {0}Style={3} in skeys9.ini, so they stay {4}, {5} and {6}. {7}",
					prefix, name, size, style, oldName, oldSize, oldStyle, e.Message.Replace(Environment.NewLine, " ")
				));
				name = oldName;
				size = oldSize;
				style = oldStyle;
			}
		}

		#endregion

		/// <summary>
		/// Apply settings in tool
		/// </summary>
		internal static void ApplySettings() {
			S.Dictionaries.SetValue(serializableSettings, "Name", Configuration.Name);
			S.Dictionaries.SetValue(serializableSettings, "Size", Configuration.Size.ToString());
			S.Dictionaries.SetValue(serializableSettings, "Style", Configuration.Style.ToString());
			S.Dictionaries.SetValue(serializableSettings, "Color", Configuration.Color.ToString("X6").Substring(2));
			S.Dictionaries.SetValue(serializableSettings, "BackColor", Configuration.BackColor.ToString("X6").Substring(2));
			S.Dictionaries.SetValue(serializableSettings, "CounterName", Configuration.CounterName);
			S.Dictionaries.SetValue(serializableSettings, "CounterSize", Configuration.CounterSize.ToString());
			S.Dictionaries.SetValue(serializableSettings, "CounterStyle", Configuration.CounterStyle.ToString());
			S.Dictionaries.SetValue(serializableSettings, "CounterColor", Configuration.CounterColor.ToString("X6").Substring(2));
			S.Dictionaries.SetValue(serializableSettings, "CounterBackColor", Configuration.CounterBackColor.ToString("X6").Substring(2));
			S.Dictionaries.SetValue(serializableSettings, "CounterColumns", Configuration.CounterColumns.ToString());
			S.Dictionaries.SetValue(serializableSettings, "CounterFillAcross", Configuration.CounterFillAcross.ToString());
			S.Dictionaries.SetValue(serializableSettings, "CounterWindowWidth", Configuration.CounterWindowWidth.ToString());
			S.Dictionaries.SetValue(serializableSettings, "CounterWindowHeight", Configuration.CounterWindowHeight.ToString());
			S.Dictionaries.SetValue(serializableSettings, "CounterSizeLocked", Configuration.CounterSizeLocked.ToString());
			S.Dictionaries.SetValue(serializableSettings, "MinimizeToTray", Configuration.MinimizeToTray.ToString());
			S.Dictionaries.SetValue(serializableSettings, "ExitToTray", Configuration.ExitToTray.ToString());
			S.Dictionaries.SetValue(serializableSettings, "KeepKeysInPlace", Configuration.KeepKeysInPlace.ToString());
			S.Dictionaries.SetValue(serializableSettings, "FillKeyGaps", Configuration.FillKeyGaps.ToString());
			S.Dictionaries.SetValue(serializableSettings, "GreyOutReleasedKeys", Configuration.GreyOutReleasedKeys.ToString());
			S.Dictionaries.SetValue(serializableSettings, "ClearDelay", Configuration.ClearDelay.ToString());
			S.Dictionaries.SetValue(serializableSettings, "FadeOpacity", Configuration.FadeOpacity.ToString());
			S.Dictionaries.SetValue(serializableSettings, "UseFadeColor", Configuration.UseFadeColor.ToString());
			S.Dictionaries.SetValue(serializableSettings, "FadeColor", Configuration.FadeColor.ToString("X6").Substring(2));
			S.Dictionaries.SetValue(serializableSettings, "LogButtons", Configuration.LogButtons.ToString());
			S.Dictionaries.SetValue(serializableSettings, "LogClicks", Configuration.LogClicks.ToString());
			S.Dictionaries.SetValue(serializableSettings, "LogMovement", Configuration.LogMovement.ToString());
			SaveSettings();
		}

		/// <summary>
		/// Saves the counter window's size and lock. They change outside the settings window, so this
		/// writes only them, which keeps any settings still being previewed out of the file
		/// </summary>
		internal static void SaveCounterWindow() {
			S.Dictionaries.SetValue(serializableSettings, "CounterWindowWidth", Configuration.CounterWindowWidth.ToString());
			S.Dictionaries.SetValue(serializableSettings, "CounterWindowHeight", Configuration.CounterWindowHeight.ToString());
			S.Dictionaries.SetValue(serializableSettings, "CounterSizeLocked", Configuration.CounterSizeLocked.ToString());
			try {
				SaveSettings();
			}
			catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) {
				Log.Write("Couldn't save the counter window's size: " + e.Message);
			}
		}

		/// <summary>
		/// The colour for released keys: the custom one, or a share of the way from the background to the text colour
		/// </summary>
		/// <returns></returns>
		internal static System.Drawing.Color ReleasedKeyColor() {
			if (UseFadeColor) {
				return System.Drawing.Color.FromArgb(FadeColor);
			}
			System.Drawing.Color text = System.Drawing.Color.FromArgb(Color);
			System.Drawing.Color back = System.Drawing.Color.FromArgb(BackColor);
			return System.Drawing.Color.FromArgb(
				back.R + (text.R - back.R) * FadeOpacity / 100,
				back.G + (text.G - back.G) * FadeOpacity / 100,
				back.B + (text.B - back.B) * FadeOpacity / 100
			);
		}

		/// <summary>
		/// Save settings to file
		/// </summary>
		/// <param name="backupSave"></param>
		internal static void SaveSettings(bool backupSave = false) {
			if (backupSave) {
				using (StreamWriter sw = new StreamWriter(Configuration.SettingsPath + ".bak")) {
					foreach (string setting in serializableSettings.Keys) {
						sw.WriteLine(string.Format("{0}={1}", setting, serializableSettings[setting]));
					}
				}
			}
			else {
				using (StreamWriter sw = new StreamWriter(Configuration.SettingsPath)) {
					foreach (string setting in serializableSettings.Keys) {
						sw.WriteLine(string.Format("{0}={1}", setting, serializableSettings[setting]));
					}
				}
			}
		}
	}
}
