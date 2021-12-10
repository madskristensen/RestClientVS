using Microsoft.VisualStudio.Text;
using RestClient;

namespace RestClientVS.Parsing
{
    public class ParsingEventArgs : EventArgs
    {
        public ParsingEventArgs(Document document, string file, ITextSnapshot snapshot)
        {
            Document = document;
            File = file;
            Snapshot = snapshot;
        }

        public Document Document { get; set; }
        public string File { get; set; }
        public ITextSnapshot Snapshot { get; set; }
    }
}
