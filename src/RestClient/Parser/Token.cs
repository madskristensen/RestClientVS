using System.Collections.Generic;

namespace RestClient
{
    public abstract class Token
    {
        public Token(int start, string text, Document document)
        {
            Start = start;
            Text = text;
            TextExcludingLineBreaks = text.TrimEnd();
            Document = document;
        }

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

        public virtual bool IntersectsWith(int position)
        {
            return Start <= position && End >= position;
        }

        public Token? Previous
        {
            get
            {
                var index = Document.Tokens.IndexOf(this);
                return index > 0 ? Document.Tokens[index - 1] : null;
            }
        }

        public string ExpandVariables()
        {
            // Then replace the references with the expanded values
            var clean = Text;

            if (Document.Variables != null)
            {
                foreach (var key in Document.Variables.Keys)
                {
                    clean = clean.Replace("{{" + key + "}}", Document.Variables[key].Trim());
                }
            }

            return clean.Trim();
        }
    }
}
