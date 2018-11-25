using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InputF8 {
	public class KeyEventArgs : EventArgs {
		public Key Key {
			get; private set;
		}

		public KeyEventArgs(Key key) {
			Key = key;
		}
	}

	public class MouseAEventArgs : EventArgs {
		public enum Buttons {
			None = 0,
			LMB = 1,
			RMB = 2,
			MMB = 3,
			XMB = 4, 
			Wheel = 5,
			HWheel = 6,
			Move = 7,
		}

		public Buttons Button {
			get; private set;
		}

		public MouseAEventArgs(Buttons button) {
			Button = button;
		}

	}
}
