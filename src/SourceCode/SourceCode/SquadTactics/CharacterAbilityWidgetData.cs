namespace SquadTactics;

public class CharacterAbilityWidgetData : KAWidgetUserData
{
	public Ability _Ability;

	public Character _Character;

	public KAWidget _AniCoolDown;

	public KAWidget _BkgCoolDown;

	public KAWidget _IcoAbility;

	public CharacterAbilityWidgetData(Ability ability, KAWidget item, Character character)
	{
		_Ability = ability;
		_Item = item;
		_Character = character;
	}
}
