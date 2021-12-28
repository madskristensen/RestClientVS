using System.Collections.Generic;
using System.Linq;

namespace RestClient
{
    public class ParseItem
    {
        public HashSet<Error> _errors = new();

        public ParseItem(int start, string text, Document document, ItemType type)
        {
            Start = start;
            Text = text;
            TextExcludingLineBreaks = text.TrimEnd();
            Document = document;
            Type = type;
        }

        public ItemType Type { get; }

        public virtual int Start { get; }

        public virtual string Text { get; protected set; }
        public virtual string TextExcludingLineBreaks { get; protected set; }

        public Document Document { get; }

        public virtual int End => Start + Text.Length;
        public virtual int EndExcludingLineBreaks => Start + TextExcludingLineBreaks.Length;

        public virtual int Length => End - Start;
        public virtual int LengthExcludingLineBreaks => EndExcludingLineBreaks - Start;

        public List<ParseItem> References { get; } = new();

        public ICollection<Error> Errors => _errors;

        public bool IsValid => _errors.Count == 0;

        public virtual bool Contains(int position)
        {
            return Start <= position && End >= position;
        }

        public ParseItem? Previous
        {
            get
            {
                var index = Document.Items.IndexOf(this);
                return index > 0 ? Document.Items[index - 1] : null;
            }
        }

        public ParseItem? Next
        {
            get
            {
                var index = Document.Items.IndexOf(this);
                return Document.Items.ElementAtOrDefault(index + 1);
            }
        }

        public string ExpandVariables()
        {
            var clean = Text;

            if (Document.VariablesExpanded != null)
            {
                foreach (var key in Document.VariablesExpanded.Keys)
                {
                    clean = clean.Replace("{{" + key + "}}", Document.VariablesExpanded[key].Trim());
                }
            }

            return clean.Trim();
        }

        public override string ToString()
        {
            return Type + " " + Text;
        }
    }

    public class Error
    {
        public Error(string errorCode, string message, ErrorCategory severity)
        {
            ErrorCode = errorCode;
            Message = message;
            Severity = severity;
        }

        public string ErrorCode { get; }
        public string Message { get; }
        public ErrorCategory Severity { get; }

        public Error WithFormat(params string[] replacements)
        {
            return new Error(ErrorCode, string.Format(Message, replacements), Severity);
        }
    }

    public enum ErrorCategory
    {
        Error,
        Warning,
        Message,
    }
}
