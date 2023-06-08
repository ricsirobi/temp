using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ParentLoginInfo", IsNullable = true)]
public class ParentLoginInfo : UserLoginInfo
{
	[XmlElement(ElementName = "LoginStatus")]
	public MembershipUserStatus Status;

	[XmlElement(ElementName = "ChildList")]
	public UserLoginInfo[] ChildList;

	[XmlElement(ElementName = "SendActivationReminder", IsNullable = true)]
	public bool? SendActivationReminder;

	[XmlElement(ElementName = "UnAuthorized", IsNullable = true)]
	public bool? UnAuthorized;
}
