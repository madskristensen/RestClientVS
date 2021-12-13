using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using RestClient;
using RestClientVS;
using RestClientVS.Parsing;

namespace MarkdownEditor.Outlining
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IStructureTag))]
    [ContentType(RestLanguage.LanguageName)]
    [Name(nameof(RestOutliningProvider))]
    public class RestOutliningProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(() => new RestOutliningTagger(buffer)) as ITagger<T>;
        }
    }

    public class RestOutliningTagger : ITagger<IStructureTag>
    {
        private Document _doc;
        private bool _isProcessing;
        private readonly ITextBuffer _buffer;

        public RestOutliningTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            ParseDocument();

            _buffer.Changed += BufferChanged;
        }

        private void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            ParseDocument();
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

        private static TagSpan<IStructureTag> CreateTag(SnapshotSpan span, string text, string tooltip = null)
        {
            //var tag = new StructureTag(span.Snapshot, false, false, text, tooltip);

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

        private void ParseDocument()
        {
            if (_isProcessing)
            {
                return;
            }

            _isProcessing = true;

            ThreadHelper.JoinableTaskFactory.RunAsync(() =>
            {
                _doc = _buffer.GetDocument();

                var span = new SnapshotSpan(_buffer.CurrentSnapshot, 0, _buffer.CurrentSnapshot.Length);

                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
                _isProcessing = false;
                return Task.CompletedTask;

            }).FireAndForget();
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}
