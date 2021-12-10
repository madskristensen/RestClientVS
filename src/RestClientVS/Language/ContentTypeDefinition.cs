using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace RestClientVS.Language
{
    public class ContentTypeDefinition
    {
        [Export(typeof(ContentTypeDefinition))]
        [Name(RestLanguage.LanguageName)]
        [BaseDefinition("plaintext")]
        public ContentTypeDefinition RestContentType { get; set; }
    }
}