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

        public List<ParseItem> Items { get; private set; } = new List<ParseItem>();

        public Dictionary<string, string>? VariablesExpanded => _variables;

        public List<Request> Requests { get; private set; } = new();
        public List<Variable> Variables { get; private set; } = new();

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
            Dictionary<string, string> expandedVars = new();

            foreach (Variable variable in Variables)
            {
                var value = variable.Value.Text;

                foreach (var key in expandedVars.Keys)
                {
                    value = value.Replace("{{" + key + "}}", expandedVars[key].Trim());
                }

                expandedVars[variable.Name.Text.Substring(1)] = value;
            }

            _variables = expandedVars;
        }

        public ParseItem? GetTokenFromPosition(int position)
        {
            ParseItem? item = Items.LastOrDefault(t => t.Contains(position));
            ParseItem? reference = item.References.FirstOrDefault(v => v.Value != null && v.Value.Contains(position))?.Value;

            // Return the reference if it exist; otherwise the item
            return reference ?? item;
        }
    }
}
