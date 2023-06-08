using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "getProductRulesResponse", Namespace = "")]
public class GetProductRulesResponse
{
	[XmlElement(ElementName = "globalKey")]
	public string GlobalSecretKey { get; set; }

	[XmlElement(ElementName = "productRules")]
	public ProductRules Rules { get; set; }
}
