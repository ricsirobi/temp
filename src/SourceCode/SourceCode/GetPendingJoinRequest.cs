using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GetPendingJoinRequest", IsNullable = true)]
public class GetPendingJoinRequest
{
	[XmlElement(ElementName = "UserID")]
	public string UserID;

	[XmlElement(ElementName = "GroupID")]
	public string GroupID;

	[XmlElement(ElementName = "ProductGroupID")]
	public int? ProductGroupID;
}
