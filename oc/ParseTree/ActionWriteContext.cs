using Generators;

namespace Opal.ParseTree
{
    //public class ActionWriteContext: Generator
    //{
    //    private readonly Productions.Grammar grammar;
    //    private readonly Productions.Production production;

    //    public ActionWriteContext(Generator generator,
    //        Productions.Grammar grammar,
    //        Productions.Production production,
    //        bool root
    //        )
    //        : base(generator)
    //    {
    //        this.grammar = grammar;
    //        this.production = production;
    //        Root = root;
    //    }

    //    public ActionWriteContext(ActionWriteContext context)
    //        : this(context, context.productions, context.production, false)
    //    { }

    //    public bool Root { get; }

    ////    /// <summary>
    ////    /// Examine production at position to see if we can determine its type
    ////    /// </summary>
    ////    public string? FindProductionType(int position)
    ////    {
    ////        string? productionType;
    ////        if (position < production.Right.Count)
    ////        {
    ////            var prodExpr = production.Right[position];
    ////            productions.DefaultTypes.TryGetValue(prodExpr.Id, out productionType);
    ////        }
    ////        else
    ////        {
    ////            productionType = null;
    ////        }

    ////        return productionType;
    ////    }

    ////    public new ActionWriteContext Write(IGeneratable generatable)
    ////    {
    ////        base.Write(generatable);
    ////        return this;
    ////    }

    ////    public new ActionWriteContext Write(char ch)
    ////    {
    ////        base.Write(ch);
    ////        return this;
    ////    }
    //}
}
