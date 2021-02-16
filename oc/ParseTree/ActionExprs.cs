using Opal.Productions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Opal.ParseTree
{
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

    public class ActionArg: ActionExpr, IReducer
    {
        protected readonly int position;

        public ActionArg(Token t)
            : base(t)
        {
            position = int.Parse(t.Value.Substring(1));
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

    public class ActionArgs: List<ActionExpr>
    {
        public ActionArgs()
        { }

        public ActionArgs(ActionExpr arg) =>
            Add(arg);

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

    public class ActionMember: ActionExpr
    {
        private readonly Identifier id;
        public ActionMember(Identifier id) =>
            this.id = id;

        public override IReduceExpr Reduce(ReduceContext context) =>
            new ReduceValueExpr(id.Value);
    }

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
