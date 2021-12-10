namespace RestClient
{
    public class TextSpan : Token
    {
        public TextSpan(int start, string text, Document document) : base(start, text, document)
        { }
    }
}
