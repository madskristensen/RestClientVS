namespace RestClient
{
    public class BodyToken : Token
    {
        public BodyToken(int start, string text, Document document) : base(start, text, document)
        { }

        internal void Increase(string text)
        {
            Text += text;
        }
    }
}
