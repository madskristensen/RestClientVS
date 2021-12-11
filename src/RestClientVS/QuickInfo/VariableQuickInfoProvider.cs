using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace RestClientVS.QuickInfo
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name(nameof(VariableQuickInfoSourceProvider))]
    [ContentType(RestLanguage.LanguageName)]
    [Order]
    internal sealed class VariableQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer buffer)
        {
            if (!TextDocumentFactoryService.TryGetTextDocument(buffer, out ITextDocument document))
            {
                return null;
            }

            return buffer.Properties.GetOrCreateSingletonProperty(() => new VariableQuickInfoSource(buffer, document.FilePath));
        }
    }
}
