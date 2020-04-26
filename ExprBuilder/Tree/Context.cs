using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExprBuilder.Tree
{
    public class Context
    {
        private readonly Dictionary<string, Expression> _variables;

        public Context()
        {
            _variables = new Dictionary<string, Expression>();
        }

        public bool TryGetVariable(string value, out Expression expr)
        {
            return _variables.TryGetValue(value, out expr);
        }

        public ParameterExpression AddVariable(string name, Type type)
        {
            ParameterExpression result;
            if (!_variables.ContainsKey(name))
            {
                result = Expression.Parameter(type, name);
                _variables.Add(name, result);
            }
            else
            {
                result = null;
            }
            return result;
        }

        public bool RmVariable(string name)
        {
            return _variables.Remove(name);
        }
    }
}
