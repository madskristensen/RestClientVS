namespace RestClient
{
    public class Reference : Token
    {
        public Reference(int start, string text, Document document) : base(start, text, document)
        { }

        public TextSpan? Open { get; set; }
        public TextSpan? Value { get; set; }
        public TextSpan? Close { get; set; }
    }
}
