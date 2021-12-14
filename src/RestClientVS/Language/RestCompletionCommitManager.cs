using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Utilities;

namespace RestClientVS.Completion
{
    [Export(typeof(IAsyncCompletionCommitManagerProvider))]
    [ContentType(RestLanguage.LanguageName)]
    [Name(RestLanguage.LanguageName)]
    internal class CompletionCommitManager : CompletionCommitManagerBase
    {
        public override IEnumerable<char> CommitChars => new char[] { ' ', '\'', '"', ',', '.', ';', ':' };
    }
}
