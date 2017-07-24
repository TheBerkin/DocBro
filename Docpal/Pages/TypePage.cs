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
using System.Linq;
using System.Text;

namespace Docpal.Pages
{
	class TypePage : Page
	{
		public Type Type { get; }

		public TypePage(Type type, MemberXmlDocs docs) : base(docs)
		{
			Type = type;

			var name = DocUtilities.GetDisplayTitle(type, false);

			if (type.IsEnum)
			{
				Title = $"{name} Enum";
			}
			else if (type.IsInterface)
			{
				Title = $"{name} Interface";
			}
			else if (type.IsValueType)
			{
				Title = $"{name} Struct";
			}
			else if (type.IsSubclassOf(typeof(Delegate)))
			{
				Title = $"{name} Delegate";
			}
			else
			{
				Title = $"{name} Class";
			}
		}

		public override void Render(PageTree parent, MarkdownWriter writer)
		{
			writer.WriteHeader(1, Title);
			writer.WriteParagraph($"**Namespace:** {Type.Namespace}");
			var inheritance = DocUtilities.GetInheritanceString(Type);
			if (!String.IsNullOrEmpty(inheritance))
				writer.WriteParagraph($"**Inheritance:** {inheritance}");
			writer.WriteParagraph(Docs?.Summary ?? "(No Description)");
			writer.WriteHeader(2, "Signature");
			writer.WriteCodeBlock("csharp", DocUtilities.GetClassSignature(Type));
			
			var methods = Type.GetMethods().OrderBy(m => m.Name).ToArray();
			if (methods.Length > 0)
			{
				writer.WriteHeader(2, "Methods");
				foreach (var methodGroup in methods.Where(m => !m.IsSpecialName).GroupBy(m => m.Name))
				{
					var sbLink = new StringBuilder($"- [{DocUtilities.GetIdentifier(methodGroup.Key)}]({DocUtilities.GetURLTitle(Type)}/{DocUtilities.GetIdentifier(methodGroup.Key)}.md)");
					bool isStatic = methodGroup.All(m => m.IsStatic);
					if (isStatic) sbLink.Append(" (static)");
					writer.WriteLine(sbLink.ToString());
				}
			}
			
			var props = Type.GetProperties()
				.Where(p => p.GetIndexParameters().Length == 0)
				.OrderBy(p => p.Name)
				.ToArray();
			if (props.Length > 0)
			{
				writer.WriteHeader(2, "Properties");
				foreach (var prop in props)
				{
					var sbLink = new StringBuilder($"- [{DocUtilities.GetIdentifier(prop.Name)}]({DocUtilities.GetURLTitle(Type)}/{DocUtilities.GetIdentifier(prop.Name)}.md)");
					bool isStatic = prop.CanRead && prop.GetGetMethod(true).IsStatic || prop.CanWrite && prop.GetSetMethod(true).IsStatic;
					if (isStatic) sbLink.Append(" (static)");
					writer.WriteLine(sbLink.ToString());
				}
			}

			var fields = Type.GetFields()
				.Where(f => !f.IsSpecialName)
				.OrderBy(f => f.Name)
				.ToArray();
			if (fields.Length > 0)
			{
				writer.WriteHeader(2, "Fields");
				foreach (var field in fields)
				{
					var sbLink = new StringBuilder($"- [{DocUtilities.GetIdentifier(field.Name)}]({DocUtilities.GetURLTitle(Type)}/{DocUtilities.GetIdentifier(field.Name)}.md)");
					if (field.IsStatic) sbLink.Append(" (static)");
					writer.WriteLine(sbLink.ToString());
				}
			}

			// TODO: Events
			//writer.WriteHeader(2, "Events");


			var operators = Type.GetMethods()
				.Where(m => m.Name.StartsWith("op_") && m.Name != "op_Explicit" && m.Name != "op_Implicit")
				.ToArray();
			if (operators.Length > 0)
			{
				writer.WriteHeader(2, "Operators");
				foreach (var op in operators)
				{
					writer.WriteLine($"- [{DocUtilities.GetOperatorSymbol(op.Name)}]({DocUtilities.GetURLTitle(Type)}/{op.Name}.md)");
				}
			}

			writer.WriteHeader(2, "Conversions");
		}
	}
}