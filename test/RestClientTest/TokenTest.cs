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
        [InlineData(@"https://example.com")]
        [InlineData(@"https://example.com?hat&ost")]
        public async Task OneLiners(string line)
        {
            var doc = Document.FromLines(line);
            await doc.WaitForParsingCompleteAsync();
            Token first = doc.Tokens?.FirstOrDefault();

            Assert.NotNull(first);
            Assert.IsType<Url>(first);
            Assert.Equal(0, first.Start);
            Assert.Equal(line, first.Text);
            Assert.Equal(line.Length, first.Length);
            Assert.Equal(line.Length, first.End);
        }

        [Fact]
        public async Task RequestTextAfterLineBreak()
        {
            var lines = new[] { "\r", Environment.NewLine, @"GET https://example.com" };

            var doc = Document.FromLines(lines);
            await doc.WaitForParsingCompleteAsync();
            var start = 0;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                Token token = doc.Tokens.ElementAt(i);

                Assert.NotNull(token);
                Assert.Equal(start, token.Start);
                Assert.Equal(line, token.Text);
                Assert.Equal(line.Length, token.Length);
                Assert.Equal(start + line.Length, token.End);

                start += line.Length;
            }
        }

        [Fact]
        public async Task MultipleRequests()
        {
            var lines = new[] {@"http://example.com",
                                "",
                                "###",
                                "",
                                "http://bing.com"};

            var doc = Document.FromLines(lines);
            await doc.WaitForParsingCompleteAsync();

            Assert.Equal(2, doc.Requests.Count());
        }

        [Fact]
        public async Task RequestWithHeaderAndBody()
        {
            var lines = new[]
            {
                "https://example.com",
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
                "https://example.com",
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
          "https://example.com",
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
            Assert.Single(doc.Hierarchy);
            Assert.IsType<Comment>(first.Children.ElementAt(2));
            Assert.IsType<BodyToken>(first.Children.ElementAt(3));
            Assert.Equal(@"{
\t""enabled"": true
}", first.Body.Text);
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
            Token first = doc.Tokens?.FirstOrDefault();

            Assert.NotNull(first);
            Assert.IsType<EmptyLine>(first);
            Assert.Equal(0, first.Start);
            Assert.Equal(line, first.Text);
            Assert.Equal(line.Length, first.Length);
            Assert.Equal(line.Length, first.End);
        }

        [Fact]

        public async Task CommentAfterNewLineInRequest()
        {
            var text = new[] { "http://bing.com\r\n", "\r\n", "###" };

            var doc = Document.FromLines(text);
            await doc.WaitForParsingCompleteAsync();
            var comment = doc.Tokens.ElementAt(2) as Comment;

            Assert.Equal(19, comment.Start);
        }

        [Fact]
        public async Task BodyAfterComment()
        {
            var text = new[] { @"PATCH https://{{host}}/authors/{{name}}\r\n",
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
                                "\r\n",};

            var doc = Document.FromLines(text);
            await doc.WaitForParsingCompleteAsync();
            Request request = doc.Requests.First();

            Assert.NotNull(request.Body);
            Assert.Contains("$test$", request.Body.Text);
            Assert.EndsWith("}", request.Body.TextExcludingLineBreaks);
        }

        [Fact]
        public async Task VariableTokenization()
        {
            var text = $"@name = value";

            var doc = Document.FromLines(text);
            await doc.WaitForParsingCompleteAsync();
            var variable = doc.Tokens.FirstOrDefault() as Variable;

            Assert.Equal(0, variable.At.Start);
            Assert.Equal(1, variable.At.Length);
            Assert.Equal(1, variable.Name.Start);
            Assert.Equal(4, variable.Name.Length);
            Assert.Equal(6, variable.Operator.Start);
            Assert.Equal(1, variable.Operator.Length);
            Assert.Equal(8, variable.Value.Start);
            Assert.Equal(5, variable.Value.Length);
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

            Assert.Equal(4, doc.Tokens.Count);
        }
    }
}