using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InputF8 {
	public class MathS {
		/// <summary>
		/// returns the distance between two coordinates
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public static double Distance2DCoords(int xA, int yA, int xB, int yB) {
			int distanceX = Math.Abs(xA - xB);
			int distanceY = Math.Abs(yA - yB);
			double distanceP2 = Math.Pow(distanceX, 2) + Math.Pow(distanceY, 2);
			return Math.Sqrt(distanceP2);
		}

		/// <summary>
		/// Combines two ints into one int by bitshifting one
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static int CombineInt(int a, int b) {
			a = a << 16;
			return a + b;
		}
	}
}
