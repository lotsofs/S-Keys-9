using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
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

		#region DLLImports

		/// <summary>
		/// enable kb hook
		/// </summary>
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		/// <summary>
		/// enable mouse hook
		/// </summary>
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

		/// <summary>
		/// disable kb hook
		/// </summary>
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		/// <summary>
		/// Lets the hook be seen by other applications that have hooks as well.
		/// </summary>
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		/// <summary>
		/// 
		/// </summary>
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

		#endregion

		public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
		public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

		public event EventHandler<KeyEventArgs> OnKeyDown;
		public event EventHandler<KeyEventArgs> OnKeyUp;
		public event EventHandler<MouseAEventArgs> OnMouseDown;
		public event EventHandler<MouseAEventArgs> OnMouseUp;

		private LowLevelKeyboardProc _kbProc;
		private IntPtr _kbHookID = IntPtr.Zero;

		private LowLevelMouseProc _mProc;
		private IntPtr _mHookID = IntPtr.Zero;


		public Hooks() {
			_kbProc = KbHookCallback;
			_mProc = MHookCallback;
		}

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
					OnKeyDown?.Invoke(this, new KeyEventArgs(KeyInterop.KeyFromVirtualKey(vkCode)));
					break;
				case WM_KEYUP:
				case WM_SYSKEYUP:
					OnKeyUp?.Invoke(this, new KeyEventArgs(KeyInterop.KeyFromVirtualKey(vkCode)));
					break;
			}

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
			Debug.WriteLine(lParam);

			Debug.WriteLine(string.Format("{0}   {1}   {2}   {3}", (short)lParam.ToInt32(), lParam.ToInt32() >> 16, (int)wParam, (int)lParam));
			if (nCode < 0) {
				return CallNextHookEx(_kbHookID, nCode, wParam, lParam);
			}


			switch ((int)wParam) {
				case WM_LBUTTONDOWN:
					OnMouseDown?.Invoke(this, new MouseAEventArgs(MouseAEventArgs.Buttons.LMB));
					break;
				case WM_RBUTTONDOWN:
					OnMouseDown?.Invoke(this, new MouseAEventArgs(MouseAEventArgs.Buttons.RMB));
					break;
				case WM_MBUTTONDOWN:
					OnMouseDown?.Invoke(this, new MouseAEventArgs(MouseAEventArgs.Buttons.MMB));
					break;
				case WM_XBUTTONDOWN:
					OnMouseDown?.Invoke(this, new MouseAEventArgs(MouseAEventArgs.Buttons.MMB));
					break;


				case WM_LBUTTONUP:
					OnMouseUp?.Invoke(this, new MouseAEventArgs(MouseAEventArgs.Buttons.LMB));
					break;
				case WM_RBUTTONUP:
					OnMouseUp?.Invoke(this, new MouseAEventArgs(MouseAEventArgs.Buttons.RMB));
					break;
				case WM_MBUTTONUP:
					OnMouseUp?.Invoke(this, new MouseAEventArgs(MouseAEventArgs.Buttons.MMB));
					break;
				case WM_XBUTTONUP:
					OnMouseUp?.Invoke(this, new MouseAEventArgs(MouseAEventArgs.Buttons.MMB));
					break;


				case WM_MOUSEWHEEL:
					//IntPtr aa = (IntPtr)0xf0000000;
					//uint aa32 = (uint)aa.ToInt64();

					Debug.WriteLine((int)(lParam) & 0xFFFF);
					Debug.WriteLine(((int)(lParam) >> 16) & 0xFFFF);
					
					Debug.WriteLine((unchecked((int)(long)wParam)) >> 16 & 0xffff);
				//	Debug.WriteLine(aa32);
					
					break;
				case WM_MOUSEHWHEEL:
					break;


				case WM_MOUSEMOVE:
					//Debug.WriteLine(string.Format("{0}   {1}   {2}   {3}", (short)lParam.ToInt32(), lParam.ToInt32() >> 16, wParam, lParam));
					break;
			}

			return CallNextHookEx(_mHookID, nCode, wParam, lParam);
		}
	}


}