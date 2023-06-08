using System;
using System.Xml.Serialization;

[Serializable]
public class MapElementInfo
{
	[XmlElement(ElementName = "PrefabFilePath")]
	public string PrefabFilePath;

	[XmlElement(ElementName = "CellSizeX")]
	public int CellSizeX;

	[XmlElement(ElementName = "CellSizeY")]
	public int CellSizeY;

	[XmlElement(ElementName = "NumCellsX")]
	public int NumCellsX;

	[XmlElement(ElementName = "NumCellsY")]
	public int NumCellsY;

	[XmlElement(ElementName = "AppearanceChance")]
	public int AppearanceChance;
}
