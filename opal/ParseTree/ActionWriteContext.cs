using Generators;

namespace Opal.ParseTree
{
    public class ActionWriteContext: Generator
    {
        private readonly ProductionList productions;
        private readonly Production production;

        public ActionWriteContext(Generator generator, 
            ProductionList productions,
            Production production,
            bool root
            )
            : base(generator)
        {
            this.productions = productions;
            this.production = production;
            Root = root;
        }

        public bool Root { get; }

        /// <summary>
        /// Examine production at position to see if we can determine its type
        /// </summary>
        public string FindProductionType(int position)
        {
            string productionType;
            if (position < production.Right.Count)
            {
                var prodExpr = production.Right[position];
                productions.DefaultTypes.TryGetValue(prodExpr.Id, out productionType);
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
