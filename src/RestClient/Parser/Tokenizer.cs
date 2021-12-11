using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace RestClient
{
    public class Tokenizer
    {
        private readonly Document _document;

        public Tokenizer(Document document)
        {
            _document = document;
        }

        public void Parse(params string[] lines)
        {
            var start = 0;

            foreach (var line in lines)
            {
                Token? current = TokenizeLine(start, line);

                if (current != null)
                {
                    _document.Tokens.Add(current);
                }

                start += line.Length;
            }
        }

        private Token? TokenizeLine(int start, string line)
        {
            var trimmedLine = line.Trim();

            // Comment
            if (trimmedLine.StartsWith("#") || trimmedLine.StartsWith("//"))
            {
                return new Comment(start, line, _document);
            }
            // Variable declaration
            else if (trimmedLine.StartsWith("@"))
            {
                return ParseVariable(start, line);
            }
            // Empty line
            else if (string.IsNullOrWhiteSpace(line))
            {
                return new EmptyLine(start, line, _document);
            }
            // Request body
            else if (IsBodyToken())
            {
                var token = new BodyToken(start, line, _document);
                AddVariableReferences(token);
                return token;
            }
            // Request url
            else if (trimmedLine.Contains("://"))
            {
                return ParseUrlToken(start, line);
            }
            // Header
            else if (trimmedLine.Contains(":") && (_document.Tokens.Last() is Header || _document.Tokens.Last() is Url))
            {
                return ParseRequestHeaders(start, line);
            }

            return null;
        }

        private bool IsBodyToken()
        {
            Token? last = _document.Tokens.LastOrDefault();

            if (last is BodyToken)
            {
                return true;
            }

            if (last is not EmptyLine)
            {
                return false;
            }

            Token? parent = _document.Tokens.ElementAt(Math.Max(0, _document.Tokens.Count - 2));

            if (parent is Header || parent is Url || (parent is Comment comment && !comment.IsSeparator))
            {
                return true;
            }

            return false;
        }

        private Token? ParseUrlToken(int start, string line)
        {
            var regex = new Regex(@"^((?<method>get|post|put|delete|patch|head|options|connect|trace)?\s*)(?<url>.+://.+)", RegexOptions.IgnoreCase);
            Match match = regex.Match(line);

            if (match.Success)
            {
                Group? methodGroup = match.Groups["method"];
                Group? urlGroup = match.Groups["url"];

                var urlToken = new Url(start, line, _document)
                {
                    Method = new TextSpan(start + methodGroup.Index, methodGroup.Value, _document),
                    Uri = new TextSpan(start + urlGroup.Index, urlGroup.Value, _document),
                };

                AddVariableReferences(urlToken.Method);
                AddVariableReferences(urlToken.Uri);

                return urlToken;
            }

            return null;
        }

        private Token? ParseRequestHeaders(int start, string line)
        {
            var regex = new Regex(@"^(?<name>[^\s]+)?([\s]+)?(?<operator>:)(?<value>.+)");
            Match match = regex.Match(line);

            if (match.Success)
            {
                Group? nameGroup = match.Groups["name"];
                Group? valueGroup = match.Groups["value"];
                Group? operatorGroup = match.Groups["operator"];

                var header = new Header(start, line, _document)
                {
                    Name = new TextSpan(start + nameGroup.Index, nameGroup.Value, _document),
                    Value = new TextSpan(start + valueGroup.Index, valueGroup.Value, _document),
                    Operator = new TextSpan(start + operatorGroup.Index, operatorGroup.Value, _document),
                };

                AddVariableReferences(header.Name);
                AddVariableReferences(header.Value);

                return header;
            }

            return null;
        }

        private Token? ParseVariable(int start, string line)
        {
            var regex = new Regex(@"^(?<at>@)(?<name>[^\s]+)\s*(?<equals>=)\s*(?<value>.+)");
            Match match = regex.Match(line);

            if (match.Success)
            {
                Group? atGroup = match.Groups["at"];
                Group? nameGroup = match.Groups["name"];
                Group? valueGroup = match.Groups["value"];
                Group? equalsGroup = match.Groups["equals"];

                var variable = new Variable(start, line, _document)
                {
                    At = new TextSpan(start + atGroup.Index, atGroup.Value, _document),
                    Name = new TextSpan(start + nameGroup.Index, nameGroup.Value, _document),
                    Value = new TextSpan(start + valueGroup.Index, valueGroup.Value, _document),
                    Operator = new TextSpan(start + equalsGroup.Index, equalsGroup.Value, _document),
                };

                AddVariableReferences(variable.Value);

                return variable;
            }

            return null;
        }

        private void AddVariableReferences(Token token)
        {
            var regex = new Regex(@"(?<open>{{)(?<value>\$?[\w]+( [\w]+)?)(?<close>}})");
            var start = token.Start;

            foreach (Match match in regex.Matches(token.Text))
            {
                Group? openGroup = match.Groups["open"];
                Group? valueGroup = match.Groups["value"];
                Group? closeGroup = match.Groups["close"];

                var reference = new Reference(start + match.Index, match.Value, _document)
                {
                    Open = new TextSpan(start + openGroup.Index, openGroup.Value, _document),
                    Value = new TextSpan(start + valueGroup.Index, valueGroup.Value, _document),
                    Close = new TextSpan(start + closeGroup.Index, closeGroup.Value, _document),
                };

                token.Variables.Add(reference);
                //_document.Tokens.Add(reference);
            }
        }
    }
}
