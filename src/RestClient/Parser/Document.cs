using System.Collections.Generic;
using System.Linq;

namespace RestClient
{
    public partial class Document
    {
        private Dictionary<string, string>? _variables = null;
        private string[] _lines;

        protected Document(string[] lines)
        {
            _lines = lines;
            _ = ParseAsync();
        }

        public List<Token> Tokens { get; private set; } = new List<Token>();

        public List<Token> Hierarchy { get; private set; } = new List<Token>();

        public Dictionary<string, string>? Variables => _variables;

        public IEnumerable<Request> Requests => Hierarchy.OfType<Request>();

        public void UpdateLines(string[] lines)
        {
            _lines = lines;
        }

        public static Document FromLines(params string[] lines)
        {
            var doc = new Document(lines);
            return doc;
        }

        private void ExpandVariables()
        {
            IEnumerable<Variable>? variables = Tokens.OfType<Variable>();

            Dictionary<string, string> expandedVars = new();

            // Start by expanding all variable definitions
            foreach (Variable variable in variables)
            {
                var value = variable.Value!.Text;

                foreach (var key in expandedVars.Keys)
                {
                    value = value.Replace("{{" + key + "}}", expandedVars[key].Trim());
                }

                expandedVars[variable.Name!.Text] = value;
            }

            _variables = expandedVars;
        }

        public Token? GetTokenFromPosition(int position)
        {
            Token token = Tokens.LastOrDefault(t => t.IntersectsWith(position));

            if (token is Url url && url.Uri!.IntersectsWith(position))
            {
                return GetVariableFromPosition(url.Uri, position);
            }

            if (token is Header header)
            {
                if (header.Name!.IntersectsWith(position))
                {
                    return GetVariableFromPosition(header.Name, position);
                }

                if (header.Value!.IntersectsWith(position))
                {
                    return GetVariableFromPosition(header.Value, position);
                }
            }

            if (token is BodyToken body)
            {
                return GetVariableFromPosition(body, position);
            }

            return token;
        }

        private Token? GetVariableFromPosition(Token token, int position)
        {
            return token.References.FirstOrDefault(v => v.IntersectsWith(position));
        }
    }
}
