using System.Collections.Generic;

namespace Opal.ParseTree
{
    public class OptionList
    {
        private readonly List<Option> data;
        
        public OptionList()
        {
            data = new List<Option>();
        }

        public OptionList(Option option)
        {
            data = new List<Option> { option };
        }

        public static OptionList Add(OptionList list, Option item)
        {
            list.data.Add(item);
            return list;
        }

        public void MergeTo(Options options)
        {
            for (var i = data.Count - 1; i >= 0; i--)
                data[i].MergeTo(options);
        }
    }
}
