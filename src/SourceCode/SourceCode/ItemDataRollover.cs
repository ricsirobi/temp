using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "IRO", Namespace = "")]
public class ItemDataRollover
{
	[XmlElement(ElementName = "d")]
	public string DialogName;

	[XmlElement(ElementName = "b")]
	public string Bundle;
}
