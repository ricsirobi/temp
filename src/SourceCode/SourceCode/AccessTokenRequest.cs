using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AccessTokenRequest", Namespace = "", IsNullable = true)]
public class AccessTokenRequest
{
	[XmlElement(ElementName = "purchase")]
	public PurchaseInfo PurchaseInfo;

	[XmlElement(ElementName = "custom_parameters")]
	public CustomParameters CustomParameters;
}
