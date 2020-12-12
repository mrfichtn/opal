using Generators;
using Opal.Containers;
using Opal.Templates;
using System;

namespace Opal.Dfa
{
    public class DfaStateWriter: IGeneratable, ITemplateContext
    {
        private readonly Dfa dfa;
        private readonly IClassWriter classWriter;
        private readonly IStateWriter stateWriter;

        public DfaStateWriter(Dfa dfa, bool compress)
        {
            this.dfa = dfa ?? throw new ArgumentNullException(nameof(dfa));

            classWriter = compress ?
                new CompressClassWriter() :
                new SparseClassWriter();

            stateWriter = compress ?
                new CompressStateWriter() :
                new StateWriter();
        }

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
                case "dfa.class.init": classWriter.WriteInit(dfa, generator); break;
                case "dfa.class.data": classWriter.WriteData(dfa, generator); break;

                case "dfa.state.init": stateWriter.WriteInit(dfa, generator); break;
                case "dfa.state.data": stateWriter.WriteData(dfa, generator, false); break;

                case "dfa.class.decompress": generator.Write(dfa.GetClassDecompressMethod()); break;
                
                case "dfa.maxClass": generator.Write((dfa.MaxClass + 1).ToString()); break;
                case "dfa.states.read": generator.Write(dfa.GetStatesReadMethod()); break;
                case "dfa.states.decompress": generator.Write(dfa.GetStatesDecompressMethod()); break;
                case "dfa.maxStates": generator.Write(dfa.States.Length.ToString()); break;
                case "scanner.char.map": dfa.WriteCompressedMap(generator); break;
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
