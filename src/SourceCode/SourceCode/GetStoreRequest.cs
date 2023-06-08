using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GetStoreRequest", Namespace = "")]
public class GetStoreRequest
{
	[XmlElement(ElementName = "StoreIDs")]
	public int[] StoreIDs;

	[XmlElement(ElementName = "PIF")]
	public bool GetPopularItems = true;
}
