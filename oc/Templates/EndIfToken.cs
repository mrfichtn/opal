namespace Opal.Templates
{
	public class EndIfToken : IToken
	{
		public void Write(FormatContext context) => context.Pop();
	}

}
