using System;
using System.Xml.Serialization;

[Serializable]
public class MapDimensionsInfo
{
	[XmlElement(ElementName = "NumCellsX")]
	public int NumCellsX;

	[XmlElement(ElementName = "NumCellsY")]
	public int NumCellsY;

	[XmlElement(ElementName = "CellWidth")]
	public int CellWidth;

	[XmlElement(ElementName = "CellHeight")]
	public int CellHeight;
}
