using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using RestClient;
using RestClientVS.Parsing;

namespace MarkdownEditor.Outlining
{
    public class RestOutliningTagger : ITagger<IOutliningRegionTag>
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

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
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

        private static TagSpan<IOutliningRegionTag> CreateTag(SnapshotSpan span, string text, string tooltip = null)
        {
            var tag = new OutliningRegionTag(false, false, text, tooltip);
            return new TagSpan<IOutliningRegionTag>(span, tag);
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
