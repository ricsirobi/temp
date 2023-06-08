using System;
using System.Xml.Serialization;

[Serializable]
public class BundleTarget
{
	[XmlAttribute]
	public string Platform = "iOS, Web, PC, Android, WSA";

	[XmlAttribute]
	public string Quality = "Low, Mid, High, Ultra";

	public BundleTarget()
	{
	}

	public BundleTarget(string platform, string quality = "")
	{
		Platform = platform;
		if (!string.IsNullOrEmpty(quality))
		{
			Quality = quality;
		}
	}
}
