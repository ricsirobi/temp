using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RegistrationResult", IsNullable = true)]
public class RegistrationResult
{
	[XmlElement(ElementName = "Status")]
	public MembershipUserStatus Status;

	[XmlElement(ElementName = "ApiToken")]
	public string ApiToken;

	[XmlElement(ElementName = "ParentLoginInfo")]
	public string ParentLoginInfo { get; set; }

	[XmlElement(ElementName = "UserID")]
	public string UserID { get; set; }

	[XmlElement(ElementName = "Suggestions", IsNullable = true)]
	public SuggestionResult Suggestions { get; set; }
}
