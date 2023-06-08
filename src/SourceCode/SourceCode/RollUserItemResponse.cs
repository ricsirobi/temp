using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RUIRES", Namespace = "")]
public class RollUserItemResponse
{
	[XmlElement(ElementName = "ST", IsNullable = false)]
	public Status Status { get; set; }

	[XmlElement(ElementName = "IS", IsNullable = false)]
	public ItemStat[] ItemStats { get; set; }
}
