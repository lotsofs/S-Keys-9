using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SKeys9 {
	/// <summary>
	/// The font dialog, able to find a font that it lists as a family and a style, such as "Zurich" with
	/// "Extended Ultra Black", but that has one name everywhere else, such as "Zurich Ultra Black Extended".
	/// On its own it can't find that name, so it opens with an empty font box, and .NET then ignores what was chosen
	/// </summary>
	public class FontPicker : FontDialog {
		private const int WM_INITDIALOG = 0x0110;
		private const int WM_COMMAND = 0x0111;

		private const int CB_GETCOUNT = 0x0146;
		private const int CB_GETCURSEL = 0x0147;
		private const int CB_GETLBTEXT = 0x0148;
		private const int CB_GETLBTEXTLEN = 0x0149;
		private const int CB_SETCURSEL = 0x014E;
		private const int CBN_SELCHANGE = 1;

		private const int FamilyBoxId = 0x0470;
		private const int StyleBoxId = 0x0471;

		#region DLLImports

		[DllImport("user32.dll")]
		private static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, StringBuilder lParam);

		#endregion

		/// <summary>
		/// Windows calls this for the dialog's messages
		/// </summary>
		/// <param name="hWnd">the dialog</param>
		/// <param name="msg">the message</param>
		/// <param name="wparam"></param>
		/// <param name="lparam"></param>
		/// <returns></returns>
		protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam) {
			if (msg == WM_INITDIALOG) {
				SelectFamilyAndStyle(hWnd);
			}
			return base.HookProc(hWnd, msg, wparam, lparam);
		}

		/// <summary>
		/// Fills in an empty font box with the family that starts the font's name, and picks the style that the rest of the name describes
		/// </summary>
		/// <param name="dialog">the dialog</param>
		void SelectFamilyAndStyle(IntPtr dialog) {
			IntPtr familyBox = GetDlgItem(dialog, FamilyBoxId);
			if (SelectedIndex(familyBox) >= 0) {
				return;
			}

			string name = Font.Name;
			List<string> families = Items(familyBox);
			string family = families.Where(item => IsFamily(item, name)).OrderByDescending(item => item.Length).FirstOrDefault();
			if (family == null) {
				return;
			}
			Select(dialog, FamilyBoxId, familyBox, families.IndexOf(family));

			// the words can come in another order: "Ultra Black Extended" is listed as "Extended Ultra Black"
			string[] words = name.Substring(family.Length + 1).Split(' ', '-');
			IntPtr styleBox = GetDlgItem(dialog, StyleBoxId);
			List<string> styles = Items(styleBox);
			int selected = SelectedIndex(styleBox);
			int style = styles.FindIndex(item => HasWords(item, words));
			if (style >= 0 && (selected < 0 || !HasWords(styles[selected], words))) {
				Select(dialog, StyleBoxId, styleBox, style);
			}
		}

		/// <summary>
		/// Whether a font's name starts with this family, as "Arial Black" does with "Arial", and "EurostileExt-Bold" with "EurostileExt"
		/// </summary>
		/// <param name="family">the family</param>
		/// <param name="name">the font's name</param>
		/// <returns></returns>
		bool IsFamily(string family, string name) {
			return name.StartsWith(family + " ", StringComparison.Ordinal) || name.StartsWith(family + "-", StringComparison.Ordinal);
		}

		/// <summary>
		/// Whether a style's name has all these words in it, in any order
		/// </summary>
		/// <param name="style">the style's name</param>
		/// <param name="words">the words</param>
		/// <returns></returns>
		bool HasWords(string style, string[] words) {
			return words.All(style.Split(' ').Contains);
		}

		/// <summary>
		/// Finds which item a box has selected
		/// </summary>
		/// <param name="box">the box</param>
		/// <returns>its index, or below 0 if there's none</returns>
		int SelectedIndex(IntPtr box) {
			return SendMessage(box, CB_GETCURSEL, IntPtr.Zero, IntPtr.Zero).ToInt32();
		}

		/// <summary>
		/// Reads the items in a box
		/// </summary>
		/// <param name="box">the box</param>
		/// <returns></returns>
		List<string> Items(IntPtr box) {
			List<string> items = new List<string>();
			int count = SendMessage(box, CB_GETCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();
			for (int i = 0; i < count; i++) {
				int length = SendMessage(box, CB_GETLBTEXTLEN, (IntPtr)i, IntPtr.Zero).ToInt32();
				StringBuilder item = new StringBuilder(length + 1);
				SendMessage(box, CB_GETLBTEXT, (IntPtr)i, item);
				items.Add(item.ToString());
			}
			return items;
		}

		/// <summary>
		/// Selects an item in one of the dialog's boxes, and tells the dialog as if it had been clicked
		/// </summary>
		/// <param name="dialog">the dialog</param>
		/// <param name="boxId">the box's id in the dialog</param>
		/// <param name="box">the box</param>
		/// <param name="index">the item</param>
		void Select(IntPtr dialog, int boxId, IntPtr box, int index) {
			SendMessage(box, CB_SETCURSEL, (IntPtr)index, IntPtr.Zero);
			SendMessage(dialog, WM_COMMAND, (IntPtr)((CBN_SELCHANGE << 16) | boxId), box);
		}
	}
}
