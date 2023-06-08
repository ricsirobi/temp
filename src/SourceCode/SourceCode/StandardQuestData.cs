using System;
using System.Xml.Serialization;

[Serializable]
public class StandardQuestData : GuideLockData
{
	[XmlElement(ElementName = "Name")]
	public LocaleString Name;

	[XmlAttribute("playerLevel")]
	public int PlayerLevel;

	[XmlElement(ElementName = "CoreData")]
	public DisciplinaryData[] DisciplinaryDatas;
}
