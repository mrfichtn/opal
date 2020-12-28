using System;
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

        public void AddTo(Logger logger, Dictionary<string, object> result)
        {
            if (result.TryGetValue(name.Value, out var oldValue))
            {
                logger.LogWarning(
                    $"option entry '{name.Value}' is overridding previous setting {oldValue}",
                    name);
            }
            result[name.Value] = constant.Value;
        }

        public void MergeTo(Options options)
        {
            options.Add(name.Value, constant.Value);
        }
    }
}
