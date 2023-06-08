using System;
using System.Xml.Serialization;

[Serializable]
public class MMOLevelData
{
	[XmlArray("Zones")]
	public string[] Zones = new string[0];

	[XmlElement(ElementName = "SpecialZoneData")]
	public SpecialZoneData[] SpecialZoneData;

	[XmlArray("PlayerOwnedLevels")]
	public string[] PlayerOwnedLevels = new string[0];

	[XmlArray("JoinNotAllowedLevels")]
	public string[] JoinNotAllowedLevels = new string[0];

	[XmlArray("MMOFullLevels")]
	public string[] MMOFullLevels = new string[0];

	[XmlArray("MemberOnlyZones")]
	public string[] MemberOnlyZones = new string[0];

	[XmlArray("MultiRoomZones")]
	public string[] MultiRoomZones = new string[0];

	[XmlArray("RacingLevels")]
	public string[] RacingLevels = new string[0];
}
