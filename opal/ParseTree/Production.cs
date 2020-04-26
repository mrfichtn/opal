using Generators;
using Opal.ParseTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Opal.ParseTree
{
    public class Production
    {
        private bool _ignore;

        public Production()
        {
            _right = new ProductionExprs();
        }

        public Production(Token id, ProductionAttr attr, ProductionExprs right, ActionExpr action)
        {
            Left = new Identifier(id);
            _right = right;
            if (attr != null)
            {
                SetAttribute(attr.Option);
                CallMethod = attr.IsMethod;
            }
            Action = action;
        }

        public Production(Token id, ProductionAttr attr, ProdDef definition)
        {
            Left = new Identifier(id);
            _right = definition.Right;
            if (attr != null)
            {
                SetAttribute(attr.Option);
                CallMethod = attr.IsMethod;
            }
            Action = definition.Action;
        }

        public Production(Segment segment, string name)
        {
            Left = new Identifier(segment, name);
            _right = new ProductionExprs();
        }

        #region Properties

        #region RuleId
        public int RuleId { get; set; }
        #endregion

        #region Index
        /// <summary>
        /// Index found in original productions container
        /// </summary>
        public int Index
        {
            get { return _declIndex; }
            set { _declIndex = value; }
        }
        private int _declIndex;
        #endregion

        #region Id Property
        /// <summary>
        /// StateId
        /// </summary>
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private int _id;
        #endregion

        #region Left Property
        public Identifier Left { get; set; }
        #endregion

        #region Right Property
        public ProductionExprs Right
        {
            get { return _right; }
        }
        private readonly ProductionExprs _right;
        #endregion

        public Identifier Type { get; set; }
        public bool CallMethod { get; set; }
        public ActionExpr Action { get; set; } 

        #endregion

        public void GetTypes(HashSet<string> types)
        {
            if ((Type != null) && !CallMethod)
                types.Add(Type.Value);
            else if (Action != null)
                Action.GetTypes(types);
        }

        public void SetAttribute(Identifier attr)
        {
            if (attr.Value == "ignore")
                _ignore = true;
            else if (Type == null)
                Type = attr;
            else
                Type = new Identifier(Type, attr);
        }

        public bool HasItem(int position, int id)
        {
            return (position < _right.Count) && (_right[position].Id == id);
        }

        public void Write(Generator generator, ProductionList parent, NoActionOption option)
        {
            var rightCount = _right.Count;

            generator.Write("case {0}:", RuleId)
                .WriteLine($" // {this}")
                .StartBlock();

            if (rightCount != 0)
                generator.WriteLine("state = _stack.SetItems({0})", rightCount).Write("    .Reduce({0}, ", Id);
            else
                generator.Write("state = _stack.Push({0}, ", Id);

            if (Action != null)
                Action.Write(generator, parent, this, true);
            else
                WriteAttributed(generator, parent, option);

            generator.WriteLine(");")
                .WriteLine("break;").EndBlock();
        }

        public void WriteAttributed(IGenerator generator, ProductionList parent, NoActionOption option)
        {
            var ignore = _ignore || (_right.Count == 0 && Type == null);
            var retType = Type ?? Left;
            var finalArgs = new StringBuilder();
            string first = null;
            var argc = 0;
            for (var i = 0; i < _right.Count; i++)
            {
                var right = _right[i];
                if (right.Ignore)
                    continue;
                else if (right.CallMethod)
                {
                    finalArgs.Append(right.PropName).Append('(');
                    if (right.Type != null)
                        finalArgs.Append('(').Append(right.Type.Value).Append(')');
                    finalArgs.Append("_stack[").Append(i).Append("])");
                }
                else
                {
                    parent.DefaultTypes.TryGetValue(right.Id, out var type);
                    right.WriteType(finalArgs, type);
                    finalArgs.Append("_stack[").Append(i).Append("]");
                }
                if (argc++ == 0)
                {
                    if (right.CallMethod)
                        first = finalArgs.ToString();
                    else
                        first = string.Format("_stack[{0}]", i);
                }
                finalArgs.Append(',');
            }
            if (finalArgs.Length > 0)
                finalArgs.Length--;

            if (ignore)
            {
                generator.Write("null");
            }
            else if (Type == null)
            {
                if (argc == 1)
                    generator.Write("{0}", first);
                else
                    switch (option)
                    {
                        case NoActionOption.First:
                            generator.Write("{0}", first);
                            break;
                        case NoActionOption.Null:
                            generator.Write("null");
                            break;
                        case NoActionOption.Tuple:
                            generator.Write("Tuple.Create({0})", finalArgs);
                            break;
                    }
            }
            else if (CallMethod)
            {
                generator
                    .Write(retType.Value)
                    .Write('(')
                    .Write(finalArgs.ToString())
                    .Write(")");
            }
            else if (retType.Value == "false" || retType.Value == "true")
            {
                generator.Write("{0}", retType.Value);
            }
            else
            {
                generator.Write("new {0}(", retType)
                        .Write(finalArgs.ToString())
                        .Write(")");
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("{0} = ", Left);
            var isFirst = true;
            foreach (var item in Right)
            {
                if (isFirst)
                    isFirst = false;
                else
                    builder.Append(" ");
                builder.Append(item);
            }
            builder.Append(";");
            return builder.ToString();
        }
    }
}
