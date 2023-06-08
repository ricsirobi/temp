using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "MissionAction", Namespace = "")]
public class MissionAction
{
	[XmlElement(ElementName = "Type")]
	public MissionActionType Type;

	[XmlElement(ElementName = "Asset")]
	public string Asset;

	[XmlElement(ElementName = "NPC")]
	public string NPC;

	[XmlElement(ElementName = "Text")]
	public string Text;

	[XmlElement(ElementName = "ID")]
	public int ID;

	[XmlElement(ElementName = "ItemID")]
	public int ItemID;

	[XmlElement(ElementName = "Priority")]
	public int Priority;

	public bool _Played;

	public string GetLocalizedString()
	{
		return StringTable.GetStringData(ID, Text);
	}
}
