using System.CodeDom.Compiler;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal enum PIIKind
{
	NotSet,
	DistinguishedName,
	GenericData,
	IPV4Address,
	IPv6Address,
	MailSubject,
	PhoneNumber,
	QueryString,
	SipAddress,
	SmtpAddress,
	Identity,
	Uri,
	Fqdn,
	IPV4AddressLegacy
}
