using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(Namespace = "http://api.jumpstart.com/", IsNullable = true)]
public class ArrayOfContentInfo
{
	[XmlElement(ElementName = "ContentInfo")]
	public ContentInfo[] ContentInfo;
}
