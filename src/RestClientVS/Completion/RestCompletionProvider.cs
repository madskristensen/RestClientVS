using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace RestClientVS.Completion
{
    [Export(typeof(IAsyncCompletionSourceProvider))]
    [Name(nameof(RestCompletionProvider))]
    [ContentType(RestLanguage.LanguageName)]
    internal class RestCompletionProvider : IAsyncCompletionSourceProvider
    {
        [Import]
        private ITextStructureNavigatorSelectorService StructureNavigator { get; set; }

        public IAsyncCompletionSource GetOrCreate(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() => new RestCompletionSource(StructureNavigator));
        }
    }
}
