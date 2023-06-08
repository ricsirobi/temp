using System;

namespace SquadTactics;

[Serializable]
public class UnitSelection
{
	public string _UnitName;

	public int _UnitLevel;

	public int _RaisedPetID;

	public bool _IsLocked;

	public string _Weapon;

	public UnitSelection()
	{
	}

	public UnitSelection(string name, int level, int rpid, bool locked, string weapon)
	{
		_UnitName = name;
		_UnitLevel = level;
		_RaisedPetID = rpid;
		_IsLocked = locked;
		_Weapon = weapon;
	}
}
