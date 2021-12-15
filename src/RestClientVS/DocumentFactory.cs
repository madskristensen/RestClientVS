using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Text;
using RestClient;

namespace RestClientVS
{
    public class DocumentFactory
    {
        public static object _syncRoot = new();
        private static readonly ConditionalWeakTable<ITextSnapshot, Document> _cachedDocuments = new();

        public static Document GetDocument(ITextBuffer buffer)
        {
            lock (_syncRoot)
            {
                return _cachedDocuments.GetValue(buffer.CurrentSnapshot, key =>
                {
                    IEnumerable<ITextSnapshotLine> lines = key.Lines;
                    var textLines = lines.Select(line => line.GetTextIncludingLineBreak()).ToArray();
                    var doc = Document.FromLines(textLines);

                    DocumentParsed?.Invoke(null, doc);

                    return doc;
                });
            }
        }

        public static EventHandler<Document> DocumentParsed;
    }
}
