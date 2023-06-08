using System.Runtime.Serialization;
using System.Xml.Serialization;

public enum PurchaseOrderValidation
{
	[EnumMember]
	[XmlEnum("1")]
	Valid = 1,
	[EnumMember]
	[XmlEnum("2")]
	ItemNotFound,
	[EnumMember]
	[XmlEnum("3")]
	InsufficientParameter,
	[EnumMember]
	[XmlEnum("4")]
	SignatureValidationFail,
	[EnumMember]
	[XmlEnum("5")]
	TicksNotMatching
}
