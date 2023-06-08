using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GetItemRequest", Namespace = "")]
public class GetItemRequest
{
	[XmlElement(ElementName = "ProductGroupID")]
	public int ProductGroupID;

	[XmlElement(ElementName = "ItemIDs")]
	public List<int> ItemIDs;
}
