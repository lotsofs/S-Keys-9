using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InputF8 {
	class KeyPresses {
		
		HashSet<int> ignoredKeys = new HashSet<int> { 0x10, 0x11, 0x12 };    // generic shift/ctrl/alt keycodes

		[DllImport("User32.dll")]
		public static extern int GetAsyncKeyState(Int32 i);

		public async void ReadKeys() {
			while (true) {
				await Task.Delay(1);
				for (Int32 key = 0; key <= 0xDF; key++) {
					if (ignoredKeys.Contains(key)) {
						continue;
					}

					int keyState = GetAsyncKeyState(key);

					if (keyState == 0) {
						continue;
					}

					OnKeyPressed(key);
				}
			}
		}

		public void OnKeyPressed(int key) {
			// print that shit
		}

	}
}
