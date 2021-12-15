using Microsoft.VisualStudio.Text;
using RestClient;

namespace RestClientVS
{
    public static class ExtensionMethods
    {
        public static Span ToSpan(this ParseItem token) =>
            new(token.Start, token.Length);
    }
}
