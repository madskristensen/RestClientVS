using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using RestClient;

namespace RestClientVS.Commands
{
    [Export(typeof(ICommandHandler))]
    [Name(nameof(CommentCommand))]
    [ContentType(RestLanguage.LanguageName)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    public class GoToDefinitionCommand : ICommandHandler<GoToDefinitionCommandArgs>
    {
        public string DisplayName => nameof(GoToDefinitionCommand);

        public bool ExecuteCommand(GoToDefinitionCommandArgs args, CommandExecutionContext executionContext)
        {
            var position = args.TextView.Caret.Position.BufferPosition.Position;

            Document document = RestDocument.FromTextbuffer(args.TextView.TextBuffer);
            ParseItem token = document.GetTokenFromPosition(position);

            if (token?.Type == ItemType.ReferenceName)
            {
                Variable definition = document.Variables.FirstOrDefault(v => v.Name.Text.Substring(1).Equals(token.Text, StringComparison.OrdinalIgnoreCase));

                if (definition != null)
                {
                    args.TextView.Caret.MoveTo(new SnapshotPoint(args.TextView.TextBuffer.CurrentSnapshot, definition.Name.Start));
                }

                return true;
            }

            return false;
        }

        public CommandState GetCommandState(GoToDefinitionCommandArgs args)
        {
            return CommandState.Available;
        }
    }
}