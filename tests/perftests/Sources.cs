using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perftests
{
    public class Sources: IEnumerable<SourceFile>
    {
        private readonly List<SourceFile> items;

        public Sources()
        {
            items = new List<SourceFile>();
        }

        public void Add(string file) =>
            items.Add(new SourceFile(file));

        public IEnumerator<SourceFile> GetEnumerator() =>
            items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
