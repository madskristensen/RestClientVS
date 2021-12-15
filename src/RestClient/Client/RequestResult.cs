using System.Net.Http;

namespace RestClient.Client
{
    public class RequestResult
    {
        public HttpRequestMessage? Request { get; internal set; }
        public HttpResponseMessage? Response { get; internal set; }
        public string? ErrorMessage { get; internal set; }
        public Request? RequestToken { get; internal set; }
    }
}
