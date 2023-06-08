using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ProfileSlotsData", Namespace = "")]
public class ProfileSlotsData
{
	[XmlElement(ElementName = "FreeSlotCount")]
	public int mFreeSlotCount = -1;

	[XmlElement(ElementName = "MemberFreeSlotCount")]
	public int mMemberFreeSlotCount = -1;
}
