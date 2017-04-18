﻿#region License
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
using System.Xml;

namespace DocBro
{
	public static class DBSlim
	{
		private const string Lang = "csharp";

		public static void BuildDocs(XmlDocument xml, Assembly dll, string outputFilePath)
		{
			var meta = new Dictionary<string, MemberDocs>();
			foreach (XmlNode item in xml.SelectNodes("//doc/members/member"))
			{
				var id = item.Attributes["name"].Value;
				var data = meta[id] = new MemberDocs
				{
					Summary = item.SelectSingleNode("summary")?.InnerText.Trim() ?? "(No Description)",
					Returns = item.SelectSingleNode("returns")?.InnerText.Trim() ?? String.Empty,
					Remarks = item.SelectSingleNode("remarks")?.InnerText.Trim() ?? String.Empty
				};

				foreach (XmlNode desc in item.SelectNodes("param"))
				{
					data.SetParameterDescription(desc.Attributes["name"].Value, desc.InnerText.Trim());
				}

				foreach (XmlNode desc in item.SelectNodes("typeparam"))
				{
					data.SetTypeParameterDescription(desc.Attributes["name"].Value, desc.InnerText.Trim());
				}
			}

			using(var writer = new MarkdownWriter(outputFilePath))
			{
				foreach (var type in dll.ExportedTypes.Where(t => !t.Name.StartsWith("_")).OrderBy(t => t.Name))
				{
					// Pull XML docs for type
					meta.TryGetValue(ID.GetIDString(type), out MemberDocs typeDocs);

					// Header, e.g. "StringBuilder class (System)"
					writer.WriteHeader(2, Util.GetTypeTitle(type, true), true);

					// Summary and other info
					PrintObsoleteWarning(type, writer);
					writer.WriteParagraph($"**Namespace:** {type.Namespace}");
					writer.WriteParagraph($"**Inheritance:** {Util.GetInheritanceString(type)}", true);
					Summary(typeDocs, writer);

					// Signature
					writer.WriteCodeBlock(Lang, Util.GetClassSignature(type));

					WriteConstructors(type, writer, meta);
					WriteProperties(type, writer, meta);
					WriteFields(type, writer, meta);
					WriteMethods(type, writer, meta);
				}
			}
		}

		private static void WriteMethods(Type type, MarkdownWriter writer, Dictionary<string, MemberDocs> docs)
		{
			var methods = type.GetMethods()
				.Where(m => !m.IsSpecialName)
				.OrderBy(m => m.Name)
				.ThenBy(m => m.GetParameters().Length)
				.ToArray();

			if (methods.Length > 0)
			{
				writer.WriteHeader(3, "Methods");
				foreach (var method in methods)
				{
					docs.TryGetValue(ID.GetIDString(method), out MemberDocs methodDocs);

					// Heading
					writer.WriteHeader(4, $"{Util.GetMethodSignature(method, false, false)} method", true);
					PrintObsoleteWarning(method, writer);
					Summary(methodDocs, writer);
					writer.WriteCodeBlock(Lang, Util.GetMethodSignature(method, true, true));

					// Parameters
					WriteParamList(4, method, writer, methodDocs);
					Returns(4, methodDocs, writer);
					Remarks(4, methodDocs, writer);
				}
			}
		}

		private static void WriteProperties(Type type, MarkdownWriter writer, Dictionary<string, MemberDocs> docs)
		{
			var props = type.GetProperties()
				.Where(p => p.GetIndexParameters().Length == 0)
				.OrderBy(p => p.Name)
				.ToArray();
			if (props.Length == 0) return;

			writer.WriteHeader(3, "Properties");

			for (int i = 0; i < props.Length; i++)
			{
				docs.TryGetValue(ID.GetIDString(props[i]), out MemberDocs propDocs);
				writer.WriteHeader(4, $"{Util.GetPropertySignature(props[i], false, false, false)} property");
				PrintObsoleteWarning(props[i], writer);
				Summary(propDocs, writer);

				writer.WriteCodeBlock(Lang, Util.GetPropertySignature(props[i], true, true, true));

				Remarks(5, propDocs, writer);
			}
		}

		private static void WriteFields(Type type, MarkdownWriter writer, Dictionary<string, MemberDocs> docs)
		{
			var fields = type.GetFields()
				.Where(f => !f.IsSpecialName)
				.OrderBy(f => f.Name)
				.ToArray();

			if (fields.Length == 0) return;

			writer.WriteHeader(3, "Fields");

			for (int i = 0; i < fields.Length; i++)
			{
				docs.TryGetValue(ID.GetIDString(fields[i]), out MemberDocs fieldDocs);
				writer.WriteHeader(4, $"{Util.GetFieldSignature(fields[i], false)} field");
				PrintObsoleteWarning(fields[i], writer);
				Summary(fieldDocs, writer);

				writer.WriteCodeBlock(Lang, Util.GetFieldSignature(fields[i], true));

				Remarks(5, fieldDocs, writer);
			}
		}

		private static void PrintObsoleteWarning(MemberInfo member, MarkdownWriter writer)
		{
			var obsAttr = member.GetCustomAttribute<ObsoleteAttribute>();
			if (obsAttr == null) return;
			writer.WriteInfoBox($"**This item is deprecated.**\n{obsAttr.Message}", "warning");
		}

		private static void WriteParamList(int rank, MethodBase method, MarkdownWriter writer, MemberDocs docs)
		{
			var plist = method.GetParameters();
			

			if (method.ContainsGenericParameters)
			{
				var tplist = method.GetGenericArguments();
				writer.WriteParagraph("**Type Parameters**");
				for (int i = 0; i < tplist.Length; i++)
				{
					writer.WriteLine($"- `{tplist[i].Name}`: {TypeParam(docs, tplist[i].Name)}");
				}
			}

			if (plist.Length > 0)
			{
				writer.WriteParagraph("**Parameters**");
				for (int i = 0; i < plist.Length; i++)
				{
					writer.WriteLine($"- `{plist[i].Name}`: {Param(docs, plist[i])}");
				}
			}
		}

		private static void WriteConstructors(Type type, MarkdownWriter writer, Dictionary<string, MemberDocs> docs)
		{
			// Constructor list
			var ctors = type.GetConstructors();
			if (ctors.Length > 0)
			{
				writer.WriteHeader(3, "Constructors");

				for (int i = 0; i < ctors.Length; i++)
				{
					// Heading for constructor section
					docs.TryGetValue(ID.GetIDString(ctors[i]), out MemberDocs ctorDocs);
					writer.WriteHeader(4, Util.GetMethodSignature(ctors[i], false, false));
					PrintObsoleteWarning(ctors[i], writer);
					Summary(ctorDocs, writer);

					// Signature
					writer.WriteCodeBlock(Lang, Util.GetMethodSignature(ctors[i], true, true));

					// Get constructor's parameters and associated docs
					WriteParamList(5, ctors[i], writer, ctorDocs);
					Remarks(5, ctorDocs, writer);
				}
			}
		}

		private static void Summary(MemberDocs docs, MarkdownWriter writer) => writer.WriteParagraph(docs?.Summary ?? "_No Summary_");

		private static void Remarks(int rank, MemberDocs docs, MarkdownWriter writer)
		{
			var remarks = docs?.Remarks;
			if (!String.IsNullOrWhiteSpace(remarks))
			{
				writer.WriteHeader(rank, "Remarks");
				writer.WriteParagraph(remarks);
			}
		}

		private static void Returns(int rank, MemberDocs docs, MarkdownWriter writer)
		{
			var returns = docs?.Returns;
			if (!String.IsNullOrWhiteSpace(returns))
			{
				writer.WriteHeader(rank, "Returns");
				writer.WriteParagraph(returns);
			}
		}

		private static string Param(MemberDocs docs, ParameterInfo param) => docs?.GetParameterDescription(param.Name) ?? "_No Description_";

		private static string TypeParam(MemberDocs docs, string name) => docs?.GetTypeParameterDescription(name) ?? "_No Description_";
	}
}