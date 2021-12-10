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
        [InlineData(@"https://example.com")]
        [InlineData(@"https://example.com?hat&ost")]
        public void OneLiners(string line)
        {
            var doc = Document.FromLines(line);
            Token first = doc.Tokens?.FirstOrDefault();

            Assert.NotNull(first);
            Assert.IsType<Url>(first);
            Assert.Equal(0, first.Start);
            Assert.Equal(line, first.Text);
            Assert.Equal(line.Length, first.Length);
            Assert.Equal(line.Length, first.End);
        }

        [Fact]
        public void RequestTextAfterLineBreak()
        {
            var lines = new[] { "\r", Environment.NewLine, @"GET https://example.com" };

            var doc = Document.FromLines(lines);
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
        public void MultipleRequests()
        {
            var lines = @"http://example.com

###

http://bing.com".Split(Environment.NewLine);

            var doc = Document.FromLines(lines);

            Assert.Equal(2, doc.Requests.Count());
        }

        [Fact]
        public void RequestWithHeaderAndBody()
        {
            var lines = new[]
            {
                "https://example.com",
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
                "https://example.com",
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
        public void RequestWithHeaderAndBodyAndComment()
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
        public void EmptyLines(string line)
        {
            var doc = Document.FromLines(line);
            Token first = doc.Tokens?.FirstOrDefault();

            Assert.NotNull(first);
            Assert.IsType<EmptyLine>(first);
            Assert.Equal(0, first.Start);
            Assert.Equal(line, first.Text);
            Assert.Equal(line.Length, first.Length);
            Assert.Equal(line.Length, first.End);
        }

        [Fact]
        public void CommentAfterNewLineInRequest()
        {
            var text = new[] { "http://bing.com\r\n", "\r\n", "###" };

            var doc = Document.FromLines(text);
            var comment = doc.Tokens.ElementAt(2) as Comment;

            Assert.Equal(19, comment.Start);
        }

        [Fact]
        public void BodyAfterComment()
        {
            var text = @"PATCH https://{{host}}/authors/{{name}}
Content-Type: at{{contentType}}svin
#ost
mads: ost

{
    ""content"": ""foo bar"",
    ""created_at"": ""{{createdAt}}"",
    ""modified_by"": ""{{modifiedBy}}""
}".Split(Environment.NewLine);

            var doc = Document.FromLines(text);
            Request request = doc.Requests.First();

            Assert.NotNull(request.Body);
        }

        [Fact]
        public void VariableTokenization()
        {
            var text = $"@name = value";

            var doc = Document.FromLines(text);
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
    }
}