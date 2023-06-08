using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "BuddyActionResult", Namespace = "")]
public class BuddyActionResult
{
	[XmlElement(ElementName = "Result")]
	public BuddyActionResultType Result;

	[XmlElement(ElementName = "Status")]
	public BuddyStatus Status;

	[XmlElement(ElementName = "BuddyUserID")]
	public string BuddyUserID;
}
