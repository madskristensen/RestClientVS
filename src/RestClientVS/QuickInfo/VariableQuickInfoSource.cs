using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using RestClient;
using RestClientVS.Parsing;

namespace RestClientVS.QuickInfo
{
    internal sealed class VariableQuickInfoSource : IAsyncQuickInfoSource
    {
        private static readonly ImageId _icon = KnownMonikers.PromoteVariable.ToImageId();

        private readonly ITextBuffer _buffer;
        private readonly string _file;

        public VariableQuickInfoSource(ITextBuffer buffer, string file)
        {
            _buffer = buffer;
            _file = file;
        }

        // This is called on a background thread.
        public Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            SnapshotPoint? triggerPoint = session.GetTriggerPoint(_buffer.CurrentSnapshot);

            if (triggerPoint != null)
            {
                Document doc = _buffer.CurrentSnapshot.ParseRestDocument(_file);
                var position = triggerPoint.Value.Position;

                IEnumerable<Token> tokens = doc.Tokens.Where(t => t.IntersectsWith(position));

                List<ContainerElement> elements = new();

                foreach (Token token in tokens)
                {
                    if (token.Text.Contains("{{"))
                    {
                        var run = new ContainerElement(
                        ContainerElementStyle.Wrapped,
                        new ImageElement(_icon),
                        new ClassifiedTextElement(
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Identifier, $"{token.ExpandVariables()}")
                        ));

                        elements.Add(run);
                    }
                }

                if (elements.Any())
                {
                    var parent = new ContainerElement(ContainerElementStyle.Stacked, elements.ToArray());

                    ITextSnapshotLine line = triggerPoint.Value.GetContainingLine();
                    ITrackingSpan lineSpan = _buffer.CurrentSnapshot.CreateTrackingSpan(line.Extent, SpanTrackingMode.EdgeInclusive);

                    return Task.FromResult(new QuickInfoItem(lineSpan, parent));
                }
            }

            return Task.FromResult<QuickInfoItem>(null);
        }

        public void Dispose()
        {
            // This provider does not perform any cleanup.
        }
    }
}
