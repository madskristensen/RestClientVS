using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Text;
using RestClient;

namespace RestClientVS.Parsing
{
    public static class RestFactory
    {
        public static object _syncRoot = new();
        private static readonly ConditionalWeakTable<ITextSnapshot, Document> _cachedDocuments = new();

        public static Document GetDocument(this ITextBuffer buffer)
        {
            lock (_syncRoot)
            {
                return _cachedDocuments.GetValue(buffer.CurrentSnapshot, key =>
                {
                    IEnumerable<ITextSnapshotLine> lines = key.Lines;
                    var textLines = lines.Select(line => line.GetTextIncludingLineBreak()).ToArray();
                    var document = Document.FromLines(textLines);

                    Parsed?.Invoke(buffer, new ParsingEventArgs(document, buffer));

                    return document;
                });
            }
        }

        public static event EventHandler<ParsingEventArgs> Parsed;
    }
}
