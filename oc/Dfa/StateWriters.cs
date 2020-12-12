using Generators;

namespace Opal.Dfa
{
    public interface IStateWriter
    {
        void WriteData(Dfa dfa, Generator generator, bool addSyntaxError);
        void WriteInit(Dfa dfa, Generator generator);
    }

    public class CompressStateWriter: IStateWriter
    {
        public void WriteInit(Dfa dfa, Generator generator)
        {
            generator.Write("Opal.ScannerStates.")
                .Write(dfa.GetStatesDecompressMethod())
                .WriteLine("_compressedStates,")
                .Write("  maxClasses: ").Write(dfa.MaxClass + 1).WriteLine()
                .Write(", maxStates: ").Write(dfa.States.Length).WriteLine(");");
        }

        public void WriteData(Dfa dfa, Generator generator, bool addSyntaxError)
        {
            generator.Indent();
            generator.WriteLine("private static readonly byte[] _compressedStates = ");

            var tableFactory = !addSyntaxError ?
                new ScannerStateTable(dfa.States) :
                new ScannerStateTableWithSyntaxErrors(dfa.States);
            var table = tableFactory.Create();
            generator.WriteCompressedArray(table);
            generator.UnIndent();
        }

    }

    public class StateWriter: IStateWriter
    {
        public void WriteInit(Dfa dfa, Generator generator)
        {
        }


        public void WriteData(Dfa dfa, Generator generator, bool addSyntaxError)
        {
            var tableFactory = !addSyntaxError ?
                new ScannerStateTable(dfa.States) :
                new ScannerStateTableWithSyntaxErrors(dfa.States);
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
