using System;
using System.Xml.Serialization;

[Serializable]
public class ArenaFrenzyMapData
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "NameID")]
	public int NameID;

	[XmlElement(ElementName = "MapElement")]
	public MapElementInfo[] MapElements;

	[XmlElement(ElementName = "MinElements")]
	public int MinElements;

	[XmlElement(ElementName = "MaxElements")]
	public int MaxElements;

	[XmlElement(ElementName = "MapDimensions")]
	public MapDimensionsInfo MapDimensions;

	[XmlElement(ElementName = "MinTargets")]
	public int MinTargets;

	[XmlElement(ElementName = "MaxTargets")]
	public int MaxTargets;

	[XmlElement(ElementName = "MinBonusTargets")]
	public int MinBonusTargets;

	[XmlElement(ElementName = "MaxBonusTargets")]
	public int MaxBonusTargets;

	[XmlElement(ElementName = "SpawnPointMarker")]
	public SpawnPointMarkerInfo[] SpawnMarkers;
}
