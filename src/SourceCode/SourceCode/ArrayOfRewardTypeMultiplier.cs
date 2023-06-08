using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfRewardTypeMultiplier")]
public class ArrayOfRewardTypeMultiplier
{
	[XmlElement(ElementName = "RewardTypeMultiplier")]
	public RewardTypeMultiplier[] RewardTypeMultiplier;
}
