namespace SquadTactics;

public class CharacterWidgetData : KAWidgetUserData
{
	public Character _Character;

	public UiCharacterStatus _UiStat;

	public KAWidget _HealthBar;

	public KAWidget _BtnStatus;

	public KAWidget _AniMovement;

	public KAWidget _AniAction;

	public KAWidget _TxtRevive;

	public KAWidget _TxtStatus;

	public KAWidget _BkgCoolDown;

	public KAWidget _TxtUnitLvl;

	public CharacterWidgetData(Character character, KAWidget item, UiCharacterStatus statui)
	{
		_Character = character;
		_Item = item;
		_UiStat = statui;
	}
}
