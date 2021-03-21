using Opal.ParseTree;

namespace Opal.Productions
{
    public interface ITypeTable
    {
        void TypeFromAttr(string name, NullableType nullable);
        
        void AddActionType(string name, string type);
    }
}
