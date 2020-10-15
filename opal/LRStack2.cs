using System;
using System.Diagnostics;

namespace Opal
{
    #region LRStack
    [DebuggerDisplay("Count = {_count}"), DebuggerTypeProxy(typeof(LRStackProxy))]
	public class LRStack2
	{
		private Rec[] array = new Rec[4];
		private int items;
		public int count = 1;

		public LRStack2 SetItems(int value)
		{
			items = value;
			return this;
		}

		public object this[int offset]
		{
			get
			{
				if (count < offset || offset < 0)
					throw new InvalidOperationException(string.Format("Unable to retrieve {0} items", offset));
				return array[count - items + offset].Value;
			}
		}

		public uint Reduce(uint state, object value)
		{
			var oldState = array[count - items - 1].State;
			for (var i = items - 1; i > 0; i--)
				array[count - i].Value = null;
			array[count - items] = new Rec(state, value);
			count = count - items + 1;
			return oldState;
		}

		public uint Push(uint state, object value)
		{
			var oldState = array[count - 1].State;
			Shift(state, value);
			return oldState;
		}

		public void Replace(uint state)
		{
			array[count - 1].State = state;
		}

		public void Shift(uint state, object value)
		{
			if (count == array.Length)
			{
				var array = new Rec[this.array.Length == 0 ? 4 : 2 * this.array.Length];
                Array.Copy(this.array, 0, array, 0, count);
				this.array = array;
			}
			array[count++] = new Rec(state, value);
		}

		public object PopValue()
		{
			return array[--count].Value;
		}

		public bool GetState(int index, out uint state)
		{
			index++;
			var isOk = count > index;
			state = isOk ? array[count - index].State : 0;
			return isOk;
		}

		public void Pop(int items)
		{
			for (var i = items - 1; i > 0; i--)
				array[count - i].Value = null;
			count -= items;
		}

		public uint PeekState()
		{
			return array[count - 1].State;
		}

		[DebuggerDisplay("{State,2}: {Value}")]
		struct Rec
		{
			public Rec(uint state, object value)
			{
				State = state;
				Value = value;
			}
			public uint State;
			public object Value;
		}

		sealed class LRStackProxy
		{
			private readonly LRStack2 _stack;
			public LRStackProxy(LRStack2 stack)
			{
				_stack = stack;
			}

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public Rec[] Items
			{
				get
				{
					var result = new Rec[_stack.count];
					for (var i = 0; i < _stack.count; i++)
						result[i] = _stack.array[_stack.count - i - 1];
					return result;
				}
			}
		}
	}
	#endregion

}
