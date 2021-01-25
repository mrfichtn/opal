using System.Text;

namespace Opal.ParseTree
{
    public class ProductionAttr: Segment
    {
        public ProductionAttr(Identifier option, FuncOption? funcOpt)
            : base(option.Start, funcOpt?.End ?? option.End)
        {
            Option = option;
            FuncOpt = funcOpt;
            if (funcOpt != null)
            {
                IsMethod = true;
                ArgType = funcOpt.ArgType;
            }
        }

        public Identifier Option { get; }
        public FuncOption? FuncOpt { get; }
        public bool IsMethod { get; }
        public Identifier? ArgType { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            if (Option != null)
                builder.Append(Option);
            if (FuncOpt != null)
                builder.Append(FuncOpt);
            return builder.ToString();
        }
    }
}
