using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GetStoreResponse", Namespace = "", IsNullable = true)]
public class GetStoreResponse
{
	[XmlElement(ElementName = "Stores")]
	public ItemsInStoreData[] Stores;
}
