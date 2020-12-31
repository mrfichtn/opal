using Generators;

namespace Opal.Dfa
{
    public interface IStateTableWriter
    {
        void WriteData(Generator generator);
        void WriteInit(Generator generator);
    }

    public class CompressStateWriter: IStateTableWriter
    {
        private readonly Dfa dfa;
        private readonly ScannerStateTable tableFactory;

        public CompressStateWriter(Dfa dfa, ScannerStateTable tableFactory)
        {
            this.dfa = dfa;
            this.tableFactory = tableFactory;
        }

        public void WriteInit(Generator generator)
        {
            generator
                .Write("_states = Opal.ScannerStates.")
                .Write(dfa.GetStatesDecompressMethod())
                .WriteLine("(_compressedStates,")
                .Write("  maxClasses: ").Write(dfa.MaxClass + 1).Write(',').WriteLine()
                .Write("  maxStates: ").Write(tableFactory.Rows).WriteLine(");");
        }

        public void WriteData(Generator generator)
        {
            generator.Indent();
            generator.WriteLine("private static readonly byte[] _compressedStates = ");
            var table = tableFactory.Create();
            generator.WriteCompressedArray(table);
            generator.UnIndent();
        }

    }

    public class UncompressedStateWriter: IStateTableWriter
    {
        private readonly ScannerStateTable tableFactory;

        public UncompressedStateWriter(ScannerStateTable tableFactory)
        {
            this.tableFactory = tableFactory;
        }
        
        public void WriteInit(Generator generator)
        {
        }


        public void WriteData(Generator generator)
        {
            var table = tableFactory.Create();
            generator.Indent();
            generator.WriteLine("int[,] _states = ");
            generator.StartBlock();
            var rows = table.GetLength(0);
            var cols = table.GetLength(1);
            
            var rowFirst = true;
            for (var row = 0; row < rows; row++)
            {
                if (rowFirst)
                    rowFirst = false;
                else
                    generator.WriteLine(",");
                
                var first = true;
                generator.Write("{ ");
                for (var col = 0; col < cols; col++)
                {
                    if (first) 
                        first = false;
                    else 
                        generator.Write(", ");
                    generator.Write(table[row, col]);
                }
                generator.Write(" }");
            }
            generator.WriteLine();
            generator.EndBlock(";");
            generator.UnIndent();
        }
    }
}
