using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RestClient.Client
{
    public class RequestBuilder
    {
        private readonly Request _token;

        public HttpRequestMessage? Request { get; private set; }
        public HttpResponseMessage? Response { get; private set; }

        public RequestBuilder(Request token)
        {
            _token = token;
            BuildRequest();
        }

        private void BuildRequest()
        {
            var expandedUrl = _token.Url?.Uri?.ExpandVariables();

            var message = new HttpRequestMessage
            {
                Method = GetMethod(_token.Url?.Method?.Text),
                RequestUri = new Uri(expandedUrl),
            };

            if (_token.Headers != null)
            {
                foreach (Header header in _token.Headers)
                {
                    var name = header?.Name?.ExpandVariables();
                    var value = header?.Value?.ExpandVariables();

                    message.Headers.Add(name, value);
                }
            }

            if (!message.Headers.Contains("User-Agent"))
            {
                message.Headers.Add("User-Agent", nameof(RestClient));
            }

            Request = message;
        }

        public async Task SendAsync(CancellationToken cancellationToken = default)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                UseDefaultCredentials = true,
            };

            var client = new HttpClient(handler);

            Response = await client.SendAsync(Request, cancellationToken);
        }

        private static HttpMethod GetMethod(string? methodName)
        {
            return methodName?.ToLowerInvariant() switch
            {
                "delete" => HttpMethod.Delete,
                "head" => HttpMethod.Head,
                "options" => HttpMethod.Options,
                "post" => HttpMethod.Post,
                "put" => HttpMethod.Put,
                "trace" => HttpMethod.Trace,
                _ => HttpMethod.Get,
            };
        }
    }
}
