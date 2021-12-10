namespace RestClient
{
    public class Variable : Token
    {
        public Variable(int start, string text, Document document) : base(start, text, document)
        { }

        public TextSpan? At { get; set; }
        public TextSpan? Name { get; set; }
        public TextSpan? Value { get; set; }
        public TextSpan? Operator { get; set; }
    }
}
