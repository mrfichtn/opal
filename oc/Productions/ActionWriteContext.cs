using Generators;

namespace Opal.Productions
{
    public class ActionWriteContext: Generator
    {
        private readonly Grammar grammar;
        private readonly Production production;

        public ActionWriteContext(Generator generator, 
            Grammar grammar,
            Production production,
            bool root)
            : base(generator)
        {
            this.grammar = grammar;
            this.production = production;
            Root = root;
        }

        public ActionWriteContext(ActionWriteContext context)
            : this(context, context.grammar, context.production, false)
        { }

        public bool Root { get; }

        /// <summary>
        /// Examine production at position to see if we can determine its type
        /// </summary>
        public string? FindProductionType(int position)
        {
            string? productionType;
            if (position < production.Right.Length)
            {
                var prodExpr = production.Right[position];
                grammar.TryFindDefault(prodExpr.Name, out productionType);
            }
            else
            {
                productionType = null;
            }

            return productionType;
        }

        public new ActionWriteContext Write(IGeneratable generatable)
        {
            base.Write(generatable);
            return this;
        }

        public new ActionWriteContext Write(char ch)
        {
            base.Write(ch);
            return this;
        }
    }
}
