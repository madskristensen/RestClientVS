using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace RestClientVS
{
    public abstract class BraceCompletionBase : IBraceCompletionContextProvider
    {
        [Import]
        private readonly IClassifierAggregatorService _classifierService = default;

        public bool TryCreateContext(ITextView textView, SnapshotPoint openingPoint, char openingBrace, char closingBrace, out IBraceCompletionContext context)
        {
            if (IsValidBraceCompletionContext(openingPoint))
            {
                context = new DefaultBraceCompletionContext();
                return true;
            }
            else
            {
                context = null;
                return false;
            }
        }

        private bool IsValidBraceCompletionContext(SnapshotPoint openingPoint)
        {
            Debug.Assert(openingPoint.Position >= 0, "SnapshotPoint.Position should always be zero or positive.");

            if (openingPoint.Position > 0)
            {
                IList<ClassificationSpan> classificationSpans = _classifierService.GetClassifier(openingPoint.Snapshot.TextBuffer)
                                                           .GetClassificationSpans(new SnapshotSpan(openingPoint - 1, 1));

                foreach (ClassificationSpan span in classificationSpans)
                {
                    if (span.ClassificationType.IsOfType(PredefinedClassificationTypeNames.Comment))
                    {
                        return false;
                    }
                    if (span.ClassificationType.IsOfType(PredefinedClassificationTypeNames.String))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    internal class DefaultBraceCompletionContext : IBraceCompletionContext
    {
        public bool AllowOverType(IBraceCompletionSession session) => true;

        public void Finish(IBraceCompletionSession session)
        {
        }

        public void OnReturn(IBraceCompletionSession session)
        {
        }

        public void Start(IBraceCompletionSession session)
        {
        }
    }
}