using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfUserBonus")]
public class ArrayOfUserBonus
{
	[XmlElement(ElementName = "UserBonus")]
	public UserBonus[] UserBonus;
}
