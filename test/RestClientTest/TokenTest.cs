using System;
using System.Linq;
using RestClient;
using Xunit;

namespace RestClientTest
{
    public class TokenTest
    {
        [Theory]
        [InlineData(@"GET https://example.com")]
        [InlineData(@"post https://example.com?hat&ost")]
        [InlineData(@"options https://example.com")]
        [InlineData(@"Trace https://example.com?hat&ost")]
        [InlineData(@"Trace https://example.com?hat&ost HTTP/1.1")]
        public void OneLiners(string line)
        {
            var doc = Document.FromLines(line);
            ParseItem request = doc.Items?.First();
            ParseItem method = doc.Items?.ElementAt(1);

            Assert.NotNull(method);
            Assert.Equal(ItemType.Request, request.Type);
            Assert.Equal(ItemType.Method, method.Type);
            Assert.Equal(0, method.Start);
            Assert.StartsWith(method.Text, line);
        }

        [Fact]
        public void RequestTextAfterLineBreak()
        {
            var lines = new[] { "\r", Environment.NewLine, @"GET https://example.com" };

            var doc = Document.FromLines(lines);

            Request request = doc.Requests?.FirstOrDefault();

            Assert.Equal("GET", request.Method.Text);
            Assert.Equal(3, request.Method.Start);
        }

        [Fact]
        public void RequestWithVersion()
        {
            var lines = new[] { "\r", Environment.NewLine, @"GET https://example.com http/1.1" };

            var doc = Document.FromLines(lines);

            Request request = doc.Requests?.FirstOrDefault();

            Assert.NotNull(request.Version);
            Assert.Equal("http/1.1", request.Version.Text);
        }

        [Fact]
        public void MultipleRequests()
        {
            var lines = new[] {@"get http://example.com\r\n",
                                "\r\n",
                                "###\r\n",
                                "\r\n",
                                "post http://bing.com"};

            var doc = Document.FromLines(lines);

            Assert.Equal(2, doc.Requests.Count);
        }

        [Fact]
        public void RequestWithHeaderAndBody()
        {
            var lines = new[]
            {
                "GET https://example.com",
                "User-Agent: ost",
                "\r\n",
                "{\"enabled\": true}"
            };

            var doc = Document.FromLines(lines);
            Request request = doc.Requests?.FirstOrDefault();

            Assert.Single(doc.Requests);
            Assert.NotNull(request.Body);
        }

        [Fact]
        public void RequestWithHeaderAndMultilineBody()
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
            Request request = doc.Requests.First();

            Assert.NotNull(request.Body);
            Assert.Equal(21, request.Body.Length);
        }

        [Fact]
        public void RequestWithHeaderAndMultilineWwwFormBody()
        {
            var lines = new[]
            {
                "POST https://myserver/mypath/myoperation HTTP/1.1",
                "content-type: application/x-www-form-urlencoded; charset=utf-8",
                "\r\n",
                "f=json\r\n",
                "&inputLocations=123,45;123,46\r\n"
            };

            var doc = Document.FromLines(lines);
            Request request = doc.Requests.First();

            Assert.NotNull(request.Body);
            Assert.Equal("f=json&inputLocations=123,45;123,46", request.Body);
        }

        [Fact]
        public void RequestWithHeaderAndBodyAndComment()
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
        public void EmptyLines(string line)
        {
            var doc = Document.FromLines(line);
            ParseItem first = doc.Items?.FirstOrDefault();

            Assert.NotNull(first);
            Assert.Equal(ItemType.EmptyLine, first.Type);
            Assert.Equal(0, first.Start);
            Assert.Equal(line, first.Text);
            Assert.Equal(line.Length, first.Length);
            Assert.Equal(line.Length, first.End);
        }

        [Fact]
        public void BodyAfterComment()
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
            Request request = doc.Requests.First();

            Assert.NotNull(request.Body);
            Assert.Contains("$test$", request.Body);
            Assert.EndsWith("}", request.Body.Trim());
        }

        [Fact]
        public void VariableTokenization()
        {
            var text = $"@name = value";

            var doc = Document.FromLines(text);
            ParseItem name = doc.Items.FirstOrDefault();

            Assert.Equal(0, name.Start);
            Assert.Equal(5, name.Length);
            Assert.Equal(8, name.Next.Start);
            Assert.Equal(5, name.Next.Length);
        }

        [Fact]
        public void CommentInBetweenHeaders()
        {
            var text = new[] { @"POST https://example.com",
                                "Content-Type:application/json",
                                "#comment",
                                "Accept: gzip" };

            var doc = Document.FromLines(text);

            Assert.Equal(8, doc.Items.Count);
        }
    }
}