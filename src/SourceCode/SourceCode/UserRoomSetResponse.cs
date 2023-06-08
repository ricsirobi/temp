using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "URSR", Namespace = "", IsNullable = false)]
public class UserRoomSetResponse
{
	[XmlElement(ElementName = "S")]
	public bool Success { get; set; }

	[XmlElement(ElementName = "SC")]
	public UserRoomValidationResult StatusCode { get; set; }
}
