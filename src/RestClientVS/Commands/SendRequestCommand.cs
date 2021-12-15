using System.Linq;
using System.Threading;
using System.Windows.Forms;
using RestClient;
using RestClient.Client;

namespace RestClientVS
{
    [Command(PackageIds.SendRequest)]
    internal sealed class SendRequestCommand : BaseCommand<SendRequestCommand>
    {
        private OutputWindowPane _pane;
        private static string _lastRequest;
        private static CancellationTokenSource _source;

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync();

            if (docView != null)
            {
                var position = docView.TextView.Caret.Position.BufferPosition.Position;
                Document doc = RestDocument.FromTextbuffer(docView.TextBuffer);
                Request request = doc.Requests.FirstOrDefault(r => r.Contains(position));

                if (request != null)
                {
                    _lastRequest = request.ToString();
                    _source?.Cancel();
                    _source = new CancellationTokenSource();

                    if (_pane == null)
                    {
                        _pane = await VS.Windows.CreateOutputWindowPaneAsync(Vsix.Name, true);
                    }

                    await VS.Windows.ShowToolWindowAsync(new Guid(WindowGuids.OutputWindow));

                    await _pane.ActivateAsync();
                    await _pane.ClearAsync();
                    await _pane.WriteLineAsync(DateTime.Now.ToString() + " - " + request.Url.ExpandVariables() + Environment.NewLine);

                    await VS.StatusBar.ShowMessageAsync($"Sending request to {request.Url.ExpandVariables()}...");
                    await VS.StatusBar.StartAnimationAsync(StatusAnimation.Sync);

                    General options = await General.GetLiveInstanceAsync();
                    RequestResult result = await RequestSender.SendAsync(request, TimeSpan.FromSeconds(options.Timeout), _source.Token);

                    if (!string.IsNullOrEmpty(_lastRequest) && result.RequestToken.ToString() != _lastRequest)
                    {
                        // Prohibits multiple requests from writing at the same time.
                        return;
                    }

                    SendKeys.Send("{ESC}"); // puts focus back in the editor

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
                }
            }
        }

        protected override void BeforeQueryStatus(EventArgs e)
        {
            var isRestFile = ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync();

                if (docView?.TextBuffer != null)
                {
                    return docView.TextBuffer.ContentType.IsOfType(RestLanguage.LanguageName);
                }

                return false;
            });

            Command.Visible = Command.Enabled = isRestFile;
        }
    }
}
