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

		/// <summary>
		/// adds (math) a value to an existing dictionary value, or adds (enumerable) a new key if it doesn't exist
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dic"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public static void AddValueToDictionaryValue(Dictionary<int, uint> dic, int key, uint value) {
			if (dic.ContainsKey(key)) {
				dic[key] += value;
			}
			else {
				dic.Add(key, value);
			}
		}

		/// <summary>
		/// adds (math) a value to an existing dictionary value, or adds (enumerable) a new key if it doesn't exist
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dic"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public static void AddValueToDictionaryValue(Dictionary<int, TimeSpan> dic, int key, TimeSpan value) {
			if (dic.ContainsKey(key)) {
				dic[key] += value;
			}
			else {
				dic.Add(key, value);
			}
		}

		/// <summary>
		/// adds (math) a value to an existing dictionary value, or adds (enumerable) a new key if it doesn't exist
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dic"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public static void AddValueToDictionaryValue(Dictionary<int, int> dic, int key, int value) {
			if (dic.ContainsKey(key)) {
				dic[key] += value;
			}
			else {
				dic.Add(key, value);
			}
		}

		/// <summary>
		/// adds (math) a value to an existing dictionary value, or adds (enumerable) a new key if it doesn't exist
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dic"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public static void AddValueToDictionaryValue(Dictionary<string, int> dic, string key, int value) {
			if (dic.ContainsKey(key)) {
				dic[key] += value;
			}
			else {
				dic.Add(key, value);
			}
		}

	}

}
