using Generators;

namespace Opal.Dfa
{
    public interface IClassTableWriter
    {
        void WriteInit(Dfa dfa, Generator generator);
        void WriteData(Dfa dfa, Generator generator);
    }

    public class CompressedClassTableWriter: IClassTableWriter
    {
        public void WriteInit(Dfa dfa, Generator generator)
        {
            generator.Write("CharClasses.")
                .Write(Dfa.GetMethod("Decompress", dfa.MaxClass))
                .Write("(_charToClassCompressed)");
        }

        public void WriteData(Dfa dfa, Generator generator)
        {
            generator.Indent();
            generator.WriteLine("private static readonly byte[] _charToClassCompressed = ");
            generator.WriteCompressedArray(dfa.MaxClass, dfa.MatchToClass);
            generator.UnIndent();
        }
    }

    public class SparseClassTableWriter: IClassTableWriter
    {
        public void WriteInit(Dfa dfa, Generator generator) =>
            generator.Write("Opal.CharClasses.ToArray(_charToClass)");

        public void WriteData(Dfa dfa, Generator generator)
        {
            //(char, int)[] charClasses =
            //{
            //    ('a', 0), ('b', 1)
            //};

            //Write map
            generator.Indent();
            generator.WriteLine("private static readonly (char ch, int cls)[] _charToClass = ");
            generator.StartBlock();

            var first = true;
            int column = 0;
            for (var i = 0; i < dfa.MatchToClass.Length; i++)
            {
                var state = dfa.MatchToClass[i];
                if (state == 0)
                    continue;
                if (first)
                    first = false;
                else
                    generator.Write(", ");

                if (column == 8)
                {
                    generator.WriteLine();
                    column = 0;
                }
                column++;

                generator.Write("('")
                    .WriteEscChar(i)
                    .Write("', ")
                    .Write(state)
                    .Write(")");
            }
            generator.WriteLine();
            generator.EndBlock(";");
            generator.UnIndent();
        }
    }
}
