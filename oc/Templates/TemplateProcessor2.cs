using Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Opal.Containers;

namespace Opal.Templates
{
	public class TemplateProcessor2
	{
		private string templ;

		public TemplateProcessor2(string templ) =>
			this.templ = templ;

        public static void FromFile(Generator generator, 
			ITemplateContext context, 
			string filePath)
        {
            var templ = File.ReadAllText(filePath);
			var processor = new TemplateProcessor2(templ);
			processor.Format(generator, context);
        }

        public static void FromAssembly(Generator generator, 
			ITemplateContext context, 
			string name)
		{
			var templ = Resources.LoadText(name);
			var processor = new TemplateProcessor2(templ);
			processor.Format(generator, context);
		}

		public void Format(Generator generator, 
			ITemplateContext context)
		{
			var scanner = new TemplateScanner(templ);
			var formatContext = new FormatContext(generator, context);

			foreach (var token in scanner.Tokens())
				token.Write(formatContext);
		}
	}
}
