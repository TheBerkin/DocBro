using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Docpal
{
	class ProjectXmlDocs
	{
		private readonly Dictionary<string, MemberXmlDocs> _docs;

		public ProjectXmlDocs(XmlDocument xml)
		{
			_docs = new Dictionary<string, MemberXmlDocs>();
			Xml = xml;
			foreach (XmlNode item in xml.SelectNodes("//doc/members/member"))
			{
				var id = item.Attributes["name"].Value;
				_docs[id] = new MemberXmlDocs(item);
			}
		}

		public XmlDocument Xml { get; }

		public MemberXmlDocs this[string memberId] => _docs.TryGetValue(memberId, out MemberXmlDocs docs) ? docs : null;
	}
}
