global using System;
global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using MarkdownEditor;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace RestClientVS
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.RestClientVSString)]

    [ProvideLanguageService(typeof(RestLanguage), RestLanguage.LanguageName, 100, ShowDropDownOptions = true, DefaultToInsertSpaces = true, EnableCommenting = true, AutoOutlining = true, MatchBraces = true, MatchBracesAtCaret = true, ShowMatchingBrace = true, ShowSmartIndent = true)]
    [ProvideLanguageEditorOptionPage(typeof(OptionsProvider.GeneralOptions), RestLanguage.LanguageName, null, "Advanced", "#101", new[] { "http", "rest", "timeout" })]
    [ProvideLanguageExtension(typeof(RestLanguage), ".http")]
    [ProvideLanguageExtension(typeof(RestLanguage), ".rest")]

    [ProvideEditorFactory(typeof(RestLanguage), 110, CommonPhysicalViewAttributes = (int)__VSPHYSICALVIEWATTRIBUTES.PVA_None, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorLogicalView(typeof(RestLanguage), VSConstants.LOGVIEWID.TextView_string, IsTrusted = true)]

    [ProvideEditorExtension(typeof(RestLanguage), ".http", 1000)]
    [ProvideEditorExtension(typeof(RestLanguage), ".rest", 1000)]

    [ProvideBraceCompletion(RestLanguage.LanguageName)]
    public sealed class RestClientVSPackage : ToolkitPackage
    {
        //public static RestLanguage Language { get; private set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // Create text view host: ITextEditorFactoryService

            RegisterEditorFactory(new RestLanguage(this));

            await this.RegisterCommandsAsync();
        }
    }
}