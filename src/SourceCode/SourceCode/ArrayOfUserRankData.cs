using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfUserRankData", Namespace = "http://api.jumpstart.com/")]
public class ArrayOfUserRankData
{
	[XmlElement(ElementName = "UserRankData")]
	public UserRankData[] UserRank;
}
