using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace RestClientVS.OutputWindow
{
    public class ClassificationTypeDefinitions
    {
        public const string StatusOk = "StatusOK";
        public const string StatusBad = "StatusBad";
        public const string HeaderName = "HeaderName";

        [Export]
        [Name(StatusOk)]
        public static ClassificationTypeDefinition StatusOkDefinition { get; set; }

        [Name(StatusOk)]
        [UserVisible(false)]
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = StatusOk)]
        [Order(Before = Priority.Default)]
        public sealed class StatusOkFormat : ClassificationFormatDefinition
        {
            public StatusOkFormat()
            {
                ForegroundColor = Colors.Green;
                IsBold = true;
            }
        }

        [Export]
        [Name(StatusBad)]
        public static ClassificationTypeDefinition StatusBadDefinition { get; set; }

        [Name(StatusBad)]
        [UserVisible(false)]
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = StatusBad)]
        [Order(Before = Priority.Default)]
        public sealed class StatusBadFormat : ClassificationFormatDefinition
        {
            public StatusBadFormat()
            {
                ForegroundColor = Colors.Crimson;
                IsBold = true;
            }
        }

        [Export]
        [Name(HeaderName)]
        public static ClassificationTypeDefinition HeaderNameDefinition { get; set; }

        [Name(HeaderName)]
        [UserVisible(false)]
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = HeaderName)]
        [Order(Before = Priority.Default)]
        public sealed class HeaderNameFormat : ClassificationFormatDefinition
        {
            public HeaderNameFormat()
            {
                IsBold = true;
            }
        }
    }
}
