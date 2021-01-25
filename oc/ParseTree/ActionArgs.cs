using Opal.Productions;
using System.Collections.Generic;
using System.Linq;

namespace Opal.ParseTree
{
    public class ActionArgs : List<ActionExpr>
    {
        public ActionArgs()
        { }

        public ActionArgs(ActionExpr arg) =>
            Add(arg);

        public IReductionExpr[] Reduce(ReduceContext context) =>
            this.Select(x => x.Reduce(context)).ToArray();

        public static ActionArgs Add(ActionArgs args, ActionExpr arg)
        {
            args.Add(arg);
            return args;
        }
    }
}
