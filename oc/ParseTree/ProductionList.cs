using System;
using System.Collections.Generic;
using System.Linq;
using Generators;
using Opal.Containers;
using Opal.Index;
using Opal.Nfa;

namespace Opal.ParseTree
{
	public class ProductionList: IEnumerable<Production>
	{
        private readonly List<Production> productions;
        
        public ProductionList()
		{
            productions = new List<Production>();
            Symbols = new Symbols();
            DefaultTypes = new Dictionary<int, string>();
		}

        public ProductionList Add(Token identifier, ProductionAttr attr, ProdDefList defs)
        {
            foreach (var def in defs)
                Add(new Production(identifier, attr, def));
            return this;
        }

        public ProductionList(Production p)
            : this()
        {
            Add(p);
        }

        public ProductionList SetLanguage(Token name)
        {
            Language = new Identifier(name);
            return this;
        }

        public static ProductionList SetLanguage(ProductionList prods, Token name)
        {
            prods.Language = new Identifier(name);
            return prods;
        }

		#region Properties
        public Identifier? Language { get; private set; }
        public int TerminalCount { get; private set; }
        public int SymbolCount => Symbols.Count;
        public Symbols Symbols { get; }
        public Dictionary<int, string> DefaultTypes { get; }
        public Production this[int index] => productions[index];
        #endregion

        public void Add(Production production)
		{
			production.Index = productions.Count;
			productions.Add(production);
		}

        public static ProductionList Add(ProductionList prods, Production prod)
        {
            prods.Add(prod);
            return prods;
        }

		/// <summary>
		/// Merges token states into production map
		/// </summary>
		/// <param name="stateMap"></param>
		public bool SetStates(ILogger logger, AcceptingStates stateMap)
		{
            TerminalCount = stateMap.Count;
            Symbols.Add(stateMap.Symbols);

            if (!MarkTerminals(logger))
                return false;

            Reduce(logger);
            MarkNonTerminals();

            var types = new HashSet<string>();
            foreach (var g in productions.GroupBy(x=>x.Id))
            {
                types.Clear();
                foreach (var item in g)
                    item.GetTypes(types);
                if (types.Count == 1)
                    DefaultTypes.Add(g.Key, types.First());
            }
            for (var i = 0; i < TerminalCount; i++)
                DefaultTypes.Add(i, "Token");

            return true;
		}

		public int GetId(string name) => Symbols.AddOrGet(name);

		public void ExpandQuantifiers()
		{
			int temp = 0;
			int length = productions.Count;

			string tempName;
			int stateId;
			Production tempExpr;

			var fmt = "$temp{0}";

			for (int i = 0; i < length; i++)
			{
				var prod = productions[i];
				for (int j = 0; j < prod.Right.Count; j++)
				{
					var expr = prod.Right[j];
					switch (expr.Quantifier)
					{
						case Quantifier.One:
							break;

						case Quantifier.Question:
							tempName = string.Format(fmt, temp++);
							stateId = GetId(tempName);
							tempExpr = new Production(prod.Left, tempName)
							{
								Id = stateId
							};
							Add(tempExpr);
							tempExpr = new Production(prod.Left, tempName)
							{
								Id = stateId,
							};
							expr.Quantifier = Quantifier.One;
							tempExpr.Right.Add(expr);
							Add(tempExpr);
							prod.Right[j] = new SymbolProd(expr, tempName, stateId);
							break;

                        case Quantifier.Star:
                            tempName = string.Format(fmt, temp++);
                            stateId = GetId(tempName);
                            tempExpr = new Production(prod.Left, tempName)
                            {
                                Id = stateId
                            };
                            Add(tempExpr);
                            tempExpr = new Production(prod.Left, tempName)
                            {
                                Id = stateId,
                            };
                            expr.Quantifier = Quantifier.One;
                            tempExpr.Right.Add(expr);
                            Add(tempExpr);
                            prod.Right[j] = new SymbolProd(expr, tempName, stateId);
                            break;


                        case Quantifier.Plus:
							expr.Quantifier = Quantifier.One;

							tempName = string.Format(fmt, temp++);
							stateId = GetId(tempName);
							tempExpr = new Production(prod.Left, tempName)
							{
								Id = stateId
							};
							tempExpr.Right.Add(expr);
							Add(tempExpr);

							tempExpr = new Production(prod.Left, tempName)
							{
								Id = stateId,
							};
							tempExpr.Right.Add(new SymbolProd(expr, tempName, stateId));
							tempExpr.Right.Add(expr);
							Add(tempExpr);
							prod.Right[j] = new SymbolProd(expr, tempName, stateId);
							break;
					}
				}
			}
		}

        private bool MarkTerminals(ILogger logger)
        {
            var isOk = true;
            var nonTerminals = productions
                .Select(x => x.Left.Value)
                .ToSet();

            foreach (var prod in productions)
            {
                foreach (var expr in prod.Right.OfType<SymbolProd>())
                {
                    if (Symbols.TryGetIndex(expr.Name, out var index))
                    {
                        expr.Id = index;
                        expr.IsTerminal = true;
                    }
                    else if (!nonTerminals.Contains(expr.Name))
                    {
                        var newState = Symbols.Add(expr.Name);
                        expr.SetTerminal(newState);
                        TerminalCount = index;
                        logger.LogMessage(Importance.High, expr, "production expr '{0}' is not defined", expr.Name);
                        isOk = false;
                    }
                }
            }
            return isOk;
        }

        private void Reduce(ILogger logger)
		{
            if (Language == null)
                return;

            var start = Language.Value;
            var notFound = new HashSet<string>(productions.Select(x => x.Left.Value));
            var prods = new HashSet<int>(Enumerable.Range(0, productions.Count));
            notFound.Remove(start);
            var stack = new Stack<Production>(productions.Where(x => x.Left.Value == start));
            while (stack.Count > 0)
            {
                var production = stack.Pop();
                prods.Remove(production.Index);
                foreach (var expr in production.Right.OfType<SymbolProd>().Where(x => !x.IsTerminal && notFound.Remove(x.Name)))
                {
                    foreach (var prod in productions.Where(x => x.Left.Value == expr.Name))
                    {
                        if (prods.Remove(prod.Index))
                            stack.Push(prod);
                    }
                }
            }

            foreach (var index in prods.OrderBy(x=>x))
            {
                var production = productions[index];
                logger.LogWarning(production.Left, "found and removed unreachable rule\n    {0}", production);
            }
            foreach (var index in prods.OrderByDescending(x => x))
                productions.RemoveAt(index);
        }

        private void MarkNonTerminals()
        {
            foreach (var prod in productions)
            {
                prod.Id = GetId(prod.Left.Value);
                foreach (var item in prod.Right.OfType<SymbolProd>().Where(x=>!x.IsTerminal))
                    item.Id = GetId(item.Name);
            }
        }

        public void Write(Generator generator, string noAction)
		{
            var option = GetOption(noAction);
            generator.Indent(1);
			foreach (var item in productions)
				item.Write(generator, this, option);
            generator.UnIndent(1);
        }

        private static NoActionOption GetOption(string noAction)
        {
            NoActionOption option;
            if (string.IsNullOrEmpty(noAction))
                option = NoActionOption.First;
            else if (noAction.Equals("first", StringComparison.InvariantCultureIgnoreCase))
                option = NoActionOption.First;
            else if (noAction.Equals("tuple", StringComparison.InvariantCultureIgnoreCase))
                option = NoActionOption.Tuple;
            else
                option = NoActionOption.Null;
            return option;
               
        }

        public IEnumerator<Production> GetEnumerator() => productions.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
