using System;
using System.Xml.Serialization;

[Serializable]
public class UiMemoryData
{
	[XmlElement(ElementName = "UiType")]
	public UiType UiType;

	[XmlElement(ElementName = "MinMemory")]
	public int MinMemory = 150;
}
