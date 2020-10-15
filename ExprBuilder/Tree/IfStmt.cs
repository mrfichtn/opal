namespace ExprBuilder.Tree
{
    public class IfStmt: Stmt
    {
        private readonly int expr;
        private readonly Stmt stmt;
        private readonly Stmt elseStmt;

        public IfStmt(int expr, Stmt stmt, Stmt elseStmt)
        {
            this.expr = expr;
            this.stmt = stmt;
            this.elseStmt = elseStmt;
        }
    }
}
