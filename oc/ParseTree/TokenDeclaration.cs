using Opal.Nfa;

namespace Opal.ParseTree
{
    public class TokenDeclaration
    {
        private readonly Identifier name;
        private readonly Token attr;
        private readonly TokenExpr expr;

        public TokenDeclaration(Token name,
            Token attr,
            TokenExpr expr)
        {
            this.name = new Identifier(name);
            this.attr = attr;
            this.expr = expr;
        }

        public Graph Build(GraphBuilder builder)
        {
            var graph = expr.BuildGraph(builder);
            return Graph.MarkEnd(id: name,
                attr: attr,
                g: graph);
        }
    }
}
