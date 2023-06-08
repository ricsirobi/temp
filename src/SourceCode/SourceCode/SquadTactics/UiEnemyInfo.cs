using UnityEngine;

namespace SquadTactics;

public class UiEnemyInfo : KAUI
{
	public string _AlertText = "Alert";

	public string _SleepingText = "Asleep";

	private KAWidget mPotrait;

	private KAWidget mTxtName;

	private KAWidget mHealthBar;

	private KAWidget mTxtMovement;

	private KAWidget mTxtMoveCount;

	private KAWidget mTxtRange;

	private KAWidget mIcoElementType;

	private KAWidget mTxtLevel;

	private UiCharacterStatusMenu mStatsMenu;

	private Character mSelectedEnemy;

	public Character pSelectedEnemy => mSelectedEnemy;

	protected override void Start()
	{
		base.Start();
		mPotrait = FindItem("AniEnemyPortrait");
		mTxtName = FindItem("TxtEnemyName");
		mHealthBar = FindItem("AniHealthBar");
		mTxtMovement = FindItem("TxtMovement");
		mTxtMoveCount = FindItem("TxtMoveCount");
		mTxtRange = FindItem("TxtRangeCount");
		mIcoElementType = FindItem("IcoEnemyAttrib");
		mTxtLevel = FindItem("TxtUnitLvl");
		mStatsMenu = (UiCharacterStatusMenu)GetMenu("SquadTactics.UiCharacterStatusMenu");
		SetVisibility(inVisible: false);
	}

	public void UpdateEnemyDetails(Character enemy, bool active)
	{
		if (enemy != null && active)
		{
			if (enemy != mSelectedEnemy)
			{
				mSelectedEnemy = enemy;
			}
			mPotrait.SetTextureFromBundle(enemy.pCharacterData._PortraitIcon);
			ElementInfo elementInfo = Settings.pInstance.GetElementInfo(enemy.pCharacterData._WeaponData._ElementType);
			if (elementInfo != null)
			{
				mIcoElementType.SetSprite(elementInfo._Icon);
			}
			else
			{
				mIcoElementType.SetSprite("");
			}
			mTxtName.SetText(enemy.pCharacterData._DisplayNameText.GetLocalizedString());
			mTxtMoveCount.SetText(enemy.pCharacterData._Stats._Movement.pCurrentValue.ToString());
			float pCurrentValue = enemy.pCharacterData._Stats._Health.pCurrentValue;
			float num = enemy.pCharacterData._Stats._Health._Limits.Max;
			if (num <= 0f)
			{
				num = 1f;
			}
			mHealthBar.SetProgressLevel(pCurrentValue / num);
			mHealthBar.SetText(Mathf.CeilToInt(pCurrentValue) + "/" + Mathf.CeilToInt(num));
			float num2 = float.MinValue;
			foreach (Ability pAbility in enemy.pAbilities)
			{
				if (pAbility._Range > num2)
				{
					num2 = pAbility._Range;
				}
			}
			mTxtRange.SetText(GameManager.pInstance._HUD.GetRangeTextFromRange(num2));
			if (mTxtLevel != null)
			{
				mTxtLevel.SetText(enemy.pCharacterData.pLevel.ToString());
			}
			UpdateEnemyStat(enemy);
			UpdateEnemyHealth(enemy);
			bool flag = enemy.pCharacterData._Team == Character.Team.INANIMATE;
			mTxtLevel.SetVisibility(!flag);
			mTxtMovement.SetVisibility(!flag);
			mTxtMoveCount.SetVisibility(!flag);
			mTxtRange.SetVisibility(!flag);
			mIcoElementType.SetVisibility(!flag);
		}
		else
		{
			mSelectedEnemy = null;
		}
		SetVisibility(active);
	}

	public void UpdateEnemyStat(Character character)
	{
		mStatsMenu.ClearItems();
		if (character.pActiveStatusEffects != null && character.pActiveStatusEffects.Count > 0)
		{
			mStatsMenu.SetupStatMenu(character);
		}
	}

	public void UpdateEnemyHealth(Character enemy)
	{
		if (enemy.pIsDead)
		{
			SetVisibility(inVisible: false);
		}
		float num = enemy.pCharacterData._Stats._Health.pCurrentValue / enemy.pCharacterData._Stats._Health._Limits.Max;
		mHealthBar.SetProgressLevel(num);
		mHealthBar.SetText(Mathf.CeilToInt(enemy.pCharacterData._Stats._Health.pCurrentValue) + "/" + Mathf.CeilToInt(enemy.pCharacterData._Stats._Health._Limits.Max));
		HealthBarRange[] healthRanges = GameManager.pInstance._HUD._HealthRanges;
		foreach (HealthBarRange healthBarRange in healthRanges)
		{
			if (num <= healthBarRange._Percentage)
			{
				mHealthBar.GetProgressBar().color = healthBarRange._Color;
				break;
			}
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		if (InteractiveTutManager._CurrentActiveTutorialObject == null || !inVisible)
		{
			base.SetVisibility(inVisible);
		}
	}
}
