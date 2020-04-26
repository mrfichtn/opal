using Generators;
using Opal.Containers;
using System;

namespace Opal.Dfa
{
    public class DfaStateWriter: IGeneratable, IVarProvider
    {
        private readonly Dfa dfa;
        
        public DfaStateWriter(Dfa dfa) =>
            this.dfa = dfa ?? throw new ArgumentNullException(nameof(dfa));

        public void Write(Generator generator) =>
            TemplateProcessor.FromAssembly(generator, this, "Opal.FrameFiles.StateScanner.txt");

        bool IVarProvider.AddVarValue(Generator generator, string varName)
        {
            var found = true;
            switch (varName)
            {
                case "dfa.class.read":      generator.Write(dfa.GetClassReadMethod());  break;
                case "dfa.maxClass":        generator.Write((dfa.MaxClass + 1).ToString()); break;
                case "dfa.states.read":     generator.Write(dfa.GetStatesReadMethod()); break;
                case "dfa.maxStates":       generator.Write(dfa.States.Length.ToString()); break;
                case "scanner.char.map":    dfa.WriteMap(generator); break;
                case "scanner.states":      WriteStates(generator); break;
                default:                    found = false; break;
            }
            return found;
        }

        private void WriteStates(IGenerator generator, bool addSyntaxError = false)
        {
            generator.WriteLine("private static readonly byte[] _compressedStates = ");

            var tableFactory = !addSyntaxError ?
                new ScannerStateTable(dfa.States) :
                new ScannerStateTableWithSyntaxErrors (dfa.States);
            var table = tableFactory.Create();
            generator.WriteCompressedArray(table);
        }
    }
}
