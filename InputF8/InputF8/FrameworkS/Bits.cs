using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S {
	class Bits {
		/// <summary>
		/// Combines two ints (lower than 65536) into one int by bitshifting 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static int CombineInt(int a, int b) {
			a = a << 16;
			return a | b;
		}
	}
}
