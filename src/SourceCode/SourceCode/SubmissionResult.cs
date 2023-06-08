using System.Xml.Serialization;

[XmlRoot(ElementName = "SubmissionResult", Namespace = "http://api.jumpstart.com/")]
public enum SubmissionResult
{
	[XmlEnum("Failed")]
	Failed,
	[XmlEnum("Success")]
	Success,
	[XmlEnum("InvalidCode")]
	InvalidCode,
	[XmlEnum("UsedCode")]
	UsedCode,
	[XmlEnum("ExpiredCode")]
	ExpiredCode,
	[XmlEnum("CardSuspended")]
	CardSuspended,
	[XmlEnum("InvalidAccount")]
	InvalidAccount,
	[XmlEnum("PartnerServerError")]
	PartnerServerError,
	[XmlEnum("CardDeactivated")]
	CardDeactivated,
	[XmlEnum("InvalidRequest")]
	InvalidRequest,
	[XmlEnum("InvalidTransaction")]
	InvalidTransaction,
	[XmlEnum("InvalidRedeemtionSource")]
	InvalidRedeemtionSource,
	[XmlEnum("PrizeEarned")]
	PrizeEarned
}
