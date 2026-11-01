﻿using System;
using System.Threading;
using System.Windows.Forms;

namespace SKeys9 {
	static class Program {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			Configuration.SetDirectories();
			Application.ThreadException += OnThreadException;
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}

		/// <summary>
		/// An error on the UI thread. Handling this event replaces the standard error dialog, so this shows it
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static void OnThreadException(object sender, ThreadExceptionEventArgs e) {
			Log.Write("Error: " + e.Exception);
			using (ThreadExceptionDialog dialog = new ThreadExceptionDialog(e.Exception)) {
				if (dialog.ShowDialog() == DialogResult.Abort) {
					Application.Exit();
				}
			}
		}

		/// <summary>
		/// An error during startup or on a background thread. The app still fails afterwards
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
			Log.Write("Fatal error: " + e.ExceptionObject);
		}
	}
}
