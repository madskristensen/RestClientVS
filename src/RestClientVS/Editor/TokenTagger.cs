using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using RestClient;

namespace RestClientVS
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(TokenTag))]
    [ContentType(LanguageFactory.LanguageName)]
    [Name(LanguageFactory.LanguageName)]
    internal sealed class TokenTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag =>
            buffer.Properties.GetOrCreateSingletonProperty(() => new TokenTagger(buffer)) as ITagger<T>;
    }

    internal class TokenTagger : TokenTaggerBase, IDisposable
    {
        private readonly RestDocument _document;
        private static readonly ImageId _errorIcon = KnownMonikers.StatusWarningNoColor.ToImageId();
        private bool _isDisposed;

        internal TokenTagger(ITextBuffer buffer) : base(buffer)
        {
            _document = buffer.GetRestDocument();
            _document.Parsed += OnDocumentParsed;
        }

        private void OnDocumentParsed(object sender = null, EventArgs e = null)
        {
            _ = TokenizeAsync();
        }

        public override Task TokenizeAsync()
        {
            // Make sure this is running on a background thread.
            ThreadHelper.ThrowIfOnUIThread();

            List<ITagSpan<TokenTag>> list = new();

            foreach (ParseItem item in _document.Items)
            {
                if (_document.IsParsing)
                {
                    // Abort and wait for the next parse event to finish
                    return Task.CompletedTask;
                }

                AddTagToList(list, item);

                foreach (ParseItem variable in item.References)
                {
                    AddTagToList(list, variable);
                }
            }

            OnTagsUpdated(list);
            return Task.CompletedTask;
        }

        private void AddTagToList(List<ITagSpan<TokenTag>> list, ParseItem item)
        {
            var supportsOutlining = item is Request request && (request.Headers.Any() || request.Body != null);
            var hasTooltip = !item.IsValid;
            IEnumerable<ErrorListItem> errors = CreateErrorListItem(item);
            TokenTag tag = CreateToken(item.Type, hasTooltip, supportsOutlining, errors);

            var span = new SnapshotSpan(Buffer.CurrentSnapshot, item.ToSpan());
            list.Add(new TagSpan<TokenTag>(span, tag));
        }

        private IEnumerable<ErrorListItem> CreateErrorListItem(ParseItem item)
        {
            if (!General.Instance.EnableValidation)
            {
                yield break;
            }

            ITextSnapshotLine line = Buffer.CurrentSnapshot.GetLineFromPosition(item.Start);

            foreach (Error error in item.Errors)
            {
                yield return new ErrorListItem
                {
                    ProjectName = _document.ProjectName ?? "",
                    FileName = _document.FileName,
                    Message = error.Message,
                    ErrorCategory = ConvertToVsCat(error.Severity),
                    Severity = ConvertToVsSeverity(error.Severity),
                    Line = line.LineNumber,
                    Column = item.Start - line.Start.Position,
                    BuildTool = Vsix.Name,
                    ErrorCode = error.ErrorCode
                };
            }
        }

        private static string ConvertToVsCat(ErrorCategory cat)
        {
            return cat switch
            {
                ErrorCategory.Message => PredefinedErrorTypeNames.Suggestion,
                ErrorCategory.Warning => PredefinedErrorTypeNames.Warning,
                _ => PredefinedErrorTypeNames.SyntaxError,
            };
        }

        private static __VSERRORCATEGORY ConvertToVsSeverity(ErrorCategory cat)
        {
            return cat switch
            {
                ErrorCategory.Message => __VSERRORCATEGORY.EC_MESSAGE,
                ErrorCategory.Warning => __VSERRORCATEGORY.EC_WARNING,
                _ => __VSERRORCATEGORY.EC_ERROR,
            };
        }

        public override Task<object> GetTooltipAsync(SnapshotPoint triggerPoint)
        {
            ParseItem item = _document.FindItemFromPosition(triggerPoint.Position);

            // Error messages
            if (item?.IsValid == false)
            {
                var elm = new ContainerElement(
                    ContainerElementStyle.Wrapped,
                    new ImageElement(_errorIcon),
                    string.Join(Environment.NewLine, item.Errors.Select(e => e.Message)));

                return Task.FromResult<object>(elm);
            }

            return Task.FromResult<object>(null);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _document.Parsed -= OnDocumentParsed;
                _document.Dispose();
            }

            _isDisposed = true;
        }
    }
}
