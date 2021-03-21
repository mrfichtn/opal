using Opal.Productions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Opal.ParseTree
{
    /// <summary>
    /// Parse action expression
    /// </summary>
    public abstract class ActionExpr: Segment, IReducer
    {
        protected ActionExpr(Segment segment)
            : base(segment)
        { }

        protected ActionExpr()
        { }

        public static readonly ActionEmpty Empty = new ActionEmpty();

        public abstract IReduceExpr Reduce(ReduceContext context);

        public virtual void AddType(DefinitionActionTypeContext context)
        {
        }
    }

    /// <summary>
    /// Positional arg (e.g. $1)
    /// </summary>
    public class ActionArg: ActionExpr, IReducer
    {
        protected readonly int position;

        public ActionArg(Token t)
            : base(t)
        {
            position = int.Parse(t.Value.Substring(1));
        }

        public ActionArg(int position)
        {
            this.position = position;
        }

        public override void AddType(DefinitionActionTypeContext context) =>
            context.AddFromActionExpr(position);

        public override IReduceExpr Reduce(ReduceContext context)
        {
            return context.TryFindProductionType(position, out var type) ?
                new ReduceCastedArgExpr(position, type!) :
                new ReduceArgExpr(position);
        }

        IReduceExpr IReducer.Reduce(ReduceContext context) =>
            new ReduceArgExpr(position);

        public override string ToString() =>
            new StringBuilder('$')
                .Append(position)
                .ToString();
    }

    /// <summary>
    /// Casted, positional arg
    /// </summary>
    public class ActionArgCasted: ActionArg
    {
        public ActionArgCasted(Token t, NullableType cast)
            : base(t)
        {
            Cast = cast;
        }

        public NullableType Cast { get; private set; }

        public override void AddType(DefinitionActionTypeContext context) =>
            context.Add(Cast.TypeName);

        public override string ToString() =>
            new StringBuilder('$')
                .Append(position)
                .Append(Cast.TypeName)
                .ToString();

        public override IReduceExpr Reduce(ReduceContext context) =>
            new ReduceCastedArgExpr(position, Cast.TypeName);

        public static ActionArg Create(Token t, NullableType cast)
        {
            return cast.TypeName switch
            {
                "object" => new ActionArg(t),
                "string" => new ActionArgString(t, cast),
                _ => new ActionArgCasted(t, cast)
            };
        }
    }

    /// <summary>
    /// List of args
    /// </summary>
    public class ActionArgs: List<ActionExpr>
    {
        public ActionArgs()
        { }

        public ActionArgs(ActionExpr arg) =>
            Add(arg);

        public ActionArgs(params ActionExpr[] args)
        {
            foreach (var arg in args)
                Add(arg);
        }

        public IReduceExpr[] Reduce(ReduceContext context) =>
            this.Select(x => x.Reduce(context)).ToArray();

        public static ActionArgs Add(ActionArgs args, ActionExpr arg)
        {
            args.Add(arg);
            return args;
        }
    }

    /// <summary>
    /// Production with no semantic action
    /// </summary>
    public class ActionEmpty: ActionExpr
    {
        public override IReduceExpr Reduce(ReduceContext context) =>
            context.TerminalsReduce();

        public override void AddType(DefinitionActionTypeContext context)
        {
            context.AddTypeFromActionEmpty();
        }
    }

    /// <summary>
    /// Expr casted as string
    /// </summary>
    public class ActionArgString: ActionArgCasted
    {
        public ActionArgString(Token t, NullableType cast)
            : base(t, cast)
        {
        }

        public override void AddType(DefinitionActionTypeContext context) =>
            context.Add(Cast.TypeName);

        public override string ToString() =>
            new StringBuilder('$')
                .Append(position)
                .Append(Cast.TypeName)
                .ToString();


        public override IReduceExpr Reduce(ReduceContext context)
        {
            context.TryFindProductionType(position, out var productionType);
            return (productionType == "Token") ?
                new ReduceFieldExpr(new ReduceCastedArgExpr(position, "Token"), "Value") :
                new ReduceCastedArgExpr(position, Cast.TypeName);
        }
    }

    /// <summary>
    /// String constant
    /// </summary>
    public class ActionStringConstant: ActionExpr
    {
        private readonly StringConst value;

        public ActionStringConstant(StringConst value) =>
            this.value = value;

        public override void AddType(DefinitionActionTypeContext context) =>
            context.Add("string");

        public override IReduceExpr Reduce(ReduceContext context) =>
            new ReduceStringExpr(value.Value);
    }

    /// <summary>
    /// Boolean constant
    /// </summary>
    public class ActionBoolConstant: ActionExpr
    {
        private readonly BoolConst value;
        public ActionBoolConstant(BoolConst value) =>
            this.value = value;

        public override IReduceExpr Reduce(ReduceContext context) =>
            new ReduceValueExpr(value.ToString());

        public override void AddType(DefinitionActionTypeContext context) =>
            context.Add("bool");
    }

    /// <summary>
    /// Function expression
    /// </summary>
    public class ActionFuncExpr: ActionExpr
    {
        protected readonly Identifier id;
        protected readonly ActionArgs args;

        public ActionFuncExpr(Identifier id, ActionArgs args)
            : base(id)
        {
            this.id = id;
            this.args = args;
        }

        public override IReduceExpr Reduce(ReduceContext context) =>
            new ReduceMethodExpr(id.Value, args.Reduce(context));
    }

    /// <summary>
    /// Integer constant
    /// </summary>
    public class ActionIntConstant: ActionExpr
    {
        private readonly Integer value;

        public ActionIntConstant(Integer value) =>
            this.value = value;

        public override void AddType(DefinitionActionTypeContext context) =>
            context.Add("int");

        public override IReduceExpr Reduce(ReduceContext context) =>
            new ReduceValueExpr(value.ToString()!);
    }

    /// <summary>
    /// Member call
    /// </summary>
    public class ActionMember: ActionExpr
    {
        private readonly Identifier id;
        public ActionMember(Identifier id) =>
            this.id = id;

        public override IReduceExpr Reduce(ReduceContext context) =>
            new ReduceValueExpr(id.Value);
    }

    /// <summary>
    /// New expression
    /// </summary>
    public class ActionNewExpr: ActionFuncExpr
    {
        public ActionNewExpr(Identifier id, ActionArgs args)
            : base(id, args)
        {
        }

        public override IReduceExpr Reduce(ReduceContext context) =>
            new ReduceNewExpr(id.Value, args.Reduce(context));

        public override void AddType(DefinitionActionTypeContext context) =>
            context.Add(id.Value);
    }

    /// <summary>
    /// Null constant
    /// </summary>
    public class ActionNullExpr: ActionExpr
    {
        public ActionNullExpr()
        { }

        public ActionNullExpr(Token t) : base(t)
        { }

        public override IReduceExpr Reduce(ReduceContext context) =>
            new ReduceNullExpr();

        public override void AddType(DefinitionActionTypeContext context)
        {
            context.Add(null);
        }
    }
}
