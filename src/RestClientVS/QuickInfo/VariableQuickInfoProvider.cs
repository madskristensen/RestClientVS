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
        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer buffer)
        {
            return buffer.Properties.GetOrCreateSingletonProperty(() => new VariableQuickInfoSource(buffer));
        }
    }
}
