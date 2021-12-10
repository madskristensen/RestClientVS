﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using RestClient;
using RestClientVS.Parsing;

namespace RestClientVS.Classify
{
    internal class RestClassifier : IClassifier
    {
        private readonly IClassificationType _varAt, _varName, _varValue, _method, _url, _headerName, _operator, _headerValue, _comment, _body, _refCurly, _refValue;
        private readonly ITextBuffer _buffer;
        private bool _isProcessing;
        private Document _doc;
        private readonly string _file;

        internal RestClassifier(ITextBuffer buffer, IClassificationTypeRegistryService registry, string file)
        {
            _buffer = buffer;
            _file = file;

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

            ParseDocument();

            _buffer.Changed += bufferChanged;
        }

        private void bufferChanged(object sender, TextContentChangedEventArgs e)
        {
            ParseDocument();
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var list = new List<ClassificationSpan>();

            if (_doc == null || _isProcessing || span.IsEmpty)
            {
                return list;
            }

            Token[] tokens = _doc.Tokens.Where(t => t.Start < span.End && t.End > span.Start).ToArray();

            foreach (Token token in tokens)
            {
                Dictionary<Span, IClassificationType> all = GetClassificationTypes(token);

                foreach (Span range in all.Keys)
                {
                    var snapspan = new SnapshotSpan(span.Snapshot, range);
                    list.Add(new ClassificationSpan(snapspan, all[range]));
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

            foreach (RestClient.Reference variable in token.Variables)
            {
                spans.Add(variable.Open.ToSimpleSpan(), _refCurly);
                spans.Add(variable.Value.ToSimpleSpan(), _refValue);
                spans.Add(variable.Close.ToSimpleSpan(), _refCurly);
            }
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
                _doc = _buffer.CurrentSnapshot.ParseRestDocument(_file);

                var span = new SnapshotSpan(_buffer.CurrentSnapshot, 0, _buffer.CurrentSnapshot.Length);

                ClassificationChanged?.Invoke(this, new ClassificationChangedEventArgs(span));

                _isProcessing = false;

                return Task.CompletedTask;
            }).FireAndForget();
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
    }
}
