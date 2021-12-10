using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace RestClientVS.Classify
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("text")]
    internal class RestClassifierProvider : IClassifierProvider
    {
        [Import]
        private IClassificationTypeRegistryService classificationRegistry { get; set; }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            if (!TextDocumentFactoryService.TryGetTextDocument(buffer, out ITextDocument document))
            {
                return null;
            }

            return buffer.Properties.GetOrCreateSingletonProperty(() => new RestClassifier(buffer, classificationRegistry, document.FilePath));
        }
    }
}
