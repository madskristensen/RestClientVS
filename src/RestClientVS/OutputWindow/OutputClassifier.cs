using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace RestClientVS.OutputWindow
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType("output")]
    public class OutputClassifier : ITaggerProvider
    {
        [Import] internal IClassificationTypeRegistryService _classificationRegistry = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag =>
            buffer.Properties.GetOrCreateSingletonProperty(() => new OutputClassificationTagger(buffer, _classificationRegistry)) as ITagger<T>;

    }

    internal class OutputClassificationTagger : ITagger<IClassificationTag>
    {
        private readonly IClassificationType _headerNameType, _statusOkType, _statusBadType;
        private readonly ITextBuffer _buffer;
        private readonly Regex _status = new(@"^HTTP/\d.\d (?<status>\d{3} .+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _header = new(@"^(?<name>[\w-]+):(?<value>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal OutputClassificationTagger(ITextBuffer buffer, IClassificationTypeRegistryService registry)
        {
            _headerNameType = registry.GetClassificationType(ClassificationTypeDefinitions.HeaderName);
            _statusOkType = registry.GetClassificationType(ClassificationTypeDefinitions.StatusOk);
            _statusBadType = registry.GetClassificationType(ClassificationTypeDefinitions.StatusBad);
            _buffer = buffer;
        }

        public IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0 ||
                !_buffer.CurrentSnapshot.Lines.Any() ||
                !_buffer.CurrentSnapshot.Lines.First().GetText().StartsWith(Vsix.Name))
            {
                yield return null;
            }

            foreach (SnapshotSpan span in spans)
            {
                var text = span.GetText();

                Match match = _header.Match(text);

                if (match.Success)
                {
                    Group nameGroup = match.Groups["name"];
                    var nameSpan = new SnapshotSpan(span.Snapshot, span.Start + nameGroup.Index, nameGroup.Length);
                    var nameTag = new ClassificationTag(_headerNameType);
                    yield return new TagSpan<IClassificationTag>(nameSpan, nameTag);
                }
                else
                {
                    match = _status.Match(text);

                    if (match.Success)
                    {
                        Group statusGroup = match.Groups["status"];
                        var statusCode = int.Parse(statusGroup.Value[0].ToString());
                        IClassificationType type = statusCode == 2 || statusCode == 3 ? _statusOkType : _statusBadType;
                        var statusTag = new ClassificationTag(type);
                        yield return new TagSpan<IClassificationTag>(span, statusTag);
                    }
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }
    }
}
