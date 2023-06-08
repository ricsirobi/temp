using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "kvp", Namespace = "")]
public class ConfigurationSetting
{
	[XmlElement(ElementName = "k")]
	public string SettingKey;

	[XmlElement(ElementName = "v")]
	public string SettingValue;
}
