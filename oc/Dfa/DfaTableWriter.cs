using Generators;
using Opal.Templates;
using System;

namespace Opal.Dfa
{
    public class DfaTableWriter: IGeneratable, ITemplateContext
    {
        private readonly Dfa dfa;
        private readonly IClassTableWriter classWriter;
        private readonly IStateTableWriter stateTableWriter;

        public DfaTableWriter(Dfa dfa, 
            ITableWriterFactory tableWriterFactory,
            ISyntaxErrorHandler syntaxErrorHandler)
        {
            this.dfa = dfa ?? throw new ArgumentNullException(nameof(dfa));
            classWriter = tableWriterFactory.CreateClassWriter();
            var tableFactory = syntaxErrorHandler.CreateStateTable(dfa);
            stateTableWriter = tableWriterFactory.CreateStateWriter(dfa, tableFactory);
        }

        public void Write(Generator generator) =>
            TemplateProcessor2.FromAssembly(generator, 
                this, 
                "Opal.FrameFiles.StateScanner.txt");


        bool ITemplateContext.WriteVariable(Generator generator, string varName)
        {
            var found = true;
            switch (varName)
            {
                case "dfa.class.init": classWriter.WriteInit(dfa, generator); break;
                case "dfa.class.data": classWriter.WriteData(dfa, generator); break;

                case "dfa.state.init": stateTableWriter.WriteInit(generator); break;
                case "dfa.state.data": stateTableWriter.WriteData(generator); break;

                default: found = false; break;
            }
            return found;
        }

        bool ITemplateContext.Condition(string varName) => false;
        
        string? ITemplateContext.Include(string name) => null;
    }
}
