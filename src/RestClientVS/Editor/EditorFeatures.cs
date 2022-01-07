using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using RestClient;

namespace RestClientVS
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType(LanguageFactory.LanguageName)]
    public class SyntaxHighligting : TokenClassificationTaggerBase
    {
        public override Dictionary<object, string> ClassificationMap { get; } = new()
        {
            { ItemType.VariableName, PredefinedClassificationTypeNames.SymbolDefinition },
            { ItemType.VariableValue, PredefinedClassificationTypeNames.Text },
            { ItemType.Method, PredefinedClassificationTypeNames.MarkupNode },
            { ItemType.Url, PredefinedClassificationTypeNames.Text },
            { ItemType.HeaderName, PredefinedClassificationTypeNames.Identifier },
            { ItemType.HeaderValue, PredefinedClassificationTypeNames.Literal },
            { ItemType.Comment, PredefinedClassificationTypeNames.Comment },
            { ItemType.Body, PredefinedClassificationTypeNames.Text },
            { ItemType.Reference, PredefinedClassificationTypeNames.MarkupAttribute },
        };
    }

    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IStructureTag))]
    [ContentType(LanguageFactory.LanguageName)]
    public class Outlining : TokenOutliningTaggerBase
    { }

    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType(LanguageFactory.LanguageName)]
    public class ErrorSquigglies : TokenErrorTaggerBase
    { }

    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [ContentType(LanguageFactory.LanguageName)]
    internal sealed class Tooltips : TokenQuickInfoBase
    { }

    [Export(typeof(IBraceCompletionContextProvider))]
    [BracePair('(', ')')]
    [BracePair('[', ']')]
    [BracePair('{', '}')]
    [BracePair('"', '"')]
    [BracePair('$', '$')]
    [ContentType(LanguageFactory.LanguageName)]
    [ProvideBraceCompletion(LanguageFactory.LanguageName)]
    internal sealed class BraceCompletion : BraceCompletionBase
    { }

    [Export(typeof(IAsyncCompletionCommitManagerProvider))]
    [ContentType(LanguageFactory.LanguageName)]
    internal sealed class CompletionCommitManager : CompletionCommitManagerBase
    {
        public override IEnumerable<char> CommitChars => new char[] { ' ', '\'', '"', ',', '.', ';', ':', '\\', '$' };
    }

    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(TextMarkerTag))]
    [ContentType(LanguageFactory.LanguageName)]
    internal sealed class BraceMatchingTaggerProvider : BraceMatchingBase
    {
        // This will match parenthesis, curly brackets, and square brackets by default.
        // Override the BraceList property to modify the list of braces to match.
    }

    [Export(typeof(IViewTaggerProvider))]
    [ContentType(LanguageFactory.LanguageName)]
    [TagType(typeof(TextMarkerTag))]
    public class SameWordHighlighter : SameWordHighlighterBase
    { }
}
