using System;
using System.Xml.Serialization;

namespace KA.Framework;

[Serializable]
public class UpgradeString
{
	[XmlElement(ElementName = "ProductID")]
	public int[] ProductID;

	[XmlElement(ElementName = "VersionState")]
	public VersionStatus VersionState;

	[XmlElement(ElementName = "UpgradeText")]
	public LocaleString UpgradeText;
}
