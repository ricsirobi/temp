using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ChallengePetData", IsNullable = true)]
public class ChallengePetData
{
	[XmlElement(ElementName = "IsPetRequired")]
	public bool _IsPetRequired;

	[XmlElement(ElementName = "BlockedStage", IsNullable = true)]
	public ChallengePetBlockedStages[] _BlockedStage;

	[XmlElement(ElementName = "PetAction", IsNullable = true)]
	public ChallengeRequiredPetAction[] _PetAction;
}
