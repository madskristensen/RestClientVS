using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace RestClientVS.Completion
{
    [Export(typeof(IAsyncCompletionCommitManagerProvider))]
    [Name(nameof(RestCompletionCommitManager))]
    [ContentType(RestLanguage.LanguageName)]
    internal class RestCompletionCommitManager : IAsyncCompletionCommitManagerProvider
    {
        public IAsyncCompletionCommitManager GetOrCreate(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() => new SampleCompletionCommitManager());
        }
    }

    /// <summary>
    /// The simplest implementation of IAsyncCompletionCommitManager that provides Commit Characters and uses default behavior otherwise
    /// </summary>
    internal class SampleCompletionCommitManager : IAsyncCompletionCommitManager
    {
        private ImmutableArray<char> _commitChars = new char[] { ' ', '\'', '"', ',', '.', ';', ':' }.ToImmutableArray();

        public IEnumerable<char> PotentialCommitCharacters => _commitChars;

        public bool ShouldCommitCompletion(IAsyncCompletionSession session, SnapshotPoint location, char typedChar, CancellationToken token)
        {
            // This method runs synchronously, potentially before CompletionItem has been computed.
            // The purpose of this method is to filter out characters not applicable at given location.

            // This method is called only when typedChar is among the PotentialCommitCharacters
            // in this simple example, all PotentialCommitCharacters do commit, so we always return true
            return true;
        }

        public CommitResult TryCommit(IAsyncCompletionSession session, ITextBuffer buffer, CompletionItem item, char typedChar, CancellationToken token)
        {
            // Objects of interest here are session.TextView and session.TextView.Caret.
            // This method runs synchronously

            return CommitResult.Unhandled; // use default commit mechanism.
        }
    }
}
