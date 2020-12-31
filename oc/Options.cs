using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opal
{
    public class Options
    {
        private readonly Dictionary<string, object> data;

        public Options()
        {
            data = new Dictionary<string, object>();
        }

        public void Add(string name, object value)
        {
            if (!data.ContainsKey(name))
                data.Add(name, value);
        }

        public bool Remove(string name) => data.Remove(name);

        public object this[string name]
        {
            get => data[name];
            set => data[name] = value;
        }

        public bool TryGet(string key, out string? text)
        {
            var result = data.TryGetValue(key, out var value);
            text = result ? value as string : null;
            return result;
        }

        public bool? HasOption(string key)
        {
            if (!data.TryGetValue(key, out var value))
                return null;

            if (value is bool b)
                return b;
            if (value is string s)
                return s != null &&
                    !s.Equals("false", StringComparison.InvariantCultureIgnoreCase) &&
                    !s.Equals("0");
            return true;
        }

        public bool HasOption(string key, bool defValue) =>
            HasOption(key) ?? defValue;

        public bool Equals(string key, string value) =>
            TryGet(key, out var actualValue) &&
                value.Equals(actualValue, StringComparison.InvariantCultureIgnoreCase);
    }
}
