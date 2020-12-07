namespace Words;

Options
	scanner				= "switch"
	no.parser			= true
	no.token			= true
	no.buffer			= true
	no.logger			= true

Characters
	letter = [a-zA-Z]
	digit					= [0-9]
	hex_digit				= [a-fA-F]+digit
	non_lf                  = ![\n]
	non_asterisk            = ![*]
	non_slash               = ![/]
	
Tokens
	word = letter
		(
			letter |
			'-'
		)*;
	hexadecimal = "0x"
	(
		hex_digit+
	);

	comment = "//" non_lf*;
	multi_line_comment = "/*" 
	(
		non_asterisk
		|
		(
			'*'
			non_slash
		)
	)* "*/";


		
Productions language
	language = word;
	