using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Data", Namespace = "")]
public class MissionStatic
{
	[XmlElement(ElementName = "Title", IsNullable = true)]
	public MissionInfo Title;

	[XmlElement(ElementName = "Desc", IsNullable = true)]
	public MissionInfo Description;

	[XmlElement(ElementName = "Setup", IsNullable = true)]
	public List<MissionSetup> Setups;

	[XmlElement(ElementName = "Offer", IsNullable = true)]
	public List<MissionAction> Offers;

	[XmlElement(ElementName = "End", IsNullable = true)]
	public List<MissionAction> Ends;

	[XmlElement(ElementName = "Repeat")]
	public bool Repeatable;

	[XmlElement(ElementName = "Hidden")]
	public bool Hidden;

	[XmlElement(ElementName = "Random")]
	public bool Random;

	[XmlElement(ElementName = "Reward", IsNullable = true)]
	public MissionReward Reward;

	[XmlElement(ElementName = "RemoveItem", IsNullable = true)]
	public List<MissionRemoveItem> RemoveItem;

	[XmlElement(ElementName = "Icon")]
	public string Icon;
}
