using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opal.LR1;
using OpalTests.Mocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpalTests
{
    [TestClass]
    public class ActionTests
    {
        [TestMethod]
        public void TestIntAction()
        {
            var source = @"
Tokens
	Int = '0'|([1-9][0-9]*);

Productions expr
    expr       =  ""1"" a  { Add($0:string, $1); }
    a          = Int   { new Integer($0); }
         ;
";
            var logger = new LoggerMock();
            var inPath = "tmp.txt";
            File.WriteAllText(inPath, source);
            var compiler = new Opal.Compiler(logger, inPath)
            {
                OutPath = "tmp.out"
            };
            compiler.Compile();

            var msg = logger.ToString();
            Assert.IsNotNull(msg);

            var result = File.ReadAllText(compiler.OutPath);
            Assert.IsNotNull(result);
        }

        //[TestMethod]
        //public void TestIntAction2()
        //{
        //    var grammer = new Grammar();
        //    grammer.Add(new Rule(grammer, 0, new Symbol("a", 1, false), new Symbol("b", 1, false)));
        //    grammer.Add(new Rule(grammer, 1, new Symbol("b")))

        //}
    }
}
