using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace RestClientVS
{
    [Export(typeof(ISuggestedActionsSourceProvider))]
    [Name(nameof(SuggestedActionsSourceProvider))]
    [ContentType(RestLanguage.LanguageName)]
    internal class SuggestedActionsSourceProvider : ISuggestedActionsSourceProvider
    {
        private ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [ImportingConstructor]
        public SuggestedActionsSourceProvider(ITextDocumentFactoryService textDocumentFactoryService)
        {
            TextDocumentFactoryService = textDocumentFactoryService;
        }

        public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer)
        {

            if (TextDocumentFactoryService.TryGetTextDocument(textView.TextBuffer, out ITextDocument document))
            {
                return textView.Properties.GetOrCreateSingletonProperty(() =>
                    new SuggestedActionsSource(textView, document.FilePath));
            }

            return null;
        }
    }
}
