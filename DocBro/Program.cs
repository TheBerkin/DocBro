using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DocBro
{
	class Program
	{
		static void Main(string[] args)
		{
			var rootDirectory = "docs";
			var dllName = args[0] + ".dll";
			var xmlName = args[0] + ".xml";
			var outputPath = Args.Property("out");
			var docs = new Node("Main");
			var pageNodes = new List<Node>();
			var ass = Assembly.LoadFrom(dllName);
			var meta = new Dictionary<string, MemberData>();
			var xml = new XmlDocument();
			xml.Load(xmlName);
			foreach (XmlNode item in xml.SelectNodes("//doc/members/member"))
			{
				var id = item.Attributes["name"].Value;
				var data = meta[id] = new MemberData
				{
					Summary = item.SelectSingleNode("summary")?.InnerText.Trim() ?? "(No Description)",
					Returns = item.SelectSingleNode("returns")?.InnerText.Trim() ?? String.Empty
				};

				foreach(XmlNode desc in item.SelectNodes("param"))
				{
					data.SetParameterDescription(desc.Attributes["name"].Value, desc.InnerText.Trim());
				}
			}

			foreach (var type in ass.GetExportedTypes())
			{
				var typePath = $"{type.Namespace.Replace('.', '/')}/{Util.GetURLTitle(type)}";
				//Console.WriteLine($"Type: {typePath}");
				//Console.WriteLine($"Signature: {Util.GetClassSignature(type)}");
				if (meta.TryGetValue(ID.GetIDString(type), out MemberData typeData))
				{
					//Console.WriteLine($"    Summary: {typeData.Summary}");
				}

				docs[typePath] = new TypePage(type, typeData);
				pageNodes.Add(docs.GetNode(typePath));

				var ctors = type.GetConstructors();
				//Console.WriteLine();
				if (ctors.Length > 0)
				{
					//Console.WriteLine("Constructors:");
					int ctorNum = 1;
					foreach (var ctor in ctors)
					{
						var ctorPath = $"{typePath}/new/{ctorNum}";
						//Console.WriteLine($"    {Util.GetMethodSignature(ctor)}");
						if (meta.TryGetValue(ID.GetIDString(ctor), out MemberData methodData))
						{
							//Console.WriteLine($"        Summary: {methodData.Summary}");
						}
						ctorNum++;
					}
					//Console.WriteLine();
				}
				
				// Method groups
				foreach (var methodGroup in type.GetMethods()
					.Where(m => !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_"))
					.GroupBy(m => m.Name))
				{
					var methodGroupPath = $"{typePath}/{methodGroup.Key}";
					docs[methodGroupPath] = new MethodGroupPage(type, methodGroup);
					pageNodes.Add(docs.GetNode(methodGroupPath));

					int methodNum = 1;
					//Console.WriteLine($"Method: {typePath}/{methodGroup.Key}");
					foreach (var method in methodGroup)
					{
						if (meta.TryGetValue(ID.GetIDString(method), out MemberData methodData))
						{
							//Console.WriteLine($"        Summary: {methodData.Summary}");
							foreach (var p in method.GetParameters())
							{
								//Console.WriteLine($"        '{p.Name}': {methodData.GetParameterDescription(p.Name)}");
							}
							foreach (var tp in method.GetGenericArguments().Select(t => t.Name))
							{
								//Console.WriteLine($"        '{tp}': {methodData.GetTypeParameterDescription(tp)}");
							}
						}

						var methodPath = $"{methodGroupPath}/{methodNum}";
						//Console.WriteLine(methodPath);
						docs[methodPath] = new MethodPage(method, methodData);
						pageNodes.Add(docs.GetNode(methodPath));

						//Console.WriteLine($"    {Util.GetMethodSignature(method)}");
						
						methodNum++;
					}
				}
				//Console.WriteLine();
				foreach (var field in type.GetFields().Where(f => (f.IsPublic || !f.IsPrivate) && (!f.DeclaringType.IsEnum || !f.IsSpecialName)))
				{
					var fieldPath = Path.Combine(typePath, field.Name).Replace('\\', '/');
					//Console.WriteLine($"Field: {fieldPath}");
					if (meta.TryGetValue(ID.GetIDString(field), out MemberData fieldData))
					{
						//Console.WriteLine($"    Summary: {fieldData.Summary}");
					}
				}
				//Console.WriteLine();

				int numIndexers = 0;
				foreach (var property in type.GetProperties())
				{
					//Console.WriteLine($"Property: {propPath}");
					if (meta.TryGetValue(ID.GetIDString(property), out MemberData propData))
					{
						//Console.WriteLine($"    Summary: {propData.Summary}");
					}

					string propPath;
					if (property.GetIndexParameters().Length > 0)
					{
						propPath = $"{typePath}/this/{++numIndexers}";
					}
					else
					{
						propPath = $"{typePath}/{property.Name}";
					}

					docs[propPath] = new PropertyPage(property, propData);
					pageNodes.Add(docs.GetNode(propPath));



				}
				//Console.WriteLine();
			}

			foreach (var node in pageNodes)
			{
				var documentDir = Directory.GetParent($"{rootDirectory}/{node.Path}").FullName;
				var documentPath = $"{rootDirectory}/{node.Path}.md";
				Console.WriteLine(documentPath);
				Directory.CreateDirectory(documentDir);
				using (var writer = new MarkdownWriter(documentPath))
				{
					node.Page.Render(node, writer);
				}
			}
		}
	}
}
