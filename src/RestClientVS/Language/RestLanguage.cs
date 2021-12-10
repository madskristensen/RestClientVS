using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace RestClientVS
{
    [Guid("e27351df-a1b0-4568-b8fc-cc7b6f156be5")]
    public class RestLanguage : LanguageService
    {
        public const string LanguageName = "Rest";
        private LanguagePreferences _preferences = null;

        public RestLanguage(object site)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SetSite(site);
        }

        public override Source CreateSource(IVsTextLines buffer)
        {
            return new RestSource(this, buffer, new RestColorizer(this, buffer, null));
        }

        public override TypeAndMemberDropdownBars CreateDropDownHelper(IVsTextView forView)
        {
            return base.CreateDropDownHelper(forView);
        }

        public override LanguagePreferences GetLanguagePreferences()
        {
            if (_preferences == null)
            {
                _preferences = new LanguagePreferences(Site, typeof(RestLanguage).GUID, Name);

                if (_preferences != null)
                {
                    _preferences.Init();

                    _preferences.EnableCodeSense = true;
                    _preferences.EnableMatchBraces = true;
                    _preferences.EnableMatchBracesAtCaret = true;
                    _preferences.EnableShowMatchingBrace = true;
                    _preferences.EnableCommenting = true;
                    _preferences.HighlightMatchingBraceFlags = _HighlightMatchingBraceFlags.HMB_USERECTANGLEBRACES;
                    _preferences.LineNumbers = false;
                    _preferences.MaxErrorMessages = 100;
                    _preferences.AutoOutlining = false;
                    _preferences.MaxRegionTime = 2000;
                    _preferences.InsertTabs = false;
                    _preferences.IndentSize = 2;
                    _preferences.IndentStyle = IndentingStyle.Smart;
                    _preferences.ShowNavigationBar = false;

                    _preferences.WordWrap = true;
                    _preferences.WordWrapGlyphs = true;

                    _preferences.AutoListMembers = true;
                    _preferences.EnableQuickInfo = true;
                    _preferences.ParameterInformation = true;
                }
            }

            return _preferences;
        }

        public override IScanner GetScanner(IVsTextLines buffer)
        {
            return null;
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            return new RestAuthoringScope();
        }

        public override string GetFormatFilterList()
        {
            return "Rest File (*.http, *.rest)|*.http;*.rest";
        }

        public override string Name => LanguageName;

        public override void Dispose()
        {
            try
            {
                if (_preferences != null)
                {
                    _preferences.Dispose();
                    _preferences = null;
                }
            }
            finally
            {
                base.Dispose();
            }
        }
    }
}
