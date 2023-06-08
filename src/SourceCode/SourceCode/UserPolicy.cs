using System;
using System.Xml.Serialization;

[Serializable]
public class UserPolicy
{
	[XmlElement(ElementName = "TermsAndConditions")]
	public bool? TermsAndConditions;

	[XmlElement(ElementName = "PrivacyPolicy")]
	public bool? PrivacyPolicy;
}
