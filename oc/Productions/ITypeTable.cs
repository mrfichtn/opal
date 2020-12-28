namespace Opal.Productions
{
    public interface ITypeTable
    {
        bool AddPrimary(string name, string type);
        void AddSecondary(string name, string type);
    }
}
