using System.Runtime.InteropServices;

namespace RestClientVS.Language
{
    [Guid(PackageGuids.RestEditorFactoryString)]
    public class EditorFactory : EditorFactoryBase
    {
        public EditorFactory(Package package, Guid languageServiceId) : base(package, languageServiceId)
        { }
    }
}
