using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class UiTeamSelection : KAUI
{
	private UiCharacterDetails mCharacterDetailsUI;

	private UiSquadSelectionMenu mSTSquadSelectionMenu;

	private UiPetSelectionMenu mSTPetSelectionMenu;

	private KAWidget mBtnBack;

	private KAWidget mBtnEnter;

	private KAWidget mBtnAdd;

	private KAWidget mBtnRemove;

	private KAWidget mBtnDetails;

	private KAWidget mTxtStageInfo;

	private KAWidget mTeamAllSet;

	private KAToggleButton mBtnName;

	private KAToggleButton mBtnSpecies;

	private KAToggleButton mBtnLevel;

	private KAToggleButton mBtnWeapon;

	private KAToggleButton mBtnElement;

	private List<UnitSelection> mSelectedSquad;

	[Header("UI")]
	public IconElementMap[] _IconElementMap;

	[Header("Age Restriction")]
	public RaisedPetStage _RequiredAge;

	[Header("Local Strings")]
	public LocaleString _TeamReadyText = new LocaleString("Team is all set!");

	public LocaleString _NoFreeSlotsText = new LocaleString("Remove another dragon to add this one!");

	public LocaleString _NoFreeSlotHeaderText = new LocaleString("No Slot");

	public LocaleString _NotAvailableText = new LocaleString("[REVIEW] - Pet not yet available!");

	public LocaleString _NotAvailableHeaderText = new LocaleString("[REVIEW] - Pet not yet available!");

	public LocaleString _PetNeedToAgeUpText = new LocaleString("[REVIEW] - This dragon isn't old enough to battle! Do you want to Age-up this dragon from the store.");

	public LocaleString _PetNeedToAgeUpHeaderText = new LocaleString("[REVIEW] - Adult Needed");

	public LocaleString _NonMemberText = new LocaleString("Become at least a 3 months member, and get NightFury to play this quest!");

	private KAUIGenericDB mGenericDB;

	private AvPhotoManager mStillPhotoManager;

	private SquadData mSelectedPet;

	private bool mIsCharacterDetailsOpen;

	public AvPhotoManager pStillPhotoManager => mStillPhotoManager;

	protected override void Start()
	{
		base.Start();
		mStillPhotoManager = AvPhotoManager.Init("PfSTPhotoMgr");
		mBtnBack = FindItem("BtnBack");
		mBtnEnter = FindItem("BtnEnter");
		mTxtStageInfo = FindItem("TxtStageInfo");
		mBtnAdd = FindItem("BtnAdd");
		mBtnRemove = FindItem("BtnRemove");
		mBtnDetails = FindItem("BtnDetails");
		mTeamAllSet = FindItem("TxtTeamAllSet");
		mBtnName = (KAToggleButton)FindItem("BtnName");
		mBtnSpecies = (KAToggleButton)FindItem("BtnSpecies");
		mBtnLevel = (KAToggleButton)FindItem("BtnLevel");
		mBtnWeapon = (KAToggleButton)FindItem("BtnRole");
		mBtnElement = (KAToggleButton)FindItem("BtnElement");
		mTeamAllSet.SetText(_TeamReadyText.GetLocalizedString());
		mSTSquadSelectionMenu = (UiSquadSelectionMenu)GetMenuByIndex(0);
		mSTPetSelectionMenu = (UiPetSelectionMenu)GetMenuByIndex(1);
		mCharacterDetailsUI = (UiCharacterDetails)_UiList[0];
		mBtnBack.SetDisabled(isDisabled: true);
		SetVisibility(inVisible: false);
	}

	private void DestroyDB()
	{
		if (mGenericDB != null)
		{
			Object.Destroy(mGenericDB.gameObject);
		}
	}

	public void OpenTeamSelectionTab()
	{
		if (RealmHolder.pInstance.pShowAgeUp && !UiDragonsAgeUp.pIsTicketPurchased)
		{
			OpenAgeUpUI();
			return;
		}
		mSelectedSquad = new List<UnitSelection>();
		mSTPetSelectionMenu.SetSelectedItem(null);
		mBtnDetails.SetVisibility(inVisible: false);
		mBtnAdd.SetVisibility(inVisible: false);
		mBtnRemove.SetVisibility(inVisible: false);
		mBtnName.SetChecked(isChecked: false);
		mBtnSpecies.SetChecked(isChecked: false);
		mBtnLevel.SetChecked(isChecked: false);
		mBtnWeapon.SetChecked(isChecked: false);
		mBtnElement.SetChecked(isChecked: false);
		RealmLevel selectedLevel = LevelManager.pInstance._LevelSelection.GetSelectedLevel();
		for (int i = 0; i < 4; i++)
		{
			UnitSelection unitSelection = new UnitSelection();
			unitSelection._UnitName = selectedLevel.Units[i]._Name;
			unitSelection._UnitLevel = selectedLevel.Units[i]._Level;
			unitSelection._Weapon = selectedLevel.Units[i]._Weapon;
			unitSelection._RaisedPetID = ((unitSelection._UnitName == "Open") ? (-1) : 0);
			unitSelection._IsLocked = unitSelection._RaisedPetID != -1;
			mSelectedSquad.Add(unitSelection);
		}
		mSelectedPet = null;
		InitializePetMenu();
		mTeamAllSet.SetVisibility(IsTeamReady());
		mTxtStageInfo.SetText(selectedLevel.StageInfo.GetLocalizedString());
		ElementCounter[] elementCounterData = Settings.pInstance.GetElementCounterData(selectedLevel.ElementName);
		if (elementCounterData.Length != _IconElementMap.Length)
		{
			return;
		}
		for (int j = 0; j < elementCounterData.Length; j++)
		{
			_IconElementMap[j]._Type = elementCounterData[j]._Element;
			if (_IconElementMap[j]._Icon != null)
			{
				_IconElementMap[j]._Icon.UpdateSprite(Settings.pInstance.GetElementInfo(elementCounterData[j]._Element)._Icon);
			}
			if (_IconElementMap[j]._Arrow != null)
			{
				_IconElementMap[j]._Arrow.color = Settings.pInstance.GetElementInfo(elementCounterData[j]._Element)._Color;
			}
		}
		InitializeSquadsMenu();
		SetVisibility(inVisible: true);
		if (mSTSquadSelectionMenu.GetItemAt(0) != null)
		{
			OnClick(mSTSquadSelectionMenu.GetItemAt(0));
		}
	}

	public void OpenAgeUpUI()
	{
		DestroyDB();
		mCharacterDetailsUI.CloseUI();
		RealmHolder.pInstance.pSaveLevel = true;
		RealmHolder.pInstance.pShowAgeUp = false;
		UiDragonsAgeUp.Init(null, closeOnAgeUp: false, base.gameObject, isTicketPurchased: false, RealmHolder.pInstance.gameObject, delegate
		{
			SetVisibility(inVisible: false);
		});
	}

	private void OnStoreOpened()
	{
		RealmHolder.pInstance.pShowAgeUp = true;
	}

	private void DoTriggerAction()
	{
		OpenTeamSelectionTab();
	}

	public void AddUnit(SquadData data)
	{
		RaisedPetData byID = RaisedPetData.GetByID(data.unitData._RaisedPetID);
		if (IsTeamReady())
		{
			mGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _NoFreeSlotsText.GetLocalizedString(), _NoFreeSlotHeaderText.GetLocalizedString(), base.gameObject, "", "", "DestroyDB", "", inDestroyOnClick: true);
		}
		else if (!data._IsAvailable)
		{
			mGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _NotAvailableText.GetLocalizedString(), _NotAvailableHeaderText.GetLocalizedString(), base.gameObject, "", "", "DestroyDB", "", inDestroyOnClick: true);
		}
		else if (TimedMissionManager.pInstance.IsPetEngaged(data.unitData._RaisedPetID))
		{
			FreeStableQuestDragon.pInstance.PurchaseStableQuest(data.unitData._RaisedPetID, InitializePetMenu);
		}
		else if (RaisedPetData.GetAgeIndex(byID.pStage) < RaisedPetData.GetAgeIndex(_RequiredAge))
		{
			mGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _PetNeedToAgeUpText.GetLocalizedString(), _PetNeedToAgeUpHeaderText.GetLocalizedString(), base.gameObject, "OpenAgeUpUI", "DestroyDB", "", "", inDestroyOnClick: true);
		}
		else if (!SanctuaryManager.IsActionAllowed(byID, PetActions.SQUADTACTICS))
		{
			UiPetEnergyGenericDB.Show(base.gameObject, "InitializePetMenu", "DestroyDB", isLowEnergy: true, byID);
		}
		else if (SanctuaryManager.IsPetLocked(byID))
		{
			SetCurrentSelectedPet();
			string localizedString = _NonMemberText.GetLocalizedString();
			localizedString = localizedString.Replace("{{Dragon}}", byID.Name);
			if (SubscriptionInfo.pIsMember && !IAPManager.IsMembershipUpgradeable())
			{
				localizedString = localizedString + "\n\n" + IAPManager.GetMembershipUpgradeText();
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", localizedString, null, base.gameObject, null, null, "OnDBClose", null, inDestroyOnClick: true);
			}
			else
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", localizedString, null, base.gameObject, "ShowMembershipPurchaseDB", "OnDBClose", null, null, inDestroyOnClick: true);
			}
		}
		else if (IsUnitInSquad(data.unitData._RaisedPetID) == UnitSelectionType.MISSING)
		{
			for (int i = 0; i < 4; i++)
			{
				if (!mSelectedSquad[i]._IsLocked && mSelectedSquad[i]._RaisedPetID == -1)
				{
					mSelectedSquad[i]._RaisedPetID = data.unitData._RaisedPetID;
					mSelectedSquad[i]._UnitName = SanctuaryData.FindSanctuaryPetTypeInfo(byID.PetTypeID)._Name;
					break;
				}
			}
		}
		RefreshSquadsMenu();
		RefreshPetMenu();
		SelectSquadUnit(data);
	}

	private void SetCurrentSelectedPet()
	{
		if (mSTPetSelectionMenu.GetSelectedItem() != null)
		{
			mSelectedPet = mSTPetSelectionMenu.GetSelectedItem().GetUserData() as SquadData;
		}
		else
		{
			mSelectedPet = null;
		}
	}

	private void OnDragonAgeUpDone()
	{
		if (mIsCharacterDetailsOpen)
		{
			RaisedPetData raisedPetData = RaisedPetData.GetActiveDragons().Find((RaisedPetData item) => mCharacterDetailsUI.GetCurrentPetID() == item.RaisedPetID);
			if (raisedPetData != null)
			{
				mCharacterDetailsUI.Reload(CharacterDatabase.pInstance.GetLevel(raisedPetData));
			}
		}
		InitializePetMenu();
		SetVisibility(inVisible: true);
		SetInteractive(interactive: true);
	}

	private void BuyGemsOnline()
	{
		DestroyDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void ShowMembershipPurchaseDB()
	{
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
	}

	private void OnIAPStoreClosed()
	{
		InitializePetMenu();
	}

	public void RemoveUnit(SquadData data)
	{
		for (int i = 0; i < 4; i++)
		{
			if (!mSelectedSquad[i]._IsLocked && mSelectedSquad[i]._RaisedPetID == data.unitData._RaisedPetID)
			{
				mSelectedSquad[i]._UnitName = "Open";
				mSelectedSquad[i]._RaisedPetID = -1;
				break;
			}
		}
		RefreshSquadsMenu();
		RefreshPetMenu();
		SelectSquadUnit(data);
	}

	private void InitializeSquadsMenu()
	{
		mSTSquadSelectionMenu.ClearItems();
		for (int i = 0; i < mSelectedSquad.Count; i++)
		{
			string unitName = mSelectedSquad[i]._UnitName;
			KAWidget kAWidget = mSTSquadSelectionMenu.AddWidget(unitName);
			kAWidget.SetVisibility(inVisible: true);
			kAWidget.FindChildItem("AniEquipLock").SetVisibility(unitName != "Open" && unitName != "Lock");
			kAWidget.FindChildItem("AniLock").SetVisibility(unitName == "Lock");
			SquadData userData = new SquadData(unitName, unitName == "Lock", mSelectedSquad[i]._RaisedPetID, mSelectedSquad[i]._UnitLevel, unitName != "Lock", mSelectedSquad[i]._Weapon);
			kAWidget.SetUserData(userData);
		}
		RefreshSquadsMenu(init: true);
		LevelManager.pInstance.SetSquad(mSelectedSquad);
	}

	public void ReLoadAvatar()
	{
		pStillPhotoManager.pPictureCache.Remove(UserInfo.pInstance.UserID);
		List<KAWidget> items = mSTSquadSelectionMenu.GetItems();
		for (int i = 0; i < items.Count; i++)
		{
			CharacterData character = CharacterDatabase.pInstance.GetCharacter(mSelectedSquad[i]._UnitName, mSelectedSquad[i]._RaisedPetID);
			KAToggleButton kAToggleButton = (KAToggleButton)items[i].FindChildItem("AniAvatarBkg");
			if (character != null && character.pIsAvatar())
			{
				AvPhotoSetter @object = new AvPhotoSetter(kAToggleButton);
				pStillPhotoManager.TakePhotoUI(UserInfo.pInstance.UserID, (Texture2D)kAToggleButton.GetTexture(), @object.PhotoCallback, null);
				break;
			}
		}
	}

	private void RefreshSquadsMenu(bool init = false)
	{
		List<KAWidget> items = mSTSquadSelectionMenu.GetItems();
		for (int i = 0; i < items.Count; i++)
		{
			KAWidget kAWidget = items[i];
			CharacterData character = CharacterDatabase.pInstance.GetCharacter(mSelectedSquad[i]._UnitName, mSelectedSquad[i]._RaisedPetID);
			SquadData squadData = new SquadData(mSelectedSquad[i]._UnitName, mSelectedSquad[i]._UnitName == "Lock", mSelectedSquad[i]._RaisedPetID, mSelectedSquad[i]._UnitLevel, character?._Available ?? false, mSelectedSquad[i]._Weapon);
			kAWidget.SetUserData(squadData);
			KAToggleButton kAToggleButton = (KAToggleButton)kAWidget.FindChildItem("AniDragonBkg");
			KAToggleButton kAToggleButton2 = (KAToggleButton)kAWidget.FindChildItem("AniAvatarBkg");
			if (character != null)
			{
				if (character.pIsAvatar())
				{
					AvPhotoSetter @object = new AvPhotoSetter(kAToggleButton2);
					pStillPhotoManager.TakePhotoUI(UserInfo.pInstance.UserID, (Texture2D)kAToggleButton2.GetTexture(), @object.PhotoCallback, null);
					kAToggleButton.SetVisibility(inVisible: false);
					kAToggleButton2.SetVisibility(inVisible: true);
				}
				else
				{
					RaisedPetData byID = RaisedPetData.GetByID(mSelectedSquad[i]._RaisedPetID);
					if (byID != null)
					{
						if (kAToggleButton.GetTexture() == null)
						{
							int slotIdx = (byID.ImagePosition.HasValue ? byID.ImagePosition.Value : 0);
							ImageData.Load("EggColor", slotIdx, base.gameObject);
						}
					}
					else
					{
						kAToggleButton.SetTextureFromBundle(character._PortraitIcon);
					}
					kAToggleButton.SetVisibility(inVisible: true);
					kAToggleButton2.SetVisibility(inVisible: false);
				}
				if (!string.IsNullOrEmpty(squadData.unitData._Weapon))
				{
					character._WeaponData = WeaponDatabase.pInstance.GetWeaponData(squadData.unitData._Weapon);
				}
				KAWidget kAWidget2 = kAWidget.FindChildItem("AniSkill1");
				kAWidget2.SetSprite(WeaponDatabase.pInstance.GetWeaponTypeSprite(character._WeaponData._WeaponType));
				kAWidget2.SetVisibility(inVisible: true);
				KAWidget kAWidget3 = kAWidget.FindChildItem("AniSkill2");
				kAWidget3.SetSprite(WeaponDatabase.pInstance.GetElementSprite(character._WeaponData._ElementType));
				kAWidget3.SetVisibility(inVisible: true);
			}
			else
			{
				kAToggleButton2.SetVisibility(inVisible: false);
				kAToggleButton.SetTexture(null);
				kAWidget.FindChildItem("AniSkill1").SetVisibility(inVisible: false);
				kAWidget.FindChildItem("AniSkill2").SetVisibility(inVisible: false);
			}
			if (init)
			{
				kAToggleButton2.SetChecked(isChecked: false);
				kAToggleButton.SetChecked(isChecked: false);
			}
		}
		LevelManager.pInstance.SetSquad(mSelectedSquad);
		ReadyButton();
	}

	public void ReadyButton()
	{
		if (LevelManager.pInstance.pPrefetchDone)
		{
			mBtnEnter.SetDisabled(!IsTeamReady());
		}
		else
		{
			mBtnEnter.SetDisabled(isDisabled: true);
		}
	}

	public void OnImageLoaded(ImageDataInstance img)
	{
		if (img.mIconTexture == null)
		{
			return;
		}
		foreach (KAWidget item in mSTSquadSelectionMenu.GetItems())
		{
			RaisedPetData byID = RaisedPetData.GetByID(((SquadData)item.GetUserData()).unitData._RaisedPetID);
			if (byID != null && (byID.ImagePosition.HasValue ? byID.ImagePosition.Value : 0) == img.mSlotIndex)
			{
				item.FindChildItem("AniDragonBkg").SetTexture(img.mIconTexture);
				break;
			}
		}
	}

	public UnitSelectionType IsUnitInSquad(int raisedPetID)
	{
		switch (raisedPetID)
		{
		case 0:
			return UnitSelectionType.LEVELDATA;
		case -1:
			return UnitSelectionType.NONE;
		default:
			foreach (UnitSelection item in mSelectedSquad)
			{
				if (item._RaisedPetID == raisedPetID && item._UnitName != "Open" && item._UnitName != "Lock")
				{
					return UnitSelectionType.EXISTS;
				}
			}
			return UnitSelectionType.MISSING;
		}
	}

	private void OnTextureLoaded(KAWidget inWidget)
	{
		inWidget.SetVisibility(inVisible: true);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mBtnEnter)
		{
			LevelManager.pInstance._LevelSelection.LoadLevel();
		}
		else if (item == mBtnBack)
		{
			LevelManager.pInstance._LevelSelection.SetVisibility(inVisible: true);
			SetVisibility(inVisible: false);
		}
		else if (mBtnName == item)
		{
			((PetMenuGrid)mSTPetSelectionMenu._DefaultGrid).SortPet(SortOrder.NAME);
		}
		else if (mBtnSpecies == item)
		{
			((PetMenuGrid)mSTPetSelectionMenu._DefaultGrid).SortPet(SortOrder.SPECIES);
		}
		else if (mBtnLevel == item)
		{
			((PetMenuGrid)mSTPetSelectionMenu._DefaultGrid).SortPet(SortOrder.LEVEL);
		}
		else if (mBtnWeapon == item)
		{
			((PetMenuGrid)mSTPetSelectionMenu._DefaultGrid).SortPet(SortOrder.WEAPON);
		}
		else if (mBtnElement == item)
		{
			((PetMenuGrid)mSTPetSelectionMenu._DefaultGrid).SortPet(SortOrder.ELEMENT);
		}
		KAWidget selectedItem = mSTPetSelectionMenu.GetSelectedItem();
		if (selectedItem != null)
		{
			SquadData squadData = (SquadData)selectedItem.GetUserData();
			if (squadData != null)
			{
				if (item == mBtnAdd)
				{
					AddUnit(squadData);
				}
				else if (item == mBtnDetails)
				{
					if (!squadData._IsAvailable)
					{
						mGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _NotAvailableText.GetLocalizedString(), _NotAvailableHeaderText.GetLocalizedString(), base.gameObject, "", "", "DestroyDB", "", inDestroyOnClick: true);
					}
					else
					{
						mCharacterDetailsUI.LoadDetails(squadData);
					}
				}
				else if (item == mBtnRemove)
				{
					RemoveUnit(squadData);
				}
				else
				{
					KAToggleButton kAToggleButton = (KAToggleButton)selectedItem.FindChildItem("BtnCheckBox");
					if (kAToggleButton != null && item == kAToggleButton)
					{
						if (!kAToggleButton.IsChecked())
						{
							RemoveUnit(squadData);
						}
						else
						{
							AddUnit(squadData);
						}
					}
				}
				RefreshButtons(squadData.unitData._RaisedPetID);
			}
		}
		else if (item == mBtnDetails)
		{
			foreach (KAWidget item2 in mSTSquadSelectionMenu.GetItems())
			{
				SquadData squadData2 = item2.GetUserData() as SquadData;
				if (squadData2.unitData._UnitName == "Avatar")
				{
					if (((KAToggleButton)item2.FindChildItem("AniAvatarBkg")).IsChecked())
					{
						mCharacterDetailsUI.LoadDetails(squadData2);
						break;
					}
				}
				else if (((KAToggleButton)item2.FindChildItem("AniDragonBkg")).IsChecked())
				{
					mCharacterDetailsUI.LoadDetails(squadData2);
					break;
				}
			}
		}
		if (mSTSquadSelectionMenu.GetItems().Contains(item) || mSTPetSelectionMenu.GetItems().Contains(item))
		{
			SelectSquadUnit((SquadData)item.GetUserData());
		}
	}

	public void RefreshButtons(int raisedPetID)
	{
		mBtnAdd.SetVisibility(IsUnitInSquad(raisedPetID) == UnitSelectionType.MISSING);
		mBtnRemove.SetVisibility(IsUnitInSquad(raisedPetID) == UnitSelectionType.EXISTS);
	}

	private void SelectSquadUnit(SquadData petData)
	{
		if (petData.unitData._RaisedPetID == -1 || petData.unitData._UnitName == "Lock")
		{
			return;
		}
		mBtnDetails.SetVisibility(inVisible: true);
		foreach (KAWidget item in mSTSquadSelectionMenu.GetItems())
		{
			SquadData squadData = item.GetUserData() as SquadData;
			KAToggleButton kAToggleButton = (KAToggleButton)item.FindChildItem("AniAvatarBkg");
			KAToggleButton kAToggleButton2 = (KAToggleButton)item.FindChildItem("AniDragonBkg");
			if (squadData == petData || (petData.unitData._RaisedPetID != 0 && petData.unitData._RaisedPetID != -1 && petData.unitData._RaisedPetID == squadData.unitData._RaisedPetID))
			{
				kAToggleButton2.SetChecked(squadData.unitData._UnitName != "Avatar");
				kAToggleButton.SetChecked(squadData.unitData._UnitName == "Avatar");
			}
			else
			{
				kAToggleButton2.SetChecked(isChecked: false);
				kAToggleButton.SetChecked(isChecked: false);
			}
		}
		mSTPetSelectionMenu.SetSelectedItem(null);
		List<KAWidget> items = mSTPetSelectionMenu.GetItems();
		for (int i = 0; i < items.Count; i++)
		{
			if ((items[i].GetUserData() as SquadData).unitData._RaisedPetID == petData.unitData._RaisedPetID)
			{
				mSTPetSelectionMenu.SetSelectedItem(items[i]);
				break;
			}
		}
		RefreshButtons(petData.unitData._RaisedPetID);
	}

	private bool IsTeamReady()
	{
		if (mSelectedSquad != null)
		{
			foreach (UnitSelection item in mSelectedSquad)
			{
				if (item._UnitName == "Open")
				{
					return false;
				}
			}
		}
		return true;
	}

	private void RefreshPetMenu()
	{
		List<KAWidget> items = mSTPetSelectionMenu.GetItems();
		for (int i = 0; i < items.Count; i++)
		{
			SquadData squadData = (SquadData)items[i].GetUserData();
			KAToggleButton obj = (KAToggleButton)items[i].FindChildItem("BtnCheckBox");
			KAWidget kAWidget = items[i].FindChildItem("AniCheckUnavailable");
			KAWidget kAWidget2 = items[i].FindChildItem("Flash");
			bool flag = IsUnitInSquad(squadData.unitData._RaisedPetID) == UnitSelectionType.EXISTS || IsUnitInSquad(squadData.unitData._RaisedPetID) == UnitSelectionType.LEVELDATA;
			obj.SetChecked(flag);
			if (!flag && !kAWidget.GetVisibility() && !IsTeamReady())
			{
				kAWidget2.PlayAnim("Flash");
			}
			else
			{
				kAWidget2.PlayAnim("Normal");
			}
		}
	}

	private void InitializePetMenu()
	{
		mSTPetSelectionMenu.ClearItems();
		if (!IsTeamReady())
		{
			foreach (RaisedPetData activeDragon in RaisedPetData.GetActiveDragons())
			{
				if (StableData.GetByPetID(activeDragon.RaisedPetID) == null)
				{
					continue;
				}
				SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(activeDragon.PetTypeID);
				CharacterData character = CharacterDatabase.pInstance.GetCharacter(sanctuaryPetTypeInfo._Name, activeDragon);
				if (character != null)
				{
					KAWidget kAWidget = mSTPetSelectionMenu.AddWidget(character._NameID);
					SquadData squadData = new SquadData(sanctuaryPetTypeInfo._Name, locked: false, activeDragon.RaisedPetID, CharacterDatabase.pInstance.GetLevel(activeDragon), character._Available, "");
					kAWidget.SetUserData(squadData);
					kAWidget.name = character._NameID;
					kAWidget.FindChildItem("TxtSpecies").SetText(character._DisplayNameText.GetLocalizedString());
					kAWidget.FindChildItem("TxtName").SetText(activeDragon.Name);
					kAWidget.FindChildItem("TxtLevel").SetText((squadData.unitData._UnitLevel > 0) ? squadData.unitData._UnitLevel.ToString() : "");
					kAWidget.FindChildItem("AniRole").SetSprite(WeaponDatabase.pInstance.GetWeaponTypeSprite(character._WeaponData._WeaponType));
					kAWidget.FindChildItem("AniElement").SetSprite(WeaponDatabase.pInstance.GetElementSprite(character._WeaponData._ElementType));
					bool flag = TimedMissionManager.pInstance.IsPetEngaged(activeDragon.RaisedPetID) || RaisedPetData.GetAgeIndex(activeDragon.pStage) < RaisedPetData.GetAgeIndex(_RequiredAge) || !SanctuaryManager.IsActionAllowed(RaisedPetData.GetByID(activeDragon.RaisedPetID), PetActions.SQUADTACTICS) || SanctuaryManager.IsPetLocked(activeDragon) || !character._Available;
					kAWidget.FindChildItem("AniCheckUnavailable").SetVisibility(flag);
					KAToggleButton obj = (KAToggleButton)kAWidget.FindChildItem("BtnCheckBox");
					obj.SetVisibility(!flag);
					bool @checked = IsUnitInSquad(activeDragon.RaisedPetID) == UnitSelectionType.EXISTS || IsUnitInSquad(activeDragon.RaisedPetID) == UnitSelectionType.LEVELDATA;
					obj.SetChecked(@checked);
					if (mSelectedPet != null && mSelectedPet.unitData._RaisedPetID == activeDragon.RaisedPetID)
					{
						mSTPetSelectionMenu.SetSelectedItem(kAWidget);
					}
				}
			}
		}
		RefreshPetMenu();
	}

	protected override void Update()
	{
		base.Update();
		if (mBtnBack.GetState() == KAUIState.DISABLED && LevelManager.pInstance.pPrefetchDone)
		{
			mBtnBack.SetDisabled(isDisabled: false);
		}
		else if (mBtnBack.GetState() != KAUIState.DISABLED && !LevelManager.pInstance.pPrefetchDone)
		{
			mBtnBack.SetDisabled(isDisabled: true);
		}
	}
}
