using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Neighbor", Namespace = "")]
public class Neighbor
{
	[XmlElement(ElementName = "NeighborUserID")]
	public Guid NeighborUserID;

	[XmlElement(ElementName = "Slot")]
	public int Slot;
}
