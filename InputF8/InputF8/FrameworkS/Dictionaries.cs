using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S {
	class Dictionaries {

		#region add value to value

		/// <summary>
		/// Increments dictionary's value at key by count
		/// </summary>
		/// <param name="dictionary">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="count">Amount to increase the value by</param>
		public static void IncrementValue(Dictionary<int, int> dictionary, int key, int count = 1) {
			if (dictionary.ContainsKey(key)) {
				dictionary[key] += count;
			}
			else {
				dictionary.Add(key, count);
			}
		}

		/// <summary>
		/// Increments dictionary's value at key by count
		/// </summary>
		/// <param name="dictionary">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="count">Amount to increase the value by</param>
		public static void IncrementValue(Dictionary<int, uint> dictionary, int key, uint count = 1) {
			if (dictionary.ContainsKey(key)) {
				dictionary[key] += count;
			}
			else {
				dictionary.Add(key, count);
			}
		}

		/// <summary>
		/// Increments dictionary's value at key by count
		/// </summary>
		/// <param name="dictionary">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="count">Amount to increase the value by</param>
		public static void IncrementValue(Dictionary<int, TimeSpan> dictionary, int key, TimeSpan count) {
			if (dictionary.ContainsKey(key)) {
				dictionary[key] += count;
			}
			else {
				dictionary.Add(key, count);
			}
		}


		/// <summary>
		/// Increments dictionary's value at key by count
		/// </summary>
		/// <param name="dictionary">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="count">Amount to increase the value by</param>
		public static void IncrementValue(Dictionary<string, int> dictionary, string key, int count = 1) {
			if (dictionary.ContainsKey(key)) {
				dictionary[key] += count;
			}
			else {
				dictionary.Add(key, count);
			}
		}

		/// <summary>
		/// Increments dictionary's value at key by count
		/// </summary>
		/// <param name="dictionary">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="count">Amount to increase the value by</param>
		public static void IncrementValue(Dictionary<string, uint> dictionary, string key, uint count = 1) {
			if (dictionary.ContainsKey(key)) {
				dictionary[key] += count;
			}
			else {
				dictionary.Add(key, count);
			}
		}

		/// <summary>
		/// Increments dictionary's value at key by count
		/// </summary>
		/// <param name="dictionary">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="count">Amount to increase the value by</param>
		public static void IncrementValue(Dictionary<string, TimeSpan> dictionary, string key, TimeSpan count) {
			if (dictionary.ContainsKey(key)) {
				dictionary[key] += count;
			}
			else {
				dictionary.Add(key, count);
			}
		}

		#endregion

		/// <summary>
		/// Sets a value in dictionary, or adds it if it doesn't exist yet
		/// </summary>
		/// <param name="dictionary">The dictionary</param>
		/// <param name="key">The key</param>
		/// <param name="value">The value</param>
		public static void SetValue(Dictionary<string, string> dictionary, string key, string value) {
			if (dictionary.ContainsKey(key)) {
				dictionary[key] = value;
			}
			else {
				dictionary.Add(key, value);
			}
		}
	}
}
