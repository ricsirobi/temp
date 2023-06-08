using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfUserActivity", Namespace = "http://api.jumpstart.com/")]
public class ArrayOfUserActivity
{
	[XmlElement(ElementName = "UserActivity")]
	public UserActivity[] UserActivity;
}
