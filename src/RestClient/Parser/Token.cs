using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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

        [DebuggerDisplay("{Text}")]
        public string Text { get; protected set; }

        public Document Document { get; }

        public virtual int End => Start + Text.Length;

        public virtual int Length => End - Start;

        public List<Reference> Variables { get; } = new List<Reference>();

        public string ExpandVariables()
        {
            IEnumerable<Variable>? variables = Document.Tokens.OfType<Variable>();

            var sb = new StringBuilder(Text);

            foreach (Variable? variable in variables)
            {
                sb.Replace("{{" + variable?.Name?.Text + "}}", variable?.Value?.Text);
            }

            return sb.ToString();
        }
    }
}
