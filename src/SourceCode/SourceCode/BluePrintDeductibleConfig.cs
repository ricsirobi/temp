using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "BPDC", Namespace = "", IsNullable = true)]
public class BluePrintDeductibleConfig
{
	[XmlElement(ElementName = "BPIID", IsNullable = false)]
	public int BluePrintItemID { get; set; }

	[XmlElement(ElementName = "DT", IsNullable = false)]
	public DeductibleType DeductibleType { get; set; }

	[XmlElement(ElementName = "IID", IsNullable = true)]
	public int? ItemID { get; set; }

	[XmlElement(ElementName = "QTY", IsNullable = false)]
	public int Quantity { get; set; }
}
