using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Linq.Expressions;


#nullable enable

namespace CalcTest
{
	
	public class Scanner: StateScannerBase
	{
	    private static readonly int[] _charToClass;
	    private static readonly int[,] _states;
	
		static Scanner()
		{
	        _charToClass = Opal.CharClasses.ToArray(_charToClass);
			
		}
	
	    public Scanner(string source, int line = 1, int column = 0)
			: this(new StringBuffer(source), line, column)
	    {
	    }
	
		public Scanner(IBuffer buffer, int line = 1, int column = 0)
			: base(buffer, line, column)
		{
		}
	
		public static Scanner FromFile(string filePath, int line = 1, int column = 0) =>
			new Scanner(new FileBuffer(filePath), line, column);
	
		private static readonly (char ch, int cls)[] _charToClass = 
		{
			('a', 1), ('b', 2)
		};
	
		int[,] _states = 
		{
			{ 0, 0, 1, 2, 0 },
			{ 0, 0, 5, 0, 0 },
			{ 0, 0, 0, 3, 0 },
			{ 0, 0, 0, 4, 0 },
			{ 2, 0, 0, 0, 0 },
			{ 0, 0, 6, 0, 0 },
			{ 1, 0, 0, 7, 0 },
			{ 0, 0, 0, 8, 0 },
			{ 0, 0, 0, 9, 0 },
			{ 0, 0, 0, 10, 0 },
			{ 1, 0, 0, 0, 0 }
		};
	
	}
	public class TokenStates
	{
		public const int SyntaxError = -1;
		public const int Empty = 0;
	}
	
	
	
}
