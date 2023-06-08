using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GetGameDataResponse", Namespace = "")]
public class GetGameDataResponse
{
	[XmlElement(ElementName = "GameDataSummaryList")]
	public List<GameDataSummary> GameDataSummaryList { get; set; }
}
