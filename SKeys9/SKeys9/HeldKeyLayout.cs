using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SKeys9 {
	/// <summary>
	/// The spot on the display of a key that's held, or was released a moment ago
	/// </summary>
	class PlacedKey {
		public string Name {
			get; private set;
		}

		public int Row {
			get; set;
		}

		public int X {
			get; set;
		}

		/// <summary>
		/// The space the key takes up in its row, including the gap to the next key
		/// </summary>
		public int Width {
			get; private set;
		}

		/// <summary>
		/// When the key was released, or null while it's held
		/// </summary>
		public int? ReleasedAt {
			get; set;
		}

		public PlacedKey(string name, int width) {
			Name = name;
			Width = width;
		}
	}

	/// <summary>
	/// Decides where the held keys show, so that a key can stay put until it's released,
	/// and how long a released key stays
	/// </summary>
	class HeldKeyLayout {
		const TextFormatFlags MeasureFlags = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;

		List<PlacedKey> _placed = new List<PlacedKey>();

		internal IEnumerable<PlacedKey> Placed {
			get {
				return _placed;
			}
		}

		/// <summary>
		/// How many rows the keys take up. The first one counts even when it's empty
		/// </summary>
		internal int Rows {
			get {
				return _placed.Count == 0 ? 1 : _placed.Max(key => key.Row) + 1;
			}
		}

		/// <summary>
		/// Forgets every position, for when they stop being valid
		/// </summary>
		internal void Clear() {
			_placed.Clear();
		}

		/// <summary>
		/// Finds how long until the next released key is due to go
		/// </summary>
		/// <returns>milliseconds, or null if no released key is waiting to go</returns>
		internal int? NextRemoval() {
			int now = Environment.TickCount;
			return _placed.Where(key => key.ReleasedAt != null).Select(key => (int?)TimeLeft(key, now)).Min();
		}

		/// <summary>
		/// Notes which keys were released, removes the ones whose time is up, and finds a spot for the newly held ones
		/// </summary>
		/// <param name="held">the held keys, in the order they were pressed</param>
		/// <param name="font">the font they're shown in</param>
		/// <param name="maxWidth">the width of a row</param>
		internal void Update(List<string> held, Font font, int maxWidth) {
			int now = Environment.TickCount;

			// removing from a copy keeps the right number of keys when two held keys share a name
			List<string> unplaced = new List<string>(held);
			foreach (PlacedKey key in _placed) {
				if (unplaced.Remove(key.Name)) {
					key.ReleasedAt = null;
				}
				else if (key.ReleasedAt == null) {
					key.ReleasedAt = now;
				}
			}
			_placed.RemoveAll(key => key.ReleasedAt != null && TimeLeft(key, now) <= 0);

			if (!Configuration.KeepKeysInPlace) {
				List<PlacedKey> kept = new List<PlacedKey>(_placed);
				_placed.Clear();
				foreach (PlacedKey key in kept) {
					Place(key, maxWidth);
				}
			}
			foreach (string name in unplaced) {
				Place(new PlacedKey(name, Measure(name, font)), maxWidth);
			}
		}

		/// <summary>
		/// How long a released key still has on the display
		/// </summary>
		/// <param name="key">the key</param>
		/// <param name="now">the current tick count</param>
		/// <returns>milliseconds, 0 or less once its time is up</returns>
		int TimeLeft(PlacedKey key, int now) {
			int duration = Configuration.GreyOutReleasedKeys ? Configuration.ClearDelay : 0;
			return duration - unchecked(now - key.ReleasedAt.Value);
		}

		/// <summary>
		/// Measures the space a key takes up, which is its name followed by a space
		/// </summary>
		/// <param name="name">the key</param>
		/// <param name="font">the font it's shown in</param>
		/// <returns></returns>
		int Measure(string name, Font font) {
			using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero)) {
				// only the overload that takes a device context honours NoPadding
				return TextRenderer.MeasureText(graphics, name + " ", font, Size.Empty, MeasureFlags).Width;
			}
		}

		/// <summary>
		/// Gives a key its spot. A key wider than a row takes an empty one, so this always finds a spot
		/// </summary>
		/// <param name="key">the key</param>
		/// <param name="maxWidth">the width of a row</param>
		void Place(PlacedKey key, int maxWidth) {
			bool fillGaps = Configuration.KeepKeysInPlace && Configuration.FillKeyGaps;
			for (int row = fillGaps ? 0 : Rows - 1; ; row++) {
				int x = FindSpot(row, key.Width, fillGaps);
				if (x == 0 || x + key.Width <= maxWidth) {
					key.Row = row;
					key.X = x;
					_placed.Add(key);
					return;
				}
			}
		}

		/// <summary>
		/// Finds where a key would go in a row
		/// </summary>
		/// <param name="row">the row</param>
		/// <param name="width">the key's width</param>
		/// <param name="fillGaps">whether it may go in a gap, instead of after the row's last key</param>
		/// <returns>its x position</returns>
		int FindSpot(int row, int width, bool fillGaps) {
			int x = 0;
			foreach (PlacedKey key in _placed.Where(placed => placed.Row == row).OrderBy(placed => placed.X)) {
				if (fillGaps && key.X - x >= width) {
					break;
				}
				x = key.X + key.Width;
			}
			return x;
		}
	}
}
