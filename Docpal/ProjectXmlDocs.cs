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
		private readonly XmlDocument _xml;

		public ProjectXmlDocs(XmlDocument xml)
		{
			_docs = new Dictionary<string, MemberXmlDocs>();
			_xml = xml;
			foreach (XmlNode item in xml.SelectNodes("//doc/members/member"))
			{
				var id = item.Attributes["name"].Value;
				var data = _docs[id] = new MemberXmlDocs
				{
					Summary = item.SelectSingleNode("summary")?.InnerText.Trim() ?? "(No Description)",
					Returns = item.SelectSingleNode("returns")?.InnerText.Trim() ?? String.Empty,
					Remarks = item.SelectSingleNode("remarks")?.InnerText.Trim() ?? String.Empty
				};

				foreach (XmlNode desc in item.SelectNodes("param"))
				{
					data.SetParameterDescription(desc.Attributes["name"].Value, desc.InnerText.Trim());
				}

				foreach (XmlNode desc in item.SelectNodes("typeparam"))
				{
					data.SetTypeParameterDescription(desc.Attributes["name"].Value, desc.InnerText.Trim());
				}
			}
		}

		public XmlDocument Xml => _xml;
		
		public MemberXmlDocs this[string memberId] => _docs.TryGetValue(memberId, out MemberXmlDocs docs) ? docs : null;
	}
}
