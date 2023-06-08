using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "USA", Namespace = "")]
public class UserStaffAttribute
{
	[XmlElement(ElementName = "id", IsNullable = true)]
	public int? UserStaffAttributeID;

	[XmlElement(ElementName = "usid", IsNullable = true)]
	public int? UserStaffID;

	[XmlElement(ElementName = "k")]
	public string AttributeKey;

	[XmlElement(ElementName = "v")]
	public string AttributeValue;
}
