using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class Option
    {
        private readonly Identifier name;
        private readonly IConstant constant;
        
        public Option(Identifier name, IConstant constant)
        {
            this.name = name;
            this.constant = constant;
        }

        public void MergeTo(Options options) =>
            options.Add(name.Value, constant.Value);
    }
}
