using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "BPFIM", Namespace = "")]
public class BluePrintFuseItemMap
{
	[XmlElement(ElementName = "BPSID", IsNullable = false)]
	public int BluePrintSpecID { get; set; }

	[XmlElement(ElementName = "UID", IsNullable = false)]
	public int UserInventoryID { get; set; }
}
