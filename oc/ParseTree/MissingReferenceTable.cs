using Opal.Productions;
using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class MissingReferenceTable
    {
        private readonly List<Record> data;

        public MissingReferenceTable() =>
            data = new List<Record>();

        public void Add(string productionName, string expressionName) =>
            data.Add(new Record(productionName, expressionName));

        public void Resolve(TypeTable typeTable)
        {
            var missing = data;
            while (missing.Count > 0)
            {
                var notFound = new List<Record>();
                foreach (var item in data)
                {
                    if (!item.TryResolve(typeTable))
                        notFound.Add(item);
                }
                if (notFound.Count == missing.Count)
                    break;
                missing = notFound;
            }
        }


        public class Record
        {
            private readonly string productionName;
            private readonly string expressionName;

            public Record(string productionName, 
                string expressionName)
            {
                this.productionName = productionName;
                this.expressionName = expressionName;
            }

            public bool TryResolve(TypeTable typeTable)
            {
                var result = typeTable.TryFind(expressionName, out var type);
                if (result)
                    typeTable.AddSecondary(productionName, type!);
                return result;
            }

            public override string ToString() => $"{productionName} -> {expressionName}";
        }
    }
}
