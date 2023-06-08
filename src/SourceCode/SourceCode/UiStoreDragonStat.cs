using SquadTactics;
using UnityEngine;

public class UiStoreDragonStat : KAUI
{
	private UiStatPopUp mUiDragonStatPopUp;

	private KAUIMenu mContentMenu;

	private KAWidget mPopUpBtn;

	private ItemData mDragonItem;

	private KAWidget mDragonPrimaryTypeIcon;

	private KAWidget mDragonSecondaryTypeIcon;

	private KAWidget mAttackPowerRating;

	private KAWidget mFirePowerRating;

	private KAWidget mHealthRating;

	private KAWidget mRacingRating;

	private SanctuaryPetTypeInfo mSanctuaryPetTypeInfo;

	protected override void Start()
	{
		base.Start();
		mPopUpBtn = FindItem("BtnShowStats");
		mContentMenu = _MenuList[0];
		mUiDragonStatPopUp = (UiStatPopUp)_UiList[0];
	}

	public void SetDragonStatIcons(ItemData item)
	{
		mDragonItem = item;
		int attribute = item.GetAttribute("PetTypeID", -1);
		mSanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(attribute);
		mDragonPrimaryTypeIcon = FindItem("PrimaryTypeIco");
		mDragonSecondaryTypeIcon = FindItem("SecondaryTypeIco");
		if (mSanctuaryPetTypeInfo.pPrimaryType != null)
		{
			if (mDragonPrimaryTypeIcon != null)
			{
				string iconSprite = mSanctuaryPetTypeInfo.pPrimaryType._IconSprite;
				if (!string.IsNullOrEmpty(iconSprite))
				{
					mDragonPrimaryTypeIcon.pBackground.spriteName = iconSprite;
					mDragonPrimaryTypeIcon.SetVisibility(inVisible: true);
				}
				else
				{
					mDragonPrimaryTypeIcon.SetVisibility(inVisible: false);
				}
			}
		}
		else if (mDragonPrimaryTypeIcon != null)
		{
			mDragonPrimaryTypeIcon.SetVisibility(inVisible: false);
		}
		if (mSanctuaryPetTypeInfo.pSecondaryType != null)
		{
			if (mDragonSecondaryTypeIcon != null)
			{
				string iconSprite2 = mSanctuaryPetTypeInfo.pSecondaryType._IconSprite;
				if (!string.IsNullOrEmpty(iconSprite2))
				{
					mDragonSecondaryTypeIcon.pBackground.spriteName = iconSprite2;
					mDragonSecondaryTypeIcon.SetVisibility(inVisible: true);
				}
				else
				{
					mDragonSecondaryTypeIcon.SetVisibility(inVisible: false);
				}
			}
		}
		else if (mDragonSecondaryTypeIcon != null)
		{
			mDragonSecondaryTypeIcon.SetVisibility(inVisible: false);
		}
		mAttackPowerRating = FindItem("AttackPowerRating");
		mAttackPowerRating.SetText(Mathf.Round(mSanctuaryPetTypeInfo._Stats._Attack).ToString());
		mFirePowerRating = FindItem("FirePowerRating");
		mFirePowerRating.SetText(Mathf.Round(mSanctuaryPetTypeInfo._Stats._FirePower).ToString());
		mHealthRating = FindItem("HealthRating");
		mHealthRating.SetText(Mathf.Round(mSanctuaryPetTypeInfo._Stats._Health).ToString());
		mRacingRating = FindItem("RacingRating");
		mRacingRating.SetText(Mathf.Round(mSanctuaryPetTypeInfo._Stats._MaxSpeed).ToString());
		mPopUpBtn.SetVisibility(inVisible: true);
	}

	public void RemoveStatPreview()
	{
		SetVisibility(inVisible: false);
		mContentMenu.ClearItems();
		mDragonItem = null;
		mSanctuaryPetTypeInfo = null;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mPopUpBtn)
		{
			CharacterData characterData = new CharacterData(CharacterDatabase.pInstance.GetCharacter(mSanctuaryPetTypeInfo._Name));
			mUiDragonStatPopUp.SetVisibility(inVisible: true);
			mUiDragonStatPopUp.ShowCombatStats(characterData, 1, mSanctuaryPetTypeInfo._NameText.GetLocalizedString());
			mUiDragonStatPopUp.ShowRacingStats(mSanctuaryPetTypeInfo);
			mUiDragonStatPopUp.ShowDescription(mDragonItem);
		}
	}
}
