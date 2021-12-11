using System.Collections.Generic;

namespace RestClient
{
    public abstract class Token
    {
        public Token(int start, string text, Document document)
        {
            Start = start;
            Text = text;
            Document = document;
        }

        public int Start { get; }

        public virtual string Text { get; protected set; }

        public Document Document { get; }

        public virtual int End => Start + Text.Length;

        public virtual int Length => End - Start;

        public List<Reference> Variables { get; } = new List<Reference>();

        public virtual bool IntersectsWith(int position)
        {
            return Start <= position && End >= position;
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
