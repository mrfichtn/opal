using Opal.Nfa;

namespace Opal.ParseTree
{
    public class NamedCharClass
    {
        public NamedCharClass()
        {
        }

        public NamedCharClass(Token identifier, IMatch chars)
        {
            Name = new Identifier(identifier);
            Chars = chars.Reduce();
        }

        #region Name Property
        public Identifier Name { get; set; }
		#endregion

		#region Chars Property
		public IMatch Chars
		{
			get { return _chars; }
			set { _chars = value; }
		}
		private IMatch _chars;
		#endregion
	}
}
