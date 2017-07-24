using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Docpal
{
	class Program
	{
		static void Main(string[] args)
		{
			var paths = Args.GetPaths();
			if (paths.Length == 0)
			{
				Console.WriteLine("No path specified.");
				return;
			}

			var dllPath = paths[0];
			var xmlPath = Args.Property("xml", DocUtilities.ChangeExtension(dllPath, "xml"));
			var outputPath = Args.Property("out", "docs");
			bool ignoreXML = Args.Flag("noxml");

			if (!File.Exists(dllPath))
			{
				Console.WriteLine($"File '{dllPath}' does not exist.");
				return;
			}

			try
			{
				Console.WriteLine("Building...");
				var dll = Assembly.LoadFile(dllPath);
				XmlDocument xml = null;
				if (File.Exists(xmlPath) && !ignoreXML)
				{
					xml = new XmlDocument();
					xml.Load(xmlPath);
					Console.WriteLine($"Found XML: {xmlPath}");
				}
				else
				{
					Console.WriteLine("No XML docs found, using only assembly...");
				}

				if (Args.Flag("slim"))
				{
					DocpalSlim.BuildDocs(xml, dll, DocUtilities.ChangeExtension(outputPath, "md"));
				}
				else
				{
					DocpalPages.BuildDocs(xml, dll, outputPath);
				}

				Console.WriteLine("Done, enjoy!");

			}
			catch (Exception e)
			{
				Console.WriteLine($"Unfortunately, there was an error.\n\n{e}");
			}
		}
	}
}
