using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Buddy", Namespace = "")]
public class Buddy
{
	[XmlElement(ElementName = "UserID")]
	public string UserID;

	[XmlElement(ElementName = "DisplayName")]
	public string DisplayName;

	[XmlElement(ElementName = "Status")]
	public BuddyStatus Status;

	[XmlElement(ElementName = "CreateDate")]
	public DateTime CreateDate;

	[XmlElement(ElementName = "Online")]
	public bool Online;

	[XmlElement(ElementName = "OnMobile")]
	public bool OnMobile;

	[XmlElement(ElementName = "BestBuddy")]
	public bool BestBuddy;
}
