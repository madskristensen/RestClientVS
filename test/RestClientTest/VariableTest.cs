using System;
using System.Linq;
using RestClient;
using Xunit;

namespace RestClientTest
{
    public class VariableTest
    {
        [Theory]
        [InlineData("@name=value", "name", "value")]
        [InlineData("@name = value", "name", "value")]
        [InlineData("@name= value", "name", "value")]
        [InlineData("@name =value", "name", "value")]
        [InlineData("@name\t=\t value", "name", "value")]
        public void VariableDeclarations(string line, string name, string value)
        {
            var doc = Document.FromLines(line);
            var first = doc.Tokens?.FirstOrDefault() as Variable;

            Assert.NotNull(first);
            Assert.Equal(0, first.Start);
            Assert.Equal(line, first.Text);
            Assert.Equal(line.Length, first.Length);
            Assert.Equal(line.Length, first.End);
            Assert.Equal(name, first.Name.Text);
            Assert.Equal(value, first.Value.Text);
        }

        [Theory]
        [InlineData("var1", "1")]
        public void ExpandUrlVariables(string name, string value)
        {
            var variable = $"@{name}={value}";
            var request = "GET http://example.com?{{" + name + "}}";

            var doc = Document.FromLines(variable, request);
            Request r = doc.Requests.FirstOrDefault();

            Assert.Equal("GET http://example.com?" + value, r.ExpandVariables());
        }

        [Fact]
        public void ExpandUrlVariablesRecursive()
        {
            var text = @"@hostname=bing.com 
@host={{hostname}}
GET https://{{host}}".Split(Environment.NewLine);

            var doc = Document.FromLines(text);
            Request r = doc.Requests.FirstOrDefault();

            Assert.Equal("GET https://bing.com", r.ExpandVariables());
        }
    }
}
