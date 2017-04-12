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
	public class PropertyPage : Page
	{
		private readonly PropertyInfo _property;

		public PropertyPage(PropertyInfo property, MemberData docs) : base(docs)
		{
			_property = property;
			Title = $"{Util.GetPropertySignature(_property, false, false, false)} Property ({Util.GetDisplayTitle(_property.DeclaringType)})";
		}

		public override void Render(Node parent, MarkdownWriter writer)
		{
			writer.WriteHeader(1, Title);
			writer.WriteParagraph(Docs?.Summary ?? "(No Description)");
			writer.WriteHeader(2, "Signature");
			writer.WriteCodeBlock("csharp", Util.GetPropertySignature(_property, true, true, true));
		}
	}
}