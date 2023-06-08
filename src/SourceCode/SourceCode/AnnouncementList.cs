using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Announcements", Namespace = "")]
public class AnnouncementList
{
	[XmlElement(ElementName = "Announcement")]
	public Announcement[] Announcements;
}
