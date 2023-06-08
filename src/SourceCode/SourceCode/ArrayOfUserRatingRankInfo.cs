using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfUserRatingRankInfo")]
public class ArrayOfUserRatingRankInfo
{
	[XmlElement(ElementName = "UserRatingRankInfo")]
	public UserRatingRankInfo[] UserRatingRankInfo;
}
