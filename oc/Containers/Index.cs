using System.Collections;
using System.Collections.Generic;

namespace Opal.Index
{
	public class Index<T>: IEnumerable<T>
		where T:notnull
	{
		private readonly Dictionary<T, int> _toIndex;
		private readonly List<T> _items;

		public Index()
		{
			_toIndex = new Dictionary<T, int>();
			_items = new List<T>();
		}

		#region Properties

		#region Indexer 

		public T this[int index]
		{
			get { return _items[index];  }
		}

		#endregion

		#region Count Property
		public int Count
		{
			get { return _items.Count; }
		}
		#endregion

		#region Pairs Property
		public IEnumerable<KeyValuePair<T, int>> Pairs
		{
			get { return _toIndex; }
		}
		#endregion

		#endregion

		public int Add(T value)
		{
			var index = _items.Count;
			_toIndex.Add(value, index);
			_items.Add(value);
			return index;
		}

        public bool Contains(T key)
        {
            return _toIndex.ContainsKey(key);
        }

		public bool TryGetIndex(T value, out int index)
		{
			return _toIndex.TryGetValue(value, out index);
		}

		public bool TryGetValue(int index, out T value)
		{
			var result = (index < _items.Count);
			value = result ? _items[index] : default!;
			return result;
		}

		public int AddOrGet(T value)
		{
            if (!_toIndex.TryGetValue(value, out int index))
            {
                index = _items.Count;
                _toIndex.Add(value, index);
                _items.Add(value);
            }
            return index;
		}

		public bool TryAdd(T value, out int index)
		{
			var result = !_toIndex.TryGetValue(value, out index);
			if (result)
			{
				index = _items.Count;
				_toIndex.Add(value, index);
				_items.Add(value);
			}
			else
				index = -index;
			return result;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _items.GetEnumerator();
		}
	}
}
