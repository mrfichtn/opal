namespace Opal.Containers
{
    public class ArrayBuffer<T>
    {
        private int pos;

        public ArrayBuffer(T[] array) => Array = array;

        public T[] Array { get; }

        public bool Next(out T element)
        {
            var hasNext = (pos < Array.Length);
            element = hasNext ? Array[pos++] : default!;
            return hasNext;
        }
    }
}
