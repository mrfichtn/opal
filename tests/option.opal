Characters
   plus_char = [+]
   digit     = [0-9]
   white_space = [\t \n]


Tokens 
   Int = '0'|([1-9]digit*) ;
   white_space<ignore> = white_space+ ;

Productions expr
   expr = "start" int_decl*   { new Grammar($1); }
   
   
   int_decl = Int		 { new Integer($0); }
