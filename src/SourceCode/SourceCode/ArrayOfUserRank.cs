using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfUserRank", Namespace = "http://api.jumpstart.com/")]
public class ArrayOfUserRank
{
	[XmlElement(ElementName = "UserRank")]
	public UserRank[] UserRank;
}
