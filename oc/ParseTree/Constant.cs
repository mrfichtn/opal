namespace Opal.ParseTree
{
    public class Constant<T>: Segment, IConstant
    {
        public Constant(Segment segment, T value)
            : base(segment)
        {
            Value = value;
        }

        public T Value { get; }

        object IConstant.Value => Value!;
    }

    public interface IConstant
    {
        object Value { get; }
    }
}
