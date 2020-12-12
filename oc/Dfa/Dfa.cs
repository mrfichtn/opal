using System.Collections.Generic;
using System.Linq;
using Generators;
using System.Text;
using Opal.Nfa;

namespace Opal.Dfa
{
    public class Dfa
    {
        public Dfa(Matches matches, 
            AcceptingStates acceptingStates, 
            IEnumerable<DfaNode> states)
        {
            Matches = matches;
            AcceptingStates = acceptingStates;
            MatchToClass = matches.ToArray();
            States = states.ToArray();
        }

        #region Properties
        public AcceptingStates AcceptingStates { get; }
        public DfaNode[] States { get; }
        public int MaxClass => Matches.NextId;
        public int[] MatchToClass { get; }
        public Matches Matches { get; }
        #endregion

        public void WriteCompressedMap(IGenerator language)
        {
            //Write map
            language.Indent();
            language.WriteLine("private static readonly byte[] _charToClassCompressed = ");
            language.WriteCompressedArray(MaxClass, MatchToClass);
            language.UnIndent();
        }

        public void WriteSparseMap(IGenerator language)
        {
            //(char, int)[] charClasses =
            //{
            //    ('a', 0), ('b', 1)
            //};

            //Write map
            language.Indent();
            language.WriteLine("private static readonly (char ch, int cls)[] _charToClass = ");
            language.StartBlock();

            var first = false;
            int column = 0;
            for (var i = 0; i < MatchToClass.Length; i++)
            {
                var state = MatchToClass[i];
                if (state == 0)
                    continue;
                if (first)
                    first = false;
                else
                    language.Write(',');
                
                if (column == 8)
                {
                    language.WriteLine();
                    column = 0;
                }
                column++;

                language.Write("('")
                    .WriteEscChar(i)
                    .Write("', ")
                    .Write(state)
                    .Write(")");
            }

            language.EndBlock(";");
        }


        public string GetClassReadMethod() => GetMethod("Read", MaxClass);

        public string GetStatesReadMethod() => GetMethod("Read", States.Length);

        public string GetClassDecompressMethod() => GetMethod("Decompress", MaxClass);
        public string GetStatesDecompressMethod() => GetMethod("Decompress", States.Length);


        public string GetMethod(string method, int max)
        {
            string result;
            if (max <= byte.MaxValue)
                result = $"{method}8";
            else if (max <= ushort.MaxValue)
                result = $"{method}16";
            else if (max < (1 << 24))
                result = $"{method}24";
            else
                result = $"{method}32";
            return result;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("     Accept  ");
            Matches.AppendTo(builder).AppendLine();
            foreach (var state in States)
                builder.AppendLine(state.ToString());
            return builder.ToString();
        }

        /// <summary>
        /// Returns safe name for accepting state
        /// </summary>
        /// <param name="accepting"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool TryGetAccepting(int accepting, out string name)
        {
            var result = AcceptingStates.TryGetName(accepting, out name);
            if (result)
                name = Identifier.SafeName(name);
            return result;
        }

        /// <summary>
        /// Writes token name to id constants
        /// </summary>
        public void WriteTokenEnum(IGenerator generator, bool emitTokenStates)
        {
            generator.WriteLine("public class TokenStates")
                .WriteLine("{")
                .Indent();

            if (emitTokenStates)
            {
                foreach (var state in AcceptingStates.AllStates)
                {
                    if (state.index == 0)
                        generator.WriteLine("public const int SyntaxError = -1;");
                    var name = Identifier.SafeName(state.name);
                    generator.WriteLine($"public const int {name} = {state.index};");
                }
            }
            else
            {
                generator.WriteLine("public const int SyntaxError = -1;")
                    .WriteLine("public const int Empty = 0;");
            }
            generator.UnIndent()
                .WriteLine("}");
        }

        public int[,] GetStateTable(bool addSyntaxError = false)
        {
            var tableFactory = !addSyntaxError ?
                new ScannerStateTable(States) :
                new ScannerStateTableWithSyntaxErrors (States);
            return tableFactory.Create();
        }
    }
}
