using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "TrackElement", IsNullable = true, Namespace = "")]
public class TrackElement
{
	[XmlElement(ElementName = "id", IsNullable = true)]
	public int? UserTrackElementID;

	[XmlElement(ElementName = "utetid")]
	public int UserTrackElementTypeID;

	[XmlElement(ElementName = "utid", IsNullable = true)]
	public int? UserTrackID;

	[XmlElement(ElementName = "px")]
	public double PosX;

	[XmlElement(ElementName = "py")]
	public double PosY;

	[XmlElement(ElementName = "pz")]
	public double PosZ;

	[XmlElement(ElementName = "rx")]
	public double RotX;

	[XmlElement(ElementName = "ry")]
	public double RotY;

	[XmlElement(ElementName = "rz")]
	public double RotZ;

	[XmlElement(ElementName = "rid")]
	public string ResourceID;

	[XmlElement(ElementName = "c")]
	public int CreativePoint;
}
