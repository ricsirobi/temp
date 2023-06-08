using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "sdnr", Namespace = "")]
public class SetDisplayNameRequest
{
	[XmlElement(ElementName = "dn")]
	public string DisplayName { get; set; }

	[XmlElement(ElementName = "iid")]
	public int ItemID { get; set; }

	[XmlElement(ElementName = "sid")]
	public int StoreID { get; set; }
}
