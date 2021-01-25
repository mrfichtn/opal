using System.Collections.Generic;

namespace Generators
{
    public interface ILanguage
    {
        void AddReference(string reference);
        
        void StartNamespace(string name);
        void EndNamespace(string name);
        
        void StartClass(string name, AccessSpecifier specifier, ClassFlags flags = ClassFlags.None, 
            IList<string>? superClasses = null);
        void EndClass();

        void WriteInterfaceProp(string type, string name, PropertyAccess access = PropertyAccess.Both);
        void MemberVariable(string type, string name, AccessSpecifier specifier = AccessSpecifier.Private);

        void StartProp(string type, string name, AccessSpecifier specifier);
        void StartGetProp(AccessSpecifier specifier);
        void StartSetProp(AccessSpecifier specifier);

        void StartMethod(string retType, string name, AccessSpecifier specifier, bool isStatic, params string[] methodParams);
        void StartConstructor(string name, AccessSpecifier specifier, params string[] methodParams);

        void DeclareScalar(string type, string name, string? init = null);

        void InlineComment(string comment);
    }
}