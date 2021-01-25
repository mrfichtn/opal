using Generators;

namespace Opal.Productions
{
    public class ProductionWriteContext: Generator<ProductionWriteContext>
    {
        public ProductionWriteContext(GeneratorBase generator,
            Grammar grammar,
            INoAction noAction)
            : base(generator)
        {
            Grammar = grammar;
            NoAction = noAction;
        }

        public Grammar Grammar { get; }
        
        public INoAction NoAction { get; }
    }
}
