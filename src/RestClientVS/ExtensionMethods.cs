using Microsoft.VisualStudio.Text;
using RestClient;

namespace RestClientVS
{
    public static class ExtensionMethods
    {
        public static Span ToSimpleSpan(this Token token) =>
            new(token.Start, token.Length);
    }
}
