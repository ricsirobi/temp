using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "PartyComplete", Namespace = "")]
public class UserPartyComplete : UserParty
{
	[XmlElement(ElementName = "Asset")]
	public string AssetBundle;
}
