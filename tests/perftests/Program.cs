using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opal;

namespace perftests
{
	public static class Program
	{
		public static void Main()
		{
			var opalDef = @"d:\src\opal\tests\opal.txt";
			var mapleDef = @"D:\src\opal\tests\OpalTests\maple.sql.cs";
			var testArgs = new TestArgs(10000)
				.Add(opalDef)
				.Add(mapleDef)
				;
			
			var intArrayTest = new IntArrayTest();
			var fullData = intArrayTest.Data;
			var stateScannerTest = new StateScannerTest();
			var switchScannerTest = new SwitchScannerTest();

			var tests = new TestBase[]
			{
				//intArrayTest,
				//new ByteArrayTest(fullData),
				//new SparseArrayTest(fullData),
				//new IntSparseArrayTest(fullData),
				stateScannerTest,
				switchScannerTest
			};

			var mapleSource = new SourceFile(mapleDef);
			var tokens1 = StateScannerTest.States(mapleSource.Source);
			var tokens2 = SwitchScannerTest.States(mapleSource.Source);

			Console.WriteLine("Comparing token streams");
			int length;
			if (tokens1.Length != tokens2.Length)
			{
				Console.WriteLine($"State length:  {tokens1.Length}");
				Console.WriteLine($"Switch length: {tokens2.Length}");
				length = Math.Min(tokens1.Length, tokens2.Length);
			}
			else
			{
				length = tokens2.Length;
				Console.WriteLine($"Lengths:  {length}");
			}
			for (var i = 0; i < length; i++)
            {
				var t1 = tokens1[i];
				var t2 = tokens2[i];

				if (t1.State != t2.State ||
					t1.Value != t2.Value)
                {
					Console.WriteLine(i);
					Console.WriteLine("  t1: {0}", tokens1[i]);
					Console.WriteLine("  t2: {0}", tokens2[i]);
                }
            }

			//foreach (var test in tests)
			//	test.Test(testArgs);
		}
	}
}
