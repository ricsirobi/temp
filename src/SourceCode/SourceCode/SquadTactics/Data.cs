using System;
using System.Xml.Serialization;

namespace SquadTactics;

[Serializable]
[XmlRoot(ElementName = "STData", Namespace = "")]
public class Data
{
	[XmlElement(ElementName = "CommonPrefetchList")]
	public string CommonPrefetchList;

	[XmlElement(ElementName = "Realm")]
	public Realm[] RealmsData;

	[XmlElement(ElementName = "Tutorial")]
	public RealmLevel TutorialData;
}
