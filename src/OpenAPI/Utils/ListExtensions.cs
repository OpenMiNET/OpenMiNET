using System;
using System.Collections.Generic;

namespace OpenAPI.Utils
{
	public static class ListExtensions
	{
		public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
		{
			foreach (T i in collection)
			{
				list.Add(i);
			}
		}

		public static void ForEach<T>(this IList<T> list, Action<T> action)
		{
			foreach (T i in list)
			{
				action.Invoke(i);
			}
		}

		public static void Sort<T>(this IList<T> list)
		{
			if (list is List<T>)
			{
				((List<T>)list).Sort();
			}
			else
			{
				var copy = new List<T>(list);
				copy.Sort();
				Copy(copy, 0, list, 0, list.Count);
			}
		}

		public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
		{
			if (list is List<T>)
			{
				((List<T>)list).Sort(comparison);
			}
			else
			{
				var copy = new List<T>(list);
				copy.Sort(comparison);
				Copy(copy, 0, list, 0, list.Count);
			}
		}

		public static void Sort<T>(this IList<T> list, IComparer<T> comparer)
		{
			if (list is List<T>)
			{
				((List<T>)list).Sort(comparer);
			}
			else
			{
				var copy = new List<T>(list);
				copy.Sort(comparer);
				Copy(copy, 0, list, 0, list.Count);
			}
		}

		public static void Sort<T>(this IList<T> list, int index, int count,
			IComparer<T> comparer)
		{
			if (list is List<T>)
			{
				((List<T>)list).Sort(index, count, comparer);
			}
			else
			{
				var range = new List<T>(count);
				for (var i = 0; i < count; i++)
				{
					range.Add(list[index + i]);
				}
				range.Sort(comparer);
				Copy(range, 0, list, index, count);
			}
		}

		private static void Copy<T>(IList<T> sourceList, int sourceIndex,
			IList<T> destinationList, int destinationIndex, int count)
		{
			for (var i = 0; i < count; i++)
			{
				destinationList[destinationIndex + i] = sourceList[sourceIndex + i];
			}
		}

		public static List<T> ShiftLeft<T>(this List<T> list, int shiftBy, bool nonLossy = true)
		{
			if (list.Count <= shiftBy)
			{
				return list;
			}

			List<T> result = list.GetRange(shiftBy, list.Count - shiftBy);
			if (nonLossy)
			{
				result.AddRange(list.GetRange(0, shiftBy));
			}
			return result;
		}

		public static List<T> ShiftRight<T>(this List<T> list, int shiftBy)
		{
			if (list.Count <= shiftBy)
			{
				return list;
			}

			List<T> result = list.GetRange(list.Count - shiftBy, shiftBy);
			result.AddRange(list.GetRange(0, list.Count - shiftBy));
			return result;
		}
	}
}
