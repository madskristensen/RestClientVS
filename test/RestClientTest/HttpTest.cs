using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using RestClient;
using RestClient.Client;
using Xunit;

namespace RestClientTest
{
    public class HttpTest
    {
        [Theory]
        [InlineData("delete https://bing.com")]
        [InlineData("POST https://bing.com")]
        [InlineData("PUT https://api.github.com/users/madskristensen")]
        [InlineData("get https://api.github.com/users/madskristensen")]
        public async Task SendAsync(string url)
        {
            var doc = Document.FromLines(url);

            RequestResult client = await RequestSender.SendAsync(doc.Requests.First(), TimeSpan.FromSeconds(10));
            var raw = await client.Response.ToRawStringAsync();

            Assert.NotNull(client.Response);
            Assert.True(raw.Length > 50);
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("application/json; charset=utf-8")]
        public void AddHeadersParseContentTypeTest(string contentType)
        {
            var lines = new[] 
            {
                "POST https://test.fake/api/users/add HTTP/1.1",
                "Content-type: " + contentType,
                "Accept: application/json",
                "",
                "{",
                "}"
            };

            var doc = Document.FromLines(lines);

            Request request = doc.Requests?.FirstOrDefault();

            //var nullParseItem = new ParseItem(0, string.Empty, doc, ItemType.EmptyLine);

            //var request = new Request(doc, nullParseItem, nullParseItem, null);

            HttpRequestMessage message = new ();

            RequestSender.AddHeaders(request, message);

            Assert.Equal(contentType, 
                request.Headers.Where(
                h => h.Name.Text.IsTokenMatch("content-type"))
                .First().Value.Text.Trim());
        }
    }
}
