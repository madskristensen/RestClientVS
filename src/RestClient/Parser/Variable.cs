namespace RestClient
{
    public class Variable
    {
        public Variable(ParseItem name, ParseItem value)
        {
            Name = name;
            Value = value;
        }

        public ParseItem Name { get; }
        public ParseItem Value { get; }
    }
}
