using System;
using System.Xml.Serialization;

[Serializable]
public class AdAttributes
{
	[XmlElement(ElementName = "URL")]
	public string URL;

	[XmlElement(ElementName = "Member", IsNullable = true)]
	public MembershipType? MemberType;

	[XmlElement(ElementName = "DaysOlder", IsNullable = true)]
	public int? DaysOlder;

	[XmlElement(ElementName = "VideoURL", IsNullable = true)]
	public string VideoURL;
}
