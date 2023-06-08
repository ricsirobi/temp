using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfString", Namespace = "http://api.jumpstart.com/")]
public class ChildListInfo
{
	[XmlElement(ElementName = "string")]
	public string[] ChildInfo;
}
