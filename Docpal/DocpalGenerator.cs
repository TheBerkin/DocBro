using System.Reflection;
using System.Xml;

namespace Docpal
{
	abstract class DocpalGenerator
	{
		public DocpalGenerator(ProjectXmlDocs docs, Assembly asm)
		{
			Docs = docs;
			Library = asm;
		}

		public ProjectXmlDocs Docs { get; }
		public Assembly Library { get; }

		protected abstract string GetMarkdownString(XmlNode textNode);

		public abstract void BuildDocs(string outputPath);
	}
}
