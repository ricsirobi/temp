using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ChallengePetBlockedStages", IsNullable = true)]
public class ChallengeRequiredPetAction
{
	[XmlElement(ElementName = "RequiredPetAction")]
	public string _RequiredPetAction;

	[XmlElement(ElementName = "PetActionText")]
	public LocaleString _Text;
}
