using System.Xml.Serialization;

public enum JoinBuddyResultType
{
	[XmlEnum("0")]
	JoinSuccess,
	[XmlEnum("1")]
	JoinFailedHandled,
	[XmlEnum("2")]
	JoinFailedCommon,
	[XmlEnum("3")]
	JoinFailedBuddyLeft
}
