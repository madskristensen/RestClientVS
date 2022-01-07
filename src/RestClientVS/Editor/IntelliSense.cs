using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using RestClient;

namespace RestClientVS.Completion
{
    [Export(typeof(IAsyncCompletionSourceProvider))]
    [ContentType(LanguageFactory.LanguageName)]
    [Name(LanguageFactory.LanguageName)]
    internal class RestCompletionSourceProvider : IAsyncCompletionSourceProvider
    {
        [Import] internal ITextStructureNavigatorSelectorService _structureNavigator = null;

        public IAsyncCompletionSource GetOrCreate(ITextView textView) =>
            textView.Properties.GetOrCreateSingletonProperty(() => new IntelliSense(_structureNavigator));
    }

    public class IntelliSense : IAsyncCompletionSource
    {
        private readonly ITextStructureNavigatorSelectorService _structureNavigator;
        private static readonly ImageElement _httpMethodIcon = new(KnownMonikers.HTTPConnection.ToImageId(), "HTTP method");
        private static readonly ImageElement _headerNameIcon = new(KnownMonikers.Metadata.ToImageId(), "HTTP header");
        private static readonly ImageElement _referenceIcon = new(KnownMonikers.LocalVariable.ToImageId(), "Variable");

        public IntelliSense(ITextStructureNavigatorSelectorService structureNavigator)
        {
            _structureNavigator = structureNavigator;
        }

        public Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken cancellationToken)
        {
            ITextSnapshotLine line = triggerLocation.GetContainingLine();

            RestClient.Document document = session.TextView.TextBuffer.GetRestDocument();
            SnapshotPoint lineStart = line.Start;
            ParseItem token = GetPreviousToken(document, lineStart, out var hasEmptyLine);

            if (applicableToSpan.Start == lineStart) // only trigger on beginning of line
            {
                // HTTP Method
                if (token == null || token.Type == ItemType.VariableName || (token.Type == ItemType.Comment && token.Text.StartsWith("###")))
                {
                    return Task.FromResult(ConvertToCompletionItems(IntelliSenseCatalog.HttpMethods, _httpMethodIcon));
                }

                // HTTP Headers
                if (!hasEmptyLine && (token.Type == ItemType.HeaderValue || token.Type == ItemType.Url))
                {
                    var spanBeforeCaret = new SnapshotSpan(lineStart, triggerLocation);
                    var textBeforeCaret = triggerLocation.Snapshot.GetText(spanBeforeCaret);
                    var colonIndex = textBeforeCaret.IndexOf(':');
                    var colonExistsBeforeCaret = colonIndex != -1;

                    if (!colonExistsBeforeCaret)
                    {
                        return Task.FromResult(ConvertToCompletionItems(IntelliSenseCatalog.HttpHeaderNames, _headerNameIcon));
                    }
                }
            }

            // Variable references
            ParseItem currentToken = document.Items.LastOrDefault(v => v.Contains(triggerLocation.Position));
            ParseItem currentReference = currentToken?.References.FirstOrDefault(v => v.Contains(triggerLocation.Position));
            if (currentReference != null)
            {
                return Task.FromResult(GetReferenceCompletion(document.VariablesExpanded));
            }

            //// User is likely in the key portion of the pair
            //if (!colonExistsBeforeCaret)
            //{
            //    return GetContextForKey();
            //}

            //// User is likely in the value portion of the pair. Try to provide extra items based on the key.
            //var KeyExtractingRegex = new Regex(@"\W*(\w+s)\W*:");
            //Match key = KeyExtractingRegex.Match(textBeforeCaret);
            //var candidateName = key.Success ? key.Groups.Count > 0 && key.Groups[1].Success ? key.Groups[1].Value : string.Empty : string.Empty;
            //return GetContextForValue(candidateName);

            return Task.FromResult<CompletionContext>(null);
        }

        private ParseItem GetPreviousToken(RestClient.Document document, SnapshotPoint point, out bool hasEmptyLine)
        {
            ParseItem current = null;
            hasEmptyLine = false;

            foreach (ParseItem token in document.Items)
            {
                if (token.End > point.Position)
                {
                    break;
                }

                if (token?.Type != ItemType.EmptyLine)
                {
                    current = token;
                }

                hasEmptyLine = token?.Type == ItemType.EmptyLine;
            }

            return current;
        }

        private CompletionContext ConvertToCompletionItems(IDictionary<string, string> dic, ImageElement icon)
        {
            List<CompletionItem> items = new();

            foreach (var key in dic.Keys)
            {
                var completion = new CompletionItem(key, this, icon);
                completion.Properties.AddProperty("description", dic[key]?.Trim());
                items.Add(completion);
            }

            return new CompletionContext(items.ToImmutableArray());
        }

        private CompletionContext GetReferenceCompletion(Dictionary<string, string> variables)
        {
            List<CompletionItem> items = new();
            foreach (var key in variables.Keys)
            {
                var completion = new CompletionItem(key, this, _referenceIcon, ImmutableArray<CompletionFilter>.Empty, "", $"{{{{{key}}}}}", key, key, ImmutableArray<ImageElement>.Empty);
                completion.Properties.AddProperty("description", variables[key]);
                items.Add(completion);
            }

            return new CompletionContext(items.ToImmutableArray());
        }

        public Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
        {
            if (item.Properties.TryGetProperty("description", out string description))
            {
                return Task.FromResult<object>(description);
            }

            return Task.FromResult<object>(null);
        }

        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            // We don't trigger completion when user typed
            if (char.IsNumber(trigger.Character)         // a number
                || char.IsPunctuation(trigger.Character) // punctuation
                || trigger.Character == '\n'             // new line
                || trigger.Character == Constants.CommentChar
                || trigger.Reason == CompletionTriggerReason.Backspace
                || trigger.Reason == CompletionTriggerReason.Deletion)
            {
                return CompletionStartData.DoesNotParticipateInCompletion;
            }

            RestClient.Document document = triggerLocation.Snapshot.TextBuffer.GetRestDocument();
            ParseItem item = document?.FindItemFromPosition(triggerLocation.Position);

            if (item?.Type == ItemType.Reference)
            {
                var tokenSpan = new SnapshotSpan(triggerLocation.Snapshot, item.ToSpan());
                return new CompletionStartData(CompletionParticipation.ProvidesItems, tokenSpan);
            }
            else
            {
                SnapshotSpan tokenSpan = FindTokenSpanAtPosition(triggerLocation);

                if (triggerLocation.GetContainingLine().GetText().StartsWith(Constants.CommentChar.ToString(), StringComparison.Ordinal))
                {
                    return CompletionStartData.DoesNotParticipateInCompletion;
                }

                return new CompletionStartData(CompletionParticipation.ProvidesItems, tokenSpan);
            }
        }

        private SnapshotSpan FindTokenSpanAtPosition(SnapshotPoint triggerLocation)
        {
            // This method is not really related to completion,
            // we mostly work with the default implementation of ITextStructureNavigator 
            // You will likely use the parser of your language
            ITextStructureNavigator navigator = _structureNavigator.GetTextStructureNavigator(triggerLocation.Snapshot.TextBuffer);
            TextExtent extent = navigator.GetExtentOfWord(triggerLocation);
            if (triggerLocation.Position > 0 && (!extent.IsSignificant || !extent.Span.GetText().Any(c => char.IsLetterOrDigit(c))))
            {
                // Improves span detection over the default ITextStructureNavigation result
                extent = navigator.GetExtentOfWord(triggerLocation - 1);
            }

            ITrackingSpan tokenSpan = triggerLocation.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);

            ITextSnapshot snapshot = triggerLocation.Snapshot;
            var tokenText = tokenSpan.GetText(snapshot);
            if (string.IsNullOrWhiteSpace(tokenText))
            {
                // The token at this location is empty. Return an empty span, which will grow as user types.
                return new SnapshotSpan(triggerLocation, 0);
            }

            // Trim quotes and new line characters.
            var startOffset = 0;
            var endOffset = 0;

            if (tokenText.Length > 0)
            {
                if (tokenText.StartsWith("\""))
                {
                    startOffset = 1;
                }
            }
            if (tokenText.Length - startOffset > 0)
            {
                if (tokenText.EndsWith("\"\r\n"))
                {
                    endOffset = 3;
                }
                else if (tokenText.EndsWith("\r\n"))
                {
                    endOffset = 2;
                }
                else if (tokenText.EndsWith("\"\n"))
                {
                    endOffset = 2;
                }
                else if (tokenText.EndsWith("\n"))
                {
                    endOffset = 1;
                }
                else if (tokenText.EndsWith("\""))
                {
                    endOffset = 1;
                }
            }

            return new SnapshotSpan(tokenSpan.GetStartPoint(snapshot) + startOffset, tokenSpan.GetEndPoint(snapshot) - endOffset);
        }
    }
}
