using System;
using System.Xml.Serialization;

[Serializable]
public class AdSection
{
	[XmlElement(ElementName = "UiName")]
	public string UiName;

	[XmlElement(ElementName = "Ad")]
	public AdAttributes[] AdAttributes;

	[XmlElement(ElementName = "RefreshRate")]
	public float RefreshRate;

	[XmlElement(ElementName = "FadeTime")]
	public float FadeTime;
}
