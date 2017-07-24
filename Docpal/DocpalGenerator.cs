using System.Reflection;

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

		public abstract void BuildDocs(string outputPath);
	}
}
