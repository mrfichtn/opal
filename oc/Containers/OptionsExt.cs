using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opal.Containers
{
    public static class OptionsExt
    {
        public static bool TryGetOption(this IDictionary<string, object> options,
            string key, out string? text)
        {
            var result = options.TryGetValue(key, out var value);
            text = result ? value as string : null;
            return result;
        }

        public static bool? HasOption(this IDictionary<string, object> options,
            string key)
        {
            if (!options.TryGetValue(key, out var value))
                return null;

            if (value is bool b)
                return b;
            if (value is string s)
                return s != null &&
                    !s.Equals("false", StringComparison.InvariantCultureIgnoreCase) &&
                    !s.Equals("0");
            return true;
        }
    }
}
