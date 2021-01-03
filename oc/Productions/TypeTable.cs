using System.Collections.Generic;
using System.IO;

namespace Opal.Productions
{
    public class TypeTable: ITypeTable
    {
        private readonly Dictionary<string, TypeRec> data;

        public TypeTable()
        {
            data = new Dictionary<string, TypeRec>();
        }

        public void Write(string filePath)
        {
            using (var stream = new StreamWriter(filePath))
            {
                foreach(var pair in data)
                {
                    stream.Write(pair.Key);
                    stream.Write(": ");
                    var found = pair.Value.TryGetType(out var type);
                    stream.Write(found ? type : "(unknown)");
                    stream.WriteLine();
                }
            }
        }

        public bool TryFind(string name, out string? type)
        {
            var isOk = data.TryGetValue(name, out var rec);
            if (isOk)
                isOk = rec!.TryGetType(out type);
            else
                type = null;
            return isOk;
        }

        public bool AddPrimary(string name, string type)
        {
            if (!data.TryGetValue(name, out var rec))
            {
                rec = new TypeRec();
                data.Add(name, rec);
            }
            return rec.SetPrimary(type);
        }

        public void AddSecondary(string name, string type)
        {
            if (!data.TryGetValue(name, out var rec))
            {
                rec = new TypeRec();
                data.Add(name, rec);
            }
            rec.SetSecondary(type);
        }


        class TypeRec
        {
            private string? primary;
            private string? secondary;

            public TypeRec()
            {
            }

            public bool SetPrimary(string type)
            {
                var ok = true;
                if (primary == null)
                    primary = type;
                else if (primary != type)
                    ok = false;
                return ok;
            }

            public void SetSecondary(string type)
            {
                if (secondary == null)
                    secondary = type;
            }

            public bool TryGetType(out string? type)
            {
                var ok = !string.IsNullOrEmpty(primary);
                if (ok)
                {
                    type = primary!;
                }
                else
                {
                    ok = !string.IsNullOrEmpty(secondary);
                    type = ok ? secondary : null;
                }
                return ok;
            }
        }
    }
}
