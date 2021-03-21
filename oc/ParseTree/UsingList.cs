using Generators;
using System.Collections.Generic;
using System.Text;

namespace Opal.ParseTree
{
    public class UsingList
    {
        private readonly List<Using> data = new List<Using>();

        public static UsingList Add(UsingList list, Using item)
        {
            list.data.Add(item);
            return list;
        }

        public void Write(Generator generator)
        {
            foreach (var item in data)
                item.Write(generator);
        }
    }
}
