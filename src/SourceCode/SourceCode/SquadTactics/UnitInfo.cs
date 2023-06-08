using System;
using System.Xml.Serialization;

namespace SquadTactics;

[Serializable]
public class UnitInfo
{
	[XmlElement(ElementName = "Name")]
	public string _Name = "";

	[XmlElement(ElementName = "Level")]
	public int _Level;

	[XmlElement(ElementName = "Weapon", IsNullable = true)]
	public string _Weapon = "";

	[Obsolete("For XML Serialization Only", true)]
	public UnitInfo()
	{
	}

	public UnitInfo(string name, int level, string weapon)
	{
		_Name = name;
		_Level = level;
		_Weapon = weapon;
	}
}
