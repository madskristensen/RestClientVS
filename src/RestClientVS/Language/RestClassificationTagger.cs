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
        private static IClassificationType _varAt, _varName, _varValue, _method, _url, _headerName, _operator, _headerValue, _comment, _body, _refCurly, _refValue;
        private readonly ITextBuffer _buffer;

        internal RestClassificationTagger(ITextBuffer buffer, IClassificationTypeRegistryService registry)
        {
            _buffer = buffer;

            if (_varAt == null)
            {
                _varAt = registry.GetClassificationType(PredefinedClassificationTypeNames.SymbolDefinition);
                _varName = registry.GetClassificationType(PredefinedClassificationTypeNames.SymbolDefinition);
                _varValue = registry.GetClassificationType(PredefinedClassificationTypeNames.Text);
                _method = registry.GetClassificationType(PredefinedClassificationTypeNames.MarkupNode);
                _url = registry.GetClassificationType(PredefinedClassificationTypeNames.Text);
                _headerName = registry.GetClassificationType(PredefinedClassificationTypeNames.Identifier);
                _headerValue = registry.GetClassificationType(PredefinedClassificationTypeNames.Literal);
                _operator = registry.GetClassificationType(PredefinedClassificationTypeNames.Operator);
                _comment = registry.GetClassificationType(PredefinedClassificationTypeNames.Comment);
                _body = registry.GetClassificationType(PredefinedClassificationTypeNames.Text);
                _refCurly = registry.GetClassificationType(PredefinedClassificationTypeNames.SymbolDefinition);
                _refValue = registry.GetClassificationType(PredefinedClassificationTypeNames.MarkupAttribute);
            }
        }

        public IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var list = new List<ITagSpan<IClassificationTag>>();

            foreach (SnapshotSpan span in spans)
            {
                if (span.IsEmpty)
                {
                    return list;
                }

                Token[] tokens = _buffer.GetDocument().Tokens.Where(t => t.Start < span.End && t.End > span.Start).ToArray();

                foreach (Token token in tokens)
                {
                    Dictionary<Span, IClassificationType> all = GetClassificationTypes(token);

                    foreach (Span range in all.Keys)
                    {
                        if (range.End <= span.Snapshot.Length)
                        {
                            var snapspan = new SnapshotSpan(span.Snapshot, range);
                            var ct = new ClassificationTag(all[range]);
                            list.Add(new TagSpan<IClassificationTag>(snapspan, ct));
                        }
                    }
                }
            }

            return list;
        }

        private Dictionary<Span, IClassificationType> GetClassificationTypes(Token token)
        {
            var spans = new Dictionary<Span, IClassificationType>();

            if (token is Variable variable)
            {
                AddSpans(spans, variable.At, _varAt);
                AddSpans(spans, variable.Name, _varName);
                AddSpans(spans, variable.Value, _varValue);
                AddSpans(spans, variable.Operator, _operator);
            }
            else if (token is Comment comment)
            {
                AddSpans(spans, comment, _comment);
            }
            else if (token is RestClient.Url url)
            {
                if (url.Method != null)
                {
                    AddSpans(spans, url.Method, _method);
                }

                AddSpans(spans, url.Uri, _url);
            }
            else if (token is Header header)
            {
                AddSpans(spans, header.Name, _headerName);
                AddSpans(spans, header.Operator, _operator);
                AddSpans(spans, header.Value, _headerValue);
            }
            else if (token is Comment ct)
            {
                AddSpans(spans, ct, _comment);
            }
            else if (token is BodyToken body)
            {
                AddSpans(spans, body, _body);
            }

            return spans;
        }

        private void AddSpans(Dictionary<Span, IClassificationType> spans, Token token, IClassificationType type)
        {
            Span tokenSpan = token.ToSimpleSpan();
            spans.Add(tokenSpan, type);

            foreach (RestClient.Reference variable in token.References)
            {
                spans.Add(variable.Open.ToSimpleSpan(), _refCurly);
                spans.Add(variable.Value.ToSimpleSpan(), _refValue);
                spans.Add(variable.Close.ToSimpleSpan(), _refCurly);
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }
    }
}
