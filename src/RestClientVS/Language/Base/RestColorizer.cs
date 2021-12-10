using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace RestClientVS
{
    public class RestColorizer : Colorizer
    {
        public RestColorizer(LanguageService svc, IVsTextLines buffer, IScanner scanner) :
            base(svc, buffer, scanner)
        { }
    }
}
