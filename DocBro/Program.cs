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

			var rootNode = new Node("Main");
			var ass = Assembly.ReflectionOnlyLoadFrom(dllName);

			var xml = new XmlDocument();
			xml.Load(xmlName);
			foreach (XmlNode item in xml.SelectNodes("//doc/members/member"))
			{
				var nameParts = item.Attributes["name"].Value.Split(':');
				if (nameParts.Length != 2) continue;
				if (String.IsNullOrWhiteSpace(nameParts[0])) continue;
				char memberId = nameParts[0][0];
				
			}

			foreach (var type in ass.GetExportedTypes())
			{
				var typePath = Path.Combine(rootDirectory, type.Namespace.Replace('.', '/'), type.Name).Replace('\\', '/');
				rootNode[typePath] = new TypePage(type);

				foreach (var method in type.GetMethods())
				{
					string methodPath;
					if (method.IsConstructor)
					{
						methodPath = $"{typePath}/new";
					}
					else
					{
						// Generate unique pages for method overloads
					}
				}

				foreach (var field in type.GetFields())
				{
					Directory.CreateDirectory(Path.Combine(typePath, field.Name));
				}

				foreach (var property in type.GetProperties())
				{
					Directory.CreateDirectory(Path.Combine(typePath, property.Name));
				}
			}


		}
	}
}
