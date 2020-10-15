using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Opal
{
    public class ImmutableQueue<T>
    {
        private readonly ImmutableQueue<T> Queue;
        private readonly T Value;

        private ImmutableQueue(ImmutableQueue<T> queue, T value)
        {
            Queue = queue;
            Value = value;
        }

        public ImmutableQueue<T> Add(T value)
        {
            return new ImmutableQueue<T>(this,
                value);
        }

        public (ImmutableQueue<T> queue, T value) Dequeue()
        {

        }

        private class Rec
        {
            public Rec(T value, Rec next)
            {
                Value = value;
                Next = next;
            }
            
            public readonly T Value;
            public readonly Rec Next;
        }
    }
}
