using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class UiCharactersInfo : KAUI
{
	public UiCharacterStatus _UiStatus;

	public float _CooldownAlpha = 0.5f;

	private KAWidget mTxtSelectedAbilityName;

	private KAWidget mTxtSelectedAbilityType;

	private KAWidget mTxtSelectedAbilityRange;

	private KAWidget mTxtSelectedAbilityTurn;

	private KAWidget mIcoSelectedAbility;

	private KAWidget mSmallAbilityInfo;

	private UiCharactersMenu mCharactersMenu;

	private UiCharacterAbilitesMenu mCharacterAbilitiesMenu;

	public UiCharactersMenu pCharactersMenu => mCharactersMenu;

	public UiCharacterAbilitesMenu pCharacterAbilitiesMenu => mCharacterAbilitiesMenu;

	protected override void Awake()
	{
		base.Awake();
		mCharactersMenu = (UiCharactersMenu)GetMenu("SquadTactics.UiCharactersMenu");
		mCharacterAbilitiesMenu = (UiCharacterAbilitesMenu)GetMenu("SquadTactics.UiCharacterAbilitesMenu");
	}

	public void Initialize(List<Character> characters)
	{
		mTxtSelectedAbilityName = FindItem("TxtAbilityName");
		mTxtSelectedAbilityType = FindItem("TxtAbility");
		mTxtSelectedAbilityRange = FindItem("TxtAbilityRange");
		mTxtSelectedAbilityTurn = FindItem("TxtAbilityCount");
		mSmallAbilityInfo = FindItem("AbilityInfoSmall");
		mIcoSelectedAbility = FindItem("IcoAbility");
		if (mIcoSelectedAbility != null)
		{
			mIcoSelectedAbility.SetVisibility(inVisible: false);
		}
		mCharactersMenu.SetupCharacterMenu(characters);
	}

	public void SetSelectedAbilityInfo(Character character, Ability ability)
	{
		if (mTxtSelectedAbilityName != null)
		{
			mTxtSelectedAbilityName.SetText(ability._Name.GetLocalizedString());
		}
		string text = ((ability._TargetType == Ability.Team.SAME && character.pCharacterData._Team == Character.Team.PLAYER) ? GameManager.pInstance._HUD._HUDStrings._HealText.GetLocalizedString() : GameManager.pInstance._HUD._HUDStrings._DamageText.GetLocalizedString());
		int num = Mathf.RoundToInt(character.pCharacterData._Stats.GetStatValue(ability._InfluencingStat) * ability._InfluencingStatMultiplier + (float)ability._BaseAmount);
		if (mTxtSelectedAbilityType != null)
		{
			if (num <= 0)
			{
				text = "";
			}
			else
			{
				StStatInfo statInfoByName = Settings.pInstance.GetStatInfoByName(ability._InfluencingStat);
				text = num + " [c]" + UtUtilities.GetKAUIColorString(statInfoByName._Color) + statInfoByName._AbbreviationText.GetLocalizedString() + " [/c][ffffff]" + text;
			}
			mTxtSelectedAbilityType.SetText(text);
			mTxtSelectedAbilityType.SetVisibility(num > 0);
		}
		if (mTxtSelectedAbilityRange != null)
		{
			mTxtSelectedAbilityRange.SetText(GameManager.pInstance._HUD.GetRangeTextFromRange(ability._Range));
		}
		if (mIcoSelectedAbility != null)
		{
			if (num <= 0 && ability._Effects != null && ability._Effects.Length != 0)
			{
				Effect[] effects = ability._Effects;
				foreach (Effect effect in effects)
				{
					if (effect is Tick)
					{
						Tick tick = (Tick)effect;
						mIcoSelectedAbility.SetSprite(Settings.pInstance.GetEffectIcon(tick._AppliedEffect));
					}
					else if (effect is Buff)
					{
						Buff buff = (Buff)effect;
						mIcoSelectedAbility.SetSprite(Settings.pInstance.GetStatEffectIcon(buff._AffectedStat));
					}
					else if (effect is CrowdControl)
					{
						CrowdControl crowdControl = (CrowdControl)effect;
						mIcoSelectedAbility.SetSprite(Settings.pInstance.GetEffectIcon(crowdControl._AppliedEffect));
					}
					else if (effect is Cleanse)
					{
						Cleanse cleanse = (Cleanse)effect;
						mIcoSelectedAbility.SetSprite(Settings.pInstance.GetEffectIcon(cleanse._AppliedEffect));
					}
				}
			}
			mIcoSelectedAbility.SetVisibility(num <= 0 && ability._Effects != null && ability._Effects.Length != 0);
		}
		if (mTxtSelectedAbilityTurn != null)
		{
			mTxtSelectedAbilityTurn.SetText(ability._Cooldown.ToString());
		}
	}

	public void UpdatePlayerStat(Character character)
	{
		foreach (KAWidget item in mCharactersMenu.GetItems())
		{
			CharacterWidgetData characterWidgetData = (CharacterWidgetData)item.GetUserData();
			if (characterWidgetData._Character == character)
			{
				if (!characterWidgetData._UiStat.GetVisibility())
				{
					characterWidgetData._BtnStatus.SetDisabled(characterWidgetData._Character.pActiveStatusEffects == null || characterWidgetData._Character.pActiveStatusEffects.Count <= 0);
				}
				else
				{
					characterWidgetData._UiStat.SetupStatMenu(character);
				}
				break;
			}
		}
	}

	public void UpdatePlayersInfoDisplay(Character character, bool updateAll)
	{
		mCharactersMenu.UpdateCharacterInfo(character, updateAll);
		mCharacterAbilitiesMenu.CheckAbilityAnims();
	}

	public void OnCloseStat(UiCharacterStatus status)
	{
		foreach (KAWidget item in mCharactersMenu.GetItems())
		{
			CharacterWidgetData characterWidgetData = (CharacterWidgetData)item.GetUserData();
			if (characterWidgetData._UiStat == status)
			{
				characterWidgetData._BtnStatus.SetDisabled(characterWidgetData._Character.pActiveStatusEffects == null || characterWidgetData._Character.pActiveStatusEffects.Count <= 0);
				break;
			}
		}
	}

	public void ShowSmallAbilityInfo(bool show)
	{
		mSmallAbilityInfo.SetVisibility(show);
	}
}
