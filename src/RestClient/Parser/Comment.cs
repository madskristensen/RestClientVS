namespace RestClient
{
    public class Comment : Token
    {
        public Comment(int start, string text, Document document) : base(start, text, document)
        { }

        public bool IsSeparator => Text.StartsWith("###");
    }
}
