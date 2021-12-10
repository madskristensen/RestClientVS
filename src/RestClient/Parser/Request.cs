using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RestClient.Client;

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

        public async Task<RequestBuilder> SendAsync(CancellationToken cancellationToken = default)
        {
            var builder = new RequestBuilder(this);
            await builder.SendAsync(cancellationToken);
            return builder;
        }
    }
}
