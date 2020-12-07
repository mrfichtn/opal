using Generators;
using Opal.Containers;
using Opal.Templates;
using System;

namespace Opal.Dfa
{
    public class DfaStateWriter: IGeneratable, ITemplateContext
    {
        private readonly Dfa dfa;
        
        public DfaStateWriter(Dfa dfa) =>
            this.dfa = dfa ?? throw new ArgumentNullException(nameof(dfa));

        public void Write(Generator generator) =>
            TemplateProcessor2.FromAssembly(generator, this, "Opal.FrameFiles.StateScanner.txt");

        private void WriteStates(IGenerator generator, bool addSyntaxError = false)
        {
            generator.Indent();
            generator.WriteLine("private static readonly byte[] _compressedStates = ");

            var tableFactory = !addSyntaxError ?
                new ScannerStateTable(dfa.States) :
                new ScannerStateTableWithSyntaxErrors (dfa.States);
            var table = tableFactory.Create();
            generator.WriteCompressedArray(table);
            generator.UnIndent();
        }

        bool ITemplateContext.WriteVariable(Generator generator, string varName)
        {
            var found = true;
            switch (varName)
            {
                case "dfa.class.read": generator.Write(dfa.GetClassReadMethod()); break;
                case "dfa.class.decompress": generator.Write(dfa.GetClassDecompressMethod()); break;
                
                case "dfa.maxClass": generator.Write((dfa.MaxClass + 1).ToString()); break;
                case "dfa.states.read": generator.Write(dfa.GetStatesReadMethod()); break;
                case "dfa.states.decompress": generator.Write(dfa.GetStatesDecompressMethod()); break;
                case "dfa.maxStates": generator.Write(dfa.States.Length.ToString()); break;
                case "scanner.char.map": dfa.WriteMap(generator); break;
                case "scanner.states": WriteStates(generator); break;
                default: found = false; break;
            }
            return found;
        }

        bool ITemplateContext.Condition(string varName)
        {
            return false;
        }

        string? ITemplateContext.Include(string name)
        {
            return null;
        }
    }
}
