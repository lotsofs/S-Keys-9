using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SKeys9 {
	/// <summary>
	/// Event Args for keyboard input
	/// </summary>
	public class KeyEventArgs : EventArgs {
		public int Key {
			get; private set;
		}

		public KeyEventArgs(int key) {
			Key = key;
		}
	}

	/// <summary>
	/// Event Args for mousewheel scrolling
	/// </summary>
	public class ScrollEventArgs : EventArgs {
		public enum Directions {
			Up = 0,
			Down = 1,
			Left = 2,
			Right = 3,
		}

		public Directions Direction {
			get; private set;
		}

		public ScrollEventArgs(Directions direction) {
			Direction = direction;
		}
	}

	/// <summary>
	/// Event Args for mouse movement
	/// </summary>
	public class MoveEventArgs : EventArgs {
		public int X {
			get; protected set;
		}

		public int Y {
			get; protected set;
		}

		public MoveEventArgs(int x, int y) {
			X = x;
			Y = y;
		}
	}

	/// <summary>
	/// Event Args for mouse button inputs
	/// </summary>
	public class MouseEventArgs : EventArgs {
		public enum Buttons {
			None = 0,
			LMB = 1,
			RMB = 2,
			MMB = 4,
			XMB1 = 5, 
			XMB2 = 6,
		}

		public int X {
			get; protected set;
		}

		public int Y {
			get; protected set;
		}

		public Buttons Button {
			get; private set;
		}

		public MouseEventArgs(Buttons button, int x, int y) {
			Button = button;
			X = x;
			Y = y;
		}
	}

	/// <summary>
	/// Event Args for when currently active keys/buttons change
	/// </summary>
	public class ChangeEventArgs {
		public List<string> ActiveButtons {
			get; private set;
		}

		public Dictionary<string, int> ScrollCount {
			get; private set;
		}

		public ChangeEventArgs(List<string> activeButtons, Dictionary<string, int> scrollCount) {
			ActiveButtons = activeButtons;
			ScrollCount = scrollCount;
		}
	}
}
