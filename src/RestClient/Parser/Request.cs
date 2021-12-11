using System.Collections.Generic;
using System.Linq;

namespace RestClient
{
    public class Request : Token
    {
        public Request(int start, string text, Document document) : base(start, text, document)
        { }

        public List<Token>? Children { get; set; } = new List<Token>();

        public Url? Url { get; set; }

        public List<Header>? Headers { get; } = new List<Header>();

        public BodyToken? Body { get; set; }

        public override string Text
        {
            get => string.Concat(Children.Select(c => c.Text));
            protected set => base.Text = value;
        }

        public override int End
        {
            get
            {
                if (Children.Any())
                {
                    return Children.Last().End;
                }

                return base.End;
            }
        }
    }
}
