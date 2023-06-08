using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "LoginContent", Namespace = "")]
public class LoginContent
{
	[XmlElement(ElementName = "AdSection")]
	public AdSection[] AdSections;

	[XmlElement(ElementName = "ReleaseNotes")]
	public ReleaseNotes ReleaseNotes;

	[XmlElement(ElementName = "ForumURL")]
	public string ForumURL;
}
