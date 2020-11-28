using System;
using System.Collections;
using System.Collections.Generic;

namespace Opal
{
    public class ImmutableStack<T>: IEnumerable<T>
    {
        public static readonly ImmutableStack<T> Empty = new ImmutableStack<T>(null!, default!);
        
        private readonly ImmutableStack<T>? next;
        private readonly T value;


        private ImmutableStack(ImmutableStack<T> next, T value)
        {
            this.next = next;
            this.value = value;
        }

        public bool IsEmpty => (next == null);

        public ImmutableStack<T> Push(T value) => new ImmutableStack<T>(this, value);

        public ImmutableStack<T> Pop(out T value)
        {
            value = this.value;
            return Pop();
        }

        public ImmutableStack<T> Pop()
        {
            if (next == null) throw new InvalidOperationException("stack is empty");
            return next;
        }

        public T Peek()
        {
            if (next == null) throw new InvalidOperationException("stack is empty");
            return value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var stack = this;
            while (stack.next != null)
            {
                yield return stack.value;
                stack = stack.next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
