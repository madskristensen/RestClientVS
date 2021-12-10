namespace RestClient
{
    public class Header : Token
    {
        public Header(int start, string text, Document document) : base(start, text, document)
        { }

        public TextSpan? Name { get; set; }
        public TextSpan? Value { get; set; }
        public TextSpan? Operator { get; set; }
    }
}
