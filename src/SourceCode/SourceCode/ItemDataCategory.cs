using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "IC", Namespace = "")]
public class ItemDataCategory
{
	[XmlElement(ElementName = "cid")]
	public int CategoryId;

	[XmlElement(ElementName = "cn")]
	public string CategoryName;

	[XmlElement(ElementName = "i", IsNullable = true)]
	public string IconName;
}
