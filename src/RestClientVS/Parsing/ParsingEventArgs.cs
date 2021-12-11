using Microsoft.VisualStudio.Text;
using RestClient;

namespace RestClientVS.Parsing
{
    public class ParsingEventArgs : EventArgs
    {
        public ParsingEventArgs(Document document, ITextBuffer buffer)
        {
            Document = document;
            TextBuffer = buffer;
        }

        public Document Document { get; set; }
        public ITextBuffer TextBuffer { get; set; }
    }
}
