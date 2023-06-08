namespace SquadTactics;

public class SquadData : KAWidgetUserData
{
	public UnitSelection unitData;

	public bool _IsAvailable;

	public SquadData(string name, bool locked, int raisedPetID, int level, bool IsAvailable, string weapon)
	{
		unitData = new UnitSelection(name, level, raisedPetID, locked, weapon);
		_IsAvailable = IsAvailable;
	}
}
