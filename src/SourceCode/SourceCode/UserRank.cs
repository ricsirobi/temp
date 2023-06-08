using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UserRank", Namespace = "")]
public class UserRank
{
	[XmlElement(ElementName = "RankID")]
	public int RankID;

	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "Description")]
	public string Description;

	[XmlElement(ElementName = "Image")]
	public string Image;

	[XmlElement(ElementName = "Audio")]
	public string Audio;

	[XmlElement(ElementName = "Value")]
	public int Value;

	[XmlElement(ElementName = "IsMember")]
	public bool IsMember;

	[XmlElement(ElementName = "PointTypeID")]
	public int PointTypeID;

	[XmlElement(ElementName = "GlobalRankID")]
	public int GlobalRankID;
}
