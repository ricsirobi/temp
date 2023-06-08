using System;
using System.Xml.Serialization;

[Serializable]
public class PromoPackageTrigger
{
	[XmlElement(ElementName = "Type")]
	public PromoPackageTriggerType Type;

	[XmlElement(ElementName = "Value")]
	public string Value;

	[XmlElement(ElementName = "Owns")]
	public bool? Owns;
}
