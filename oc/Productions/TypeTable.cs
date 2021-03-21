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
            using var stream = new StreamWriter(filePath);
            foreach (var pair in data)
            {
                stream.Write(pair.Key);
                stream.Write(": ");
                stream.Write(pair.Value.Nullable());
                stream.WriteLine();
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


        public bool TryFindNullable(string name, out NullableType? type)
        {
            var isOk = data.TryGetValue(name, out var rec);
            if (isOk)
                isOk = rec!.TryGetNullable(out type);
            else
                type = null;
            return isOk;
        }

        public void TypeFromAttr(string name, NullableType nullable)
        {
            if (!data.TryGetValue(name, out var rec))
            {
                rec = new TypeRec();
                data.Add(name, rec);
            }
            rec.AddPrimary(nullable);
        }

        public void AddActionType(string name, string? type)
        {
            if (!data.TryGetValue(name, out var rec))
            {
                rec = new TypeRec();
                data.Add(name, rec);
            }
            rec.AddSecondary(type);
        }

        public void AddActionType(string name, NullableType type)
        {
            if (!data.TryGetValue(name, out var rec))
            {
                rec = new TypeRec();
                data.Add(name, rec);
            }
            rec.AddSecondary(type);
        }

        class TypeRec
        {
            private readonly List<string> primary;
            private bool primaryNullable;

            private readonly List<string> secondary;
            private bool secondaryNullable;

            public TypeRec()
            {
                primary = new List<string>();
                secondary = new List<string>();
            }

            public bool AddPrimary(NullableType nullable)
            {
                var ok = true;

                if (!primary.Contains(nullable.TypeName))
                    primary.Add(nullable.TypeName);
                if (nullable.Nullable)
                    primaryNullable = true;
                return ok;
            }

            public void AddSecondary(string? type)
            {
                if (type != null)
                    secondary.Add(type);
                else
                    secondaryNullable = true;
            }

            public void AddSecondary(NullableType type)
            {
                if (type != null)
                {
                    secondary.Add(type.TypeName);
                    if (type.Nullable)
                        secondaryNullable = true;
                }
                else
                {
                    secondaryNullable = true;
                }
            }


            public bool TryGetType(out string? type)
            {
                var ok = primary.Count > 0;
                if (ok)
                {
                    type = primary[0];
                }
                else if (secondary.Count > 0)
                {
                    ok = true;
                    type = secondary[0];
                }
                else 
                {
                    type = null;
                }
                return ok;
            }

            public string Nullable()
            {
                string result;
                if (primary.Count > 0)
                {
                    result = primary[0];
                    if (primaryNullable)
                        result += "?";
                }
                else if (secondary != null)
                {
                    result = secondary[0];
                }
                else
                {
                    result = "(unknown)";
                }
                return result;
            }

            public bool TryGetNullable(out NullableType? type)
            {
                type = (primary.Count > 0) ?
                    new NullableType(primary[0], primaryNullable) : 
                    (secondary.Count > 0) ?
                    new NullableType(secondary[0], secondaryNullable) : 
                    null;
                return (type != null);
            }
        }
    }
}
