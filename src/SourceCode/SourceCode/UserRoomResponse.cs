using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "URR", Namespace = "")]
public class UserRoomResponse
{
	[XmlElement(ElementName = "ur")]
	public List<UserRoom> UserRoomList;
}
