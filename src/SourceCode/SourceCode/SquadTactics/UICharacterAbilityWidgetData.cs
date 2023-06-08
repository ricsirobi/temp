namespace SquadTactics;

public class UICharacterAbilityWidgetData : KAWidgetUserData
{
	public Ability _Ability;

	public CharacterData _Character;

	public KAWidget _AniCoolDown;

	public KAWidget _BkgCoolDown;

	public KAWidget _IcoAbility;

	public UICharacterAbilityWidgetData(Ability ability, KAWidget item, CharacterData character)
	{
		_Ability = ability;
		_Item = item;
		_Character = character;
	}
}
