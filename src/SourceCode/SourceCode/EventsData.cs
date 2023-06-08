using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "EventsData", Namespace = "")]
public class EventsData
{
	[XmlElement(ElementName = "AdEventType")]
	public AdEventType AdEventType;

	[XmlElement(ElementName = "Percent")]
	public float Percentage;

	[XmlElement(ElementName = "LimitsPerDay")]
	public int LimitsPerDay;

	[XmlElement(ElementName = "CoolDown")]
	public float CoolDown;

	[XmlElement(ElementName = "HideForMember")]
	public bool HideForMember;

	[XmlElement(ElementName = "RewardData")]
	public string RewardData;

	[XmlElement(ElementName = "ModuleNameText")]
	public LocaleString ModuleNameText;

	public int pAvailableCount { get; set; }

	public DateTime pCoolDownEndTime { get; set; }
}
