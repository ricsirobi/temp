using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfUserPartyComplete", Namespace = "http://api.jumpstart.com/")]
public class ArrayOfUserPartyComplete
{
	[XmlElement(ElementName = "UserPartyComplete")]
	public UserPartyComplete[] UserPartyComplete;
}
