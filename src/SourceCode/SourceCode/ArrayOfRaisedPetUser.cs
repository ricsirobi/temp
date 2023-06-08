using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfRaisedPetUser", Namespace = "http://api.jumpstart.com/")]
public class ArrayOfRaisedPetUser
{
	[XmlElement(ElementName = "RaisedPetUser")]
	public RaisedPetUser[] RaisedPetUsers;
}
