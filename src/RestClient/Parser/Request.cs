using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestClient
{
    public class Request
    {
        private readonly Document _document;

        public Request(Document document)
        {
            _document = document;
        }

        public List<ParseItem>? Children { get; set; } = new List<ParseItem>();

        public ParseItem? Method { get; set; }
        public ParseItem? Url { get; set; }

        public List<Header>? Headers { get; } = new();

        public string? Body { get; set; }

        public int Start => Method?.Start ?? 0;
        public int End => Children.LastOrDefault()?.End ?? 0;

        public int Length => End - Start;

        public bool Contains(int position)
        {
            return position >= Start && position <= End;
        }

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.AppendLine($"{Method?.Text} {Url?.ExpandVariables()}");

            foreach (Header header in Headers!)
            {
                sb.AppendLine($"{header?.Name?.ExpandVariables()}: { header?.Value?.ExpandVariables()}");
            }

            if (!string.IsNullOrEmpty(Body))
            {
                sb.AppendLine(ExpandBodyVariables());
            }

            return sb.ToString().Trim();
        }

        public string ExpandBodyVariables()
        {
            if (Body == null)
            {
                return "";
            }

            // Then replace the references with the expanded values
            var clean = Body;

            if (_document.VariablesExpanded != null)
            {
                foreach (var key in _document.VariablesExpanded.Keys)
                {
                    clean = clean.Replace("{{" + key + "}}", _document.VariablesExpanded[key].Trim());
                }
            }

            return clean.Trim();
        }
    }
}
