using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace SKeys9 {
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

		private const int HookCheckMargin = 100; // ms. Input this recent may still be on its way to a callback

		private const int InReach = -1;
		private const int SecureDesktop = 0; // never a foreground process's id, which fills this slot otherwise

		private const uint DESKTOP_SWITCHDESKTOP = 0x0100;
		private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
		private const uint TOKEN_QUERY = 0x0008;
		private const int TokenElevation = 20;

		private Dictionary<int, HeldInput> _currentlyPressed = new Dictionary<int, HeldInput>();

		public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
		public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

		public event EventHandler<KeyEventArgs> OnKeyDown;
		public event EventHandler<KeyEventArgs> OnKeyUp;
		public event EventHandler<MouseEventArgs> OnMouseDown;
		public event EventHandler<MouseEventArgs> OnMouseUp;
		public event EventHandler<ScrollEventArgs> OnMouseScroll;
		public event EventHandler<MoveEventArgs> OnMouseMove;
		public event EventHandler OnHooksReinstalled;

		Timer _hookCheckTimer;
		uint _lastCallback;
		bool _elevated;
		volatile bool _locked;
		int _outOfReach = InReach;
		int _outOfReachLogged = InReach;

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

		/// <summary>
		/// When the last input arrived, whether or not a hook saw it
		/// </summary>
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

		/// <summary>
		/// Whether Windows has a key or mouse button down, whether or not a hook saw it. Wrong while input is out of reach
		/// </summary>
		[DllImport("user32.dll")]
		private static extern short GetAsyncKeyState(int vKey);

		[StructLayout(LayoutKind.Sequential)]
		private struct LASTINPUTINFO {
			public uint cbSize;
			public uint dwTime;
		}

		#endregion

		#region DLLImports processes

		/// <summary>
		/// 
		/// </summary>
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		/// <summary>
		/// Fails on Windows' secure desktop, which is what the lock screen is on
		/// </summary>
		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, uint dwDesiredAccess);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseDesktop(IntPtr hDesktop);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseHandle(IntPtr hObject);

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetTokenInformation(IntPtr TokenHandle, int TokenInformationClass, out int TokenInformation, int TokenInformationLength, out int ReturnLength);

		#endregion

		public Hooks() {
			_kbProc = KbHookCallback;
			_mProc = MHookCallback;

			_lastCallback = unchecked((uint)Environment.TickCount);

			using (Process curProcess = Process.GetCurrentProcess()) {
				_elevated = IsElevated((uint)curProcess.Id);
			}
			SystemEvents.SessionSwitch += OnSessionSwitch;

			_hookCheckTimer = new Timer();
			_hookCheckTimer.Interval = 1000;
			_hookCheckTimer.Tick += OnHookCheck;
			_hookCheckTimer.Start();
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
		/// Windows can't be asked whether a hook is still installed, so this looks for the symptom:
		/// input newer than the last callback, which neither hook saw. Reinstalls both if there is any,
		/// unless Windows is keeping that input from the hooks, which says nothing about the hooks.
		/// Also lets go of held input whose release the hooks missed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OnHookCheck(object sender, EventArgs e) {
			LASTINPUTINFO lastInput = new LASTINPUTINFO();
			lastInput.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));
			if (!GetLastInputInfo(ref lastInput)) {
				return;
			}

			uint now = unchecked((uint)Environment.TickCount);
			bool settled = unchecked((int)(now - lastInput.dwTime)) >= HookCheckMargin;
			bool unseen = unchecked((int)(lastInput.dwTime - _lastCallback)) >= HookCheckMargin;

			int outOfReach = FindOutOfReach();
			if (outOfReach == InReach) {
				_outOfReachLogged = InReach;
			}
			else if (unseen && outOfReach != _outOfReachLogged) {
				_outOfReachLogged = outOfReach;
				Log.Write("Not seeing input: " + DescribeOutOfReach(outOfReach));
			}

			ReleaseMissedInputs(outOfReach, _outOfReach);

			// the last of that input can land just before the check that finds input back in reach
			bool skipCheck = outOfReach != InReach || _outOfReach != InReach;
			_outOfReach = outOfReach;
			if (skipCheck) {
				_lastCallback = now;
				return;
			}

			if (settled && unseen) {
				_lastCallback = now;
				DisableHooks();
				EnableHooks();
				TrustReleases(); // a release may have come while the hooks were gone
				Log.Write("Reinstalled the hooks: input arrived that neither hook saw");
				OnHooksReinstalled?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Notes the PC being locked. The lock screen starts out on the normal desktop, where FindOutOfReach can't spot it
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnSessionSwitch(object sender, SessionSwitchEventArgs e) {
			if (e.Reason == SessionSwitchReason.SessionLock) {
				_locked = true;
			}
			else if (e.Reason == SessionSwitchReason.SessionUnlock) {
				_locked = false;
			}
		}

		/// <summary>
		/// Looks for what's keeping input from the hooks. Windows hides it from them
		/// on its secure desktop, and while a process with more rights than this one has focus
		/// </summary>
		/// <returns>SecureDesktop, that process's id, or InReach</returns>
		int FindOutOfReach() {
			if (_locked) {
				return SecureDesktop;
			}
			IntPtr desktop = OpenInputDesktop(0, false, DESKTOP_SWITCHDESKTOP);
			if (desktop == IntPtr.Zero) {
				return SecureDesktop;
			}
			CloseDesktop(desktop);

			uint processId;
			if (GetWindowThreadProcessId(GetForegroundWindow(), out processId) != 0 && !_elevated && IsElevated(processId)) {
				return (int)processId;
			}
			return InReach;
		}

		/// <summary>
		/// Whether a process is elevated. One that can't even be looked at counts too, because it's a system process
		/// </summary>
		/// <param name="processId">the process</param>
		/// <returns></returns>
		bool IsElevated(uint processId) {
			IntPtr process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
			if (process == IntPtr.Zero) {
				return true;
			}

			IntPtr token;
			bool opened = OpenProcessToken(process, TOKEN_QUERY, out token);
			CloseHandle(process);
			if (!opened) {
				return true;
			}

			int elevated;
			int length;
			bool read = GetTokenInformation(token, TokenElevation, out elevated, sizeof(int), out length);
			CloseHandle(token);
			return read && elevated != 0;
		}

		/// <summary>
		/// Puts what FindOutOfReach found into words
		/// </summary>
		/// <param name="outOfReach">what it found</param>
		/// <returns></returns>
		string DescribeOutOfReach(int outOfReach) {
			if (outOfReach == SecureDesktop) {
				return "the lock screen or a UAC prompt is showing";
			}
			try {
				using (Process process = Process.GetProcessById(outOfReach)) {
					return process.ProcessName + " is running as administrator";
				}
			}
			catch (Exception e) when (e is ArgumentException || e is InvalidOperationException) {
				return "a program running as administrator had focus"; // it has closed since
			}
		}

		#region held inputs

		/// <summary>
		/// A key or mouse button that a hook saw go down and not yet up
		/// </summary>
		class HeldInput {
			public bool Mouse; // which event its release raises
			public uint KnownDownAt; // its press, or the last check that found it down. The hold time of a release no hook saw ends here
			public bool UpMeansReleased; // Windows has shown it down, it was pressed while input was out of reach, or the hooks were reinstalled while it was held. Before that, Windows showing it up may only mean another program's hook blocks it
		}

		/// <summary>
		/// Marks a key or mouse button as held
		/// </summary>
		/// <param name="code">its key code</param>
		/// <param name="mouse">whether it's a mouse button</param>
		/// <returns>false if it was held already, which for a key is auto-repeat</returns>
		bool Press(int code, bool mouse) {
			if (_currentlyPressed.ContainsKey(code)) {
				return false;
			}
			_currentlyPressed.Add(code, new HeldInput { Mouse = mouse, KnownDownAt = unchecked((uint)Environment.TickCount) });
			return true;
		}

		/// <summary>
		/// Whether Windows has a key or mouse button down. It reports the physical mouse buttons, which a left-handed setup swaps,
		/// so the left and right buttons count as down while either one is
		/// </summary>
		/// <param name="code">its key code</param>
		/// <returns></returns>
		bool IsDown(int code) {
			if (code == (int)MouseEventArgs.Buttons.LMB || code == (int)MouseEventArgs.Buttons.RMB) {
				return GetAsyncKeyState((int)MouseEventArgs.Buttons.LMB) < 0 || GetAsyncKeyState((int)MouseEventArgs.Buttons.RMB) < 0;
			}
			return GetAsyncKeyState(code) < 0;
		}

		/// <summary>
		/// Lets Windows showing a held key or mouse button as up count as its release, because that release may have been kept from the hooks
		/// </summary>
		void TrustReleases() {
			foreach (HeldInput held in _currentlyPressed.Values) {
				held.UpMeansReleased = true;
			}
		}

		/// <summary>
		/// Lets go of held keys and mouse buttons whose release no hook saw. Windows keeps releases from the hooks
		/// while input is out of reach, such as L's in Win+L, and what it says is down then can't be trusted either.
		/// So everything held goes when input goes out of reach, and anything pressed after that once Windows has it up
		/// </summary>
		/// <param name="outOfReach">what FindOutOfReach found</param>
		/// <param name="previous">what it found at the previous check</param>
		void ReleaseMissedInputs(int outOfReach, int previous) {
			uint now = unchecked((uint)Environment.TickCount);
			List<string> released = new List<string>();
			foreach (KeyValuePair<int, HeldInput> held in new List<KeyValuePair<int, HeldInput>>(_currentlyPressed)) {
				int code = held.Key;
				if (outOfReach == InReach) {
					if (IsDown(code)) {
						held.Value.UpMeansReleased = true;
						held.Value.KnownDownAt = now;
						continue;
					}
					if (!held.Value.UpMeansReleased) {
						continue;
					}
				}
				else if (outOfReach == previous) {
					// pressed while out of reach, which the hooks shouldn't have seen. Its release may not reach them either
					held.Value.UpMeansReleased = true;
					continue;
				}

				_currentlyPressed.Remove(code);
				TimeSpan lateBy = TimeSpan.FromMilliseconds(unchecked(now - held.Value.KnownDownAt));
				if (held.Value.Mouse) {
					System.Drawing.Point position = Control.MousePosition;
					OnMouseUp?.Invoke(this, new MouseEventArgs((MouseEventArgs.Buttons)code, position.X, position.Y, lateBy));
				}
				else {
					OnKeyUp?.Invoke(this, new KeyEventArgs(code, lateBy));
				}
				released.Add(Keys.KeyNames.ContainsKey(code) ? Keys.KeyNames[code] : ("?" + code));
			}

			if (released.Count > 0) {
				Log.Write("Released input that no hook saw go up: " + string.Join(", ", released));
			}
		}

		void MarkMouseButtonAsPressed(MSLLHOOKSTRUCT hookStruct, MouseEventArgs.Buttons button, bool pressed) {
			if (pressed && Press((int)button, true)) {
				OnMouseDown?.Invoke(this, new MouseEventArgs(button, hookStruct.pt.x, hookStruct.pt.y));
			}
			else if (!pressed && _currentlyPressed.Remove((int)button)) {
				OnMouseUp?.Invoke(this, new MouseEventArgs(button, hookStruct.pt.x, hookStruct.pt.y));
			}
		}

		#endregion

		#region hook callbacks

		/// <summary>
		/// keyhook callback happens.
		/// </summary>
		/// <param name="nCode">not sure</param>
		/// <param name="wParam">event</param>
		/// <param name="lParam">the key being pressed</param>
		/// <returns></returns>
		private IntPtr KbHookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
			_lastCallback = unchecked((uint)Environment.TickCount);

			if (nCode < 0) {
				return CallNextHookEx(_kbHookID, nCode, wParam, lParam);
			}

			int vkCode = Marshal.ReadInt32(lParam);

			switch ((int)wParam) {
				case WM_KEYDOWN:
				case WM_SYSKEYDOWN:
					if (Press(vkCode, false)) {
						OnKeyDown?.Invoke(this, new KeyEventArgs(vkCode));
					}
					break;
				case WM_KEYUP:
				case WM_SYSKEYUP:
					// a key ReleaseMissedInputs let go of may still send its release, which mustn't take a key with the same name along
					if (_currentlyPressed.Remove(vkCode)) {
						OnKeyUp?.Invoke(this, new KeyEventArgs(vkCode));
					}
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
			_lastCallback = unchecked((uint)Environment.TickCount);

			if (nCode < 0) {
				return CallNextHookEx(_mHookID, nCode, wParam, lParam);
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
					if (Configuration.LogMovement) {
						OnMouseMove?.Invoke(this, new MoveEventArgs(hookStruct.pt.x, hookStruct.pt.y, hookStruct.time));
					}
					break;
			}

			return CallNextHookEx(_mHookID, nCode, wParam, lParam);
		}

		#endregion

		#region mouseLParamsStructs
		// totally didn't copy this from the internet without understanding it

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