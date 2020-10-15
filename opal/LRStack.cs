using System;

namespace Opal
{
    public class LRStack
	{
		public static readonly LRStack Root = new LRStack(0, null, null!);

		public readonly uint State;
		public readonly object? Value;
		public readonly LRStack? Next;

		public LRStack(uint state, object? value, LRStack next)
		{
			State = state;
			Value = value;
			Next = next;
		}

		public LRStack Push(uint state, object value, out uint oldState)
		{
			oldState = State;
			return new LRStack(state, value, this);
		}

		public LRStack Replace(uint state) => new LRStack(state, Value, Next!);
		
		public LRStack Shift(uint state, object value) => new LRStack(state, value, this);

		public LRStack this[int index] => Find(index);

		private LRStack Find(int index)
        {
			var node = this;
			for (; (index > 0) && (node != null); index--)
				node = node!.Next;
			return node ?? throw new ArgumentOutOfRangeException(nameof(index));
		}

		private bool TryFind(int index, out LRStack? node)
        {
			node = this;
			for (; index > 0; index--)
			{
				if (node == null)
					return false;
				node = node!.Next;
			}
			return true;
		}

		public bool GetState(int index, out uint state)
		{
			var result = TryFind(index, out var node);
			state = result ? node!.State : 0;
			return result;
		}

		public LRStack Pop(int items)
		{
			var node = this;
			for (; items > 0; items--)
				node = node!.Next;
			return node!;
		}
	}
}
