using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace RestClientVS.Classify
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(RestLanguage.LanguageName)]
    internal class RestClassifierProvider : IClassifierProvider
    {
        [Import]
        private IClassificationTypeRegistryService classificationRegistry { get; set; }

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            return buffer.Properties.GetOrCreateSingletonProperty(() => new RestClassifier(buffer, classificationRegistry));
        }
    }
}
