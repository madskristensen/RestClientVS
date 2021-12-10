using Microsoft.VisualStudio.Text;
using RestClient;

namespace RestClientVS
{
    public static class ExtensionMethods
    {
        public static Span ToSimpleSpan(this Token span)
        {
            return new Span(span.Start, span.Length);
        }
    }
}
