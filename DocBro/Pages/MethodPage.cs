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

using System.Reflection;

namespace DocBro
{
	public class MethodPage : Page
	{
		private readonly MethodInfo _method;

		public MethodPage(MethodInfo method, MemberData docs) : base(docs)
		{
			_method = method;
			Title = $"{Util.GetMethodSignature(method, false, false)} method ({Util.GetDisplayTitle(_method.DeclaringType)})";
		}

		public override void Render(Node parent, MarkdownWriter writer)
		{
			writer.WriteHeader(1, Title);
			if (Docs != null) writer.WriteParagraph(Docs.Summary);
			writer.WriteHeader(2, "Signature");
			writer.WriteCodeBlock("csharp", Util.GetMethodSignature(_method, true));
			
			if (Docs != null)
			{
				if (Docs.HasTypeParameters)
				{
					writer.WriteHeader(2, "Type Parameters");
					foreach (var tp in _method.GetGenericArguments())
					{
						var desc = Docs?.GetTypeParameterDescription(tp.Name) ?? "(No Description)";
						writer.WriteLine($"- `{tp.Name}`: {desc}");
					}
					writer.WriteLine();
				}

				if (Docs.HasParameters)
				{
					writer.WriteHeader(2, "Parameters");
					foreach (var p in _method.GetParameters())
					{
						var desc = Docs?.GetParameterDescription(p.Name) ?? "(No Description)";
						writer.WriteLine($"- `{p.Name}`: {desc}");
					}
					writer.WriteLine();
				}
			}
		}
	}
}