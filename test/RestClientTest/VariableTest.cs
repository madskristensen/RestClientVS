using System.Linq;
using System.Threading.Tasks;
using RestClient;
using Xunit;

namespace RestClientTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class VariableTest
    {
        [Theory]
        [InlineData("@name=value", "value")]
        [InlineData("@name = value", "value")]
        [InlineData("@name= value", "value")]
        [InlineData("@name =value", "value")]
        [InlineData("@name\t=\t value", "value")]
        public async Task VariableDeclarations(string line, string value)
        {
            var doc = Document.FromLines(line);
            await doc.WaitForParsingCompleteAsync();

            Variable first = doc.Variables?.FirstOrDefault();

            Assert.NotNull(first);
            Assert.Equal(0, first.Name.Start);
            Assert.EndsWith(value, first.Value.Text);
            //Assert.Equal(name, first.Name.Text);
            //Assert.Equal(value, first.Value.Text);
        }

        [Theory]
        [InlineData("var1", "1")]
        public async Task ExpandUrlVariables(string name, string value)
        {
            var variable = $"@{name}={value}";
            var request = "GET http://example.com?{{" + name + "}}";

            var doc = Document.FromLines(variable, request);
            await doc.WaitForParsingCompleteAsync();

            Request r = doc.Requests.FirstOrDefault();

            Assert.Equal("GET http://example.com?" + value, r.ToString());
        }

        [Fact]
        public async Task ExpandUrlVariablesRecursive()
        {
            var text = new[] { "@hostname=bing.com\r\n",
                       "@host={{hostname}}\r\n",
                       "GET https://{{host}}" };

            var doc = Document.FromLines(text);
            await doc.WaitForParsingCompleteAsync();

            Request r = doc.Requests.FirstOrDefault();

            Assert.Equal("GET https://bing.com", r.ToString());
        }
    }
}
