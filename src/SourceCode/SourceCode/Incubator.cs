using System;
using System.Collections.Generic;
using UnityEngine;

public class Incubator : KAMonoBase, IAdResult
{
	public enum IncubatorStates
	{
		WAITING_FOR_EGG,
		IDLE,
		HATCHING,
		HATCHED,
		LOCKED
	}

	public int _DragonEggsCategory = 456;

	public Vector3 _EggOffset = Vector3.zero;

	public string _DragonCustomizationAsset = "RS_DATA/PfUiDragonCustomizationDO.unity3d/PfUiDragonCustomizationDO";

	public ContextSensitiveState[] _HatchingTutorialStateMenuItems;

	public ContextSensitiveState[] _WaitingStateMenuItems;

	public ContextSensitiveState[] _HatchingStateMenuItems;

	public ContextSensitiveState[] _HatchedStateMenuItems;

	public HatcheryManager _HatcheryManager;

	public ObProximityStableHatch _ObProximityStableHatch;

	public ObProximityHatch _ObProximityHatch;

	public StoreLoader.Selection _StoreInfo;

	public LocaleString _NotEnoughVCashText = new LocaleString("You don't have enough Gems to customize your dragon. Do you want to buy more Gems?");

	public LocaleString _UseGemsForCustomizeText = new LocaleString("Speed Up Timer will cost you {cost} Gems. Do you want to continue?");

	public LocaleString _OfflineText = new LocaleString("You need to be connected to the Internet.");

	public LocaleString _StoreUnavailableText = new LocaleString("The store is unavailable at this time. Please try again later.");

	public LocaleString _TicketPurchaseFailedText = new LocaleString("Transaction failed. Please try again.");

	public LocaleString _TicketPurchaseProcessingText = new LocaleString("Processing purchase...");

	public LocaleString _HatchWarningText = new LocaleString("Hatching time is tampared. Please launch again.");

	public Color _SlotHighlightColor = new Color(0.5f, 0.215f, 0.098f, 1f);

	public AdEventType _AdEventType;

	private KAUIGenericDB mKAUIGenericDB;

	private int mSpeedUpTimerTicketStoreID = 93;

	private int mSpeedUpTimerTicketItemID;

	private int mSpeedUpTimerTicketCost = -1;

	private float mHatchDuration = -1f;

	private UserItemData mCurrentUserItemData;

	private IncubatorStates mMyState;

	private RaisedPetData mMyPetData;

	private List<StoreData> mStoreDataList = new List<StoreData>();

	private KAWidget mAdBtn;

	[NonSerialized]
	public GameObject mPickedUpEgg;

	[NonSerialized]
	public int mPetType = -1;

	[SerializeField]
	private int mID;

	public GameObject _HatchedParticle;

	private GameObject mHatchedEffect;

	private string mAdWatchedKey = "AdWatched";

	private Action<bool, Incubator> OnEggPlaced;

	private Action<bool, Incubator> OnSpeedUpDone;

	public SanctuaryPet mIncubatorPet;

	[SerializeField]
	private Vector3 m_DragonPositionOffset;

	private bool mSkipEggSpawn;

	public int pSpeedUpTimerTicketItemID => mSpeedUpTimerTicketItemID;

	public int pSpeedUpTimerTicketCost => mSpeedUpTimerTicketCost;

	public IncubatorStates pMyState => mMyState;

	public RaisedPetData pMyPetData
	{
		get
		{
			return mMyPetData;
		}
		set
		{
			mMyPetData = value;
		}
	}

	public int pID => mID;

	public ItemData pLinkedItemData { get; private set; }

	public bool pReadyToBuy { get; private set; }

	public bool pInitPetOnProximityStableHatch { get; set; }

	public Vector3 pDragonPositionOffset => m_DragonPositionOffset;

	public void Setup(int unlockCount, bool skipEggSpawn = false)
	{
		if ((object)Array.Find(ServerTime.OnServerTimeReady.GetInvocationList(), (Delegate t) => t.Target == this) == null)
		{
			ServerTime.OnServerTimeReady = (Action<WsServiceEvent>)Delegate.Combine(ServerTime.OnServerTimeReady, new Action<WsServiceEvent>(OnGetServerTime));
		}
		mSkipEggSpawn = skipEggSpawn;
		Init(unlockCount);
	}

	private void Init(int unlockCount)
	{
		pReadyToBuy = false;
		base.gameObject.SetActive(value: true);
		int num = unlockCount + _HatcheryManager.pIncubatorStartSlotIndex;
		if (mID <= _HatcheryManager.pIncubatorStartSlotIndex || mID <= num)
		{
			SetInitialState();
			return;
		}
		SetIncubatorState(IncubatorStates.LOCKED);
		base.gameObject.SetActive(value: false);
		pReadyToBuy = mID == num + 1;
	}

	private void SetInitialState()
	{
		if (mMyState != IncubatorStates.LOCKED)
		{
			HighlightObject(canShowHightlight: true);
		}
		pInitPetOnProximityStableHatch = false;
		SetIncubatorState(IncubatorStates.WAITING_FOR_EGG);
		mMyPetData = RaisedPetData.GetHatchingPet(mID);
		if (mMyPetData != null)
		{
			mPetType = mMyPetData.PetTypeID;
			if (SanctuaryManager.IsPetHatched(mMyPetData) && StableData.GetByPetID(mMyPetData.RaisedPetID) == null)
			{
				SetIncubatorState(IncubatorStates.IDLE);
			}
			else if (GetHatchTimeLeft().TotalSeconds < 0.0)
			{
				CheckHatchEnd();
			}
			else
			{
				SetIncubatorState(IncubatorStates.HATCHING);
			}
		}
	}

	public TimeSpan GetHatchTimeLeft()
	{
		return mMyPetData.pHatchEndTime - ServerTime.pCurrentTime;
	}

	private void CheckHatchEnd()
	{
		if (UtPlatform.IsWSA() || UtPlatform.IsStandAlone())
		{
			mMyState = IncubatorStates.HATCHED;
			ServerTime.Init(inForceInit: true);
		}
		else
		{
			SetIncubatorState(IncubatorStates.HATCHED);
		}
	}

	public void HighlightObject(bool canShowHightlight)
	{
		ObClickable component = GetComponent<ObClickable>();
		if (canShowHightlight && component?._HighlightMaterial != null)
		{
			component?.Highlight();
		}
		else
		{
			component?.UnHighlight();
		}
	}

	public void SetIncubatorState(IncubatorStates state)
	{
		mMyState = state;
		switch (state)
		{
		case IncubatorStates.WAITING_FOR_EGG:
		case IncubatorStates.LOCKED:
			mMyPetData = null;
			break;
		case IncubatorStates.HATCHING:
			LoadEgg();
			break;
		case IncubatorStates.HATCHED:
			ShowHatchedEffect();
			if (mPickedUpEgg == null)
			{
				LoadEgg();
			}
			break;
		case IncubatorStates.IDLE:
			if ((bool)mHatchedEffect)
			{
				UnityEngine.Object.Destroy(mHatchedEffect);
			}
			break;
		}
	}

	private void LoadEgg()
	{
		SanctuaryPetTypeInfo[] pPetTypes = SanctuaryData.pPetTypes;
		foreach (SanctuaryPetTypeInfo sanctuaryPetTypeInfo in pPetTypes)
		{
			if (sanctuaryPetTypeInfo._TypeID == mPetType)
			{
				SetSpeedUpTimerTicketInfo(sanctuaryPetTypeInfo);
				if (!mSkipEggSpawn)
				{
					LoadPetResources(sanctuaryPetTypeInfo);
				}
				break;
			}
		}
	}

	private void SetSpeedUpTimerTicketInfo(SanctuaryPetTypeInfo petTypeInfo)
	{
		mSpeedUpTimerTicketStoreID = petTypeInfo._InstantHatchTicketItemStoreID;
		mSpeedUpTimerTicketItemID = petTypeInfo._InstantHatchTicketItemID;
		ItemStoreDataLoader.Load(mSpeedUpTimerTicketStoreID, OnStoreLoaded);
		mSpeedUpTimerTicketCost = GetSpeedupCost();
	}

	public void OnStoreLoaded(StoreData sd)
	{
		mStoreDataList.Add(sd);
	}

	protected int GetSpeedupCost()
	{
		if (CommonInventoryData.pInstance != null)
		{
			ItemData itemData = GetItemData(mSpeedUpTimerTicketItemID);
			if (itemData != null)
			{
				return itemData.FinalCashCost;
			}
		}
		return -1;
	}

	public ItemData GetItemData(int inItemID)
	{
		if (mStoreDataList != null)
		{
			foreach (StoreData mStoreData in mStoreDataList)
			{
				ItemData[] items = mStoreData._Items;
				foreach (ItemData itemData in items)
				{
					if (itemData.ItemID == inItemID)
					{
						return itemData;
					}
				}
			}
		}
		return null;
	}

	private void ShowHatchedEffect()
	{
		if (!(_HatchedParticle == null) && !(mHatchedEffect != null))
		{
			mHatchedEffect = UnityEngine.Object.Instantiate(_HatchedParticle);
			mHatchedEffect.transform.localPosition = base.gameObject.transform.position;
			mHatchedEffect.transform.localRotation = Quaternion.identity;
		}
	}

	private void OnPetCreated(SanctuaryPet pet)
	{
		mIncubatorPet = pet;
		if (pet.pData.pStage >= RaisedPetStage.BABY)
		{
			mIncubatorPet.PlayAnimation("IdleSit", WrapMode.Loop);
			mIncubatorPet.AIActor.SetState(AISanctuaryPetFSM.IDLE);
		}
	}

	private void OnPetReady(SanctuaryPet pet)
	{
		pInitPetOnProximityStableHatch = true;
	}

	public void SetPetInProximityStableHatch()
	{
		if (_ObProximityStableHatch != null && mIncubatorPet != null)
		{
			_ObProximityStableHatch.OnPetReady(mIncubatorPet);
		}
	}

	public void ResetIncubatorPet()
	{
		UnityEngine.Object.Destroy(mIncubatorPet.gameObject);
		mIncubatorPet = null;
		pInitPetOnProximityStableHatch = false;
	}

	public void OnUnlockDone(int unlockCount)
	{
		Init(unlockCount);
	}

	public bool IsIdle()
	{
		return mMyState == IncubatorStates.IDLE;
	}

	public void SpeedUpHatching(Action<bool, Incubator> onSpeedUpDone = null)
	{
		OnSpeedUpDone = onSpeedUpDone;
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		KillGenericDB();
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._NoMessage = "CloseDB";
		KAUI.SetExclusive(mKAUIGenericDB);
		if (Money.pCashCurrency < mSpeedUpTimerTicketCost)
		{
			mKAUIGenericDB.SetText(_NotEnoughVCashText.GetLocalizedString(), interactive: false);
			mKAUIGenericDB._YesMessage = "ProceedToStore";
		}
		else
		{
			mKAUIGenericDB.SetText(_UseGemsForCustomizeText.GetLocalizedString().Replace("{cost}", mSpeedUpTimerTicketCost.ToString()), interactive: false);
			mKAUIGenericDB._YesMessage = "PurchaseHatchSpeedUpTicket";
		}
	}

	public void UseSpeedUpTicket(Action<bool, Incubator> onSpeedUpDone = null)
	{
		OnSpeedUpDone = onSpeedUpDone;
		RefreshHatchTime();
		if (CommonInventoryData.pInstance.FindItem(mSpeedUpTimerTicketItemID) != null)
		{
			RemoveHatchTicketItem(mSpeedUpTimerTicketItemID);
		}
		else if (ParentData.pIsReady && ParentData.pInstance.pInventory.pData.FindItem(mSpeedUpTimerTicketItemID) != null)
		{
			RemoveHatchTicketItem(mSpeedUpTimerTicketItemID, isParentData: true);
		}
		else if (CommonInventoryData.pInstance.FindItem(SanctuaryData.pInstance._CommonHatchTicketInfo._InstantHatchTicketItemID) != null)
		{
			RemoveHatchTicketItem(SanctuaryData.pInstance._CommonHatchTicketInfo._InstantHatchTicketItemID);
		}
		else if (ParentData.pIsReady && ParentData.pInstance.pInventory.pData.FindItem(SanctuaryData.pInstance._CommonHatchTicketInfo._InstantHatchTicketItemID) != null)
		{
			RemoveHatchTicketItem(SanctuaryData.pInstance._CommonHatchTicketInfo._InstantHatchTicketItemID, isParentData: true);
		}
		SetIncubatorState(IncubatorStates.HATCHED);
		OnSpeedUpDone?.Invoke(arg1: true, this);
		OnSpeedUpDone = null;
	}

	private void RefreshHatchTime()
	{
		mMyPetData.pHatchEndTime = ServerTime.pCurrentTime;
		mMyPetData.SaveDataReal();
	}

	private void RemoveHatchTicketItem(int itemID, bool isParentData = false)
	{
		if (isParentData)
		{
			ParentData.pInstance.pInventory.pData.RemoveItem(itemID, updateServer: true);
			ParentData.pInstance.pInventory.pData.Save();
		}
		else
		{
			CommonInventoryData.pInstance.RemoveItem(itemID, updateServer: true);
			CommonInventoryData.pInstance.Save();
		}
	}

	private void LoadPetResources(SanctuaryPetTypeInfo petTypeInfo)
	{
		string[] separator = new string[1] { "/" };
		string[] array = petTypeInfo._DragonEggAssetpath.Split(separator, StringSplitOptions.None);
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], EggEventHandler, typeof(GameObject));
	}

	private void EggEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mPickedUpEgg = UnityEngine.Object.Instantiate(position: new Vector3(base.transform.position.x, base.transform.position.y, base.transform.position.z) + _EggOffset, original: (GameObject)inObject, rotation: Quaternion.identity);
			if (!string.IsNullOrEmpty(inURL))
			{
				string[] array = inURL.Split('/');
				if (array != null && array.Length != 0)
				{
					mPickedUpEgg.name = array[^1];
				}
			}
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Item could not be downloaded.");
			break;
		}
	}

	public void CheckEggSelected(string inActionName, Action<bool, Incubator> onEggPlaced = null)
	{
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(_DragonEggsCategory);
		if (items != null && items.Length == 0)
		{
			return;
		}
		UserItemData[] array = items;
		foreach (UserItemData userItemData in array)
		{
			if (userItemData.Item.ItemName.Equals(inActionName))
			{
				mCurrentUserItemData = userItemData;
				mPetType = mCurrentUserItemData.Item.GetAttribute("PetTypeID", 14);
				mHatchDuration = mCurrentUserItemData.Item.GetAttribute("HatchDuration", -1f);
				SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(mPetType);
				mMyPetData = RaisedPetData.InitDefault(mPetType, RaisedPetStage.HATCHING, "", Gender.Male, addToActivePets: false);
				if (mHatchDuration < 0f)
				{
					mHatchDuration = sanctuaryPetTypeInfo._HatchDuration;
				}
				mMyPetData.SetHatchEndTime(ServerTime.pCurrentTime.AddMinutes(mHatchDuration));
				mMyPetData.pIncubatorID = mID;
				mMyPetData.Geometry = sanctuaryPetTypeInfo._DragonEggAssetpath;
				mMyPetData.UserID = new Guid(UserInfo.pInstance.UserID);
				mMyPetData.SetupSaveData();
				CommonInventoryRequest[] array2 = new CommonInventoryRequest[1]
				{
					new CommonInventoryRequest()
				};
				array2[0].CommonInventoryID = userItemData.UserInventoryID;
				array2[0].ItemID = userItemData.Item.ItemID;
				array2[0].Quantity = -1;
				OnEggPlaced = onEggPlaced;
				RaisedPetData.CreateNewPet(mPetType, setAsSelectedPet: false, unselectOtherPets: false, mMyPetData, array2, RaisedPetCreateCallback, userItemData.Item.ItemID);
				SetIncubatorState(IncubatorStates.HATCHING);
				InteractiveTutManager._CurrentActiveTutorialObject?.SendMessage("TutorialManagerAsyncMessage", "Incubator_Click");
				break;
			}
		}
	}

	public void RaisedPetCreateCallback(int ptype, RaisedPetData pdata, object inUserData)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (pdata == null)
		{
			UtDebug.LogError("Egg Create failed");
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			SetIncubatorState(IncubatorStates.WAITING_FOR_EGG);
			OnEggPlaced?.Invoke(arg1: false, this);
			OnEggPlaced = null;
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", SanctuaryManager.pInstance._CreatePetFailureText.GetLocalizedString(), base.gameObject, "OnCreatePetFailOK");
		}
		else
		{
			CommonInventoryData.pInstance.RemoveItem((int)inUserData, updateServer: false);
			CommonInventoryData.pInstance.ClearSaveCache();
			mMyPetData = pdata;
			OnEggPlaced?.Invoke(arg1: true, this);
			OnEggPlaced = null;
		}
	}

	public void PickUpEgg(bool fromStable = true)
	{
		SetIncubatorState(IncubatorStates.IDLE);
		StableManager.pCurIncubatorID = mID;
		_HatcheryManager.AttachDragonEgg(mPetType, AvAvatar.pObject, base.gameObject, fromStable);
	}

	public void StartHatching()
	{
		if (_ObProximityStableHatch != null)
		{
			_ObProximityStableHatch.pPedestalObject = base.gameObject;
			_ObProximityStableHatch.HatchDragonEgg(mPetType);
		}
		else if (_ObProximityHatch != null)
		{
			_ObProximityHatch.HatchDragonEgg(mPetType, this);
		}
		ClearEgg();
	}

	public void ClearEgg()
	{
		if (mPickedUpEgg != null)
		{
			UnityEngine.Object.Destroy(mPickedUpEgg);
		}
	}

	public void OnCreatePetFailOK()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		SetIncubatorState(IncubatorStates.WAITING_FOR_EGG);
		ClearEgg();
	}

	public string GetStatusText(TimeSpan timeSpan)
	{
		if (!(timeSpan.TotalSeconds > 0.0))
		{
			return "0";
		}
		return $"{timeSpan.Hours:d2}:{timeSpan.Minutes:d2}:{timeSpan.Seconds:d2}";
	}

	private void OnIAPStoreClosed()
	{
		CloseDB();
	}

	private void ProceedToStore()
	{
		KillGenericDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void CloseDB()
	{
		OnSpeedUpDone?.Invoke(arg1: false, this);
		OnSpeedUpDone = null;
		KillGenericDB();
	}

	private void KillGenericDB()
	{
		UnityEngine.Object.Destroy(mKAUIGenericDB?.gameObject);
	}

	private void PurchaseHatchSpeedUpTicket()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		CommonInventoryData.pInstance.AddPurchaseItem(mSpeedUpTimerTicketItemID, 1, ItemPurchaseSource.INCUBATOR.ToString());
		CommonInventoryData.pInstance.DoPurchase(2, mSpeedUpTimerTicketStoreID, TicketPurchaseHandler);
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
		mKAUIGenericDB.SetText(_TicketPurchaseProcessingText.GetLocalizedString(), interactive: false);
	}

	public void TicketPurchaseHandler(CommonInventoryResponse ret)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (ret != null && ret.Success)
		{
			RefreshHatchTime();
			RemoveHatchTicketItem(mSpeedUpTimerTicketItemID);
			SetIncubatorState(IncubatorStates.HATCHED);
			OnSpeedUpDone?.Invoke(arg1: true, this);
			OnSpeedUpDone = null;
			CloseDB();
		}
		else
		{
			CloseDB();
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			mKAUIGenericDB.SetText(_TicketPurchaseFailedText.GetLocalizedString(), interactive: false);
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._OKMessage = "CloseDB";
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	private void OnGetServerTime(WsServiceEvent inEvent)
	{
		if (inEvent == WsServiceEvent.COMPLETE && mMyState == IncubatorStates.HATCHED)
		{
			SetIncubatorState((ServerTime.pServerTime.HasValue && mMyPetData != null && (mMyPetData.pHatchEndTime - ServerTime.pServerTime.Value).TotalSeconds < 0.0) ? IncubatorStates.HATCHED : IncubatorStates.HATCHING);
		}
	}

	private void OnDestroy()
	{
		ServerTime.OnServerTimeReady = (Action<WsServiceEvent>)Delegate.Remove(ServerTime.OnServerTimeReady, new Action<WsServiceEvent>(OnGetServerTime));
	}

	public bool IsAdWatchAvailable()
	{
		if (!AdManager.pInstance.AdSupported(_AdEventType, AdType.REWARDED_VIDEO))
		{
			return false;
		}
		GameObject currentActiveTutorialObject = InteractiveTutManager._CurrentActiveTutorialObject;
		if (currentActiveTutorialObject != null && currentActiveTutorialObject.GetComponent<StableIncubatorTutorial>() != null && !currentActiveTutorialObject.GetComponent<StableIncubatorTutorial>().TutorialComplete())
		{
			return false;
		}
		if (mMyPetData != null)
		{
			if (mMyState == IncubatorStates.HATCHING && !AdManager.pInstance.IsReductionTimeGreater(_AdEventType, (int)GetHatchTimeLeft().TotalSeconds))
			{
				return false;
			}
			if (mMyPetData.FindAttrData(mAdWatchedKey) != null && UtStringUtil.Parse(mMyPetData.FindAttrData(mAdWatchedKey).Value, inDefault: false))
			{
				return false;
			}
		}
		return true;
	}

	public void ShowAd(Action<bool, Incubator> onSpeedUpDone = null)
	{
		if (AdManager.pInstance.AdAvailable(_AdEventType, AdType.REWARDED_VIDEO))
		{
			OnSpeedUpDone = onSpeedUpDone;
			AdManager.DisplayAd(_AdEventType, AdType.REWARDED_VIDEO, base.gameObject);
		}
	}

	public void OnAdWatched()
	{
		AdManager.pInstance.LogAdWatchedEvent(_AdEventType, "Incubation");
		AdManager.pInstance.SyncAdAvailableCount(_AdEventType, isConsumed: true);
		mMyPetData.pHatchEndTime = mMyPetData.pHatchEndTime.AddSeconds(-AdManager.pInstance.GetReductionTime(_AdEventType, (int)GetHatchTimeLeft().TotalSeconds));
		mMyPetData.SetAttrData(mAdWatchedKey, "true", DataType.BOOL);
		mMyPetData.SaveDataReal();
		if (OnSpeedUpDone != null)
		{
			OnSpeedUpDone?.Invoke(arg1: true, this);
			OnSpeedUpDone = null;
		}
	}

	public void OnAdFailed()
	{
		UtDebug.LogError("OnAdFailed for event: " + _AdEventType);
	}

	public void OnAdSkipped()
	{
	}

	public void OnAdClosed()
	{
	}

	public void OnAdFinished(string eventDataRewardString)
	{
	}

	public void OnAdCancelled()
	{
	}
}
