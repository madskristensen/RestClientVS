using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace RestClientVS
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(nameof(ResponseMarginProvider))]
    [Order(After = PredefinedMarginNames.RightControl)]
    [MarginContainer(PredefinedMarginNames.Right)]
    [ContentType(LanguageFactory.LanguageName)]
    [TextViewRole(PredefinedTextViewRoles.Debuggable)] // This is to prevent the margin from loading in the diff view
    public class ResponseMarginProvider : IWpfTextViewMarginProvider
    {
        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer) =>
             wpfTextViewHost.TextView.Properties.GetOrCreateSingletonProperty(() => new ResponseMargin(wpfTextViewHost.TextView));
    }
}
