namespace Opal.Templates
{
    class ElseToken : IToken
    {
        public void Write(FormatContext context)
        {
            var cond = context.Write;
            if (context.Pop())
                context.Push(!cond);
            else
                context.Push(false);
        }
    }
}
