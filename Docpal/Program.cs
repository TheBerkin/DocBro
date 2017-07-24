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

			// Check whether the file even exists
			if (!File.Exists(dllPath))
			{
				Console.WriteLine($"File '{dllPath}' does not exist.");
				return;
			}

			try
			{
				Console.WriteLine("Building docs...");

				var dll = Assembly.LoadFile(dllPath);
				DocpalGenerator docpal;
				XmlDocument xmlData = null;

				// Load XML document
				if (File.Exists(xmlPath) && !ignoreXML)
				{
					xmlData = new XmlDocument();
					xmlData.Load(xmlPath);
					Console.WriteLine($"Found XML: {xmlPath}");
				}
				else
				{
					Console.WriteLine("No XML docs found, using only assembly...");
				}

				var xmlDocs = new ProjectXmlDocs(xmlData);				

				// Determine type of docs generator to use
				if (Args.Flag("slim"))
				{
					docpal = new DocpalSlim(xmlDocs, dll);
				}
				else
				{
					docpal = new DocpalPages(xmlDocs, dll);
				}
				
				// Run build
				docpal.BuildDocs(outputPath);

				Console.WriteLine("Done, enjoy!");

			}
			catch (Exception e)
			{
				Console.WriteLine($"Unfortunately, there was an error.\n\n{e}");
			}
		}
	}
}
