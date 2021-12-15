namespace RestClient
{
    public class BodyToken : Token
    {
        public BodyToken(int start, string text, Document document) : base(start, text, document)
        { }

        internal void Increase(string text)
        {
            Text += text;
            TextExcludingLineBreaks = Text.TrimEnd();
        }

        //// Removes any extra empty lines from the end of the body
        //public override int Length => Text.TrimEnd().Length;
    }
}
