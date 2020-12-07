using Generators;
using Opal.Containers;
using System.Collections.Generic;

namespace Opal.Templates
{
    public class FormatContext
	{
		private readonly Stack<bool> writeStack;

		public FormatContext(Generator generator,
			ITemplateContext templateContext)
		{
			Generator = generator;
			TemplateContext = templateContext;
			writeStack = new Stack<bool>();
			Write = true;
		}

		public Generator Generator { get; }
		public ITemplateContext TemplateContext { get; }

		public bool Write { get; private set; }

		public void WriteVar(string name)
		{
			if (Write)
				TemplateContext.WriteVariable(Generator, name);
		}

		public void Include(string name)
		{
			var text = TemplateContext.Include(name);
			if (string.IsNullOrEmpty(text))
				text = Resources.LoadText(name);

			var processor = new TemplateProcessor2(text);
			processor.Format(Generator, TemplateContext);
		}

		public void Push(bool value)
		{
			writeStack.Push(Write);
			Write = value;
		}
		public bool Pop()
		{
			if (writeStack.Count > 0)
				Write = writeStack.Pop();
			return Write;
		}

		public void WriteBlock(string text)
		{
			if (Write)
				Generator.WriteBlock(text);
		}
	}


}
