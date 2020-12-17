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

        public DfaStateWriter(Dfa dfa, 
            bool compress,
            bool addSyntaxError = false)
        {
            this.dfa = dfa ?? throw new ArgumentNullException(nameof(dfa));

            classWriter = compress ?
                new CompressClassWriter() :
                new SparseClassWriter();

            var tableFactory = !addSyntaxError ?
                new ScannerStateTable(dfa.States) :
                new ScannerStateTableWithSyntaxErrors(dfa.States);


            stateWriter = compress ?
                new CompressStateWriter(dfa, tableFactory) :
                new StateWriter(tableFactory);
        }

        public void Write(Generator generator) =>
            TemplateProcessor2.FromAssembly(generator, this, "Opal.FrameFiles.StateScanner.txt");


        bool ITemplateContext.WriteVariable(Generator generator, string varName)
        {
            var found = true;
            switch (varName)
            {
                case "dfa.class.init": classWriter.WriteInit(dfa, generator); break;
                case "dfa.class.data": classWriter.WriteData(dfa, generator); break;

                case "dfa.state.init": stateWriter.WriteInit(generator); break;
                case "dfa.state.data": stateWriter.WriteData(generator); break;

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
