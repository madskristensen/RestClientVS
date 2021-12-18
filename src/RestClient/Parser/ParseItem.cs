using System.Collections.Generic;
using System.Linq;

namespace RestClient
{
    public class ParseItem
    {
        public ParseItem(int start, string text, Document document, ItemType type)
        {
            Start = start;
            Text = text;
            TextExcludingLineBreaks = text.TrimEnd();
            Document = document;
            Type = type;
        }

        public ItemType Type { get; }

        public int Start { get; }

        public virtual string Text { get; protected set; }
        public virtual string TextExcludingLineBreaks { get; protected set; }

        public Document Document { get; }

        public virtual int End => Start + Text.Length;
        public virtual int EndExcludingLineBreaks => Start + TextExcludingLineBreaks.Length;

        public virtual int Length => End - Start;
        public virtual int LengthExcludingLineBreaks => EndExcludingLineBreaks - Start;

        public List<Reference> References { get; } = new List<Reference>();

        public List<string> Errors = new();

        public bool IsValid => Errors.Count == 0;

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
}
