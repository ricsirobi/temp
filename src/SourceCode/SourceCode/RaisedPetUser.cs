using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RaisedPetUser", Namespace = "")]
public class RaisedPetUser
{
	[XmlElement(ElementName = "UID")]
	public string UserId;

	[XmlElement(ElementName = "DN")]
	public string DisplayName;
}
