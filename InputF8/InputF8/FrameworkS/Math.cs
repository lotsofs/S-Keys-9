using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S {
	public class Math {
		/// <summary>
		/// Pythagorean theorem between two coordinates
		/// </summary>
		/// <param name="x1">X coordinate of point 1</param>
		/// <param name="y1">Y coordinate of point 1</param>
		/// <param name="x2">X coordinate of point 2</param>
		/// <param name="y2">Y coordinate of point 2</param>
		/// <returns></returns>
		public static double Distance2D(double x1, double y1, double x2, double y2) {
			double distanceX = System.Math.Abs(x1 - x2);
			double distanceY = System.Math.Abs(y1 - y2);
			return Pythagorean(distanceX, distanceY);
		}

		/// <summary>
		/// Calculates the length of the hypotenuse in a right angled triangle
		/// </summary>
		/// <param name="a">length of cathetus a</param>
		/// <param name="b">length of cathetus b</param>
		/// <returns></returns>
		public static double Pythagorean(double a, double b) {
			double c2 = System.Math.Pow(a, 2) + System.Math.Pow(b, 2);
			return System.Math.Sqrt(c2);
		}
	}

}
