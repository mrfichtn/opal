using System;
using System.Collections.Generic;
using System.IO;

namespace Generators
{
    public class CSharp: Generator, ILanguage
    {
        public CSharp(string file)
            :   base(file)
        {
        }

        public CSharp(TextWriter writer, bool ownsStream = true)
            :   base(writer, ownsStream)
        {
        }

        public CSharp(Generator generator)
            :   base(generator)
        {
        }

        #region ILanguage Members

        public void AddReference(string reference)
        {
            WriteLine("using {0};", reference);
        }

        public void StartNamespace(string name)
        {
            WriteLine("namespace {0}", name);
            StartBlock();
        }
        
        public void EndNamespace(string name)
        {
            EndBlock(string.Format("//End namespace {0}", name));
        }

        public void StartClass(string name, AccessSpecifier specifier, ClassFlags flags = ClassFlags.None,
            IList<string> superClasses = null)
        {
            Write(ToString(specifier));
            if (flags.HasFlag(ClassFlags.Partial))
                Write(" partial");
            if (flags.HasFlag(ClassFlags.Static))
                Write(" static");
            WriteLine(" class {0}", name);
            if (superClasses.Count > 0)
            {
                Indent();
                Write(": {0}", superClasses[0]);
                for (int i = 1; i < superClasses.Count; i++)
                {
                    Write(" , {0}", superClasses[i]);
                }
                WriteLine();
                UnIndent();
            }
            StartBlock();
        }

        public void EndClass()
        {
            EndBlock();
        }

        public void MemberVariable(string type, string name, AccessSpecifier specifier = AccessSpecifier.Private)
        {
            WriteLine("{0} {1} {2};", ToString(specifier), type, name);
        }
            
        public void WriteInterfaceProp(string type, string name, PropertyAccess access)
        {
            Write("{0} {1} {{ ", type, name);
            switch (access)
            {
                case PropertyAccess.Both:
                    WriteLine("get; set; }");
                    break;
                case PropertyAccess.ReadOnly:
                    WriteLine("get; }");
                    break;
                case PropertyAccess.WriteOnly:
                    WriteLine("set; }");
                    break;
            }
        }

        public void StartProp(string type, string name, AccessSpecifier specifier)
        {
            WriteLine("{0} {1} {2}", ToString(specifier), type, name);
            StartBlock();
        }

        public void StartGetProp(AccessSpecifier specifier)
        {
            WriteLine("get");
            StartBlock();
        }

        public void StartSetProp(AccessSpecifier specifier)
        {
            WriteLine("set");
            StartBlock();
        }

        public void StartMethod(string retType, string name, AccessSpecifier specifier, bool isStatic, params string[] methodParams)
        {
            Write(ToString(specifier));
            if (isStatic)
                Write(" static");
            Write(" {0} {1}(", retType, name);

            if (methodParams.Length >= 2)
                Write("{0} {1}", methodParams[0], methodParams[1]);
            for (int i = 2; i+1 < methodParams.Length; i+=2)
                Write(", {0} {1}", methodParams[i], methodParams[i + 1]);

            WriteLine(")");
            StartBlock();
        }

        public void StartConstructor(string name, AccessSpecifier specifier, params string[] methodParams)
        {
            Write(ToString(specifier));
            Write(" {0}(", name);

            if (methodParams.Length >= 2)
                Write("{0} {1}", methodParams[0], methodParams[1]);
            for (int i = 2; i + 1 < methodParams.Length; i += 2)
                Write(", {0} {1}", methodParams[i], methodParams[i + 1]);

            WriteLine(")");
            StartBlock();
        }


        public void InlineComment(string comment)
        {
            WriteLine(inlineComment(comment));
        }

        public void DeclareScalar(string type, string name, string init = null)
        {
            if (string.IsNullOrEmpty(init))
                WriteLine("{0} {1};", type, name);
            else
                WriteLine("{0} {1} = {2};", type, name, init);
        }


        #endregion

        private string inlineComment(string comment)
        {
            return string.Format("//{0}", comment);
        }

        private string ToString(AccessSpecifier specifier)
        {
            string result;
            switch (specifier)
            {
                case AccessSpecifier.Internal:
                    result = "internal";
                    break;
                case AccessSpecifier.Private:
                    result = "private";
                    break;
                case AccessSpecifier.Protected:
                    result = "protected";
                    break;
                case AccessSpecifier.Public:
                    result = "public";
                    break;
                default:
                    result = string.Empty;
                    break;
            }
            return result;
        }
    }
}