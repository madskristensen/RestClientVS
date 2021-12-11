using System.Linq;
using System.Threading.Tasks;
using RestClient;
using RestClient.Client;
using Xunit;

namespace RestClientTest
{
    public class HttpTest
    {
        [Theory]
        [InlineData("https://bing.com")]
        [InlineData("POST https://bing.com")]
        [InlineData("PUT https://api.github.com/users/madskristensen")]
        [InlineData("get https://api.github.com/users/madskristensen")]
        public async Task SendAsync(string url)
        {
            var doc = Document.FromLines(url);
            RequestResult client = await RequestSender.SendAsync(doc.Requests.First());
            var raw = await client.Response.ToRawStringAsync();

            Assert.NotNull(client.Response);
            Assert.True(raw.Length > 50);
        }
    }
}
