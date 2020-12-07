namespace Opal.Templates
{
	public class FalseCondition : ICondition
	{
		public bool Eval(FormatContext context) => false;
	}
}
