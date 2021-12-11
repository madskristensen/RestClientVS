using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using RestClient;
using RestClient.Client;

namespace RestClientVS.SuggestedActions
{
    public class SendRequestAction : ISuggestedAction
    {
        private readonly Request _request;
        private static OutputWindowPane _pane;

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
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                if (_pane == null)
                {
                    _pane = await VS.Windows.CreateOutputWindowPaneAsync(Vsix.Name, true);
                }

                await VS.Windows.ShowToolWindowAsync(new Guid(WindowGuids.OutputWindow));

                await _pane.ActivateAsync();
                await _pane.ClearAsync();
                await _pane.WriteLineAsync(DateTime.Now.ToString() + " - " + _request.Url.Uri.ExpandVariables() + Environment.NewLine);

                await VS.StatusBar.ShowMessageAsync($"Sending request to {_request.Url.Uri.ExpandVariables()}...");
                await VS.StatusBar.StartAnimationAsync(StatusAnimation.Sync);

                RequestResult result = await RequestSender.SendAsync(_request, cancellationToken);

                if (result.Response != null)
                {
                    await _pane.WriteLineAsync(await result.Response.ToRawStringAsync());
                }
                else
                {
                    await _pane.WriteLineAsync(result.ErrorMessage);
                }

                await VS.StatusBar.ShowMessageAsync("Request completed");
                await VS.StatusBar.EndAnimationAsync(StatusAnimation.Sync);

            }).FireAndForget();
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.Empty;
            return false;
        }
    }
}
