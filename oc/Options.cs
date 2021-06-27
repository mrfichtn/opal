using System;
using System.Collections.Generic;

namespace Opal
{
    public class Options
    {
        private readonly Dictionary<string, object> data;

        public Options()
        {
            data = new Dictionary<string, object>();
        }

        /// <summary>
        /// Adds value if it doesn't already exists
        /// </summary>
        public void Add(string name, object value)
        {
            if (!data.ContainsKey(name))
                data.Add(name, value);
        }

        /// <summary>
        /// Removes option, returning true if the option existed
        /// </summary>
        public bool Remove(string name) => data.Remove(name);

        /// <summary>
        /// Returns / sets option value
        /// </summary>
        public object this[string name]
        {
            get => data[name];
            set => data[name] = value;
        }

        /// <summary>
        /// Returns option value as text
        /// </summary>
        public bool TryGet(string key, out string? text)
        {
            var result = data.TryGetValue(key, out var value);
            text = result ? value as string : null;
            return result;
        }

        /// <summary>
        /// If option doesn't exist, returns null.  If the item isn't null, false, or 0,
        /// returns true
        /// </summary>
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

        /// <summary>
        /// Returns true if not null, 0, or false.  If not specified, returns
        /// defValue
        /// </summary>
        public bool HasOption(string key, bool defValue) =>
            HasOption(key) ?? defValue;

        /// <summary>
        /// Returns true if option exists and is equal to value
        /// </summary>
        public bool Equals(string key, string value) =>
            TryGet(key, out var actualValue) &&
                value.Equals(actualValue, StringComparison.InvariantCultureIgnoreCase);
    }
}
