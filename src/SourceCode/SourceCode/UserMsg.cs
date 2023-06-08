using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UserMsg", Namespace = "")]
public class UserMsg
{
	[XmlElement(ElementName = "Category")]
	public string mCategory = "";

	[XmlElement(ElementName = "Guest")]
	public string mGuest = "Register to get more benefits.";

	[XmlElement(ElementName = "Registered")]
	public string mRegistered = "You are a registered member.";
}
