using System.Linq;
using System.Threading;
using RestClient;
using RestClient.Client;

namespace RestClientVS
{
    [Command(PackageIds.SendRequest)]
    internal sealed class SendRequestCommand : BaseCommand<SendRequestCommand>
    {
        private static string _lastRequest;
        private static CancellationTokenSource _source;

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync();

            if (docView != null)
            {
                var position = docView.TextView.Caret.Position.BufferPosition.Position;
                RestClient.Document doc = docView.TextBuffer.GetRestDocument();
                Request request = doc.Requests.FirstOrDefault(r => r.Contains(position));

                if (request != null)
                {
                    _lastRequest = request.ToString();
                    _source?.Cancel();
                    _source = new CancellationTokenSource();

                    await VS.StatusBar.ShowMessageAsync($"Sending request to {request.Url.ExpandVariables()}...");
                    await VS.StatusBar.StartAnimationAsync(StatusAnimation.Sync);

                    General options = await General.GetLiveInstanceAsync();                                        
                    RequestResult result = await RequestSender.SendAsync(request, TimeSpan.FromSeconds(options.Timeout), _source.Token);

                    if (!string.IsNullOrEmpty(_lastRequest) && result.RequestToken.ToString() != _lastRequest)
                    {
                        // Prohibits multiple requests from writing at the same time.
                        return;
                    }

                    if (docView.TextView.Properties.TryGetProperty(typeof(ResponseMargin), out ResponseMargin margin))
                    {
                        await margin.UpdateReponseAsync(result);
                    }
                    
                    request.EndActive(result.Response?.IsSuccessStatusCode ?? false);
                    await VS.StatusBar.ShowMessageAsync("Request completed");
                    await VS.StatusBar.EndAnimationAsync(StatusAnimation.Sync);
                }
            }
        }

        protected override Task InitializeCompletedAsync()
        {
            Command.Supported = false;
            return base.InitializeCompletedAsync();
        }
    }
}
