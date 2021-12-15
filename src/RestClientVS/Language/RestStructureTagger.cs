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
        private readonly ITextBuffer _buffer;
        private readonly RestDocument _document;
        private List<ITagSpan<IStructureTag>> _structureTags = new();
        private bool _isDisposed;

        public RestStructureTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _document = RestDocument.FromTextbuffer(buffer);
            _document.Parsed += DocumentParsed;

            StartParsing();
        }

        private void DocumentParsed(object sender, EventArgs e)
        {
            StartParsing();
        }

        public IEnumerable<ITagSpan<IStructureTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0 || !_structureTags.Any())
            {
                return null;
            }

            return _structureTags;
        }

        private void StartParsing()
        {
            ThreadHelper.JoinableTaskFactory.StartOnIdle(() =>
            {
                if (TagsChanged == null || _document.IsParsing)
                {
                    return Task.CompletedTask;
                }

                _structureTags.Clear();
                ReParse();
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(_buffer.CurrentSnapshot, 0, _buffer.CurrentSnapshot.Length)));

                return Task.CompletedTask;
            },
                VsTaskRunContext.UIThreadIdlePriority).FireAndForget();
        }

        private void ReParse()
        {
            ITextSnapshot snapshot = _buffer.CurrentSnapshot;
            List<ITagSpan<IStructureTag>> list = new();

            foreach (Request request in _document.Requests.Where(r => r.Children.Count > 1))
            {
                var text = request.Url.Text.Trim();
                var tooltip = request.Text.Trim();

                var simpleSpan = new Span(request.Start, request.Length - 1);
                var snapShotSpan = new SnapshotSpan(snapshot, simpleSpan);
                TagSpan<IStructureTag> tag = CreateTag(snapShotSpan, text, tooltip);
                list.Add(tag);
            }

            _structureTags = list;
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

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _document.Parsed -= DocumentParsed;
            }

            _isDisposed = true;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}
