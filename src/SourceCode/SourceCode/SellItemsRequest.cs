using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "SIR", Namespace = "")]
public class SellItemsRequest
{
	[XmlElement(ElementName = "CID", IsNullable = false)]
	public int ContainerID { get; set; }

	[XmlElement(ElementName = "UICDS", IsNullable = false)]
	public int[] UserInventoryCommonIDs { get; set; }
}
