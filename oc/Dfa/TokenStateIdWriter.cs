using Generators;
using Opal.Nfa;

namespace Opal.Dfa
{
    public abstract class TokenStateWriterBase
    {
        public void Write(IGenerator generator, Dfa dfa)
        {
            generator.WriteLine("public class TokenStates")
                .WriteLine("{")
                .Indent();

            Write(generator, dfa.AcceptingStates);
            generator.UnIndent()
                .WriteLine("}");
        }
        
        protected abstract void Write(IGenerator generator, AcceptingStates acceptingStates);
    }

    public class AllTokenStatesWriter: TokenStateWriterBase
    {
        protected override void Write(IGenerator generator, AcceptingStates acceptingStates)
        {
            foreach (var state in acceptingStates.AllStates)
            {
                if (state.index == 0)
                    generator.WriteLine("public const int SyntaxError = -1;");
                var name = Identifier.SafeName(state.name);
                generator.WriteLine($"public const int {name} = {state.index};");
            }
        }
    }

    public class MinimumStatesWriter: TokenStateWriterBase
    {
        protected override void Write(IGenerator generator, AcceptingStates acceptingStates)
        {
            generator.WriteLine("public const int SyntaxError = -1;")
                .WriteLine("public const int Empty = 0;");
        }
    }
}
