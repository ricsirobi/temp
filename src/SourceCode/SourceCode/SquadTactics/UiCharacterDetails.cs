using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class UiCharacterDetails : KAUI
{
	[Header("UI")]
	private UiAbilityDetails mAbilityDetails;

	private UiCharacterDetailsMenu mCharacterDetailsMenu;

	private CharacterData mCharacterData;

	private Weapon mWeaponLoaded;

	private SquadData mPetData;

	private int mUnitRank;

	private KAWidget mTxtValueStrength;

	private KAWidget mTxtValueFPR;

	private KAWidget mTxtValueMove;

	private KAWidget mTxtValueDefence;

	private KAWidget mTxtValueHPR;

	private KAWidget mTxtValueCRIT;

	private KAWidget mTxtHealthValue;

	private KAWidget mTxtAvatarName;

	private KAWidget mTxtSpecies;

	private KAWidget mTxtPlayerRank;

	private KAWidget mAniSkill1;

	private KAWidget mAniSkill2;

	private KAWidget mBtnAvatar;

	private KAWidget mBtnClose;

	private KAWidget mBtnAdd;

	private KAWidget mBtnRemove;

	private KAWidget mAniLock;

	private KAWidget mBtnCustomize;

	private KAWidget mBtnBlackSmith;

	private KAWidget mBtnJournal;

	private KAWidget mCustomizePopUp;

	private Ability mCurrentAbility;

	public string _WeaponBundlePath = "RS_DATA/STWeapons.unity3d/";

	public LocaleString _HealText = new LocaleString("healing");

	public LocaleString _DamageText = new LocaleString("damage");

	public RangeTextMap[] _RangeTextMap;

	[Header("Ability Details")]
	public List<AbilityInfo> _AbilitiesInfoList;

	[Header("Customization")]
	public string _CustomizationAssetPath = "RS_DATA/PfUiAvatarCustomizationDO.unity3d/PfUiAvatarCustomizationDO";

	private UiAvatarCustomization mUiAvatarCustomization;

	public Ability pCurrentAbility
	{
		get
		{
			return mCurrentAbility;
		}
		set
		{
			mCurrentAbility = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		mTxtValueStrength = FindItem("TxtValueStrength");
		mTxtValueFPR = FindItem("TxtValueFPR");
		mTxtValueMove = FindItem("TxtValueMove");
		mTxtValueDefence = FindItem("TxtValueDefence");
		mTxtValueHPR = FindItem("TxtValueHPR");
		mTxtValueCRIT = FindItem("TxtValueCRIT");
		mTxtHealthValue = FindItem("TxtHealthValue");
		mTxtAvatarName = FindItem("TxtAvatarName");
		mTxtSpecies = FindItem("TxtSpecies");
		mTxtPlayerRank = FindItem("TxtPlayerRank");
		mAniSkill1 = FindItem("AniSkill1");
		mAniSkill2 = FindItem("AniSkill2");
		mBtnAvatar = FindItem("BtnAvatar");
		mBtnClose = FindItem("BtnClose");
		mBtnAdd = FindItem("BtnAdd");
		mBtnRemove = FindItem("BtnRemove");
		mAniLock = FindItem("AniLock");
		mBtnCustomize = FindItem("BtnCustomize");
		mBtnBlackSmith = FindItem("BtnBlackSmith");
		mBtnJournal = FindItem("BtnJournal");
		mCustomizePopUp = FindItem("CustomizePopUp");
		mCharacterDetailsMenu = (UiCharacterDetailsMenu)_MenuList[0];
		mAbilityDetails = (UiAbilityDetails)_UiList[0];
	}

	private void OnEnable()
	{
		UiToolbar.AvatarUpdated += AvatarUpdated;
	}

	private void OnDisable()
	{
		UiToolbar.AvatarUpdated -= AvatarUpdated;
	}

	private void AvatarUpdated()
	{
		LevelManager.pInstance._TeamSelection.ReLoadAvatar();
		LoadDetails(mPetData);
	}

	public void Reload(int updatedLevel)
	{
		mPetData.unitData._UnitLevel = updatedLevel;
		LoadDetails(mPetData);
	}

	public int GetCurrentPetID()
	{
		if (mPetData != null && mPetData.unitData != null)
		{
			return mPetData.unitData._RaisedPetID;
		}
		return 0;
	}

	public void LoadDetails(SquadData petData)
	{
		mPetData = petData;
		if (petData.unitData._RaisedPetID > 0)
		{
			RaisedPetData byID = RaisedPetData.GetByID(petData.unitData._RaisedPetID);
			SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(byID.PetTypeID);
			RefreshButtons(mPetData.unitData._RaisedPetID);
			mCharacterData = new CharacterData(CharacterDatabase.pInstance.GetCharacter(sanctuaryPetTypeInfo._Name, byID));
			mAniLock.SetVisibility(LevelManager.pInstance._TeamSelection.IsUnitInSquad(mPetData.unitData._RaisedPetID) == UnitSelectionType.LEVELDATA);
			mTxtAvatarName.SetText(byID.Name);
			mTxtSpecies.SetText(mCharacterData._DisplayNameText.GetLocalizedString());
		}
		else
		{
			mCharacterData = new CharacterData(CharacterDatabase.pInstance.GetCharacter(petData.unitData._UnitName, petData.unitData._RaisedPetID));
			mTxtAvatarName.SetText(mCharacterData._DisplayNameText.GetLocalizedString());
			mTxtSpecies.SetText(mCharacterData._SpeciesText.GetLocalizedString());
			RefreshButtons(0);
			mAniLock.SetVisibility(inVisible: true);
		}
		if (!string.IsNullOrEmpty(petData.unitData._Weapon))
		{
			mCharacterData._WeaponData = WeaponDatabase.pInstance.GetWeaponData(petData.unitData._Weapon);
		}
		else if (mCharacterData.pIsAvatar())
		{
			CharacterDatabase.pInstance.ReInitAvatarWeapon();
			mCharacterData._WeaponData = CharacterDatabase.pInstance.GetAvatarWeaponData();
		}
		if (mCharacterData.pIsAvatar() && AvatarData.pIsReady)
		{
			mUnitRank = UserRankData.pInstance.RankID;
			mTxtAvatarName.SetText(AvatarData.pInstance.DisplayName);
		}
		else
		{
			mUnitRank = petData.unitData._UnitLevel;
		}
		SetInfo();
	}

	private void SetStats()
	{
		mTxtValueStrength.SetText(mCharacterData._Stats._Strength.pCurrentValue.ToString());
		mTxtValueFPR.SetText(mCharacterData._Stats._FirePower.pCurrentValue.ToString());
		mTxtValueMove.SetText(mCharacterData._Stats._Movement.pCurrentValue.ToString());
		mTxtValueDefence.SetText(mCharacterData._Stats._DodgeChance.GetMultipliedValue().ToString());
		mTxtValueHPR.SetText(mCharacterData._Stats._HealingPower.pCurrentValue.ToString());
		mTxtValueCRIT.SetText(mCharacterData._Stats._CriticalChance.GetMultipliedValue().ToString());
		mTxtHealthValue.SetText(mCharacterData._Stats._Health.pCurrentValue.ToString());
	}

	private void SetInfo()
	{
		mTxtPlayerRank.SetText(mUnitRank.ToString());
		mBtnCustomize.SetVisibility(mCharacterData.pIsAvatar());
		mCustomizePopUp.SetVisibility(inVisible: false);
		ResetAbilitySelection();
		KAUI.SetExclusive(this, new Color(0f, 0f, 0f, 0.7f));
		RsResourceManager.LoadAssetFromBundle(_WeaponBundlePath + mCharacterData._WeaponData._PrefabName, OnWeaponBundleLoaded, typeof(GameObject));
		if (mCharacterData.pIsAvatar())
		{
			AvPhotoSetter @object = new AvPhotoSetter(mBtnAvatar);
			LevelManager.pInstance._TeamSelection.pStillPhotoManager.TakePhotoUI(UserInfo.pInstance.UserID, (Texture2D)mBtnAvatar.GetTexture(), @object.PhotoCallback, null);
		}
		else if (mPetData.unitData._RaisedPetID > 0)
		{
			RaisedPetData byID = RaisedPetData.GetByID(mPetData.unitData._RaisedPetID);
			if (byID != null)
			{
				mBtnAvatar.SetTexture(null);
				int slotIdx = (byID.ImagePosition.HasValue ? byID.ImagePosition.Value : 0);
				ImageData.Load("EggColor", slotIdx, base.gameObject);
			}
		}
		else
		{
			mBtnAvatar.SetTextureFromBundle(mCharacterData._PortraitIcon);
		}
		mAniSkill1.SetSprite(WeaponDatabase.pInstance.GetWeaponTypeSprite(mCharacterData._WeaponData._WeaponType));
		mAniSkill2.SetSprite(WeaponDatabase.pInstance.GetElementSprite(mCharacterData._WeaponData._ElementType));
	}

	public void OnImageLoaded(ImageDataInstance img)
	{
		if (!(img.mIconTexture == null))
		{
			mBtnAvatar.SetTexture(img.mIconTexture);
		}
	}

	private void OnWeaponBundleLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			SetVisibility(inVisible: true);
			if (mWeaponLoaded != null)
			{
				Object.Destroy(mWeaponLoaded.gameObject);
			}
			GameObject gameObject = Object.Instantiate((GameObject)inObject);
			mWeaponLoaded = gameObject.GetComponent<Weapon>();
			gameObject.transform.parent = base.transform;
			if (!string.IsNullOrEmpty(mPetData.unitData._Weapon))
			{
				mCharacterData._Stats.SetInitialValues(mUnitRank, mCharacterData._WeaponData._Stats);
			}
			else if (mCharacterData.pIsAvatar())
			{
				mCharacterData._Stats.SetInitialValues(mUnitRank, AvatarData.pInstanceInfo.GetPartsCombinedStats());
			}
			else
			{
				mCharacterData._Stats.SetInitialValues(mUnitRank);
			}
			SetStats();
			mCharacterDetailsMenu.SetupAbilityMenu(mWeaponLoaded, mCharacterData);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Weapon Yet to be Loaded");
			break;
		}
	}

	public void CloseUI()
	{
		KAUI.RemoveExclusive(this);
		SetVisibility(inVisible: false);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mBtnClose)
		{
			CloseUI();
		}
		else if (item == mBtnBlackSmith)
		{
			LevelManager.pInstance._TeamSelection.SetVisibility(inVisible: false);
			UiBlacksmith.Init(Mode.ENHANCE);
			UiBlacksmith.OnClosed = delegate
			{
				BlackSmithClosed();
			};
		}
		else if (item == mBtnJournal)
		{
			LevelManager.pInstance._TeamSelection.SetVisibility(inVisible: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			RsResourceManager.LoadAssetFromBundle(_CustomizationAssetPath, OnAvatarCustomizationLoaded, typeof(GameObject));
		}
		else if (item == mBtnCustomize)
		{
			mCustomizePopUp.SetVisibility(!mCustomizePopUp.GetVisibility());
		}
		else if (item == mBtnAdd)
		{
			LevelManager.pInstance._TeamSelection.AddUnit(mPetData);
		}
		else if (item == mBtnRemove)
		{
			LevelManager.pInstance._TeamSelection.RemoveUnit(mPetData);
		}
		if (mPetData.unitData._RaisedPetID > 0)
		{
			RefreshButtons(mPetData.unitData._RaisedPetID);
		}
		foreach (AbilityInfo abilitiesInfo in _AbilitiesInfoList)
		{
			if (abilitiesInfo._Widget == item)
			{
				mAbilityDetails.OnSetVisible(abilitiesInfo);
				break;
			}
		}
	}

	private void BlackSmithClosed()
	{
		KAUI.SetExclusive(this);
		LevelManager.pInstance._TeamSelection.SetVisibility(inVisible: true);
	}

	private void AvatarCreatorClosed()
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.SetAvatar(null);
			SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: false);
			SanctuaryManager.pCurPetInstance.SetPosition(new Vector3(0f, -1000f, 0f));
		}
		KAUI.SetExclusive(this);
		LevelManager.pInstance._TeamSelection.SetVisibility(inVisible: true);
	}

	public void ResetAbilitySelection()
	{
		foreach (AbilityInfo abilitiesInfo in _AbilitiesInfoList)
		{
			((KAToggleButton)abilitiesInfo._Widget).SetChecked(isChecked: false);
		}
	}

	private void RefreshButtons(int raisedPetID)
	{
		UnitSelectionType unitSelectionType = LevelManager.pInstance._TeamSelection.IsUnitInSquad(raisedPetID);
		mBtnAdd.SetVisibility(unitSelectionType == UnitSelectionType.MISSING);
		mBtnRemove.SetVisibility(unitSelectionType == UnitSelectionType.EXISTS || unitSelectionType == UnitSelectionType.LEVELDATA);
		mBtnRemove.SetDisabled(unitSelectionType == UnitSelectionType.LEVELDATA);
		LevelManager.pInstance._TeamSelection.RefreshButtons(raisedPetID);
	}

	public string GetRangeTextFromRange(float range)
	{
		if (_RangeTextMap == null || _RangeTextMap.Length == 0)
		{
			return string.Empty;
		}
		RangeTextMap[] rangeTextMap = _RangeTextMap;
		foreach (RangeTextMap rangeTextMap2 in rangeTextMap)
		{
			if (rangeTextMap2._Range.IsInRange(range))
			{
				return rangeTextMap2._RangeText.GetLocalizedString();
			}
		}
		return string.Empty;
	}

	private void OnAvatarCustomizationLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject gameObject = Object.Instantiate((GameObject)inObject);
			mUiAvatarCustomization = gameObject.GetComponentInChildren<UiAvatarCustomization>();
			mUiAvatarCustomization.pPrevPosition = new Vector3(0f, -5000f, 0f);
			mUiAvatarCustomization._CloseMsgObject = base.gameObject;
			mUiAvatarCustomization.pDefaultTabIndex = mUiAvatarCustomization.BattleReadyTabIndex;
			mUiAvatarCustomization.Initialize();
			mUiAvatarCustomization.ShowBackButton(show: true);
			KAUI.RemoveExclusive(this);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Failed to load Avatar Equipment....");
			break;
		}
	}
}
