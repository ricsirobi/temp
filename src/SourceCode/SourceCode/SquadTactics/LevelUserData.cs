namespace SquadTactics;

public class LevelUserData : KAWidgetUserData
{
	public bool _Locked;

	public LevelUserData(int index, bool locked)
	{
		_Index = index;
		_Locked = locked;
	}
}
