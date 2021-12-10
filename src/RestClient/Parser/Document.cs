﻿using System.Collections.Generic;
using System.Linq;

namespace RestClient
{
    public class Document
    {
        private Document()
        { }

        public List<Token> Tokens { get; } = new List<Token>();
        public List<Token> Hierarchy { get; } = new List<Token>();
        public IEnumerable<Variable> Variables => Hierarchy.OfType<Variable>();
        public IEnumerable<Request> Requests => Hierarchy.OfType<Request>();

        public static Document FromLines(params string[] lines)
        {
            var doc = new Document();
            var parser = new Tokenizer(doc);

            parser.Parse(lines);

            doc.CreateHierarchyOfChildren();

            return doc;
        }

        private void CreateHierarchyOfChildren()
        {
            Request? currentRequest = null;

            foreach (Token? token in Tokens)
            {
                if (token is Variable)
                {
                    Hierarchy.Add(token);
                }

                else if (token is Url url)
                {
                    currentRequest = new Request(token.Start, token.Text, this)
                    {
                        Url = url
                    };

                    Hierarchy.Add(currentRequest);
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
                        if (currentRequest.Body != null)
                        {
                            currentRequest.Body.Increase(body.Text);
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
                        Hierarchy.Add(token);
                    }
                }
            }
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
            return token.Variables.FirstOrDefault(v => v.IntersectsWith(position));
        }
    }
}
