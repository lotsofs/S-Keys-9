using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace InputF8 {
	class KeyPresses {

		public delegate void KeyReadDelegate(int key);
		public delegate void KeyChangeDelegate();
		public event KeyChangeDelegate KeyChangeEvent;
		public event KeyReadDelegate KeyDownEvent;
		public event KeyReadDelegate KeyUpEvent;

		List<int> pressedKeys = new List<int>();

		#region dll import black magic i don't understand

		[DllImport("User32.dll")]
		public static extern int GetAsyncKeyState(Int32 i);

		public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
		
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		#endregion

		public KeyPresses() {

		}

		public async void ReadKeys() {
			bool listChanged;
			while (true) {
				listChanged = false;
				await Task.Delay(1);
				for (Int32 key = 0; key <= 0xFF; key++) {
				//foreach (int key in Keys.keyNames.Keys) {

					int keyState = GetAsyncKeyState(key);

					// if key isn't pressed
					if (keyState == 0) {
						// check if key is released
						if (pressedKeys.Contains(key)) {
							pressedKeys.Remove(key);
							KeyUpEvent(key);
							listChanged = true;
						}
						continue;
					}
					// if key is pressed
					else {
						if (!pressedKeys.Contains(key)) {
							pressedKeys.Add(key);
							KeyDownEvent(key);
							listChanged = true;
						}
					}
				}
				if (listChanged == true) {
					KeyChangeEvent();
				}
			}
		}

	}
}
