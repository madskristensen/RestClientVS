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
    [TagType(typeof(IErrorTag))]
    [ContentType(RestLanguage.LanguageName)]
    [Name(RestLanguage.LanguageName)]
    public class RestErrorTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag =>
            buffer.Properties.GetOrCreateSingletonProperty(() => new RestErrorTagger(buffer)) as ITagger<T>;
    }

    public class RestErrorTagger : ITagger<IErrorTag>, IDisposable
    {
        private Document _doc;
        private bool _isDisposed;
        private readonly ITextBuffer _buffer;

        public RestErrorTagger(ITextBuffer buffer)
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

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0 || _doc == null)
            {
                yield return null;
            }

            SnapshotSpan span = spans[0];

            ITextSnapshot snapshot = _buffer.CurrentSnapshot;
            IEnumerable<Token> tokens = _doc.Tokens.Where(t => t.Start <= span.Start && t.End >= span.End).ToArray();

            foreach (RestClient.Reference reference in tokens.SelectMany(t => t.References))
            {
                if (!reference.IsValid)
                {
                    var tooltip = string.Join(Environment.NewLine, reference.Errors);

                    var simpleSpan = new Span(reference.Value.Start, reference.Value.Length);
                    var snapShotSpan = new SnapshotSpan(snapshot, simpleSpan);
                    var errorTag = new ErrorTag(PredefinedErrorTypeNames.CompilerError, tooltip);

                    yield return new TagSpan<IErrorTag>(snapShotSpan, errorTag);
                }
            }
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
