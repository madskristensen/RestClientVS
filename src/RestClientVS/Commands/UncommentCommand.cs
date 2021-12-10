using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Utilities;

namespace RestClientVS.Commands
{
    [Export(typeof(ICommandHandler))]
    [Name(nameof(CommentCommand))]
    [ContentType(RestLanguage.LanguageName)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    public class UncommentCommand : ICommandHandler<UncommentSelectionCommandArgs>
    {
        public string DisplayName => nameof(UncommentCommand);

        public bool ExecuteCommand(UncommentSelectionCommandArgs args, CommandExecutionContext executionContext)
        {
            SnapshotSpan spans = args.TextView.Selection.SelectedSpans.First();
            Collection<ITextViewLine> lines = args.TextView.TextViewLines.GetTextViewLinesIntersectingSpan(spans);

            foreach (ITextViewLine line in lines.Reverse())
            {
                var span = Span.FromBounds(line.Start, line.End);
                var text = args.TextView.TextBuffer.CurrentSnapshot.GetText(span).TrimStart('#');
                args.TextView.TextBuffer.Replace(span, text);
            }

            return true;
        }

        public CommandState GetCommandState(UncommentSelectionCommandArgs args)
        {
            return CommandState.Available;
        }
    }
}