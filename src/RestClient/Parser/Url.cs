namespace RestClient
{
    public class Url : Token
    {
        public Url(int start, string text, Document document) : base(start, text, document)
        { }

        public TextSpan? Method { get; set; }

        public TextSpan? Uri { get; set; }
    }
}
