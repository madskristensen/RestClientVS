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

    public class RestStructureTagger : ITagger<IStructureTag>
    {
        private readonly ITextBuffer _buffer;

        public RestStructureTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
        }

        public IEnumerable<ITagSpan<IStructureTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield return null;
            }

            ITextSnapshot snapshot = _buffer.CurrentSnapshot;

            foreach (Request request in _buffer.GetDocument().Requests.Where(r => r.Children.Count > 1))
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

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }
    }
}
