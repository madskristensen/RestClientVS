using Microsoft.VisualStudio.Shell.Interop;

namespace BaseClasses
{
    public class ErrorListItem
    {
        /// <summary>
        /// Project name of the error item.
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// File name of the error item.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 0-based line of code on the error item.
        /// </summary>
        public int Line { get; set; }

        public int Column { get; set; }

        /// <summary>
        /// Error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Error code for the error item.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Error code tool tip.
        /// </summary>
        public string ErrorCodeToolTip { get; set; }

        /// <summary>
        /// Error category.
        /// </summary>
        public string ErrorCategory { get; set; }

        /// <summary>
        /// Severity of the error item.
        /// </summary>
        public __VSERRORCATEGORY Severity { get; set; }

        /// <summary>
        /// Error help link.
        /// </summary>
        public string HelpLink { get; set; }

        /// <summary>
        /// Column used to display the build tool that generated the error (e.g. "FxCop").
        /// </summary>
        public string BuildTool { get; set; }
    }
}
