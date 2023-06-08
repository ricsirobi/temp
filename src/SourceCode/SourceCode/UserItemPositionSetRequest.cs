using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UIPSR", Namespace = "")]
public class UserItemPositionSetRequest : UserItemPosition
{
	[XmlElement(ElementName = "pix")]
	public int? ParentIndex;
}
