using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfItemExchange", Namespace = "")]
public class ArrayOfItemExchange
{
	[XmlElement(ElementName = "ItemExchange")]
	public ItemExchange[] ItemExchangeArray { get; set; }
}
