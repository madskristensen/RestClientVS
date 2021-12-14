global using System;
global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell.Interop;

namespace RestClientVS
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.RestClientVSString)]

    [ProvideLanguageService(typeof(RestLanguage), RestLanguage.LanguageName, 0)]
    [ProvideLanguageExtension(typeof(RestLanguage), ".http")]
    [ProvideLanguageExtension(typeof(RestLanguage), ".rest")]

    [ProvideLanguageEditorOptionPage(typeof(OptionsProvider.GeneralOptions), RestLanguage.LanguageName, null, "Advanced", null, new[] { "http", "rest", "timeout" })]

    [ProvideEditorFactory(typeof(RestLanguage), 0, false, CommonPhysicalViewAttributes = (int)__VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    //[ProvideEditorLogicalView(typeof(RestLanguage), VSConstants.LOGVIEWID.TextView_string, IsTrusted = true)]

    //[ProvideBraceCompletion(RestLanguage.LanguageName)]
    public sealed class RestClientVSPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // Create text view host: ITextEditorFactoryService

            RegisterEditorFactory(new RestLanguage(this));

            await this.RegisterCommandsAsync();
        }
    }
}