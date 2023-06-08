using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "PT", Namespace = "")]
public class ProfileTag
{
	[XmlElement(ElementName = "ID")]
	public int TagID;

	[XmlElement(ElementName = "NA", IsNullable = true)]
	public string TagName;

	[XmlElement(ElementName = "VAL", IsNullable = true)]
	public int? Value;
}
