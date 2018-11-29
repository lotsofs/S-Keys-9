using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace InputF8 {
	public class Hooks {
		private const int WH_KEYBOARD_LL = 13;
		private const int WH_MOUSE_LL = 14;

		private const int WM_KEYDOWN = 0x0100;
		private const int WM_KEYUP = 0x0101;
		private const int WM_SYSKEYDOWN = 0x0104;
		private const int WM_SYSKEYUP = 0x0105;

		private const int WM_LBUTTONDOWN = 0x201;
		private const int WM_LBUTTONUP = 0x202;
		private const int WM_RBUTTONDOWN = 0x204;
		private const int WM_RBUTTONUP = 0x205;
		private const int WM_MBUTTONDOWN = 0x207;
		private const int WM_MBUTTONUP = 0x208;
		private const int WM_MOUSEWHEEL = 0x20A;
		private const int WM_XBUTTONDOWN = 0x20B;
		private const int WM_XBUTTONUP = 0x20C;
		private const int WM_MOUSEHWHEEL = 0x20E;
		private const int WM_MOUSEMOVE = 0x200;

		private List<int> _currentlyPressed = new List<int>();

		public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
		public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

		public event EventHandler<KeyEventArgs> OnKeyDown;
		public event EventHandler<KeyEventArgs> OnKeyUp;
		public event EventHandler<MouseEventArgs> OnMouseDown;
		public event EventHandler<MouseEventArgs> OnMouseUp;
		public event EventHandler<ScrollEventArgs> OnMouseScroll;
		public event EventHandler<MoveEventArgs> OnMouseMove;

		Timer _hookTimeoutTimer;

		private LowLevelKeyboardProc _kbProc;
		private IntPtr _kbHookID = IntPtr.Zero;

		private LowLevelMouseProc _mProc;
		private IntPtr _mHookID = IntPtr.Zero;

		#region DLLImports input

		/// <summary>
		/// enable kb hook
		/// </summary>
		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		/// <summary>
		/// enable mouse hook
		/// </summary>
		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

		/// <summary>
		/// disable kb hook
		/// </summary>
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		/// <summary>
		/// Lets the hook be seen by other applications that have hooks as well.
		/// </summary>
		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		#endregion

		#region DLLImports processes

		/// <summary>
		/// 
		/// </summary>
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

		//[DllImport("user32.dll")]
		//public static extern IntPtr GetForegroundWindow();

		//[DllImport("user32.dll", SetLastError = true)]
		//private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		#endregion

		public Hooks() {
			_kbProc = KbHookCallback;
			_mProc = MHookCallback;

			_hookTimeoutTimer = new Timer();
			_hookTimeoutTimer.Interval = 5000; // TODO: Not hardcoded
			_hookTimeoutTimer.Tick += OnHookTimeout;
			_hookTimeoutTimer.Start();
		}

		#region enable/disable

		/// <summary>
		/// Enable the keyboard hook
		/// </summary>
		public void EnableHooks() {
			_kbHookID = SetKbHook(_kbProc);
			_mHookID = SetMHook(_mProc);
		}

		/// <summary>
		/// Disable the keyboard hook
		/// </summary>
		public void DisableHooks() {
			UnhookWindowsHookEx(_kbHookID);
			UnhookWindowsHookEx(_mHookID);
		}

		private IntPtr SetKbHook(LowLevelKeyboardProc proc) {
			using (Process curProcess = Process.GetCurrentProcess())
			using (ProcessModule curModule = curProcess.MainModule) {
				return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		private IntPtr SetMHook(LowLevelMouseProc proc) {
			using (Process curProcess = Process.GetCurrentProcess())
			using (ProcessModule curModule = curProcess.MainModule) {
				return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		#endregion

		/// <summary>
		/// If hook stopped working
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OnHookTimeout(object sender, EventArgs e) {
			_hookTimeoutTimer.Stop();
			_hookTimeoutTimer.Start();
			DisableHooks();
			EnableHooks();
		}

		void MarkMouseButtonAsPressed(MSLLHOOKSTRUCT hookStruct, MouseEventArgs.Buttons button, bool pressed) {
			if (pressed && !_currentlyPressed.Contains((int)button)) {
				_currentlyPressed.Add((int)button);
				OnMouseDown?.Invoke(this, new MouseEventArgs(button, hookStruct.pt.x, hookStruct.pt.y));
			}
			else if (!pressed && _currentlyPressed.Contains((int)button)) {
				_currentlyPressed.Remove((int)button);
				OnMouseUp?.Invoke(this, new MouseEventArgs(button, hookStruct.pt.x, hookStruct.pt.y));
			}
		}

		#region hook callbacks

		/// <summary>
		/// keyhook callback happens.
		/// </summary>
		/// <param name="nCode">not sure</param>
		/// <param name="wParam">event</param>
		/// <param name="lParam">the key being pressed</param>
		/// <returns></returns>
		private IntPtr KbHookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
			if (nCode < 0) {
				return CallNextHookEx(_kbHookID, nCode, wParam, lParam);
			}

			int vkCode = Marshal.ReadInt32(lParam);

			switch ((int)wParam) {
				case WM_KEYDOWN:
				case WM_SYSKEYDOWN:
					if (!_currentlyPressed.Contains(vkCode)) {
						_currentlyPressed.Add(vkCode);
						OnKeyDown?.Invoke(this, new KeyEventArgs(vkCode));
					}
					break;
				case WM_KEYUP:
				case WM_SYSKEYUP:
					if (_currentlyPressed.Contains(vkCode)) {
						_currentlyPressed.Remove(vkCode);
					}
					OnKeyUp?.Invoke(this, new KeyEventArgs(vkCode));
					break;
			}
			_hookTimeoutTimer.Stop();
			_hookTimeoutTimer.Start();

			return CallNextHookEx(_kbHookID, nCode, wParam, lParam);
		}

		/// <summary>
		/// mousehook callback happens.
		/// </summary>
		/// <param name="nCode"></param>
		/// <param name="wParam"></param>
		/// <param name="lParam"></param>
		/// <returns></returns>
		private IntPtr MHookCallback(int nCode, IntPtr wParam, IntPtr lParam) {

			if (nCode < 0) {
				return CallNextHookEx(_kbHookID, nCode, wParam, lParam);
			}

			MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
			switch ((int)wParam) {
				case WM_LBUTTONDOWN:
					MarkMouseButtonAsPressed(hookStruct, MouseEventArgs.Buttons.LMB, true);
					break;
				case WM_RBUTTONDOWN:
					MarkMouseButtonAsPressed(hookStruct, MouseEventArgs.Buttons.RMB, true);
					break;
				case WM_MBUTTONDOWN:
					MarkMouseButtonAsPressed(hookStruct, MouseEventArgs.Buttons.MMB, true);
					break;
				case WM_XBUTTONDOWN:
					switch (((uint)hookStruct.mouseData >> 16 & (uint)ushort.MaxValue)) {
						case 1:
							MarkMouseButtonAsPressed(hookStruct, MouseEventArgs.Buttons.XMB1, true);
							break;
						case 2:
							MarkMouseButtonAsPressed(hookStruct, MouseEventArgs.Buttons.XMB2, true);
							break;
					}
					break;

				case WM_LBUTTONUP:
					MarkMouseButtonAsPressed(hookStruct, MouseEventArgs.Buttons.LMB, false);
					break;
				case WM_RBUTTONUP:
					MarkMouseButtonAsPressed(hookStruct, MouseEventArgs.Buttons.RMB, false);
					break;
				case WM_MBUTTONUP:
					MarkMouseButtonAsPressed(hookStruct, MouseEventArgs.Buttons.MMB, false);
					break;
				case WM_XBUTTONUP:
					switch (((uint)hookStruct.mouseData >> 16 & (uint)ushort.MaxValue)) {
						case 1:
							MarkMouseButtonAsPressed(hookStruct, MouseEventArgs.Buttons.XMB1, false);
							break;
						case 2:
							MarkMouseButtonAsPressed(hookStruct, MouseEventArgs.Buttons.XMB2, false);
							break;
					}
					break;


				case WM_MOUSEWHEEL:
					if (hookStruct.mouseData >> 16 > 0) {
						OnMouseScroll?.Invoke(this, new ScrollEventArgs(ScrollEventArgs.Directions.Up));
					}
					else {
						OnMouseScroll?.Invoke(this, new ScrollEventArgs(ScrollEventArgs.Directions.Down));
					}
					break;
				case WM_MOUSEHWHEEL:
					if (hookStruct.mouseData >> 16 > 0) {
						OnMouseScroll?.Invoke(this, new ScrollEventArgs(ScrollEventArgs.Directions.Right));
					}
					else {
						OnMouseScroll?.Invoke(this, new ScrollEventArgs(ScrollEventArgs.Directions.Left));
					}
					break;


				case WM_MOUSEMOVE:
					OnMouseMove?.Invoke(this, new MoveEventArgs(hookStruct.pt.x, hookStruct.pt.y));
					break;
			}

			return CallNextHookEx(_mHookID, nCode, wParam, lParam);
		}

		#endregion

		#region mouseLParamsStructs
		// totally didn't copy this from online without understanding it

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT {
			public int x;
			public int y;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MSLLHOOKSTRUCT {
			public POINT pt;
			public int mouseData;
			public uint flags;
			public uint time;
			public IntPtr dwExtraInfo;
		}

		#endregion
	}
}