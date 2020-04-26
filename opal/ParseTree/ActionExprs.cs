using Generators;
using Opal.ParseTree;
using System.Collections.Generic;
using System.Text;

namespace Opal.ParseTree
{
    public class ActionExpr: Segment
    {
        public ActionExpr(Segment segment)
            : base(segment)
        {}

        public ActionExpr()
        {}

        /// <summary>
        /// Writes action code
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="prods"></param>
        /// <param name="prod"></param>
        /// <param name="root">True, if written from production</param>
        public virtual void Write(Generator generator, ProductionList prods, Production prod, bool root = false)
        {}

        /// <summary>
        /// Returns types found in action expression
        /// </summary>
        /// <param name="types"></param>
        public virtual void GetTypes(HashSet<string> types)
        { }
    }

    public class ActionArg: ActionExpr
    {
        private readonly int _position;

        public ActionArg(Token t): base(t)
        {
            _position = int.Parse(t.Value.Substring(1));
        }

        public ActionArg(Token t, Identifier cast)
            : this(t)
        {
            Cast = cast;
        }

        public Identifier Cast { get; set; }

        /// <summary>
        /// True if written from production
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="prods"></param>
        /// <param name="prod"></param>
        /// <param name="root"></param>
        public override void Write(Generator generator, ProductionList prods, Production prod, bool root)
        {
            //Attempt to find a default type
            var productionType = FindProductionType(prods, prod);

            //If true, appends .Value to token to access token value
            //  (production type is token and the user specified a 'string' cast)
            var tokenValue = false;
            if (Cast == null)
            {
                if (productionType != null && !root)
                    generator.Write("({0})", productionType);
            }
            else if (Cast.Value == "string")
            {
                generator.Write('(');
                if (productionType == "Token")
                {
                    generator.Write('(')
                        .Write(productionType);
                    tokenValue = true;
                }
                else
                {
                    generator.Write(Cast);
                }
                generator.Write(')');
            }
            else if (Cast.Value != "object")
            {
                generator.Write('(').Write(Cast).Write(") ");
            }
            generator.Write("_stack[{0}]", _position);
            if (tokenValue)
                generator.Write(").Value");
        }

        /// <summary>
        /// Examine productions to see if we can determine its type
        /// </summary>
        /// <param name="prods">Productions</param>
        /// <param name="prod">Current production</param>
        /// <returns>Type, if it can be determined</returns>
        private string FindProductionType(ProductionList prods, Production prod)
        {
            string productionType;
            if (_position < prod.Right.Count)
            {
                var prodExpr = prod.Right[_position];
                prods.DefaultTypes.TryGetValue(prodExpr.Id, out productionType);
            }
            else
            {
                productionType = null;
            }

            return productionType;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append('$')
                .Append(_position);
            if (Cast != null)
                builder.Append(Cast.Value);
            return builder.ToString();
        }
    }

    public class ActionFuncExpr: ActionExpr
    {
        protected readonly Identifier _id;
        private readonly ActionArgs _args;
        public ActionFuncExpr(Identifier id, ActionArgs args)
            : base(id)
        {
            _id = id;
            _args = args;
        }

        public override void Write(Generator generator, ProductionList prods, Production prod, bool isRoot)
        {
            generator.Write(_id).Write('(');
            _args.Write(generator, prods, prod);
            generator.Write(')');
        }
    }

    public class ActionNewExpr: ActionFuncExpr
    {
        public ActionNewExpr(Identifier id, ActionArgs args)
            : base(id, args)
        {
        }

        public override void Write(Generator generator, ProductionList prods, Production prod, bool isRoot)
        {
            generator.Write("new ");
            base.Write(generator, prods, prod, isRoot);
        }

        public override void GetTypes(HashSet<string> types)
        {
            types.Add(_id.Value);
        }
    }

    public class ActionArgs : List<ActionExpr>
    {
        public ActionArgs()
        {}

        public ActionArgs(ActionExpr arg)
        {
            Add(arg);
        }

        public static ActionArgs Add(ActionArgs args, ActionExpr arg)
        {
            args.Add(arg);
            return args;
        }

        public void Write(Generator generator, ProductionList prods, Production prod)
        {
            var isFirst = true;
            foreach (var item in this)
            {
                if (isFirst) isFirst = false; else generator.Write(',');
                item.Write(generator, prods, prod);
            }
        }
    }

    public class ActionNullExpr: ActionExpr
    {
        public ActionNullExpr()
        {
        }

        public ActionNullExpr(Token t): base(t)
        {
        }

        public override void Write(Generator generator, ProductionList prods, Production prod, bool isRoot)
        {
            generator.Write("null");
        }
    }

    public class ActionMember: ActionExpr
    {
        private readonly Identifier _id;
        public ActionMember(Identifier id)
        {
            _id = id;
        }

        public override void Write(Generator generator, ProductionList prods, Production prod, bool isRoot)
        {
            generator.Write(_id);
        }
    }

    public class ActionIntConstant: ActionExpr
    {
        private readonly Integer _value;

        public ActionIntConstant(Integer value)
        {
            _value = value;
        }

        public override void Write(Generator generator, ProductionList prods, Production prod, bool root = false)
        {
            generator.Write(_value.ToString());
        }

        public override void GetTypes(HashSet<string> types)
        {
            types.Add("int");
        }
    }

    public class ActionStringConstant : ActionExpr
    {
        private readonly StringConst _value;

        public ActionStringConstant(StringConst value)
        {
            _value = value;
        }

        public override void Write(Generator generator, ProductionList prods, Production prod, bool root = false)
        {
            generator.Write(_value.ToString());
        }

        public override void GetTypes(HashSet<string> types)
        {
            types.Add("string");
        }
    }
}
