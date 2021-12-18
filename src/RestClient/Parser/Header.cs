namespace RestClient
{
    public class Header
    {
        public Header(ParseItem name, ParseItem value)
        {
            Name = name;
            Value = value;
        }

        public ParseItem Name { get; }
        public ParseItem Value { get; }
    }
}
