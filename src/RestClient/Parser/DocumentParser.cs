using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RestClient
{
    public partial class Document
    {
        public bool IsParsing { get; private set; }

        public Task ParseAsync()
        {
            IsParsing = true;

            return Task.Run(() =>
            {
                var start = 0;

                List<Token> tokens = new();

                foreach (var line in _lines)
                {
                    Token? current = TokenizeLine(start, line, tokens);

                    if (current != null)
                    {
                        tokens.Add(current);
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

        private Token? TokenizeLine(int start, string line, List<Token> tokens)
        {
            var trimmedLine = line.Trim();

            // Comment
            if (trimmedLine.StartsWith(Constants.CommentChar.ToString()))
            {
                return new Comment(start, line, this);
            }
            // Variable declaration
            else if (trimmedLine.StartsWith("@"))
            {
                return ParseVariable(start, line);
            }
            // Request body
            else if (IsBodyToken(line, tokens))
            {
                var token = new BodyToken(start, line, this);
                AddVariableReferences(token);
                return token;
            }
            // Empty line
            else if (string.IsNullOrWhiteSpace(line))
            {
                return new EmptyLine(start, line, this);
            }
            // Request url
            else if (trimmedLine.Contains("://"))
            {
                return ParseUrlToken(start, line);
            }
            // Header
            else if (trimmedLine.Contains(":") && tokens.Any())
            {
                Token? last = tokens.Last();
                if (last is Header || last is Url || last is Comment)
                {
                    return ParseRequestHeaders(start, line);
                }
            }

            return null;
        }

        private bool IsBodyToken(string line, List<Token> tokens)
        {
            Token? last = tokens.LastOrDefault();

            if (last != null && string.IsNullOrWhiteSpace(last.Text) && string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            if (last is BodyToken)
            {
                return true;
            }

            if (last is not EmptyLine)
            {
                return false;
            }

            Token? parent = tokens.ElementAt(Math.Max(0, tokens.Count - 2));

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

                var urlToken = new Url(start, line, this)
                {
                    Method = new TextSpan(start + methodGroup.Index, methodGroup.Value, this),
                    Uri = new TextSpan(start + urlGroup.Index, urlGroup.Value, this),
                };

                AddVariableReferences(urlToken);
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

                var header = new Header(start, line, this)
                {
                    Name = new TextSpan(start + nameGroup.Index, nameGroup.Value, this),
                    Value = new TextSpan(start + valueGroup.Index, valueGroup.Value, this),
                    Operator = new TextSpan(start + operatorGroup.Index, operatorGroup.Value, this),
                };

                AddVariableReferences(header);
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

                var variable = new Variable(start, line, this)
                {
                    At = new TextSpan(start + atGroup.Index, atGroup.Value, this),
                    Name = new TextSpan(start + nameGroup.Index, nameGroup.Value, this),
                    Value = new TextSpan(start + valueGroup.Index, valueGroup.Value, this),
                    Operator = new TextSpan(start + equalsGroup.Index, equalsGroup.Value, this),
                };

                AddVariableReferences(variable.Value);
                AddVariableReferences(variable);

                return variable;
            }

            return null;
        }

        private void AddVariableReferences(Token token)
        {
            var regex = new Regex(@"(?<open>{{)(?<value>\$?[\w]+( [\w]+)?)?(?<close>}})");
            var start = token.Start;

            foreach (Match match in regex.Matches(token.Text))
            {
                Group? openGroup = match.Groups["open"];
                Group? valueGroup = match.Groups["value"];
                Group? closeGroup = match.Groups["close"];

                var reference = new Reference(start + match.Index, match.Value, this)
                {
                    Open = new TextSpan(start + openGroup.Index, openGroup.Value, this),
                    Value = new TextSpan(start + valueGroup.Index, valueGroup.Value, this),
                    Close = new TextSpan(start + closeGroup.Index, closeGroup.Value, this),
                };

                token.References.Add(reference);
                //_document.Tokens.Add(reference);
            }
        }

        private void ValidateDocument()
        {
            // Variable references
            foreach (Token? token in Tokens)
            {
                foreach (Reference reference in token.References)
                {
                    if (Variables != null && reference.Value != null && !Variables.ContainsKey(reference.Value.Text))
                    {
                        reference.Errors.Add($"The variable \"{reference.Value.Text}\" is not defined.");
                    }
                }
            }

            // URLs
            foreach (Url? url in Tokens.OfType<Url>())
            {
                var uri = url.Uri?.ExpandVariables();

                if (!Uri.TryCreate(uri, UriKind.Absolute, out _))
                {
                    url.Errors.Add($"\"{uri}\" is not a valid absolute URI");
                }
            }
        }

        private void CreateHierarchyOfChildren()
        {
            Request? currentRequest = null;
            List<Token> hierarchy = new();

            foreach (Token? token in Tokens)
            {
                if (token is Variable)
                {
                    hierarchy.Add(token);
                }

                else if (token is Url url)
                {
                    currentRequest = new Request(token.Start, token.Text, this)
                    {
                        Url = url
                    };

                    hierarchy.Add(currentRequest);
                    currentRequest?.Children?.Add(url);
                }

                else if (currentRequest != null)
                {
                    if (token is Header header)
                    {
                        currentRequest?.Headers?.Add(header);
                        currentRequest?.Children?.Add(header);
                    }
                    else if (token is BodyToken body)
                    {
                        if (string.IsNullOrWhiteSpace(body.Text))
                        {
                            continue;
                        }

                        var prevEmptyLine = body.Previous is BodyToken && string.IsNullOrWhiteSpace(body.Previous.Text) ? body.Previous.Text : "";

                        if (currentRequest.Body != null)
                        {
                            currentRequest.Body.Increase(prevEmptyLine + body.Text);
                        }
                        else
                        {
                            currentRequest.Body = body;
                            currentRequest?.Children?.Add(body);
                        }
                    }
                    else if (token is Comment comment)
                    {
                        if (comment.IsSeparator)
                        {
                            currentRequest = null;
                            hierarchy.Add(comment);
                        }
                        else
                        {
                            currentRequest?.Children?.Add(comment);
                        }
                    }
                }
                else
                {
                    if (token is Comment)
                    {
                        hierarchy.Add(token);
                    }
                }
            }

            Hierarchy = hierarchy;
        }

        public event EventHandler? Parsed;
    }
}
