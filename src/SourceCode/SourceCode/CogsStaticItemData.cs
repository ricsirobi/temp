using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "CogsStaticItemData", Namespace = "")]
public class CogsStaticItemData
{
	[XmlElement(ElementName = "StartCogs")]
	public Cog[] StartCogs;

	[XmlElement(ElementName = "VictoryCogs")]
	public Cog[] VictoryCogs;

	[XmlElement(ElementName = "StaticCogs")]
	public Cog[] StaticCogs;
}
