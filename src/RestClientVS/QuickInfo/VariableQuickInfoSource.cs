using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using RestClient;
using RestClientVS.Parsing;

namespace RestClientVS.QuickInfo
{
    internal sealed class VariableQuickInfoSource : IAsyncQuickInfoSource
    {
        private readonly ITextBuffer _buffer;

        public VariableQuickInfoSource(ITextBuffer buffer)
        {
            _buffer = buffer;
        }

        // This is called on a background thread.
        public Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            SnapshotPoint? triggerPoint = session.GetTriggerPoint(_buffer.CurrentSnapshot);

            if (triggerPoint.HasValue)
            {
                Document doc = _buffer.GetDocument();
                var position = triggerPoint.Value.Position;

                Token token = doc.Tokens.FirstOrDefault(t => t.IntersectsWith(position));

                if (token != null && token.Text.Contains("{{"))
                {
                    ITextSnapshotLine line = triggerPoint.Value.GetContainingLine();
                    ITrackingSpan lineSpan = _buffer.CurrentSnapshot.CreateTrackingSpan(line.Extent, SpanTrackingMode.EdgeInclusive);

                    return Task.FromResult(new QuickInfoItem(lineSpan, token.ExpandVariables()));
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
