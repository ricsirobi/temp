using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "URGR", Namespace = "")]
public class UserRoomGetRequest
{
	[XmlElement(ElementName = "UID")]
	public Guid? UserID { get; set; }

	[XmlElement(ElementName = "CID")]
	public int? CategoryID { get; set; }
}
