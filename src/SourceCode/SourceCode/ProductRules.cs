using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "productRules", Namespace = "")]
public class ProductRules
{
	[XmlElement(ElementName = "sslRules")]
	public List<Rule> SslRules { get; set; }

	[XmlElement(ElementName = "responseHashValidationRules")]
	public List<Rule> ResponseHashValidationRules { get; set; }
}
