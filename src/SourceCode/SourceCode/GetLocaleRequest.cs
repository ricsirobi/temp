using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GetLocaleRequest", IsNullable = true)]
public class GetLocaleRequest
{
	[XmlElement(ElementName = "Locale")]
	public string Locale;

	[XmlElement(ElementName = "UserID")]
	public Guid UserID;

	[XmlElement(ElementName = "ProductID")]
	public int ProductID;

	[XmlElement(ElementName = "ProductGroupID")]
	public int ProductGroupID;
}
