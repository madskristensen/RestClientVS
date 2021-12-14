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
    public class CommentCommand : ICommandHandler<CommentSelectionCommandArgs>
    {
        public string DisplayName => nameof(CommentCommand);

        public bool ExecuteCommand(CommentSelectionCommandArgs args, CommandExecutionContext executionContext)
        {
            SnapshotSpan spans = args.TextView.Selection.SelectedSpans.First();
            Collection<ITextViewLine> lines = args.TextView.TextViewLines.GetTextViewLinesIntersectingSpan(spans);

            foreach (ITextViewLine line in lines.Reverse())
            {
                args.TextView.TextBuffer.Insert(line.Start.Position, RestClient.Constants.CommentChar.ToString());
            }

            return true;
        }

        public CommandState GetCommandState(CommentSelectionCommandArgs args)
        {
            return CommandState.Available;
        }
    }
}