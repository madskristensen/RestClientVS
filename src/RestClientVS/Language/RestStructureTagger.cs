using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using RestClient;
using RestClientVS;

namespace MarkdownEditor.Outlining
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IStructureTag))]
    [ContentType(RestLanguage.LanguageName)]
    [Name(RestLanguage.LanguageName)]
    public class RestStructureTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag =>
            buffer.Properties.GetOrCreateSingletonProperty(() => new RestStructureTagger(buffer)) as ITagger<T>;
    }

    public class RestStructureTagger : ITagger<IStructureTag>, IDisposable
    {
        private Document _doc;
        private bool _isDisposed;
        private readonly ITextBuffer _buffer;

        public RestStructureTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            ParseDocumentAsync().FireAndForget();

            if (buffer is ITextBuffer2 buffer2)
            {
                buffer2.ChangedOnBackground += BufferChanged;
            }
        }

        private void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            ParseDocumentAsync().FireAndForget();
        }

        public IEnumerable<ITagSpan<IStructureTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0 || _doc == null)
            {
                yield return null;
            }

            ITextSnapshot snapshot = _buffer.CurrentSnapshot;

            foreach (Request request in _doc.Requests.Where(r => r.Children.Count > 1))
            {
                var text = request.Url.Text.Trim();
                var tooltip = request.Text.Trim();

                var simpleSpan = new Span(request.Start, request.Length - 1);
                var snapShotSpan = new SnapshotSpan(snapshot, simpleSpan);
                yield return CreateTag(snapShotSpan, text, tooltip);
            }
        }

        private static TagSpan<IStructureTag> CreateTag(SnapshotSpan span, string text, string tooltip)
        {
            var structureTag = new StructureTag(
                        span.Snapshot,
                        outliningSpan: span,
                        guideLineSpan: span,
                        guideLineHorizontalAnchor: span.Start,
                        type: PredefinedStructureTagTypes.Structural,
                        isCollapsible: true,
                        collapsedForm: text,
                        collapsedHintForm: tooltip);

            return new TagSpan<IStructureTag>(span, structureTag);
        }

        private Task ParseDocumentAsync()
        {
            _doc = _buffer.GetDocument();
            var span = new SnapshotSpan(_buffer.CurrentSnapshot, 0, _buffer.CurrentSnapshot.Length);
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                if (_buffer is ITextBuffer2 buffer2)
                {
                    buffer2.ChangedOnBackground += BufferChanged;
                }
            }

            _isDisposed = true;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}
