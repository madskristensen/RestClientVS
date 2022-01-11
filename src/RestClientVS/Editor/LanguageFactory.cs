using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace RestClientVS
{
    [ComVisible(true)]
    [Guid(PackageGuids.RestEditorFactoryString)]
    public class LanguageFactory : LanguageBase
    {
        public const string LanguageName = "Rest";
        public const string FileExtension = ".http";
        private DropdownBars _dropdownBars;

        public LanguageFactory(object site) : base(site)
        { }

        public void RegisterLanguageService(Package package)
        {
            ((IServiceContainer)package).AddService(GetType(), this, true);
        }

        public override string Name => LanguageName;

        public override string[] FileExtensions => new[] { FileExtension };

        public override void SetDefaultPreferences(LanguagePreferences preferences)
        {
            preferences.EnableCodeSense = false;
            preferences.EnableMatchBraces = true;
            preferences.EnableMatchBracesAtCaret = true;
            preferences.EnableShowMatchingBrace = true;
            preferences.EnableCommenting = true;
            preferences.HighlightMatchingBraceFlags = _HighlightMatchingBraceFlags.HMB_USERECTANGLEBRACES;
            preferences.LineNumbers = true;
            preferences.MaxErrorMessages = 100;
            preferences.AutoOutlining = false;
            preferences.MaxRegionTime = 2000;
            preferences.InsertTabs = false;
            preferences.IndentSize = 2;
            preferences.IndentStyle = IndentingStyle.Smart;
            preferences.ShowNavigationBar = true;

            preferences.WordWrap = false;
            preferences.WordWrapGlyphs = true;

            preferences.AutoListMembers = true;
            preferences.EnableQuickInfo = true;
            preferences.ParameterInformation = true;
        }

        public override TypeAndMemberDropdownBars CreateDropDownHelper(IVsTextView textView)
        {
            _dropdownBars?.Dispose();
            _dropdownBars = new DropdownBars(textView, this);

            return _dropdownBars;
        }

        public override void Dispose()
        {
            _dropdownBars?.Dispose();
            _dropdownBars = null;
            base.Dispose();
        }
    }
}
