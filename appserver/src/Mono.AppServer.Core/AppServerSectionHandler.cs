using System;
using System.Configuration;
using System.Xml;
using System.Collections;

namespace Mono.AppServer
{
	public class AppServerSectionHandler : IConfigurationSectionHandler
	{
		public AppServerSectionHandler():base()
		{
		}

		public object Create(
			object parent,
			object configContext,
			XmlNode section
			)
		{
			ArrayList list=new ArrayList();
			foreach (XmlElement elem in section.SelectNodes("ApplicationTypes/ApplicationType"))
			{
				list.Add(new ApplicationType(
					elem.Attributes["Name"].Value,
					elem.Attributes["Assembly"].Value,
					elem.Attributes["Type"].Value));
			}
			return (ApplicationType[]) list.ToArray(typeof(ApplicationType));
		}
	}
}
