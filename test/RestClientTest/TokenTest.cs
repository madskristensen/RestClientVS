using System;
using System.Linq;
using System.Threading.Tasks;
using RestClient;
using Xunit;

namespace RestClientTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class TokenTest
    {
        [Theory]
        [InlineData(@"GET https://example.com")]
        [InlineData(@"post https://example.com?hat&ost")]
        [InlineData(@"options https://example.com")]
        [InlineData(@"Trace https://example.com?hat&ost")]
        public async Task OneLiners(string line)
        {
            var doc = Document.FromLines(line);
            await doc.WaitForParsingCompleteAsync();
            ParseItem first = doc.Items?.FirstOrDefault();

            Assert.NotNull(first);
            Assert.Equal(ItemType.Method, first.Type);
            Assert.Equal(0, first.Start);
            Assert.StartsWith(first.Text, line);
        }

        [Fact]
        public async Task RequestTextAfterLineBreak()
        {
            var lines = new[] { "\r", Environment.NewLine, @"GET https://example.com" };

            var doc = Document.FromLines(lines);
            await doc.WaitForParsingCompleteAsync();

            Request request = doc.Requests?.FirstOrDefault();

            Assert.Equal("GET", request.Method.Text);
            Assert.Equal(3, request.Method.Start);
        }

        [Fact]
        public async Task MultipleRequests()
        {
            var lines = new[] {@"get http://example.com\r\n",
                                "\r\n",
                                "###\r\n",
                                "\r\n",
                                "post http://bing.com"};

            var doc = Document.FromLines(lines);
            await doc.WaitForParsingCompleteAsync();

            Assert.Equal(2, doc.Requests.Count);
        }

        [Fact]
        public async Task RequestWithHeaderAndBody()
        {
            var lines = new[]
            {
                "GET https://example.com",
                "User-Agent: ost",
                "\r\n",
                "{\"enabled\": true}"
            };

            var doc = Document.FromLines(lines);
            await doc.WaitForParsingCompleteAsync();
            Request request = doc.Requests?.FirstOrDefault();

            Assert.Single(doc.Requests);
            Assert.NotNull(request.Body);
        }

        [Fact]
        public async Task RequestWithHeaderAndMultilineBody()
        {
            var lines = new[]
            {
                "GET https://example.com",
                "User-Agent: ost",
                "\r\n",
                "{\r\n",
                "\"enabled\": true\r\n",
                "}"
            };

            var doc = Document.FromLines(lines);
            await doc.WaitForParsingCompleteAsync();
            Request request = doc.Requests.First();

            Assert.NotNull(request.Body);
            Assert.Equal(21, request.Body.Length);
        }

        [Fact]
        public async Task RequestWithHeaderAndBodyAndComment()
        {
            var lines = new[]
            {
          "DELETE https://example.com",
          "User-Agent: ost",
          "#ost:hat",
          "\r\n",
          @"{
\t""enabled"": true
}"
      };

            var doc = Document.FromLines(lines);
            await doc.WaitForParsingCompleteAsync();
            Request first = doc.Requests.FirstOrDefault();

            Assert.NotNull(first);
            Assert.Single(doc.Requests);
            Assert.Equal(ItemType.HeaderName, first.Children.ElementAt(2).Type);
            Assert.Equal(ItemType.Body, first.Children.ElementAt(4).Type);
            Assert.Equal(@"{
\t""enabled"": true
}", first.Body);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t\t")]
        [InlineData("\r")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        public async Task EmptyLines(string line)
        {
            var doc = Document.FromLines(line);
            await doc.WaitForParsingCompleteAsync();
            ParseItem first = doc.Items?.FirstOrDefault();

            Assert.NotNull(first);
            Assert.Equal(ItemType.EmptyLine, first.Type);
            Assert.Equal(0, first.Start);
            Assert.Equal(line, first.Text);
            Assert.Equal(line.Length, first.Length);
            Assert.Equal(line.Length, first.End);
        }

        [Fact]

        public async Task CommentAfterNewLineInRequest()
        {
            var text = new[] { "GET http://bing.com\r\n", "\r\n", "###" };

            var doc = Document.FromLines(text);
            await doc.WaitForParsingCompleteAsync();
            ParseItem comment = doc.Items.ElementAt(3);

            Assert.Equal(23, comment.Start);
        }

        [Fact]
        public async Task BodyAfterComment()
        {
            var text = new[] { @"TraCe https://{{host}}/authors/{{name}}\r\n",
                                "Content-Type: at{{contentType}}svin\r\n",
                                "#ost\r\n",
                                "mads: ost\r\n",
                                "\r\n",
                                "{\r\n",
                                "    \"content\": \"foo bar\",\r\n",
                                "    \"created_at\": \"{{createdAt}}\",\r\n",
                                "\r\n",
                                "    \"modified_by\": \"$test$\"\r\n",
                                "}\r\n",
                                "\r\n",
                                "\r\n",};

            var doc = Document.FromLines(text);
            await doc.WaitForParsingCompleteAsync();
            Request request = doc.Requests.First();

            Assert.NotNull(request.Body);
            Assert.Contains("$test$", request.Body);
            Assert.EndsWith("}", request.Body.Trim());
        }

        [Fact]
        public async Task VariableTokenization()
        {
            var text = $"@name = value";

            var doc = Document.FromLines(text);
            await doc.WaitForParsingCompleteAsync();
            ParseItem name = doc.Items.FirstOrDefault();

            Assert.Equal(0, name.Start);
            Assert.Equal(5, name.Length);
            Assert.Equal(8, name.Next.Start);
            Assert.Equal(5, name.Next.Length);
        }

        [Fact]
        public async Task CommentInBetweenHeaders()
        {
            var text = new[] { @"POST https://example.com",
                                "Content-Type:application/json",
                                "#comment",
                                "Accept: gzip" };

            var doc = Document.FromLines(text);
            await doc.WaitForParsingCompleteAsync();

            Assert.Equal(7, doc.Items.Count);
        }
    }
}