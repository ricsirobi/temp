using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "InviteData", Namespace = "", IsNullable = false)]
public class InviteData
{
	[XmlElement(ElementName = "NameFilterResponse")]
	public bool FilterNameResponse;

	[XmlElement(ElementName = "FriendInfo")]
	public InviteFriendResult[] InviteFriendResult;
}
