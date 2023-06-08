using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "PRIREQ", IsNullable = true)]
public class ProcessRewardedItemsRequest
{
	[XmlElement(ElementName = "IATM", IsNullable = false)]
	public ItemActionTypeMap[] ItemsActionMap { get; set; }
}
