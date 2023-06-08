using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "CustomParameters", Namespace = "", IsNullable = true)]
public class CustomParameters
{
	[XmlElement(ElementName = "profileuserid")]
	public string ProfileUserId;

	[XmlElement(ElementName = "game_name")]
	public string GameName;

	[XmlElement(ElementName = "item_name")]
	public string ItemName;

	[XmlElement(ElementName = "itemid")]
	public string ItemId;
}
