using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Party", Namespace = "")]
public class UserPartyData
{
	[XmlElement(ElementName = "BuddyParties")]
	public UserParty[] BuddyParties;

	[XmlElement(ElementName = "NonBuddyParties")]
	public UserParty[] NonBuddyParties;
}
