using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using RestClient;

namespace RestClientVS.SuggestedActions
{
    [Export(typeof(ISuggestedActionsSourceProvider))]
    [ContentType(RestLanguage.LanguageName)]
    [Name(RestLanguage.LanguageName)]
    internal class RestSuggestedActionsSourceProvider : ISuggestedActionsSourceProvider
    {
        public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer) =>
            textView.Properties.GetOrCreateSingletonProperty(() => new RestSuggestedActionsSource(textView));
    }

    internal class RestSuggestedActionsSource : ISuggestedActionsSource
    {
        private readonly ITextView _view;
        private Request _request;

        public RestSuggestedActionsSource(ITextView view)
        {
            _view = view;
        }

        public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            Document doc = _view.TextBuffer.GetDocument();
            _request = doc.Requests.LastOrDefault(r => r.IntersectsWith(range.Start.Position));
            return Task.FromResult(_request != null);
        }

        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            if (_request != null)
            {
                yield return new SuggestedActionSet(
                    categoryName: PredefinedSuggestedActionCategoryNames.Any,
                    actions: new[] { new SendRequestAction(_request) },
                    title: Vsix.Name,
                    priority: SuggestedActionSetPriority.Medium,
                    applicableToSpan: _request.Url.ToSimpleSpan());
            }
        }

        public void Dispose()
        { }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            // This is a sample provider and doesn't participate in LightBulb telemetry
            telemetryId = Guid.Empty;
            return false;
        }


        public event EventHandler<EventArgs> SuggestedActionsChanged
        {
            add { }
            remove { }
        }
    }
}
