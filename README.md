# opal

Opal is an LR(1) parser generator somewhat optimized for building tree structures following a parser.  For example, for the following sample file

`using System.Linq.Expressions;
`namespace CalcTest;
`Options
`	scanner                 = "state"
`	syntax.error.tokens     = true
`Characters
`	plus_char   = [+]
`	white_space = [\t \n]
`	digit       = [0-9]
`	alpha       = [a-zA-Z]
`	id	        = [_]+alpha
`	id2         = id + digit 
`Tokens
`	Int = '0'|([1-9]digit*) ;
`	identifier = id id2* ;
`	white_space<ignore> = white_space+ ;
`Productions expr
`
`	expr	= term				
`			| expr '+' term    		{ new AddExpr($0:Expr, $2:Expr); }
`			| expr '-' term			{ new SubExpr($0:Expr, $2:Expr); }
`
`	term	= primary
`			| term '*' primary 		{ new MultiExpr($0:Expr, $2:Expr); }
`			| term '/' primary 		{ new DivExpr($0:Expr, $2:Expr); }
`	
`	primary = Int					{ new Constant($0); }

will parse text, generating a final Expr class.