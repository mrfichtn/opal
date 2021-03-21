using System.Collections;
using System.Collections.Generic;

namespace Opal.Index
{
	public class Index<T>: IEnumerable<T>
		where T:notnull
	{
		private readonly Dictionary<T, int> toIndex;
		private readonly List<T> items;

		public Index()
		{
			toIndex = new Dictionary<T, int>();
			items = new List<T>();
		}

		#region Properties

		public T this[int index] => items[index];

		public int Count => items.Count;

		public IEnumerable<KeyValuePair<T, int>> Pairs => toIndex;

		#endregion

		public int Add(T value)
		{
			var index = items.Count;
			toIndex.Add(value, index);
			items.Add(value);
			return index;
		}

        public bool Contains(T key) => toIndex.ContainsKey(key);

		public bool TryGetIndex(T value, out int index) =>
			toIndex.TryGetValue(value, out index);

		public bool TryGetValue(int index, out T value)
		{
			var result = (index < items.Count);
			value = result ? items[index] : default!;
			return result;
		}

		public int AddOrGet(T value)
		{
            if (!toIndex.TryGetValue(value, out int index))
            {
                index = items.Count;
                toIndex.Add(value, index);
                items.Add(value);
            }
            return index;
		}

		public bool TryAdd(T value, out int index)
		{
			var result = !toIndex.TryGetValue(value, out index);
			if (result)
			{
				index = items.Count;
				toIndex.Add(value, index);
				items.Add(value);
			}
			else
				index = -index;
			return result;
		}

		public IEnumerator<T> GetEnumerator() =>
			items.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
