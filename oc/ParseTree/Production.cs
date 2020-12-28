using Opal.Productions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opal.ParseTree
{
    public class Production
    {
        private readonly Identifier name;
        private readonly ProductionAttr? attr;
        private readonly ProdDefList definitions;
        
        public Production(Token name, 
            ProductionAttr? attr, 
            ProdDefList definitions)
            : this(new Identifier(name), attr, definitions)
        {
        }

        public Production(Identifier name,
            ProductionAttr? attr,
            ProdDefList definitions)
        {
            this.name = name;
            this.attr = attr;
            this.definitions = definitions;
        }

        public ProductionAttr? Attribute => attr;

        public Identifier Name => name;

        public ProdDefList Definitions => definitions;

        public void DeclareTokens(DeclareTokenContext context)
        {
            foreach (var item in definitions)
                item.DeclareTokens(context);
        }

        public IEnumerable<ProductionExpr> Expressions =>
            definitions.SelectMany(x => x.Expressions);

        public void AddActionType(Productions.ITypeTable typeTable)
        {
            if ((attr != null) && 
                !attr.IsMethod && 
                (attr.Option != null) &&
                (attr.Option.Value != "ignore"))
            {
                typeTable.AddPrimary(name.Value, attr.Option.Value);
            }

            foreach (var def in Definitions)
                def.AddActionType(name.Value, typeTable);
        }

        //public Production(Identifier id, 
        //    ProdDef definition, 
        //    ProductionAttr? attr = null)
        //{
        //    Left = id;
        //    Attr = attr;
        //    _right = definition.Right;
        //    if (attr != null)
        //    {
        //        SetAttribute(attr.Option);
        //        CallMethod = attr.IsMethod;
        //    }
        //    Action = definition.Action;
        //}

        //public Production(Segment segment, string name)
        //{
        //    Left = new Identifier(segment, name);
        //    _right = new ProductionExprList();
        //}

        //#region Properties

        //public ProductionAttr? Attr { get; }

        //public void InsertStrings(ProductionContext context)
        //{
        //    foreach (var expr in Right)
        //        expr.InsertStrings(context);
        //}

        //#region RuleId
        //public int RuleId { get; set; }
        //#endregion

        //#region Index
        ///// <summary>
        ///// Index found in original productions container
        ///// </summary>
        //public int Index
        //{
        //    get { return _declIndex; }
        //    set { _declIndex = value; }
        //}
        //private int _declIndex;
        //#endregion

        //#region Id Property
        ///// <summary>
        ///// StateId
        ///// </summary>
        //public int Id
        //{
        //    get { return _id; }
        //    set { _id = value; }
        //}
        //private int _id;
        //#endregion

        //#region Left Property
        //public Identifier Left { get; set; }
        //#endregion

        //public string Name => Left.Value;

        //#region Right Property
        //public ProductionExprList Right
        //{
        //    get { return _right; }
        //}
        //private readonly ProductionExprList _right;
        //#endregion

        //public Identifier? Type { get; set; }
        //public bool CallMethod { get; private set; }
        //public ActionExpr? Action { get; set; } 

        //#endregion

        //public void GetTypes(HashSet<string> types)
        //{
        //    if ((Type != null) && !CallMethod)
        //        types.Add(Type.Value);
        //    else if (Action != null)
        //        Action.GetTypes(types);
        //}

        //private void SetAttribute(Identifier attr)
        //{
        //    if (attr.Value == "ignore")
        //        ignore = true;
        //    else
        //        Type = attr;
        //}

        //public bool HasItem(int position, int id) =>
        //    (position < _right.Count) && (_right[position].Id == id);

        //public void Write(Generator generator, ProductionList parent, NoActionOption option)
        //{
        //    var rightCount = _right.Count;

        //    generator.Write("case {0}:", RuleId)
        //        .WriteLine($" // {this}")
        //        .Indent();
        //        //.StartBlock();

        //    //if (rightCount != 0)
        //    //    generator.WriteLine("state = _stack.SetItems({0})", rightCount).Write("    .Reduce({0}, ", Id);
        //    //else
        //    //    generator.Write("state = _stack.Push({0}, ", Id);
        //    if (rightCount != 0)
        //    {
        //        generator.WriteLine($"items = {rightCount};");
        //        generator.Write($"state = Reduce({Id}, ");
        //    }
        //    else
        //    {
        //        generator.Write($"state = Push({Id}, ");
        //    }

        //    if (Action != null)
        //        Action.Write(new ActionWriteContext(generator, parent, this, true));
        //    else
        //        WriteAttributed(generator, parent, option);

        //    generator.WriteLine(");")
        //        .WriteLine("break;")
        //        .UnIndent();
        //        //.EndBlock();
        //}

        //public void WriteAttributed(IGenerator generator, ProductionList parent, NoActionOption option)
        //{
        //    var ignore = this.ignore || (_right.Count == 0 && Type == null);
        //    var retType = Type ?? Left;
        //    var finalArgs = new StringBuilder();
        //    string? first = null;
        //    var argc = 0;
        //    for (var i = 0; i < _right.Count; i++)
        //    {
        //        var right = _right[i];
        //        if (right.Ignore)
        //        {
        //            continue;
        //        }
        //        else if (right.CallMethod)
        //        {
        //            finalArgs.Append(right.PropName)
        //                .Append('(')
        //                .Append("At");
        //            if (right.Type != null)
        //                finalArgs.Append('<').Append(right.Type.Value).Append('>');
        //            finalArgs.Append('(').Append(i).Append("))");
        //        }
        //        else
        //        {
        //            parent.DefaultTypes.TryGetValue(right.Id, out var type);
        //            finalArgs.Append("At");
        //            right.WriteType(finalArgs, type);
        //            finalArgs.Append('(').Append(i).Append(')');
        //        }
        //        if (argc++ == 0)
        //        {
        //            if (right.CallMethod)
        //                first = finalArgs.ToString();
        //            else
        //                first = $"At({i})";
        //        }
        //        finalArgs.Append(',');
        //    }
        //    if (finalArgs.Length > 0)
        //        finalArgs.Length--;

        //    if (ignore)
        //    {
        //        generator.Write("null");
        //    }
        //    else if (Type == null)
        //    {
        //        if (argc == 1)
        //            generator.Write("{0}", first!);
        //        else
        //            switch (option)
        //            {
        //                case NoActionOption.First:
        //                    generator.Write("{0}", first ?? string.Empty);
        //                    break;
        //                case NoActionOption.Null:
        //                    generator.Write("null");
        //                    break;
        //                case NoActionOption.Tuple:
        //                    generator.Write("Tuple.Create({0})", finalArgs);
        //                    break;
        //            }
        //    }
        //    else if (CallMethod)
        //    {
        //        generator
        //            .Write(retType.Value)
        //            .Write('(')
        //            .Write(finalArgs.ToString())
        //            .Write(")");
        //    }
        //    else if (retType.Value == "false" || retType.Value == "true")
        //    {
        //        generator.Write("{0}", retType.Value);
        //    }
        //    else
        //    {
        //        generator.Write("new {0}(", retType)
        //                .Write(finalArgs.ToString())
        //                .Write(")");
        //    }
        //}

        //public override string ToString()
        //{
        //    var builder = new StringBuilder();
        //    builder.AppendFormat("{0} = ", Left);
        //    var isFirst = true;
        //    foreach (var item in Right)
        //    {
        //        if (isFirst)
        //            isFirst = false;
        //        else
        //            builder.Append(' ');
        //        builder.Append(item);
        //    }
        //    builder.Append(';');
        //    return builder.ToString();
        //}
    }
}
