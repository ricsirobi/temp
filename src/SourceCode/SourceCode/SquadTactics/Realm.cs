using System;
using System.Xml.Serialization;

namespace SquadTactics;

[Serializable]
public class Realm
{
	[XmlElement(ElementName = "Name")]
	public LocaleString Name;

	[XmlElement(ElementName = "Description")]
	public LocaleString Description;

	[XmlElement(ElementName = "LevelPrefetchList")]
	public string LevelPrefetchList;

	[XmlElement(ElementName = "Level")]
	public RealmLevel[] RealmLevelsInfo;

	[XmlElement(ElementName = "RealmIcon")]
	public string RealmIcon;

	[XmlElement(ElementName = "Event")]
	public string Event;
}
