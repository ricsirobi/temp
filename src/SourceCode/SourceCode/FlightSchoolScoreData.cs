using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "FlightSchoolScoreData", Namespace = "")]
public class FlightSchoolScoreData
{
	[XmlElement(ElementName = "GMS")]
	public List<int> GlideModeScores { get; set; }

	[XmlElement(ElementName = "GMLPT")]
	public DateTime GlideModeLastPlayedTime { get; set; }

	[XmlElement(ElementName = "GMLUL")]
	public int GlideModeLastUnlockedLevel { get; set; }

	[XmlElement(ElementName = "FMS")]
	public List<int> FlightModeScores { get; set; }

	[XmlElement(ElementName = "FMLPT")]
	public DateTime FlightModeLastPlayedTime { get; set; }

	[XmlElement(ElementName = "FMLUL")]
	public int FlightModeLastUnlockedLevel { get; set; }

	[XmlElement(ElementName = "FSMS")]
	public List<int> FlightSuitModeScores { get; set; }

	[XmlElement(ElementName = "FSMLPT")]
	public DateTime FlightSuitModeLastPlayedTime { get; set; }

	[XmlElement(ElementName = "FSMLUL")]
	public int FlightSuitModeLastUnlockedLevel { get; set; }
}
