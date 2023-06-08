using System.Collections.Generic;
using UnityEngine;

public class FishTrapFarmItem : FarmItem
{
	public int _ItemID = 8771;

	public int _FishTrapWorkingStageID = 77;

	public int _FishTrapFinishedStageID = 78;

	public Vector3 _FishTrapPosition;

	public Vector3 _FishTrapRotation;

	public string _RoomID = "StaticFarmItems";

	public string _FishTrapFeedState = "FishWrapFeed";

	public string _DialogAssetName = "PfKAUILabActivityDB";

	public string _GameModuleName = "Fishing";

	public string _GameResult = "Win";

	public List<ItemStateCriteriaConsumable> _FishTrapConsumables;

	public LocaleString _FishTrapRewardText = new LocaleString("You are awarded with  %reward_amount% %reward_item% ");

	public LocaleString _BecomeAMemberText = new LocaleString("This is a Member Only feature. Proceed to become a Member?");

	public LocaleString _FeedInsufficientText = new LocaleString("You need %reqd_amount% %reqd_item% to get the compostor started, you only have %available_amount%");

	public LocaleString _FeedUnavailableText = new LocaleString("You need %reqd_amount% %reqd_item% to get the compostor started, you have none");

	public LocaleString _BuyFishTrapFeedText = new LocaleString("Buy");

	public AudioClip _FishTrapWorkingSFX;

	public AudioClip _FishTrapCompleteSFX;

	public UiDragonsEndDB _EndDBUI;

	public StoreLoader.Selection _StoreInfo;

	private bool mIsHarvestParamsSet;

	private bool mItemsAdded;

	private Dictionary<string, int> mContextDataName_InventoryID_Map = new Dictionary<string, int>();

	private ItemStateCriteriaConsumable mCurrentUsedConsumableCriteria;

	private ItemData mConsumableItemData;

	private KAUIGenericDB mKAUIGenericDB;

	private UserItemPosition mUserItemPosition;

	private ObStatus mObStatus;

	private UserItemData mFishTrapUserItemData;

	protected override void Start()
	{
		base.Start();
		mObStatus = GetComponent<ObStatus>();
	}

	protected override void Update()
	{
		if (base.pFarmManager == null)
		{
			base.pFarmManager = (FarmManager)MyRoomsIntMain.pInstance;
		}
		if (base.pFarmManager.pDefaultItemsChecked && !mItemsAdded)
		{
			UserItemPositionList.Init(MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt() ? MyRoomsIntLevel.pInstance.GetMyRoomsIntHostID() : UserInfo.pInstance.UserID, _RoomID, OnNewUserItemPositionEvent);
			mItemsAdded = true;
		}
		base.Update();
	}

	protected override UserItemPosition GetUserItemPosition()
	{
		return mUserItemPosition;
	}

	public void AwardDefaultItemsEventHandler(bool inSaveSuccess, object inUserData)
	{
		if (inSaveSuccess)
		{
			Save();
		}
		else
		{
			mItemsAdded = false;
		}
	}

	public void OnNewUserItemPositionEvent(WsServiceType inType, WsServiceEvent inEvent, string inRoomID)
	{
		if (inEvent != WsServiceEvent.COMPLETE)
		{
			return;
		}
		switch (inType)
		{
		case WsServiceType.SET_USER_ITEM_POSITION_LIST:
			SetUpFishTrap();
			break;
		case WsServiceType.GET_USER_ITEM_POSITION_LIST:
		{
			UserItemPosition[] list = UserItemPositionList.GetList(_RoomID);
			if (list == null || list.Length == 0)
			{
				if (MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
				{
					if (mObStatus != null)
					{
						mObStatus.pIsReady = true;
					}
					DisableFishTrapRoomItem();
					break;
				}
				mFishTrapUserItemData = CommonInventoryData.pInstance.FindItem(_ItemID);
				if (mFishTrapUserItemData == null)
				{
					CommonInventoryData.pInstance.AddItem(_ItemID, updateServer: false);
					CommonInventoryData.pInstance.Save(AwardDefaultItemsEventHandler, null);
				}
				else
				{
					Save();
				}
				break;
			}
			if (!MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
			{
				SetUpFishTrap();
				break;
			}
			if (mObStatus != null)
			{
				mObStatus.pIsReady = true;
			}
			ObClickable component = base.gameObject.GetComponent<ObClickable>();
			if (component != null)
			{
				component._Active = false;
				if (component._HighlightMaterial == null)
				{
					component._HighlightMaterial = MyRoomsIntMain.pInstance._HighlightMaterial;
				}
			}
			base.enabled = false;
			break;
		}
		}
	}

	public void Save()
	{
		mFishTrapUserItemData = CommonInventoryData.pInstance.FindItem(_ItemID);
		if (mFishTrapUserItemData != null)
		{
			UserItemPositionList.CreateObject(_RoomID, base.gameObject, mFishTrapUserItemData, _FishTrapPosition, _FishTrapRotation, base.gameObject);
			UserItemPositionList.Save(_RoomID, OnNewUserItemPositionEvent);
			CommonInventoryData.pInstance.RemoveItem(mFishTrapUserItemData, 1);
		}
	}

	public void SetUpFishTrap()
	{
		if (!(mObStatus != null))
		{
			return;
		}
		UserItemPosition[] list = UserItemPositionList.GetList(_RoomID);
		for (int i = 0; i < list.Length; i++)
		{
			UserItemPosition userItemPosition = (mUserItemPosition = list[i]);
			if (mFishTrapUserItemData == null)
			{
				mFishTrapUserItemData = base.pFarmManager.GetRoomData().CreateUserItemData(userItemPosition);
			}
			base.pFarmManager.AddFarmItem(base.gameObject, mFishTrapUserItemData);
			SetState(userItemPosition.UserItemState);
			mObStatus.pIsReady = true;
		}
		if (base.pCurrentStage != null)
		{
			PlaySFX(base.pCurrentStage._ID == _FishTrapWorkingStageID, _FishTrapWorkingSFX, inLoop: true);
		}
	}

	protected override void SetNextItemStateEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		base.SetNextItemStateEventHandler(inType, inEvent, inProgress, inObject, inUserData);
		if (inEvent == WsServiceEvent.COMPLETE)
		{
			if (base.pCurrentStage._ID == _FishTrapWorkingStageID)
			{
				PlaySFX(play: true, _FishTrapWorkingSFX, inLoop: true);
			}
			else if (base.pCurrentStage._ID == _FishTrapFinishedStageID)
			{
				PlaySFX(play: true, _FishTrapCompleteSFX);
			}
			else
			{
				PlaySFX(play: false);
			}
		}
	}

	private void PlaySFX(bool play, AudioClip inAudio = null, bool inLoop = false)
	{
		if (play)
		{
			if (!_AmbientSFXChannel.pIsPlaying || _AmbientSFXChannel.pClip != inAudio)
			{
				_AmbientSFXChannel.pClip = inAudio;
				_AmbientSFXChannel.pLoop = inLoop;
				_AmbientSFXChannel.Play();
			}
		}
		else if (_AmbientSFXChannel.pIsPlaying)
		{
			_AmbientSFXChannel.Stop();
		}
	}

	public override void ProcessCurrentStage()
	{
		if (base.pFarmManager == null || !base.pFarmManager.pIsReady || !mInitialized || base.pFarmManager.pIsBuildMode || !base.pIsStateSet || !base.pIsRuleSetInitialized)
		{
			return;
		}
		base.ProcessCurrentStage();
		if (!(GetTimeLeftInCurrentStage() > 0f))
		{
			if (base.pCurrentStage != null && !mIsHarvestParamsSet && base.pCurrentStage._Name.Equals(GetHarvestStageName()))
			{
				mIsHarvestParamsSet = true;
				UpdateItemWithStage(base.pCurrentStage);
				UpdateContextIcon();
			}
			if (base.pCurrentStage._Name.Equals(_FishTrapFeedState))
			{
				SetBaitQuantity();
			}
		}
	}

	public override void UpdateContextIcon()
	{
		if (base.pIsBuildMode && base.pClickable != null)
		{
			base.pClickable._RollOverCursorName = "Activate";
		}
	}

	protected override void OnContextAction(string inActionName)
	{
		base.OnContextAction(inActionName);
		switch (inActionName)
		{
		case "Store":
			if (AvAvatar.pToolbar != null && _StoreInfo != null)
			{
				StoreLoader.Load(setDefaultMenuItem: true, _StoreInfo._Category, _StoreInfo._Category, AvAvatar.pToolbar);
			}
			return;
		case "FT Harvest":
			GotoNextStage();
			return;
		case "FTBoost":
			if (CheckGemsAvailable(GetSpeedupCost()))
			{
				GotoNextStage(speedup: true);
			}
			return;
		}
		foreach (ItemStateCriteriaConsumable fishTrapConsumable in _FishTrapConsumables)
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(fishTrapConsumable.ItemID);
			if (userItemData == null)
			{
				ItemData itemData = base.pFarmManager.GetItemData(fishTrapConsumable.ItemID);
				if (itemData != null)
				{
					string localizedString = _FeedUnavailableText.GetLocalizedString();
					localizedString = localizedString.Replace("%reqd_amount%", fishTrapConsumable.Amount.ToString());
					localizedString = localizedString.Replace("%reqd_item%", itemData.ItemName);
					ShowDialog(_DialogAssetName, localizedString, base.pFarmManager._FarmingDBTitleText.GetLocalizedString(), string.Empty, string.Empty, "BuyFishTrapFeed", "KillGenericDB", destroyDB: true, base.gameObject);
					mKAUIGenericDB.FindItem("OKBtn").SetText(_BuyFishTrapFeedText.GetLocalizedString());
				}
			}
			if (userItemData != null)
			{
				if (fishTrapConsumable.Amount <= userItemData.Quantity)
				{
					mCurrentUsedConsumableCriteria = fishTrapConsumable;
					GotoNextStage();
					break;
				}
				string localizedString2 = _FeedInsufficientText.GetLocalizedString();
				localizedString2 = localizedString2.Replace("%reqd_amount%", fishTrapConsumable.Amount.ToString());
				localizedString2 = localizedString2.Replace("%reqd_item%", userItemData.Item.ItemName);
				localizedString2 = localizedString2.Replace("%available_amount%", userItemData.Quantity.ToString());
				ShowDialog(_DialogAssetName, localizedString2, base.pFarmManager._FarmingDBTitleText.GetLocalizedString(), string.Empty, string.Empty, "BuyFishTrapFeed", "KillGenericDB", destroyDB: true, base.gameObject);
				mKAUIGenericDB.FindItem("OKBtn").SetText(_BuyFishTrapFeedText.GetLocalizedString());
				break;
			}
		}
	}

	protected override void OnActivate()
	{
		if (!SubscriptionInfo.pIsMember)
		{
			ShowDialog("PfKAUIGenericDB", _BecomeAMemberText.GetLocalizedString(), base.pFarmManager._FarmingDBTitleText.GetLocalizedString(), "BecomeAMemeber", "KillGenericDB", string.Empty, string.Empty, destroyDB: true, base.gameObject);
		}
		else
		{
			base.OnActivate();
		}
	}

	protected override bool CanProcessUpdateData()
	{
		return CanActivate();
	}

	protected override void ProcessSensitiveData(ref List<string> menuItemNames)
	{
		if (!base.pIsBuildMode)
		{
			if (menuItemNames.Contains("Store"))
			{
				menuItemNames.Remove("Store");
			}
			UpdateFishTrapMenu(ref menuItemNames);
		}
	}

	private void UpdateFishTrapMenu(ref List<string> inNames)
	{
		if (base.pCurrentStage == null || _StateDetails == null || !(base.pFarmManager != null) || _StateDetails.Count <= 0 || _StateDetails[0]._ID != base.pCurrentStage._ID)
		{
			return;
		}
		ContextData contextData = GetContextData(_FishTrapFeedState);
		if (contextData == null || _FishTrapConsumables == null)
		{
			return;
		}
		foreach (ItemStateCriteriaConsumable fishTrapConsumable in _FishTrapConsumables)
		{
			ItemData itemData = base.pFarmManager.GetItemData(fishTrapConsumable.ItemID);
			if (itemData != null)
			{
				inNames.Add(itemData.ItemName);
				AddChildContextDataToParent(contextData, itemData, inShowInventoryCount: false, itemData.IconName);
				mContextDataName_InventoryID_Map[itemData.ItemName] = itemData.ItemID;
				mConsumableItemData = itemData;
			}
		}
	}

	private void SetBaitQuantity()
	{
		if (!(base.pUI != null) || mConsumableItemData == null)
		{
			return;
		}
		KAWidget kAWidget = base.pUI.FindItem(mConsumableItemData.ItemName);
		if (kAWidget != null)
		{
			UILabel uILabel = kAWidget.FindChildNGUIItem("Text") as UILabel;
			if (uILabel != null)
			{
				uILabel.text = "0";
				int amount = _FishTrapConsumables[0].Amount;
				uILabel.text = amount.ToString();
			}
		}
	}

	protected override string GetHarvestStageName()
	{
		return "Harvest";
	}

	public override void ParseConsumablesInventoryData(OnParseConsumableInventoryData parseDelegate)
	{
		base.ParseConsumablesInventoryData(parseDelegate);
		if (mCurrentUsedConsumableCriteria != null)
		{
			CommonInventoryData.pInstance.RemoveItem(mCurrentUsedConsumableCriteria.ItemID, updateServer: true, mCurrentUsedConsumableCriteria.Amount);
			mCurrentUsedConsumableCriteria = null;
		}
	}

	protected override bool CanDestroyOnHarvest()
	{
		return false;
	}

	protected override void SpawnRewards(AchievementReward[] inRewards)
	{
		_EndDBUI.SetVisibility(Visibility: true);
		_EndDBUI.SetGameSettings(_GameModuleName, base.gameObject, _GameResult, inShowHighScore: false);
		_EndDBUI.GetComponentInChildren<UiFishTrapRewards>().DisplayRewards(inRewards, base.pFarmManager);
	}

	private void OnEndDBClose()
	{
		if (AvAvatar.pToolbar != null)
		{
			UiToolbar component = AvAvatar.pToolbar.GetComponent<UiToolbar>();
			if (component != null)
			{
				component.OnUpdateRank();
			}
		}
	}

	private void BecomeAMemeber()
	{
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
	}

	private void KillGenericDB()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
	}

	protected override void OnIAPStoreClosed()
	{
		KillGenericDB();
	}

	private void BuyFishTrapFeed()
	{
		if (AvAvatar.pToolbar != null && _StoreInfo != null)
		{
			StoreLoader.Load(setDefaultMenuItem: true, _StoreInfo._Category, _StoreInfo._Store, AvAvatar.pToolbar);
		}
	}

	public void ShowDialog(string assetName, string text, string title, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, GameObject messageObject)
	{
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB.Destroy();
		}
		mKAUIGenericDB = GameUtilities.DisplayGenericDB(assetName, text, title, messageObject, yesMessage, noMessage, okMessage, closeMessage, destroyDB);
		if (mKAUIGenericDB != null)
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
		}
	}

	public void DisableFishTrapRoomItem()
	{
		ObClickable component = base.gameObject.GetComponent<ObClickable>();
		if (component != null)
		{
			component._Active = false;
			if (component._HighlightMaterial == null)
			{
				component._HighlightMaterial = MyRoomsIntMain.pInstance._HighlightMaterial;
			}
		}
		base.enabled = false;
	}
}
