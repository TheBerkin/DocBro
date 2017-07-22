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
			var xmlPath = DocUtilities.ChangeExtension(dllPath, "xml");
			var outputPath = Args.Property("out", "docs");

			if (!File.Exists(dllPath))
			{
				Console.WriteLine($"File '{dllPath}' does not exist.");
				return;
			}

			try
			{
				Console.WriteLine("Building...");
				var dll = Assembly.LoadFile(dllPath);
				var xml = new XmlDocument();
				if (File.Exists(xmlPath))
				{
					xml.Load(xmlPath);
				}
				else
				{
					xml = null;
					Console.WriteLine("No matching XML file found. Using only reflected types...");
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
				Console.WriteLine(e);
			}
		}
	}
}
