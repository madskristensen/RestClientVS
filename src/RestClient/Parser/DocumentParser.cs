using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RestClient
{
    public partial class Document
    {
        private readonly Regex _regexUrl = new(@"^((?<method>get|post|put|delete|head|options|trace)\s*)(?<url>.+)", RegexOptions.IgnoreCase);
        private readonly Regex _regexHeader = new(@"^(?<name>[^\s]+)?([\s]+)?(?<operator>:)(?<value>.+)");
        private readonly Regex _regexVariable = new(@"^(?<name>@[^\s]+)\s*(?<equals>=)\s*(?<value>.+)");
        private readonly Regex _regexRef = new(@"(?<open>{{)(?<value>\$?[\w]+( [\w]+)?)?(?<close>}})");

        public bool IsParsing { get; private set; }

        public Task ParseAsync()
        {
            IsParsing = true;

            return Task.Run(() =>
            {
                var start = 0;

                List<ParseItem> tokens = new();

                foreach (var line in _lines)
                {
                    IEnumerable<ParseItem>? current = TokenizeLine(start, line, tokens);

                    if (current != null)
                    {
                        tokens.AddRange(current);
                    }

                    start += line.Length;
                }

                Tokens = tokens;

                CreateHierarchyOfChildren();
                ExpandVariables();
                ValidateDocument();

                IsParsing = false;
                Parsed?.Invoke(this, EventArgs.Empty);
            });
        }

        private IEnumerable<ParseItem> TokenizeLine(int start, string line, List<ParseItem> tokens)
        {
            var trimmedLine = line.Trim();
            List<ParseItem> items = new();

            // Comment
            if (trimmedLine.StartsWith(Constants.CommentChar.ToString()))
            {
                items.Add(new ParseItem(start, line, this, ItemType.Comment));
            }
            // Variable declaration
            else if (IsMatch(_regexVariable, trimmedLine, out Match matchVar))
            {
                items.AddRange(ParseVariable(matchVar, start, line));
            }
            // Request body
            else if (IsBodyToken(line, tokens))
            {
                var token = new ParseItem(start, line, this, ItemType.Body);
                AddVariableReferences(token);
                items.Add(token);
            }
            // Empty line
            else if (string.IsNullOrWhiteSpace(line))
            {
                items.Add(new ParseItem(start, line, this, ItemType.EmptyLine));
            }
            // Request url
            else if (IsMatch(_regexUrl, trimmedLine, out Match matchUrl))
            {
                items.AddRange(ParseUrlToken(matchUrl, start, line));
            }
            // Header
            else if (tokens.Any() && IsMatch(_regexHeader, trimmedLine, out Match matchHeader))
            {
                ParseItem? last = tokens.Last();
                if (last?.Type == ItemType.HeaderValue || last?.Type == ItemType.Url || last?.Type == ItemType.Comment)
                {
                    items.AddRange(ParseRequestHeaders(matchHeader, start, line));
                }
            }

            return items;
        }

        private bool IsBodyToken(string line, List<ParseItem> tokens)
        {
            ParseItem? last = tokens.LastOrDefault();

            if (last != null && string.IsNullOrWhiteSpace(last.Text) && string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            if (last?.Type == ItemType.Body)
            {
                return true;
            }

            if (last?.Type != ItemType.EmptyLine)
            {
                return false;
            }

            ParseItem? parent = tokens.ElementAtOrDefault(Math.Max(0, tokens.Count - 2));

            if (parent?.Type == ItemType.HeaderValue || parent?.Type == ItemType.Url || (parent?.Type == ItemType.Comment && parent?.TextExcludingLineBreaks != "###"))
            {
                return true;
            }

            return false;
        }

        public static bool IsMatch(Regex regex, string line, out Match match)
        {
            match = regex.Match(line);
            return match.Success;
        }

        private IEnumerable<ParseItem> ParseUrlToken(Match match, int start, string line)
        {
            Group? methodGroup = match.Groups["method"];
            Group? urlGroup = match.Groups["url"];

            var method = new ParseItem(start + methodGroup.Index, methodGroup.Value, this, ItemType.Method);
            var url = new ParseItem(start + urlGroup.Index, urlGroup.Value, this, ItemType.Url);

            AddVariableReferences(method);
            AddVariableReferences(url);

            yield return method;
            yield return url;
        }

        private IEnumerable<ParseItem> ParseRequestHeaders(Match match, int start, string line)
        {
            Group? nameGroup = match.Groups["name"];
            Group? valueGroup = match.Groups["value"];

            var name = new ParseItem(start + nameGroup.Index, nameGroup.Value, this, ItemType.HeaderName);
            var value = new ParseItem(start + valueGroup.Index, valueGroup.Value, this, ItemType.HeaderValue);

            AddVariableReferences(name);
            AddVariableReferences(value);

            yield return name;
            yield return value;
        }

        private IEnumerable<ParseItem> ParseVariable(Match match, int start, string line)
        {
            Group? nameGroup = match.Groups["name"];
            Group? valueGroup = match.Groups["value"];

            var name = new ParseItem(start + nameGroup.Index, nameGroup.Value, this, ItemType.VariableName);
            var value = new ParseItem(start + valueGroup.Index, valueGroup.Value, this, ItemType.VariableValue);

            AddVariableReferences(value);

            yield return name;
            yield return value;
        }

        private void AddVariableReferences(ParseItem token)
        {
            var start = token.Start;

            foreach (Match match in _regexRef.Matches(token.Text))
            {
                Group? openGroup = match.Groups["open"];
                Group? valueGroup = match.Groups["value"];
                Group? closeGroup = match.Groups["close"];

                var reference = new Reference
                {
                    Open = new ParseItem(start + openGroup.Index, openGroup.Value, this, ItemType.ReferenceBraces),
                    Value = new ParseItem(start + valueGroup.Index, valueGroup.Value, this, ItemType.ReferenceName),
                    Close = new ParseItem(start + closeGroup.Index, closeGroup.Value, this, ItemType.ReferenceBraces),
                };

                token.References.Add(reference);
            }
        }

        private void ValidateDocument()
        {
            // Variable references
            foreach (ParseItem? token in Tokens)
            {
                foreach (Reference reference in token.References)
                {
                    if (VariablesExpanded != null && reference.Value != null && !VariablesExpanded.ContainsKey(reference.Value.Text))
                    {
                        reference.Value.Errors.Add($"The variable \"{reference.Value.Text}\" is not defined.");
                    }
                }
            }

            // URLs
            foreach (ParseItem? url in Tokens.Where(t => t.Type == ItemType.Url))
            {
                var uri = url.ExpandVariables();

                if (!Uri.TryCreate(uri, UriKind.Absolute, out _))
                {
                    url.Errors.Add($"\"{uri}\" is not a valid absolute URI");
                }
            }
        }

        private void CreateHierarchyOfChildren()
        {
            Request? currentRequest = null;
            List<Request> requests = new();
            List<Variable> variables = new();

            foreach (ParseItem? token in Tokens)
            {
                if (token.Type == ItemType.VariableName)
                {
                    var variable = new Variable
                    {
                        Name = token,
                        Value = token.Next,
                    };

                    variables.Add(variable);
                }

                else if (token.Type == ItemType.Method)
                {
                    currentRequest = new Request(this)
                    {
                        Method = token,
                        Url = token.Next
                    };

                    requests.Add(currentRequest);
                    currentRequest?.Children?.Add(currentRequest.Method);
                    currentRequest?.Children?.Add(currentRequest.Url!);
                }

                else if (currentRequest != null)
                {
                    if (token.Type == ItemType.HeaderName)
                    {
                        var header = new Header
                        {
                            Name = token,
                            Value = token.Next,
                        };

                        currentRequest?.Headers?.Add(header);
                        currentRequest?.Children?.Add(header.Name);
                        currentRequest?.Children?.Add(header.Value!);
                    }
                    else if (token.Type == ItemType.Body)
                    {
                        if (string.IsNullOrWhiteSpace(token.Text))
                        {
                            continue;
                        }

                        var prevEmptyLine = token.Previous?.Type == ItemType.Body && string.IsNullOrWhiteSpace(token.Previous.Text) ? token.Previous.Text : "";
                        currentRequest.Body += prevEmptyLine + token.Text;
                        currentRequest?.Children?.Add(token);
                    }
                    else if (token?.Type == ItemType.Comment)
                    {
                        if (token.Text.StartsWith("###"))
                        {
                            currentRequest = null;
                        }
                    }
                }
            }

            Variables = variables;
            Requests = requests;
        }

        public event EventHandler? Parsed;
    }
}
