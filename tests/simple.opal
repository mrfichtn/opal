// Test
using System.Linq.Expressions;

namespace CalcTest;

Options
	scanner					= "switch"
	emit_token_states		= false
	no.token				= true
	no.buffer				= true
	no.logger				= true
	no.lrstack				= true
	//no.parser				= true
	scanner.compress		= false

Tokens
	comma = ",";
	t1 = "aaa" ("bbbb")?;
	t2 = "bbb";
	t3 = "x";

Productions expr
	expr	= t1							{ Expr.Add($0); }
			| t2                            { $0; }
			| t3_list                       { $0; }
			;

	t3_list = t3							{ new List<T3>($0); }
