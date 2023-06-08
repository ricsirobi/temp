using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "MissionInfo", Namespace = "")]
public class MissionInfo
{
	[XmlElement(ElementName = "Text")]
	public string Text;

	[XmlElement(ElementName = "ID")]
	public int ID;

	[XmlElement(ElementName = "VO")]
	public string VO;

	public string GetLocalizedString()
	{
		return StringTable.GetStringData(ID, Text);
	}
}
