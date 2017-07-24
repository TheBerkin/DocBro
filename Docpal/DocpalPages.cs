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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace Docpal
{
	static class DocpalPages
	{
		public static void BuildDocs(ProjectXmlDocs docs, Assembly dll, string outputDir)
		{
			var pages = new Node("docs");
			var pageNodes = new List<Node>();

			foreach (var type in dll.GetExportedTypes())
			{
				var typePath = $"{type.Namespace.Replace('.', '/')}/{DocUtilities.GetURLTitle(type)}";
				var typeData = docs.GetDocs(ID.GetIDString(type));

				pages[typePath] = new TypePage(type, typeData);
				pageNodes.Add(pages.GetNode(typePath));

				// Constructors
				var ctors = type.GetConstructors();
				if (ctors.Length > 0)
				{
					int ctorNum = 1;
					foreach (var ctor in ctors)
					{
						var ctorPath = $"{typePath}/new/{ctorNum}";
						var methodData = docs.GetDocs(ID.GetIDString(ctor));
						// TODO: Generate constructor pages
						ctorNum++;
					}
				}

				// Method groups
				foreach (var methodGroup in type.GetMethods()
					.Where(m => !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_"))
					.GroupBy(m => m.Name))
				{
					// Path to method group
					var methodGroupPath = $"{typePath}/{methodGroup.Key}";

					// Map of reflected methods and documentation
					var methods = new Dictionary<MethodInfo, MemberXmlDocs>();

					foreach (var method in methodGroup)
					{
						var methodData = docs.GetDocs(ID.GetIDString(method));
						methods[method] = methodData;
					}

					pages[methodGroupPath] = new MethodGroupPage(type, methodGroup.Key, methods);
					pageNodes.Add(pages.GetNode(methodGroupPath));
				}

				// Fields
				foreach (var field in type.GetFields().Where(f => (f.IsPublic || !f.IsPrivate) && (!f.DeclaringType.IsEnum || !f.IsSpecialName)))
				{
					var fieldPath = Path.Combine(typePath, field.Name).Replace('\\', '/');
					var fieldData = docs.GetDocs(ID.GetIDString(field));
					pages[fieldPath] = new FieldPage(field, fieldData);
					pageNodes.Add(pages.GetNode(fieldPath));
				}

				// Properties and Indexers
				int numIndexers = 0;
				foreach (var property in type.GetProperties())
				{
					var propData = docs.GetDocs(ID.GetIDString(property));

					string propPath;
					if (property.GetIndexParameters().Length > 0)
					{
						propPath = $"{typePath}/this/{++numIndexers}";
					}
					else
					{
						propPath = $"{typePath}/{property.Name}";
					}

					pages[propPath] = new PropertyPage(property, propData);
					pageNodes.Add(pages.GetNode(propPath));
				}
			}

			var exportTasks = new Task[pageNodes.Count];
			for (int i = 0; i < pageNodes.Count; i++)
			{
				var node = pageNodes[i];
				exportTasks[i] = Task.Run(() =>
				{
					var documentDir = Directory.GetParent($"{outputDir}/{node.Path}").FullName;
					var documentPath = $"{outputDir}/{node.Path}.md";
					Directory.CreateDirectory(documentDir);
					using (var writer = new MarkdownWriter(documentPath))
					{
						node.Page.Render(node, writer);
					}
				});
			}
			Task.WaitAll(exportTasks);
		}
	}
}