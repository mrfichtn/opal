/* Test */
using System.Linq.Expressions;

namespace CalcTest;

Options
	scanner					= "state"
	emit_token_states		= false
	no.token				= true
	no.buffer				= true
	no.logger				= true
	no.lrstack				= true
	no.parser				= true
	scanner.compress		= false

Tokens
	t1 = "aaa" ("bbbb")?;
	t2 = "bbb";

Productions expr
	expr	= t1 
			| t2
			;
