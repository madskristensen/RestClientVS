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

    public class RestErrorTagger : ITagger<IErrorTag>
    {
        private readonly ITextBuffer _buffer;
        private readonly RestDocument _document;

        public RestErrorTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _document = RestDocument.FromTextbuffer(buffer);
        }

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0 || _document.IsParsing)
            {
                yield return null;
            }

            SnapshotSpan span = spans[0];
            ITextSnapshot snapshot = _buffer.CurrentSnapshot;
            IEnumerable<ParseItem> tokens = _document.Items.Where(t => t.Start <= span.Start && t.End >= span.End).ToArray();

            // Variable references
            foreach (RestClient.Reference reference in tokens.SelectMany(t => t.References))
            {
                if (!reference.Value.IsValid)
                {
                    var tooltip = string.Join(Environment.NewLine, reference.Value.Errors);

                    var simpleSpan = new Span(reference.Value.Start, reference.Value.Length);
                    var snapShotSpan = new SnapshotSpan(snapshot, simpleSpan);
                    var errorTag = new ErrorTag(PredefinedErrorTypeNames.CompilerError, tooltip);

                    yield return new TagSpan<IErrorTag>(snapShotSpan, errorTag);
                }
            }

            // Tokens
            foreach (ParseItem item in _document.Items)
            {
                if (!item.IsValid)
                {
                    var tooltip = string.Join(Environment.NewLine, item.Errors);

                    var simpleSpan = new Span(item.Start, item.Length);
                    var snapShotSpan = new SnapshotSpan(snapshot, simpleSpan);
                    var errorTag = new ErrorTag(PredefinedErrorTypeNames.CompilerError, tooltip);

                    yield return new TagSpan<IErrorTag>(snapShotSpan, errorTag);
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
