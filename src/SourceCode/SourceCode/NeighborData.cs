using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "NeighborData", Namespace = "")]
public class NeighborData
{
	[XmlElement(ElementName = "UserID")]
	public Guid UserID;

	[XmlElement(ElementName = "Neighbors")]
	public Neighbor[] Neighbors;
}
