using Generators;
using Opal.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpalTests.Mocks
{
    public class VarProviderMock : ITemplateContext
    {
        public readonly Dictionary<string, bool> Conditions;
        public readonly Dictionary<string, string> Variables;

        public VarProviderMock()
        {
            Conditions = new Dictionary<string, bool>();
            Variables = new Dictionary<string, string>();
        }
        
        public bool Condition(string varName) =>
            Conditions.TryGetValue(varName, out var value) && value;

        public string Include(string name)
        {
            Variables.TryGetValue(name, out var result);
            return result;
        }

        public bool WriteVariable(Generator generator, string varName)
        {
            var result = Variables.TryGetValue(varName, out var value);
            if (result)
                generator.Write(value);
            return result;
        }
    }
}
