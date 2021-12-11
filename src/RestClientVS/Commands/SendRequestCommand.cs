//using System.Linq;
//using EnvDTE;
//using RestClient;
//using RestClient.Client;
//using RestClientVS.Parsing;

//namespace RestClientVS
//{
//    [Command(PackageIds.MyCommand)]
//    internal sealed class SendRequestCommand : BaseCommand<SendRequestCommand>
//    {
//        private Community.VisualStudio.Toolkit.OutputWindowPane _pane;

//        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
//        {
//            DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync();

//            if (docView != null)
//            {
//                var position = docView.TextView.Caret.Position.BufferPosition.Position;
//                RestClient.Document doc = docView.TextBuffer.CurrentSnapshot.ParseRestDocument(docView.FilePath);
//                Request request = doc.Requests.FirstOrDefault(r => r.IntersectsWith(position));

//                if (request != null)
//                {
//                    RequestResult result = await RequestSender.SendAsync(request, Package.DisposalToken);
//                    await VS.Windows.ShowToolWindowAsync(new Guid(WindowGuids.OutputWindow));
//                    Community.VisualStudio.Toolkit.OutputWindowPane pane = _pane ??= await VS.Windows.CreateOutputWindowPaneAsync(Vsix.Name);
//                    await pane.ActivateAsync();
//                    await pane.ClearAsync();
//                    await pane.WriteLineAsync(await result.Response.ToRawStringAsync());
//                }
//            }
//        }

//        protected override void BeforeQueryStatus(EventArgs e)
//        {
//            var isRestFile = ThreadHelper.JoinableTaskFactory.Run(async () =>
//            {
//                DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync();

//                if (docView?.TextBuffer != null)
//                {
//                    return docView.TextBuffer.ContentType.IsOfType(RestLanguage.LanguageName);
//                }

//                return false;
//            });

//            Command.Visible = Command.Enabled = isRestFile;
//        }
//    }
//}
