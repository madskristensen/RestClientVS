using System.Collections.Generic;
using System.ComponentModel.Composition;
using BaseClasses;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using RestClient;

namespace RestClientVS
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(RestLanguage.LanguageName)]
    [Name(RestLanguage.LanguageName)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    public class ErrorListManager : WpfTextViewCreationListener
    {
        private DocumentView _docView;
        private Project _project;
        private TableDataSource _dataSource;
        private RestDocument _document;

        protected override async Task CreatedAsync(DocumentView docView)
        {
            _docView = docView;
            _project = await VS.Solutions.GetActiveProjectAsync();
            _dataSource = new TableDataSource(RestLanguage.LanguageName, RestLanguage.LanguageName);
            _document = RestDocument.FromTextbuffer(docView.TextBuffer);
            _document.Parsed += ParseErrors;

            ParseErrors();
        }

        private void ParseErrors(object sender = null, EventArgs e = null)
        {
            if (_document.IsValid)
            {
                _dataSource.CleanAllErrors();
                return;
            }

            ThreadHelper.JoinableTaskFactory.StartOnIdle(() =>
            {
                if (_document.IsParsing)
                {
                    return;
                }

                List<ErrorListItem> errors = new();

                foreach (ParseItem item in _document.Items)
                {
                    if (!item.IsValid)
                    {
                        errors.AddRange(CreateErrorListItem(item));
                    }

                    foreach (RestClient.Reference reference in item.References)
                    {
                        if (!reference.Value.IsValid)
                        {
                            errors.AddRange(CreateErrorListItem(reference.Value));
                        }
                    }
                }

                _dataSource.CleanAllErrors();
                _dataSource.AddErrors(_project?.Name ?? "", errors);
            }, VsTaskRunContext.UIThreadBackgroundPriority).FireAndForget();
        }

        private IEnumerable<ErrorListItem> CreateErrorListItem(ParseItem item)
        {
            ITextSnapshotLine line = _docView.TextBuffer.CurrentSnapshot.GetLineFromPosition(item.Start);

            foreach (var error in item.Errors)
            {
                yield return new ErrorListItem
                {
                    ProjectName = _project?.Name ?? "",
                    FileName = _docView.FilePath,
                    Message = error,
                    ErrorCategory = "syntax",
                    Severity = __VSERRORCATEGORY.EC_WARNING,
                    Line = line.LineNumber,
                    Column = item.Start - line.Start.Position,
                    BuildTool = Vsix.Name,
                };
            }
        }

        protected override void Closed(IWpfTextView textView)
        {
            _document.Parsed -= ParseErrors;
            _dataSource.CleanAllErrors();
        }
    }
}
