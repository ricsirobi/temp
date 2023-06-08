using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfRankAttributeData", Namespace = "http://tempuri.org/")]
public class ArrayOfRankAttributeData
{
	[XmlElement(ElementName = "RankAttributeData")]
	public RankAttributeData[] RankAttributeData;
}
