using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfUserProfileDisplayData")]
public class UserProfileDataList
{
	[XmlElement(ElementName = "UserProfileDisplayData")]
	public UserProfileData[] UserProfiles;
}
