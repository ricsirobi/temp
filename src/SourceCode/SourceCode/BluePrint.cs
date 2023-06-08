using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "BP", Namespace = "", IsNullable = true)]
public class BluePrint
{
	[XmlElement(ElementName = "BPDC", IsNullable = true)]
	public List<BluePrintDeductibleConfig> Deductibles { get; set; }

	[XmlElement(ElementName = "ING", IsNullable = false)]
	public List<BluePrintSpecification> Ingredients { get; set; }

	[XmlElement(ElementName = "OUT", IsNullable = false)]
	public List<BluePrintSpecification> Outputs { get; set; }
}
