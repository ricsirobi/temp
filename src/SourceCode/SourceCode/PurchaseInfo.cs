using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "PurchaseInfo", Namespace = "", IsNullable = true)]
public class PurchaseInfo
{
	[XmlElement(ElementName = "description")]
	public Description Description;
}
