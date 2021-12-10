using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Text;
using RestClient;

namespace RestClientVS.Parsing
{
    public static class RestFactory
    {
        public static object _syncRoot = new object();
        private static readonly ConditionalWeakTable<ITextSnapshot, Document> _cachedDocuments = new();

        public static Document ParseRestDocument(this ITextSnapshot snapshot, string file = null)
        {
            lock (_syncRoot)
            {
                return _cachedDocuments.GetValue(snapshot, key =>
                {
                    IEnumerable<ITextSnapshotLine> lines = key.Lines;
                    Document document = Parse(lines);
                    Parsed?.Invoke(snapshot, new ParsingEventArgs(document, file, snapshot));
                    return document;
                });
            }
        }

        public static Document Parse(IEnumerable<ITextSnapshotLine> lines)
        {
            var textLines = lines.Select(line => line.GetTextIncludingLineBreak()).ToArray();

            return Document.FromLines(textLines);
        }

        public static event EventHandler<ParsingEventArgs> Parsed;
    }
}
