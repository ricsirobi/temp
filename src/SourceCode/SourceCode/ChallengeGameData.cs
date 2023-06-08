using System;
using System.Xml.Serialization;

[Serializable]
public class ChallengeGameData
{
	[XmlElement(ElementName = "SceneName")]
	public string _SceneName;

	[XmlElement(ElementName = "GameID")]
	public int _GameID;

	[XmlElement(ElementName = "GameTitle")]
	public LocaleString _GameTitle;

	[XmlElement(ElementName = "PetData", IsNullable = true)]
	public ChallengePetData _PetData;
}
