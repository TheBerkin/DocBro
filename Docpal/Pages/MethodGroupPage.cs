#region License
// https://github.com/TheBerkin/DocBro
// 
// Copyright (c) 2017 Nicholas Fleck
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Docpal.Pages
{
    public class MethodGroupPage : Page
    {
        private readonly Type _type;
        private readonly Dictionary<MethodInfo, MemberXmlDocs> _methodData;

        public MethodGroupPage(Type type, string name, Dictionary<MethodInfo, MemberXmlDocs> methodData) : base(null)
        {
            _type = type;
            _methodData = methodData;
            Title = $"{DocUtilities.GetDisplayTitle(type)}.{DocUtilities.GetIdentifier(name)} method";
        }

        public override void Render(PageTree parent, MarkdownWriter writer)
        {
            writer.WriteHeader(1, Title);
            foreach (var (data, idx) in _methodData                
                .OrderBy(m => m.Key.GetParameters().Length)
                .Select((data, idx) => (data, idx)))
            {
                var method = data.Key;
                var docs = data.Value;

                if (Args.MethodGroupsSpacing && idx > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine("<p>&nbsp;</p>");
                    writer.WriteLine("<p>&nbsp;</p>");
                    writer.WriteLine("<hr/>");
                    writer.WriteLine();
                }

                writer.WriteHeader(2, DocUtilities.GetMethodSignature(method, false, false));
                writer.WriteParagraph(docs?.Summary);
                writer.WriteHeader(3, "Signature");
                writer.WriteCodeBlock("csharp", DocUtilities.GetMethodSignature(method, true, true));

                if (docs != null)
                {
                    if (docs.HasTypeParameters)
                    {
                        writer.WriteHeader(3, "Type Parameters");
                        foreach (var tp in method.GetGenericArguments())
                        {
                            var desc = docs?.GetTypeParameterDescription(tp.Name) ?? "_(No Description)_";
                            writer.WriteLine($"- `{tp.Name}`: {desc}");
                        }
                        writer.WriteLine();
                    }

                    if (docs.HasParameters)
                    {
                        writer.WriteHeader(3, "Parameters");
                        foreach (var p in method.GetParameters())
                        {
                            var desc = docs?.GetParameterDescription(p.Name) ?? "_(No Description)_";
                            writer.WriteLine($"- `{p.Name}`: {desc}");
                        }
                        writer.WriteLine();
                    }

                    if (docs.Returns != null)
                    {
                        writer.WriteHeader(3, "Returns");
                        writer.WriteLine(docs.Returns);
                    }

                    if (docs.Remarks != null)
                    {
                        writer.WriteHeader(3, "Remarks");
                        writer.WriteLine(docs.Remarks);
                    }

                }
            }
        }
    }
}