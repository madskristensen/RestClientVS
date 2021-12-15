using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using RestClient;

namespace RestClientVS.Classify
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType(RestLanguage.LanguageName)]
    [Name(RestLanguage.LanguageName)]
    internal class RestClassificationTaggerProvider : ITaggerProvider
    {
        [Import]
        private readonly IClassificationTypeRegistryService _classificationRegistry = default;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag =>
            buffer.Properties.GetOrCreateSingletonProperty(() => new RestClassificationTagger(buffer, _classificationRegistry)) as ITagger<T>;
    }

    internal class RestClassificationTagger : ITagger<IClassificationTag>
    {
        private readonly RestDocument _document;
        private static Dictionary<ItemType, IClassificationType> _map;

        internal RestClassificationTagger(ITextBuffer buffer, IClassificationTypeRegistryService registry)
        {
            _document = RestDocument.FromTextbuffer(buffer);

            _map ??= new Dictionary<ItemType, IClassificationType> {
                { ItemType.VariableName, registry.GetClassificationType(PredefinedClassificationTypeNames.SymbolDefinition) },
                { ItemType.VariableValue, registry.GetClassificationType(PredefinedClassificationTypeNames.Text) },
                { ItemType.Method, registry.GetClassificationType(PredefinedClassificationTypeNames.MarkupNode)},
                { ItemType.Url, registry.GetClassificationType(PredefinedClassificationTypeNames.Text)},
                { ItemType.HeaderName,  registry.GetClassificationType(PredefinedClassificationTypeNames.Identifier)},
                { ItemType.HeaderValue, registry.GetClassificationType(PredefinedClassificationTypeNames.Literal)},
                { ItemType.Comment, registry.GetClassificationType(PredefinedClassificationTypeNames.Comment)},
                { ItemType.Body,  registry.GetClassificationType(PredefinedClassificationTypeNames.Text)},
                { ItemType.ReferenceBraces, registry.GetClassificationType(PredefinedClassificationTypeNames.SymbolDefinition)},
                { ItemType.ReferenceName, registry.GetClassificationType(PredefinedClassificationTypeNames.MarkupAttribute)},
            };
        }

        public IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield return null;
            }

            foreach (SnapshotSpan span in spans)
            {
                foreach (ParseItem item in _document.Tokens.Where(t => t.Start < span.End && t.End > span.Start))
                {
                    if (_map.ContainsKey(item.Type))
                    {
                        var itemSpan = new SnapshotSpan(span.Snapshot, item.Start, item.Length);
                        var itemTag = new ClassificationTag(_map[item.Type]);
                        yield return new TagSpan<IClassificationTag>(itemSpan, itemTag);

                        foreach (RestClient.Reference variable in item.References)
                        {
                            var openSpan = new SnapshotSpan(span.Snapshot, variable.Open.Start, variable.Open.Length);
                            var openTag = new ClassificationTag(_map[variable.Open.Type]);
                            yield return new TagSpan<IClassificationTag>(openSpan, openTag);

                            var valueSpan = new SnapshotSpan(span.Snapshot, variable.Value.Start, variable.Value.Length);
                            var valueTag = new ClassificationTag(_map[variable.Value.Type]);
                            yield return new TagSpan<IClassificationTag>(valueSpan, valueTag);

                            var closeSpan = new SnapshotSpan(span.Snapshot, variable.Close.Start, variable.Close.Length);
                            var closeTag = new ClassificationTag(_map[variable.Close.Type]);
                            yield return new TagSpan<IClassificationTag>(closeSpan, closeTag);
                        }
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
