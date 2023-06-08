using System;
using System.Xml.Serialization;

[Serializable]
public class DailyBonus
{
	[XmlElement(ElementName = "Day")]
	public int Day;

	[XmlElement(ElementName = "Type")]
	public string Type;

	[XmlElement(ElementName = "AchID")]
	public int AchID;

	[XmlElement(ElementName = "IconRes")]
	public string IconRes;

	[XmlElement(ElementName = "DisplayText")]
	public LocaleString DisplayText;
}
