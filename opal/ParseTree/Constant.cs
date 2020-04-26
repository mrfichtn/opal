namespace Opal.ParseTree
{
    public class Constant<T>: Segment
    {
        public Constant(Segment segment, T value)
            : base(segment)
        {
            Value = value;
        }

        public T Value { get; }
    }
}
