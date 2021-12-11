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
        public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() => new SuggestedActionsSource(textView));
        }
    }
}
