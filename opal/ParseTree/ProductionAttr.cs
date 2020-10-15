using Opal.ParseTree;

namespace Opal.ParseTree
{
    public class ProductionAttr
    {
        public ProductionAttr(Identifier option, bool isMethod)
        {
            Option = option;
            IsMethod = isMethod;
        }

        public ProductionAttr(Identifier option, FuncOption funcOpt)
        {
            Option = option;
            if (funcOpt != null)
            {
                IsMethod = true;
                ArgType = funcOpt.ArgType;
            }
        }

        public Identifier Option { get; }
        public bool IsMethod { get; }
        public Identifier? ArgType { get; }
    }
}
