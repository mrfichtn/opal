using Generators;
using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class ActionArgs : List<ActionExpr>
    {
        public ActionArgs()
        { }

        public ActionArgs(ActionExpr arg) =>
            Add(arg);

        public static ActionArgs Add(ActionArgs args, ActionExpr arg)
        {
            args.Add(arg);
            return args;
        }
    }

    public static class ActionArgExt
    {
        public static ActionWriteContext Write(this ActionWriteContext context, ActionArgs args)
        {
            var isFirst = true;
            foreach (var item in args)
            {
                if (isFirst) 
                    isFirst = false; 
                else 
                    context.Write(',');
                item.Write(context);
            }
            return context;
        }
    }
}
