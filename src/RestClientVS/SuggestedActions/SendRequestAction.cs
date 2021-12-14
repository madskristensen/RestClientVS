using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using RestClient;

namespace RestClientVS.SuggestedActions
{
    public class SendRequestAction : ISuggestedAction
    {
        private readonly Request _request;

        public SendRequestAction(Request request)
        {
            _request = request;
        }

        public bool HasActionSets => false;

        public string DisplayText => $"Send Request to {_request.Url.Uri.ExpandVariables()}";

        public ImageMoniker IconMoniker => KnownMonikers.HTTPSend;

        public string IconAutomationText => null;

        public string InputGestureText => null;

        public bool HasPreview => false;

        public void Dispose()
        {
            // Nothing to dispose
        }

        public Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<SuggestedActionSet>>(null);
        }

        public Task<object> GetPreviewAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<object>(null);
        }

        public void Invoke(CancellationToken cancellationToken)
        {
            VS.Commands.ExecuteAsync(PackageGuids.RestClientVS, PackageIds.SendRequest).FireAndForget();
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.Empty;
            return false;
        }
    }
}
