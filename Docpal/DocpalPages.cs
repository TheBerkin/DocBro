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

using Docpal.Pages;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Docpal
{
    class DocpalPages : DocpalGenerator
    {
        public DocpalPages(ProjectXmlDocs docs, Assembly asm) : base(docs, asm)
        {
        }

        public override void BuildDocs(string outputPath)
        {
            var pages = new PageTree("docs");
            var PageTrees = new List<PageTree>();

            foreach (var type in Library.GetExportedTypes())
            {
                var typePath = $"{DocUtilities.GetURLTitle(type)}";
                if (type.Namespace != null)
                    typePath = $"{type.Namespace.Replace('.', '/')}/{typePath}";
                var typeData = Docs[ID.GetIDString(type)];

                pages[typePath] = new TypePage(type, typeData, Docs);
                PageTrees.Add(pages.GetNode(typePath));

                // Constructors
                var ctors = type.GetConstructors();
                if (ctors.Length > 0)
                {
                    // Path to ctors group
                    var ctorsGroupPath = $"{typePath}/ctors";

                    var ctorsData = new Dictionary<ConstructorInfo, MemberXmlDocs>();
                    foreach (var ctor in ctors)
                    {
                        var ctorData = Docs[ID.GetIDString(ctor)];
                        ctorsData.Add(ctor, ctorData);
                    }

                    pages[ctorsGroupPath] = new ConstructorsPage(type, ctors, ctorsData);
                    PageTrees.Add(pages.GetNode(ctorsGroupPath));
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
                        var methodData = Docs[ID.GetIDString(method)];
                        methods[method] = methodData;
                    }

                    pages[methodGroupPath] = new MethodGroupPage(type, methodGroup.Key, methods);
                    PageTrees.Add(pages.GetNode(methodGroupPath));
                }

                // Fields
                foreach (var field in type.GetFields().Where(f => (f.IsPublic || !f.IsPrivate) && (!f.DeclaringType.IsEnum || !f.IsSpecialName)))
                {
                    var fieldPath = Path.Combine(typePath, field.Name).Replace('\\', '/');
                    var fieldData = Docs[ID.GetIDString(field)];
                    pages[fieldPath] = new FieldPage(field, fieldData);
                    PageTrees.Add(pages.GetNode(fieldPath));
                }

                // Properties and Indexers
                int numIndexers = 0;
                foreach (var property in type.GetProperties())
                {
                    var propData = Docs[ID.GetIDString(property)];

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
                    PageTrees.Add(pages.GetNode(propPath));
                }
            }

            // Create a task for each document that needs to be exported, run them all at once
            var exportTasks = new Task[PageTrees.Count];
            for (int i = 0; i < PageTrees.Count; i++)
            {
                var node = PageTrees[i];
                exportTasks[i] = Task.Run(() =>
                {
                    var documentDir = Directory.GetParent($"{outputPath}/{node.Path}").FullName;
                    var documentPath = $"{outputPath}/{node.Path}.md";
                    Directory.CreateDirectory(documentDir);
                    using (var writer = new MarkdownWriter(documentPath))
                    {
                        node.Page.Render(node, writer);
                    }
                });
            }

            // Wait for all export tasks to finish
            Task.WaitAll(exportTasks);
        }
    }
}