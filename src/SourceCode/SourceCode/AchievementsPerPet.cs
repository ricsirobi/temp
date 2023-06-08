using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AchievementsPerPet", Namespace = "")]
public class AchievementsPerPet
{
	[XmlElement(ElementName = "PetCount")]
	public int PetCount;

	[XmlElement(ElementName = "AchID")]
	public int AchID;
}
