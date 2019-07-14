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
using System.Xml;

namespace Docpal
{
    class DocpalSlim : DocpalGenerator
    {
        private const BindingFlags MemberSearchFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const string Lang = "csharp";

        public DocpalSlim(ProjectXmlDocs docs, Assembly asm) : base(docs, asm)
        {
        }

        public override void BuildDocs(string outputPath)
        {
            outputPath = DocUtilities.ChangeExtension(outputPath, "md");

            // Write markdown file
            using (var writer = new MarkdownWriter(outputPath))
            {
                foreach (var type in Library.ExportedTypes.Where(t => !t.Name.StartsWith("_")).OrderBy(t => t.Name))
                {
                    // Pull XML docs for type
                    var typeDocs = Docs[ID.GetIDString(type)];

                    // Header, e.g. "StringBuilder class (System)"
                    writer.WriteHeader(2, DocUtilities.GetTypeTitle(type, true), true);

                    // Summary and other info
                    PrintObsoleteWarning(type, writer);
                    writer.WriteParagraph($"**Namespace:** {type.Namespace}");
                    writer.WriteParagraph($"**Inheritance:** {DocUtilities.GetInheritanceString(type)}", true);
                    Summary(typeDocs, writer);

                    // Signature
                    writer.WriteCodeBlock(Lang, DocUtilities.GetClassSignature(type));

                    // Members
                    WriteConstructors(type, writer);
                    WriteProperties(type, writer);
                    WriteIndexers(type, writer);
                    WriteFields(type, writer);
                    WriteMethods(type, writer);
                }
            }
        }

        private void WriteMethods(Type type, MarkdownWriter writer)
        {
            var methods = type.GetMethods(MemberSearchFlags)
                // Exclude compiler-generated and internal methods
                .Where(m => !m.IsSpecialName && !m.IsAssembly)
                // Exclude protected methods when class is sealed
                .Where(m => type.IsSealed
                    ? m.IsPublic
                    : !m.IsPrivate)
                // Sort alphabetically...
                .OrderBy(m => m.Name)
                // ... then by parameter count.
                .ThenBy(m => m.GetParameters().Length)
                .ToArray();

            if (methods.Length > 0)
            {
                writer.WriteHeader(3, "Methods");
                foreach (var method in methods)
                {
                    var methodDocs = Docs[ID.GetIDString(method)];

                    // Heading
                    writer.WriteHeader(4, DocUtilities.GetMethodSignature(method, false, false), true);
                    PrintObsoleteWarning(method, writer);
                    Summary(methodDocs, writer);
                    writer.WriteCodeBlock(Lang, DocUtilities.GetMethodSignature(method, true, true));

                    // Parameters
                    WriteParamList(4, method, writer, methodDocs);
                    Returns(4, methodDocs, writer);
                    Remarks(4, methodDocs, writer);
                }
            }
        }

        private void WriteProperties(Type type, MarkdownWriter writer)
        {
            var props = type.GetProperties(MemberSearchFlags)
                // Show protected members if class is not sealed
                .Where(p => type.IsSealed
                    ? (p.CanRead && p.GetMethod.IsPublic) || (p.CanWrite && p.SetMethod.IsPublic)
                    : (p.CanRead && !p.GetMethod.IsPrivate) || (p.CanWrite && !p.SetMethod.IsPrivate))
                // Indexers are technically properties, but we want to handle them separately
                .Where(p => p.GetIndexParameters().Length == 0)
                // Sort alphabetically
                .OrderBy(p => p.Name)
                .ToArray();

            if (props.Length == 0) return;

            writer.WriteHeader(3, "Properties");

            for (int i = 0; i < props.Length; i++)
            {
                var propDocs = Docs[ID.GetIDString(props[i])];
                writer.WriteHeader(4, DocUtilities.GetPropertySignature(props[i], false, false, false));
                PrintObsoleteWarning(props[i], writer);
                Summary(propDocs, writer);

                writer.WriteCodeBlock(Lang, DocUtilities.GetPropertySignature(props[i], true, true, true));

                Remarks(5, propDocs, writer);
            }
        }

        private void WriteIndexers(Type type, MarkdownWriter writer)
        {
            var indexers = type.GetProperties()
                .Where(p => p.GetIndexParameters().Length > 0)
                .OrderBy(p => p.GetIndexParameters().Length)
                .ToArray();

            if (indexers.Length == 0) return;

            writer.WriteHeader(3, "Indexers");

            for (int i = 0; i < indexers.Length; i++)
            {
                var indexDocs = Docs[ID.GetIDString(indexers[i])];
                writer.WriteHeader(4, DocUtilities.GetPropertySignature(indexers[i], false, true, false));
                PrintObsoleteWarning(indexers[i], writer);
                Summary(indexDocs, writer);

                writer.WriteCodeBlock(Lang, DocUtilities.GetPropertySignature(indexers[i], true, true, true));

                Remarks(5, indexDocs, writer);
            }
        }

        private void WriteFields(Type type, MarkdownWriter writer)
        {
            var fields = type.GetFields()
                .Where(f => !f.IsSpecialName)
                .OrderBy(f => f.Name)
                .ToArray();

            if (fields.Length == 0) return;

            writer.WriteHeader(3, "Fields");

            for (int i = 0; i < fields.Length; i++)
            {
                var fieldDocs = Docs[ID.GetIDString(fields[i])];
                writer.WriteHeader(4, DocUtilities.GetFieldSignature(fields[i], false));
                PrintObsoleteWarning(fields[i], writer);
                Summary(fieldDocs, writer);

                writer.WriteCodeBlock(Lang, DocUtilities.GetFieldSignature(fields[i], true));

                Remarks(5, fieldDocs, writer);
            }
        }

        private void PrintObsoleteWarning(MemberInfo member, MarkdownWriter writer)
        {
            var obsAttr = member.GetCustomAttribute<ObsoleteAttribute>();
            if (obsAttr == null) return;
            writer.WriteInfoBox($"**This item is deprecated.**\n{obsAttr.Message}", "warning");
        }

        private void WriteParamList(int rank, MethodBase method, MarkdownWriter writer, MemberXmlDocs docs)
        {
            var plist = method.GetParameters();


            if (method.ContainsGenericParameters)
            {                
                if (method.IsGenericMethod)
                {
                    var tplist = method.GetGenericArguments();
                    writer.WriteParagraph("**Type Parameters**");
                    for (int i = 0; i < tplist.Length; i++)
                    {
                        writer.WriteLine($"- `{tplist[i].Name}`: {TypeParam(docs, tplist[i].Name)}");
                    }
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

        private void WriteConstructors(Type type, MarkdownWriter writer)
        {
            // Constructor list
            var ctors = type.GetConstructors();
            if (ctors.Length > 0)
            {
                writer.WriteHeader(3, "Constructors");

                for (int i = 0; i < ctors.Length; i++)
                {
                    // Heading for constructor section
                    var ctorDocs = Docs[ID.GetIDString(ctors[i])];
                    writer.WriteHeader(4, DocUtilities.GetMethodSignature(ctors[i], false, false));
                    PrintObsoleteWarning(ctors[i], writer);
                    Summary(ctorDocs, writer);

                    // Signature
                    writer.WriteCodeBlock(Lang, DocUtilities.GetMethodSignature(ctors[i], true, true));

                    // Get constructor's parameters and associated docs
                    WriteParamList(5, ctors[i], writer, ctorDocs);
                    Remarks(5, ctorDocs, writer);
                }
            }
        }

        private static void Summary(MemberXmlDocs docs, MarkdownWriter writer) => writer.WriteParagraph(docs?.Summary ?? "_No Summary_");

        private static void Remarks(int rank, MemberXmlDocs docs, MarkdownWriter writer)
        {
            var remarks = docs?.Remarks;
            if (!String.IsNullOrWhiteSpace(remarks))
            {
                writer.WriteHeader(rank, "Remarks");
                writer.WriteParagraph(remarks);
            }
        }

        private static void Returns(int rank, MemberXmlDocs docs, MarkdownWriter writer)
        {
            var returns = docs?.Returns;
            if (!String.IsNullOrWhiteSpace(returns))
            {
                writer.WriteHeader(rank, "Returns");
                writer.WriteParagraph(returns);
            }
        }

        private static string Param(MemberXmlDocs docs, ParameterInfo param) => docs?.GetParameterDescription(param.Name) ?? "_No Description_";

        private static string TypeParam(MemberXmlDocs docs, string name) => docs?.GetTypeParameterDescription(name) ?? "_No Description_";
    }
}