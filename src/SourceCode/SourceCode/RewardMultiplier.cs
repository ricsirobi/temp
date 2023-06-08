using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ARM", IsNullable = true, Namespace = "")]
public class RewardMultiplier
{
	[XmlElement(ElementName = "PT")]
	public int PointTypeID;

	[XmlElement(ElementName = "MF")]
	public int MultiplierFactor;

	[XmlElement(ElementName = "MET")]
	public DateTime MultiplierEffectTime;
}
