using System;

public class NestCSM : PetCSM
{
	public static SanctuaryPet SanctuaryPetInstance;

	private SanctuaryPet mCurrentPet;

	private ObClickableNest mMyNest;

	protected override void Start()
	{
		base.Start();
		mMyNest = GetComponent<ObClickableNest>();
	}

	public void SetCurrentPet(SanctuaryPet pet)
	{
		mCurrentPet = pet;
	}

	protected override void OnMenuActive(ContextSensitiveStateType inMenuType)
	{
		base.OnMenuActive(inMenuType);
		if (base.pUI != null)
		{
			KAUI.RemoveExclusive(base.pUI);
		}
	}

	public override void OnContextAction(string inName)
	{
		mCurrentPet = mMyNest.pCurrPet;
		if (inName == "Store")
		{
			ObClickableNest obClickableNest = mMyNest;
			obClickableNest.pActionOnClose = (Action)Delegate.Combine(obClickableNest.pActionOnClose, (Action)delegate
			{
				if (_StoreInfo != null)
				{
					StoreLoader.Load(setDefaultMenuItem: true, _StoreInfo._Category, _StoreInfo._Store, base.gameObject);
				}
			});
			Close(closeStableUi: true);
		}
		else if (inName == "MoveIn")
		{
			mMyNest.OnPetMoveIn();
			Close();
		}
		else if (inName != _FeedInventoryCSItemName)
		{
			float maxMeter = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.ENERGY, mCurrentPet.pData);
			if (mCurrentPet.GetMeterValue(SanctuaryPetMeterType.ENERGY) >= maxMeter)
			{
				AvAvatar.SetUIActive(inActive: false);
				AvAvatar.pState = AvAvatarState.PAUSED;
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _FullEnergyText.GetLocalizedString(), base.gameObject, "OnDBClose");
			}
			else
			{
				RemoveItemFromFishInventory(inName);
			}
		}
	}

	protected override void OnDBClose()
	{
	}

	protected override void OnInventoryDataSave(bool success, object inUserData)
	{
		if (success)
		{
			UserItemData userItemData = (UserItemData)inUserData;
			mCurrentPet.DoEat(userItemData.Item);
			if (userItemData.Quantity == 0)
			{
				RemoveDataFromStateAndDataList(_FeedInventoryCSItemName, userItemData.Item.ItemName);
			}
			UserAchievementTask.Set(_EatFishAchievementTaskID);
			if (IsFeedTypeFish(userItemData.Item.ItemName))
			{
				RaisedPetAttribute raisedPetAttribute = mCurrentPet.pData.FindAttrData("fish");
				int num = 1;
				if (raisedPetAttribute != null)
				{
					num = int.Parse(raisedPetAttribute.Value) + num;
				}
				mCurrentPet.pData.SetAttrData("fish", num.ToString(), DataType.INT);
				mCurrentPet.pData.SaveDataReal(null, null, savePetMeterAlone: true);
			}
			mCurrentPet.AIActor.PlayCustomAnim(_FeedAnim);
			UpdateFeedItems();
			if (base.pUI != null)
			{
				base.pUI.RefreshChildItems("Feed");
			}
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		Close(closeStableUi: true);
	}

	public void Close(bool closeStableUi = false)
	{
		DestroyMenu(checkProximity: true);
		if (closeStableUi)
		{
			mMyNest.OnCSMClosed();
		}
	}

	protected override void ProcessForClickOutside()
	{
		if (!mMyNest.HasDragon())
		{
			base.ProcessForClickOutside();
		}
	}
}
