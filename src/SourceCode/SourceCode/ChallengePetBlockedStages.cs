using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ChallengePetBlockedStages", IsNullable = true)]
public class ChallengePetBlockedStages
{
	[XmlElement(ElementName = "PetStage")]
	public RaisedPetStage _PetStage;

	[XmlElement(ElementName = "StageText")]
	public LocaleString _StageText;
}
