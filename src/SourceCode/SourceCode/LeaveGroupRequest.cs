using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "LeaveGroupRequest", IsNullable = true)]
public class LeaveGroupRequest
{
	[XmlElement(ElementName = "UserID")]
	public string UserID;

	[XmlElement(ElementName = "GroupID")]
	public string GroupID;

	[XmlElement(ElementName = "ProductGroupID")]
	public int? ProductGroupID;

	[XmlElement(ElementName = "ProductID")]
	public int? ProductID;
}
