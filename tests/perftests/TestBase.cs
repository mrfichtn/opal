using System;
using System.Diagnostics;

namespace perftests
{
    public abstract class TestBase
    {
        private readonly string name;

        protected TestBase(string name)
        {
            this.name = name;
        }

		public void Test(TestArgs args)
        {
			Console.WriteLine($"Algorithm:   {name}");
			foreach (var source in args.Sources)
				Test(source, args.Iterations);
			Console.WriteLine();
		}

		private void Test(SourceFile sourceFile, int iterations)
        {
			Console.WriteLine($"  Source:    {sourceFile.Name}");
			var process = Process.GetCurrentProcess();
			var start = process.TotalProcessorTime;
			//var sw = Stopwatch.StartNew();
			long sum = 0;
			var source = sourceFile.Source;
			for (var i = 0; i < iterations; i++)
				sum += Test(source);
			var end = process.TotalProcessorTime;
			//var elapsed = sw.Elapsed;
			//Console.WriteLine($"    Stopwatch: {elapsed}");
			Console.WriteLine($"    Process:   {end - start}");
			Console.WriteLine($"    Checksum:  {sum}");
		}

		protected abstract long Test(string source);
    }
}
