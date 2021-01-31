namespace Opal.ParseTree
{
    public class NullableType
    {
        public NullableType(string typeName, bool nullable = false)
        {
            TypeName = typeName;
            Nullable = nullable;
        }

        public string TypeName { get; }

        public bool Nullable { get; }
    }
}
