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

    [ProvideLanguageService(typeof(LanguageFactory), LanguageFactory.LanguageName, 0, ShowHotURLs = false, DefaultToNonHotURLs = true)]
    [ProvideLanguageExtension(typeof(LanguageFactory), LanguageFactory.FileExtension)]
    [ProvideLanguageEditorOptionPage(typeof(OptionsProvider.GeneralOptions), LanguageFactory.LanguageName, null, "Advanced", null, new[] { "http", "rest", "timeout" })]

    [ProvideEditorFactory(typeof(LanguageFactory), 351, CommonPhysicalViewAttributes = (int)__VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorExtension(typeof(LanguageFactory), LanguageFactory.FileExtension, 65535, NameResourceID = 351)]
    [ProvideEditorLogicalView(typeof(LanguageFactory), VSConstants.LOGVIEWID.TextView_string, IsTrusted = true)]

    [ProvideFileIcon(LanguageFactory.FileExtension, "KnownMonikers.WebScript")]
    public sealed class RestClientVSPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            var language = new LanguageFactory(this);
            RegisterEditorFactory(language);
            language.RegisterLanguageService(this);

            await this.RegisterCommandsAsync();
            await Commenting.InitializeAsync();
        }
    }
}