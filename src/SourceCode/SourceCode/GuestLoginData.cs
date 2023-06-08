using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GuestLoginData", IsNullable = true)]
public class GuestLoginData : LoginData
{
	[XmlElement(ElementName = "SubscriptionID")]
	public int SubscriptionID;

	[XmlElement(ElementName = "UserPolicy", IsNullable = true)]
	public UserPolicy UserPolicy { get; set; }
}
