using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Text;
using RestClient;

namespace RestClientVS
{
    public static class DocumentFactory
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
                    return Document.FromLines(textLines);
                });
            }
        }
    }
}
