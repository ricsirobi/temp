namespace SquadTactics;

public class UnitData
{
	public Character.Team _Team;

	public int _NumRevived;

	public float _Health;

	public UnitData(Character character)
	{
		_Team = character.pCharacterData._Team;
		_NumRevived = 0;
		StStat stat = character.pCharacterData._Stats.GetStat(Stat.HEALTH);
		_Health = stat.pCurrentValue;
	}
}
