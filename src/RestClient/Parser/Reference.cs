namespace RestClient
{
    public class Reference
    {
        public Reference(ParseItem open, ParseItem value, ParseItem close)
        {
            Open = open;
            Value = value;
            Close = close;
        }

        public ParseItem Open { get; }
        public ParseItem Value { get; }
        public ParseItem Close { get; }
    }
}
