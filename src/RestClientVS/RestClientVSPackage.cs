global using System;
global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace RestClientVS
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.RestClientVSString)]

    [ProvideLanguageService(typeof(LanguageFactory), LanguageFactory.LanguageName, 0)]
    [ProvideLanguageExtension(typeof(LanguageFactory), LanguageFactory.FileExtension)]
    [ProvideFileIcon(LanguageFactory.FileExtension, "KnownMonikers.WebScript")]

    [ProvideLanguageEditorOptionPage(typeof(OptionsProvider.GeneralOptions), LanguageFactory.LanguageName, null, "Advanced", null, new[] { "http", "rest", "timeout" })]

    [ProvideEditorFactory(typeof(LanguageFactory), 0, false, CommonPhysicalViewAttributes = (int)__VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorLogicalView(typeof(LanguageFactory), VSConstants.LOGVIEWID.TextView_string, IsTrusted = true)]
    [ProvideEditorExtension(typeof(LanguageFactory), LanguageFactory.FileExtension, 0x32)]
    public sealed class RestClientVSPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            RegisterEditorFactory(new LanguageFactory(this));

            await this.RegisterCommandsAsync();
        }
    }
}