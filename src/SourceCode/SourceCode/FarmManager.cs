using System;
using System.Collections.Generic;
using System.Reflection;
using KA.Framework;
using UnityEngine;

public class FarmManager : MyRoomsIntMain
{
	[Serializable]
	public class DisplayBoxData
	{
		public string _BundleName;

		public string _YesMessage;

		public string _NoMessage;

		public string _OkMessage;

		public string _CloseMessage;

		public bool _DestroyOnClick;
	}

	[Serializable]
	public class ActionData
	{
		public string _Name;

		public List<int> _SkipOnConsumableIDs;
	}

	[Serializable]
	public class FirstTimeAction
	{
		public string _PairDataName;

		public ActionData[] _Actions;

		public LocaleString _DisplayText;

		public DisplayBoxData _DBData;
	}

	[Serializable]
	public class FarmingSFX
	{
		public string _ActionName;

		public string _StageName;

		public bool _CheckForExactMatch;

		public AudioClip _Clip;
	}

	[Serializable]
	public class FarmItemHelp
	{
		public int _ID;

		public LocaleString _Text;
	}

	[Serializable]
	public class FarmItemContextSensitiveActions
	{
		public string _ActionName;

		public string _UpdateQuantityFunction;
	}

	[Serializable]
	public class HarvestAchievement
	{
		public int _ItemID;

		public int _AchievementID;

		public int _ClanAchievementID;
	}

	[Serializable]
	public class AdContextData
	{
		public string _PropName;

		public ContextData _ContextData;
	}

	public const string DEFAULT_FARM_ROOM_ID = "";

	public LocaleString _FarmingDBTitleText = new LocaleString("Farming");

	public LocaleString _NoHarvestToolText = new LocaleString("You do not have a harvest tool equipped");

	public LocaleString _NotEnoughFeedText = new LocaleString("You do not have enough feed, You can purchase more from the store.");

	public LocaleString _NoWaterMessageText = new LocaleString("You do not have enough water in well to use.");

	public LocaleString _WebServiceErrorText = new LocaleString("The game encountered a server error, we'll take you to Berk");

	public LocaleString _ConsumableInsufficientText = new LocaleString("You need %items_list%.");

	public LocaleString _InsufficientGemsText = new LocaleString("You need %gems_count% Gems for this. Do you want to buy gems now?");

	public string _ForceLevelOnWsError = "HubBerkDO";

	private KAUIGenericDB mKAUIGenericDB;

	public string _DialogAssetName = "PfKAUIGenericDBSm";

	public float _YRotOffset = 90f;

	public Color _MaskColor = Color.clear;

	public GameObject _FarmGameArea;

	public GameObject _FarmHarvestEmitter;

	public string _ExitMarkerName = "PfMarker_FarmingExit";

	public const string FARM_SLOT_CONVERSION = "SlotsConverted";

	public const string UNLOCKED = "UnlockedGC";

	public int _FarmSlotItemID = 13125;

	public int _OldFarmSlotPairID = 5001;

	public int _PairID;

	public int[] _FarmingStoreIDs;

	public int[] _MoveToInventoryExcludeCategoryList;

	public Vector3[] _DefaultFarmSlotPositionList;

	public int _WellInventoryID = 7234;

	public Vector3 _WellPosition = new Vector3(0f, 0f, 11f);

	public Vector3 _WellOrientation = Vector3.zero;

	public FirstTimeAction[] _FarmingActions;

	public KAWidget _TipTextWidget;

	public int _TipDisplayTime;

	public AudioClip _OnCSMActivationSFX;

	public AudioClip _StatusTimerSFX;

	public bool _PlayStatusTimerSFX;

	public FarmingSFX[] _FarmingActionSFX;

	public FarmItemHelp[] _FarmingItemHelpTexts;

	public FarmItemContextSensitiveActions[] _ContextSensitiveActions;

	public UiToolbar _UiToolbar;

	public Transform _BuildModeAvatarMarker;

	public List<HarvestAchievement> _HarvestAchievementData;

	public GameObject _QuestArrow;

	public List<ContextData> _DataList;

	public LocaleString _UserItemSaveErrorText;

	public bool _WellNotRequired = true;

	public int _DefaultFarmItemID = 13105;

	public AdEventType _AdEventType;

	private bool mIsWsErrorReceived;

	private bool mIsPlayingTimerSFX;

	private FarmItem mFarmItemWithStatusTimer;

	public static UserRoom pCurrentFarmData;

	private PairData mPairOldSlotData;

	private bool mPairSlotDataRequested;

	private bool mFarmSlotConversionDone;

	private bool mFarmIntialized;

	private bool mFarmInfoReady;

	private bool mIsDeserialized;

	private bool mDefaultItemsChecked;

	public FarmGridManager _GridManager;

	public bool _ShowDebugInfo = true;

	public List<RemovableInfo> _Removables = new List<RemovableInfo>();

	private Transform mRemovablesTransform;

	public int _NumberofDynamicRemovables = 5;

	private List<FarmItem> mFarmItems = new List<FarmItem>();

	private FarmItem mMouseHoverItem;

	private PairData mFarmPairData;

	private const string PAIR_KEY = "FarmItems";

	private string mPairDataHeaderV3 = "v3";

	public UiLandscapeMode _LandScapeMode;

	private UiFarmToolbar mFarmToolbar;

	public GameObject _WaterTutorial;

	public GameObject _HarvestTutorial;

	public GameObject _StoreTutorial;

	public GameObject _PlantTutorial;

	public FarmBuildModeTutorial _BuildmodeTutorial;

	public InteractiveTutManager _AnimalTutorial;

	public InteractiveTutManager _ExpansionTutorial;

	public InteractiveTutManager _CreativePointTutorial;

	public int _RequiredLevelForBuildmodeTutorial = 3;

	public int _RequiredLevelForAnimalTutorial = 4;

	private bool mRefreshRankUI = true;

	private List<StoreData> mStoreDataList;

	public string _HarvestPropName = "Harvest";

	public string _FeedPropName = "Feed";

	public GameObject _UserNotifyObject;

	public int _LeaderboardCategoryID = 29;

	public StoreLoader.Selection _AnimalsStoreInfo;

	public StoreLoader.Selection _AnimalFeedStoreInfo;

	public List<AdContextData> _AdContextDataList = new List<AdContextData>();

	public float _DebugTextXOffset = 20f;

	public bool pIsWsErrorReceived
	{
		get
		{
			return mIsWsErrorReceived;
		}
		set
		{
			mIsWsErrorReceived = value;
		}
	}

	public bool pIsPlayingTimerSFX => mIsPlayingTimerSFX;

	public FarmItem pFarmItemWithStatusTimer
	{
		get
		{
			return mFarmItemWithStatusTimer;
		}
		set
		{
			mFarmItemWithStatusTimer = value;
		}
	}

	public UiLandscapeMode pBuilder
	{
		get
		{
			if (_Rooms == null || _Rooms.Length == 0 || _Rooms[0] == null || _Rooms[0]._MyRoomBuilder == null)
			{
				return null;
			}
			return _Rooms[0]._MyRoomBuilder.GetComponent<UiLandscapeMode>();
		}
	}

	public static string pCurrentFarmID
	{
		get
		{
			if (pCurrentFarmData == null)
			{
				return "";
			}
			return pCurrentFarmData.RoomID;
		}
	}

	public bool pDefaultItemsChecked => mDefaultItemsChecked;

	public bool pIsReady
	{
		get
		{
			if (mFarmInfoReady && _GridManager != null && _GridManager.pIsReady)
			{
				return mIsDeserialized;
			}
			return false;
		}
	}

	public List<FarmItem> pFarmItems => mFarmItems;

	public FarmItem pMouseHoverItem
	{
		get
		{
			return mMouseHoverItem;
		}
		set
		{
			mMouseHoverItem = value;
		}
	}

	public UiFarmToolbar pFarmToolbar => mFarmToolbar;

	public override void Start()
	{
		MainStreetMMOClient.DestroyPool();
		base.Start();
		mRemovablesTransform = base.transform.Find("Removables");
		mFarmToolbar = pBuilder._MyRoomsInt.GetComponent<UiFarmToolbar>();
		ItemStoreDataLoader.Load(_FarmingStoreIDs, OnStoreLoaded, null);
		if (_QuestArrow != null)
		{
			_QuestArrow.SetActive(value: false);
		}
		if (!MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
		{
			Pair pair = ProductData.pPairData.FindByKey("SlotsConverted");
			if (pair != null && !string.IsNullOrEmpty(pair.PairValue))
			{
				mFarmSlotConversionDone = true;
			}
		}
	}

	public override void LoadRoomData()
	{
		mInitializeRoomData = true;
		if (!MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
		{
			UserRoom.LoadRooms(541, force: false, FarmRoomDataEvent);
		}
		else if (pCurrentFarmData != null)
		{
			if (pCurrentFarmData.RoomID.Equals(""))
			{
				pCurrentFarmData.pItemID = _DefaultFarmItemID;
			}
			pCurrentFarmData.MaxCreativePointsLimit(MaxCreativePointEventHandler);
		}
		else
		{
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			WsWebService.GetUserRoomList(MainStreetMMOClient.pInstance.GetOwnerIDForLevel(RsResourceManager.pCurrentLevel), 541, GetRoomServiceEventHandler, null);
		}
	}

	public void GetRoomServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.GET_USER_ROOM_LIST)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			if (inObject == null)
			{
				break;
			}
			UserRoomResponse userRoomResponse = (UserRoomResponse)inObject;
			if (userRoomResponse.UserRoomList.Count == 0)
			{
				pCurrentFarmData = new UserRoom
				{
					pItemID = _DefaultFarmItemID,
					pLocaleName = "BaseFarm",
					CategoryID = 541,
					RoomID = ""
				};
				pCurrentFarmData.MaxCreativePointsLimit(MaxCreativePointEventHandler);
				break;
			}
			{
				foreach (UserRoom userRoom in userRoomResponse.UserRoomList)
				{
					if (userRoom.RoomID.Equals(MainStreetMMOClient.UserRoomID))
					{
						MainStreetMMOClient.UserRoomID = string.Empty;
						pCurrentFarmData = userRoom;
						if (pCurrentFarmData.RoomID.Equals(""))
						{
							pCurrentFarmData.pItemID = _DefaultFarmItemID;
						}
						pCurrentFarmData.MaxCreativePointsLimit(MaxCreativePointEventHandler);
						break;
					}
				}
				break;
			}
		}
		case WsServiceEvent.ERROR:
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			break;
		}
	}

	private void MaxCreativePointEventHandler(int creativePoints)
	{
		base.pMaxCreativePoints = creativePoints;
		_Rooms[0]._RoomID = pCurrentFarmData.RoomID;
		mFarmToolbar.UpdateCreativePointsProgress();
		mLoadedRoomData = true;
	}

	private void FarmRoomDataEvent(bool success)
	{
		if (pCurrentFarmData == null)
		{
			pCurrentFarmData = UserRoom.GetByRoomID("");
			if (pCurrentFarmData == null)
			{
				if (CommonInventoryData.pInstance.FindItem(_DefaultFarmItemID) == null)
				{
					CommonInventoryData.pInstance.AddItem(_DefaultFarmItemID, 1);
					CommonInventoryData.pInstance.Save(AwardDefaultFarmEventHandler, null);
				}
				else
				{
					AwardDefaultFarmEventHandler(inSaveSuccess: true, null);
				}
			}
			else
			{
				pCurrentFarmData.ItemID = _DefaultFarmItemID;
				if (CommonInventoryData.pInstance.FindItem(_DefaultFarmItemID) == null)
				{
					CommonInventoryData.pInstance.AddItem(_DefaultFarmItemID, 1);
					CommonInventoryData.pInstance.Save(AwardDefaultFarmEventHandler, null);
					return;
				}
			}
		}
		if (pCurrentFarmData != null)
		{
			_Rooms[0]._RoomID = pCurrentFarmData.RoomID;
			base.pMaxCreativePoints = pCurrentFarmData.MaxCreativePointsLimit();
			mFarmToolbar.UpdateCreativePointsProgress();
			mLoadedRoomData = true;
		}
	}

	public void AwardDefaultFarmEventHandler(bool inSaveSuccess, object inUserData)
	{
		if (inSaveSuccess)
		{
			if (pCurrentFarmData == null)
			{
				pCurrentFarmData = UserRoom.AddRoom(CommonInventoryData.pInstance.FindItem(_DefaultFarmItemID), 541);
				pCurrentFarmData.RoomID = "";
			}
			_Rooms[0]._RoomID = pCurrentFarmData.RoomID;
			base.pMaxCreativePoints = pCurrentFarmData.MaxCreativePointsLimit();
			mFarmToolbar.UpdateCreativePointsProgress();
			mLoadedRoomData = true;
		}
	}

	public override void OnPetReady(SanctuaryPet pet)
	{
		pet.OnFlyDismountImmediate(AvAvatar.pObject);
		SanctuaryManager.pCurPetInstance = pet;
		SanctuaryManager.pCurPetData = pet.pData;
		pet.pMeterPaused = true;
		pet.SetFollowAvatar(follow: false);
		pet.SetFidgetOnOff(isOn: false);
		pet.SetClickableActive(active: false);
		SetLoadComplete();
		PositionPetOnBed();
		pet.AIActor.SetState(AISanctuaryPetFSM.IDLE);
	}

	public MethodInfo GetMethod(FarmItem farmItem, string actionName)
	{
		string value = "";
		FarmItemContextSensitiveActions[] contextSensitiveActions = _ContextSensitiveActions;
		foreach (FarmItemContextSensitiveActions farmItemContextSensitiveActions in contextSensitiveActions)
		{
			if (farmItemContextSensitiveActions._ActionName.Equals(actionName))
			{
				value = farmItemContextSensitiveActions._UpdateQuantityFunction;
				break;
			}
		}
		if ((bool)farmItem && !string.IsNullOrEmpty(value))
		{
			return farmItem.GetType().GetMethodInfo(value, BindingFlags.Instance | BindingFlags.Public);
		}
		return null;
	}

	private void InitializeRemovables()
	{
		List<GridCell> unstampedCells = _GridManager.GetUnstampedCells();
		List<int> list = new List<int>();
		int num = 0;
		while (num < _NumberofDynamicRemovables)
		{
			int num2 = UnityEngine.Random.Range(0, unstampedCells.Count);
			if (!list.Contains(num2))
			{
				list.Add(num2);
				int num3 = UnityEngine.Random.Range(0, 10);
				RemovableType inRemovableType = RemovableType.ROCK;
				if (num3 > 5)
				{
					inRemovableType = RemovableType.TREE;
				}
				Vector3 inPosition = new Vector3(unstampedCells[num2]._GridCellRect.center.x, 0f, unstampedCells[num2]._GridCellRect.center.y);
				new RemovableInfo(inRemovableType, num + 100, inPosition).Load(this);
				num++;
			}
		}
		foreach (RemovableInfo removable in _Removables)
		{
			removable.Load(this);
		}
	}

	protected override void UpdateCamera()
	{
	}

	public void RemovablesHandler(int inID, object inObject, RemovableInfo inrInfo)
	{
		if (inObject != null)
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.transform.parent = mRemovablesTransform;
			Removable component = obj.GetComponent<Removable>();
			if (component != null)
			{
				component.SetInfo(inrInfo, this);
			}
		}
	}

	public override void ObjectDestroyCallback(GameObject inObject)
	{
		if (!(inObject == null))
		{
			ProcessRemoveRoomObject(inObject);
		}
	}

	public override void ObjectCreatedCallback(GameObject inObject, UserItemData inUserItemData, bool inSaved)
	{
		if (inObject == null || inUserItemData == null)
		{
			return;
		}
		FarmItem farmItem = AddFarmItem(inObject, inUserItemData);
		if (inUserItemData.Item.CreativePoints > 0)
		{
			UpdateCreativePoints(inUserItemData.Item.CreativePoints);
			pBuilder.UpdateCreativePointsProgress();
			mFarmToolbar.UpdateCreativePointsProgress();
		}
		if (!(farmItem != null))
		{
			return;
		}
		if (farmItem.pClickable != null && !base.pIsBuildMode)
		{
			farmItem.pClickable._Active = true;
		}
		if (inSaved)
		{
			if (_GridManager != null && farmItem._StampOnGrids)
			{
				_GridManager.AddItem(inObject, inUserItemData, !farmItem._CanPlaceOnNotPurchasedSlot);
			}
			return;
		}
		farmItem.AddDefaultFarmItems();
		if (farmItem._DefaultItems == null)
		{
			return;
		}
		DefaultFarmItem[] defaultItems = farmItem._DefaultItems;
		foreach (DefaultFarmItem defaultFarmItem in defaultItems)
		{
			if (defaultFarmItem != null)
			{
				RemoveItemFromInventory(defaultFarmItem._ItemID);
			}
		}
		pBuilder.UpdateItemQuantity();
	}

	public override void UpdateRoomLoadedCount()
	{
		base.UpdateRoomLoadedCount();
		SetParentChildRelation();
		if (mRoomsLoaded == _Rooms.Length && mObStatus != null && mObStatus.pIsReady && MyRoomsIntLevel.pInstance != null)
		{
			MyRoomsIntMain.EnableRoomObjectsClickable(!MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt());
		}
	}

	public override Vector3 CalculatePosition(Vector3 inPos, Transform inObj)
	{
		if (inObj != null)
		{
			FarmItem component = inObj.GetComponent<FarmItem>();
			if (!(component != null))
			{
				return inPos;
			}
			if (component.IsDependent() || !component._SnapToGrids)
			{
				return inPos;
			}
		}
		if (_GridManager == null)
		{
			return inPos;
		}
		GridCell gridCellfromPoint = _GridManager.GetGridCellfromPoint(inPos);
		if (gridCellfromPoint == null)
		{
			return inPos;
		}
		return _GridManager.GetGridCellPosition(gridCellfromPoint);
	}

	public override void UpdateRoomObject(GameObject inGameObject, UserItemData inUserItemData, GameObject inParentData)
	{
		base.UpdateRoomObject(inGameObject, inUserItemData, inParentData);
		if (_GridManager != null)
		{
			FarmItem component = inGameObject.GetComponent<FarmItem>();
			if (component != null)
			{
				_GridManager.UpdateItem(inGameObject, inUserItemData, inGameObject.transform.position, (int)component._GridCellCount.x, (int)component._GridCellCount.y);
			}
		}
	}

	public override void RemoveRoomObject(GameObject inObject, bool isDestroy)
	{
		base.RemoveRoomObject(inObject, isDestroy);
		ProcessRemoveRoomObject(inObject);
	}

	public virtual void ProcessRemoveRoomObject(GameObject inObject)
	{
		FarmItem component = inObject.GetComponent<FarmItem>();
		if (!(component == null) && component.pUserItemData != null)
		{
			if (component.pUserItemData.Item.CreativePoints > 0)
			{
				UpdateCreativePoints(-component.pUserItemData.Item.CreativePoints);
				pBuilder.UpdateCreativePointsProgress();
				mFarmToolbar.UpdateCreativePointsProgress();
			}
			RemoveFarmItem(component);
			_GridManager.RemoveItem(inObject);
		}
	}

	public virtual void RemoveRoomObject(GameObject inObject, bool isDestroy, bool noSaveInServer)
	{
		if (noSaveInServer)
		{
			ProcessRemoveRoomObject(inObject);
			if (mDictRoomData.ContainsKey(MyRoomsIntMain.pInstance.pCurrentRoomID))
			{
				mDictRoomData[base.pCurrentRoomID].RemoveRoomObject(inObject, isDestroy);
				UserItemPositionList.GetInstance(base.pCurrentRoomID).ClearRemoveObjectList();
			}
		}
		else
		{
			RemoveRoomObject(inObject, isDestroy);
		}
	}

	public void RemoveAllItems(string roomID)
	{
		List<GameObject> list = new List<GameObject>();
		list.AddRange(base.pDictRoomData[roomID].pGameObjectReferenceList);
		foreach (GameObject item in list)
		{
			if (!(item != null) || !base.pDictRoomData[roomID].pGameObjectReferenceList.Contains(item))
			{
				continue;
			}
			FarmItem component = item.GetComponent<FarmItem>();
			if (component == null || !component._CanMoveToInventory)
			{
				continue;
			}
			if (component.pUserItemData != null && component.pUserItemData.Item != null)
			{
				AddItemToInventory(component.pUserItemData);
			}
			MyRoomObject.ChildData[] pChildList = item.GetComponent<MyRoomObject>().pChildList;
			for (int i = 0; i < pChildList.Length; i++)
			{
				MyRoomObject component2 = pChildList[i]._Child.GetComponent<MyRoomObject>();
				if (component2.pUserItemData != null && component2.pUserItemData.Item != null)
				{
					AddItemToInventory(component2.pUserItemData);
				}
			}
			RemoveRoomObject(item, isDestroy: true);
		}
	}

	public void AddItemToInventory(UserItemData userItemData)
	{
		int[] moveToInventoryExcludeCategoryList = _MoveToInventoryExcludeCategoryList;
		foreach (int categoryID in moveToInventoryExcludeCategoryList)
		{
			if (userItemData.Item.HasCategory(categoryID))
			{
				return;
			}
		}
		pBuilder.AddItemToInventory(userItemData);
	}

	public override void UpdateCreativePoints(int amount)
	{
		base.UpdateCreativePoints(amount);
		pCurrentFarmData.CreativePoints = base.pCurrentCreativePoints;
	}

	public override void AddRoomObject(GameObject inGameObject, UserItemData inUserItemData, GameObject inParentData, bool isUpdateLocalList)
	{
		base.AddRoomObject(inGameObject, inUserItemData, inParentData, isUpdateLocalList);
		if (inGameObject == null)
		{
			return;
		}
		FarmItem component = inGameObject.GetComponent<FarmItem>();
		if (component == null)
		{
			return;
		}
		if (_GridManager != null && component._StampOnGrids)
		{
			_GridManager.AddItem(inGameObject, inUserItemData, !component._CanPlaceOnNotPurchasedSlot);
		}
		if (inParentData != null && component.IsDependent())
		{
			FarmItem component2 = inParentData.GetComponent<FarmItem>();
			if (component2 == null)
			{
				Debug.LogError("Trying to place object on wrong place!!!");
			}
			else if (!component.IsDependentOnFarmItem(component2))
			{
				Debug.LogError("Trying to place object on wrong place!");
			}
			else
			{
				component.SetParent(component2);
			}
		}
		if (component.pChildren == null || component.pChildren.Count <= 0)
		{
			return;
		}
		foreach (FarmItem pChild in component.pChildren)
		{
			AddRoomObject(pChild.gameObject, pChild.pUserItemData, component.gameObject, isUpdateLocalList: true);
		}
	}

	public FarmItem AddFarmItem(GameObject inObject, UserItemData inUserItemData)
	{
		if (inObject == null)
		{
			return null;
		}
		FarmItem component = inObject.GetComponent<FarmItem>();
		if (component == null)
		{
			Debug.LogError("The script missing on object!!!   " + inObject.name);
			return null;
		}
		component.pFarmManager = this;
		component.pUserItemData = inUserItemData;
		component.InitializeRuleSet();
		mFarmItems.Add(component);
		component.Initialize();
		return component;
	}

	public FarmItem RemoveFarmItem(FarmItem inFarmItem)
	{
		if (inFarmItem == null)
		{
			return null;
		}
		if (!mFarmItems.Contains(inFarmItem))
		{
			return null;
		}
		foreach (FarmItem pChild in inFarmItem.pChildren)
		{
			if (mFarmItems.Contains(pChild))
			{
				mFarmItems.Remove(pChild);
			}
		}
		if (mFarmItems.Remove(inFarmItem))
		{
			return inFarmItem;
		}
		return null;
	}

	public FarmItem RemoveFarmItemByInventoryID(int inCommonInventoryID)
	{
		if (inCommonInventoryID <= 0)
		{
			return null;
		}
		foreach (FarmItem mFarmItem in mFarmItems)
		{
			if (mFarmItem != null && mFarmItem.pUserItemData != null && mFarmItem.pUserItemData.UserInventoryID == inCommonInventoryID)
			{
				return RemoveFarmItem(mFarmItem);
			}
		}
		return null;
	}

	public FarmItem FindFarmItem(int inInventoryID)
	{
		if (inInventoryID <= 0)
		{
			return null;
		}
		foreach (FarmItem mFarmItem in mFarmItems)
		{
			if (mFarmItem != null && mFarmItem.pUserItemData != null && mFarmItem.pUserItemData.UserInventoryID == inInventoryID)
			{
				return mFarmItem;
			}
		}
		return null;
	}

	public void OnMouseEnterOnFarmItem(FarmItem inItem)
	{
		if (mMouseHoverItem != inItem)
		{
			mMouseHoverItem = inItem;
		}
	}

	public void OnMouseExitFromFarmItem(FarmItem inItem)
	{
		if (mMouseHoverItem == inItem)
		{
			mMouseHoverItem = null;
		}
	}

	public override void Update()
	{
		base.Update();
		if (!mFarmIntialized && UserInfo.pIsReady)
		{
			mFarmIntialized = true;
			GetSavedFarmItemsInfo();
		}
		if (_GridManager != null && _GridManager.pIsReady)
		{
			if (mRoomsLoaded == _Rooms.Length && mFarmInfoReady && !mIsDeserialized)
			{
				if (mFarmItems != null && mFarmItems.Count > 0)
				{
					Deserialize();
				}
				MyRoomsIntMain.EnableRoomObjectsClickable(isEnable: true);
				if (mObStatus != null)
				{
					mObStatus.pIsReady = true;
				}
			}
			if (mFarmIntialized && !MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt() && AvAvatar.pToolbar != null && AvAvatar.pToolbar.activeSelf && InteractiveTutManager._CurrentActiveTutorialObject == null)
			{
				if (_GridManager.pCurrentLevel >= _RequiredLevelForAnimalTutorial && _AnimalTutorial != null && !_AnimalTutorial.TutorialComplete() && (_BuildmodeTutorial == null || _BuildmodeTutorial.TutorialComplete()))
				{
					_AnimalTutorial.ShowTutorial();
				}
				else if (_GridManager.pCurrentLevel >= _RequiredLevelForBuildmodeTutorial && _HarvestTutorial == null && _BuildmodeTutorial != null && !_BuildmodeTutorial.TutorialComplete())
				{
					_BuildmodeTutorial.ShowTutorial();
				}
				else if (_BuildmodeTutorial == null && _ExpansionTutorial != null && !_ExpansionTutorial.TutorialComplete())
				{
					_ExpansionTutorial.ShowTutorial();
				}
				else if (_ExpansionTutorial == null && _CreativePointTutorial != null && !_CreativePointTutorial.TutorialComplete())
				{
					_CreativePointTutorial.ShowTutorial();
				}
			}
			if (CommonInventoryData.pIsReady && mPairSlotDataRequested)
			{
				mPairSlotDataRequested = false;
				if (MyRoomsIntLevel.pInstance != null && MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
				{
					string myRoomsIntHostID = MyRoomsIntLevel.pInstance.GetMyRoomsIntHostID();
					WsWebService.GetKeyValuePairByUserID(myRoomsIntHostID, ProductSettings.pInstance._PairDataID, OnVisitedUserPairDataReady, myRoomsIntHostID);
				}
				else
				{
					PairData.Load(_OldFarmSlotPairID, UserFarmPairDataEventHandler, null);
				}
			}
		}
		if (mRefreshRankUI)
		{
			UpdateAchievementUI();
		}
		if (mObStatus != null && mObStatus.pIsReady && !mDefaultItemsChecked)
		{
			UserItemPositionListData instance = UserItemPositionList.GetInstance(base.pCurrentRoomID);
			if (instance != null && instance.pList != null)
			{
				mDefaultItemsChecked = true;
				MyRoomData roomData = GetRoomData();
				if (roomData != null)
				{
					MyRoomDefaultItems[] defaultItems = roomData._DefaultItems;
					for (int i = 0; i < defaultItems.Length; i++)
					{
						MyRoomDefaultItem[] items = defaultItems[i]._Items;
						foreach (MyRoomDefaultItem myRoomDefaultItem in items)
						{
							if (myRoomDefaultItem._CheckForMissing)
							{
								AddMissingItem(myRoomDefaultItem._ItemID, instance);
							}
						}
					}
				}
			}
		}
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pData != SanctuaryManager.pCurPetData)
		{
			UnityEngine.Object.Destroy(SanctuaryManager.pCurPetInstance.gameObject);
			SanctuaryManager.pCurPetInstance = null;
			SanctuaryManager.CreatePet(SanctuaryManager.pCurPetData, SanctuaryManager.pInstance._PetOffScreenPosition, Quaternion.identity, base.gameObject, "Basic");
		}
	}

	private void UserFarmPairDataEventHandler(bool success, PairData inData, object inUserData)
	{
		if (inData == null)
		{
			Debug.LogError("Error!! User Pair data not initialized. ");
			return;
		}
		mPairOldSlotData = inData;
		ConvertFarmSlotsToInventory();
	}

	private void ConvertFarmSlotsToInventory()
	{
		int value = _GridManager.GetGridLevelInfo(_GridManager.pCurrentLevel)._Value;
		UserItemPosition userItemPosition = Array.Find(UserItemPositionList.GetList(pCurrentFarmData.RoomID), (UserItemPosition x) => x.ItemID == _FarmSlotItemID);
		if (CommonInventoryData.pInstance.FindItem(_FarmSlotItemID) == null)
		{
			if (userItemPosition == null)
			{
				CommonInventoryData.pInstance.AddItem(_FarmSlotItemID, value);
				CommonInventoryData.pInstance.Save(ConvertFarmSlotsToUserItemPosition, null);
			}
			else
			{
				FarmSlotsConversionComplete();
			}
		}
		else if (userItemPosition == null)
		{
			ConvertFarmSlotsToUserItemPosition(inSaveSuccess: true, null);
		}
		else
		{
			FarmSlotsConversionComplete();
		}
	}

	private void ConvertFarmSlotsToUserItemPosition(bool inSaveSuccess, object inUserData)
	{
		if (inSaveSuccess)
		{
			Pair pair = mPairOldSlotData.FindByKey("UnlockedGC");
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(_FarmSlotItemID);
			if (userItemData == null)
			{
				FarmSlotsConversionComplete();
				return;
			}
			if (pair == null)
			{
				Vector3[] defaultFarmSlotPositionList = _DefaultFarmSlotPositionList;
				foreach (Vector3 inPosition in defaultFarmSlotPositionList)
				{
					AddRoomObject(new GameObject(), userItemData, inPosition, Vector3.zero, null, isUpdateLocalList: false);
					CommonInventoryData.pInstance.RemoveItem(userItemData, 1);
				}
			}
			else
			{
				if (string.IsNullOrEmpty(pair.PairValue))
				{
					FarmSlotsConversionComplete();
					return;
				}
				string[] array = pair.PairValue.Split('|');
				if (array == null || array.Length == 0)
				{
					FarmSlotsConversionComplete();
					return;
				}
				string[] array2 = array;
				foreach (string s in array2)
				{
					int result = -1;
					if (int.TryParse(s, out result))
					{
						GridCell gridCellFromID = _GridManager.GetGridCellFromID(result);
						if (gridCellFromID != null)
						{
							AddRoomObject(inPosition: new Vector3(gridCellFromID._GridCellRect.center.x, base.transform.position.y, gridCellFromID._GridCellRect.center.y), inGameObject: new GameObject(), inUserItemData: userItemData, inRotation: Vector3.zero, inParentData: null, isUpdateLocalList: false);
							CommonInventoryData.pInstance.RemoveItem(userItemData, 1);
						}
					}
				}
			}
			UserItemPositionList.Save(pCurrentFarmData.RoomID, OnUserItemPositionSaveEvent);
		}
		else
		{
			int value = _GridManager.GetGridLevelInfo(_GridManager.pCurrentLevel)._Value;
			CommonInventoryData.pInstance.RemoveItem(_FarmSlotItemID, updateServer: false, value);
			FarmSlotsConversionFailed();
			Debug.LogError("Failed to convert Farm Slots into Inventory items");
		}
	}

	private void OnUserItemPositionSaveEvent(WsServiceType inType, WsServiceEvent inEvent, string inRoomID)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			if (inType == WsServiceType.SET_USER_ITEM_POSITION_LIST)
			{
				FarmSlotsConversionComplete(save: true, recreateItems: false);
				UserItemPositionList.Init(UserInfo.pInstance.UserID, inRoomID, OnUserItemPositionEvent);
			}
			break;
		case WsServiceEvent.ERROR:
			Debug.LogError("Failed to set User Item position for Farm Slots");
			FarmSlotsConversionFailed();
			break;
		}
	}

	private void OnVisitedUserPairDataReady(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			VisitedUserPairDataEventHandler(null, null);
			break;
		case WsServiceEvent.COMPLETE:
			if (inType == WsServiceType.GET_KEY_VALUE_PAIR_BY_USER_ID)
			{
				if (inObject != null)
				{
					PairData pairData = (PairData)inObject;
					pairData.Init();
					VisitedUserPairDataEventHandler(pairData, inUserData);
				}
				else
				{
					PairData inData = new PairData();
					VisitedUserPairDataEventHandler(inData, inUserData);
				}
			}
			break;
		}
	}

	private void VisitedUserPairDataEventHandler(PairData inData, object inUserData)
	{
		if (inData == null)
		{
			Debug.LogError("Error!! Visited User Pair data not initialized. ");
		}
		else if (inUserData != null && MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
		{
			string myRoomsIntHostID = MyRoomsIntLevel.pInstance.GetMyRoomsIntHostID();
			if (inData.FindByKey("SlotsConverted") == null)
			{
				WsWebService.GetKeyValuePairByUserID(myRoomsIntHostID, _OldFarmSlotPairID, OnVisitedUserFarmPairDataReady, null);
			}
			else
			{
				FarmSlotsConversionFailed();
			}
		}
	}

	private void OnVisitedUserFarmPairDataReady(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			VisitedUserPairDataEventHandler(null, null);
			break;
		case WsServiceEvent.COMPLETE:
			if (inType == WsServiceType.GET_KEY_VALUE_PAIR_BY_USER_ID)
			{
				if (inObject != null)
				{
					PairData pairData = (PairData)inObject;
					pairData.Init();
					VisitedUserFarmPairDataEventHandler(pairData, inUserData);
				}
				else
				{
					PairData inData = new PairData();
					VisitedUserFarmPairDataEventHandler(inData, inUserData);
				}
			}
			break;
		}
	}

	private void VisitedUserFarmPairDataEventHandler(PairData inData, object inUserData)
	{
		if (inData == null)
		{
			Debug.LogError("Error!! Visited User Pair data not initialized. ");
			return;
		}
		mPairOldSlotData = inData;
		if (MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
		{
			WsWebService.GetItemData(_FarmSlotItemID, OnItemDataLoaded, null);
		}
	}

	private void OnItemDataLoaded(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			string[] array = ((ItemData)inObject).AssetName.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnFarmSlotLoaded, typeof(GameObject));
			break;
		}
		case WsServiceEvent.ERROR:
			FarmSlotsConversionFailed();
			break;
		}
	}

	private void OnFarmSlotLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			if (inObject != null)
			{
				Pair pair = mPairOldSlotData.FindByKey("UnlockedGC");
				if (pair == null)
				{
					Vector3[] defaultFarmSlotPositionList = _DefaultFarmSlotPositionList;
					foreach (Vector3 position in defaultFarmSlotPositionList)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject, position, Quaternion.identity);
						DisableObjectClickable(gameObject);
					}
				}
				else if (!string.IsNullOrEmpty(pair.PairValue))
				{
					string[] array = pair.PairValue.Split('|');
					if (array != null || array.Length != 0)
					{
						string[] array2 = array;
						foreach (string s in array2)
						{
							int result = -1;
							if (int.TryParse(s, out result))
							{
								GridCell gridCellFromID = _GridManager.GetGridCellFromID(result);
								if (gridCellFromID != null)
								{
									GameObject gameObject2 = UnityEngine.Object.Instantiate(position: new Vector3(gridCellFromID._GridCellRect.center.x, base.transform.position.y, gridCellFromID._GridCellRect.center.y), original: (GameObject)inObject, rotation: Quaternion.identity);
									DisableObjectClickable(gameObject2);
								}
							}
						}
					}
				}
			}
			FarmSlotsConversionComplete(save: false);
		}
		if (inEvent == RsResourceLoadEvent.ERROR)
		{
			FarmSlotsConversionFailed();
		}
	}

	private void DisableObjectClickable(GameObject gameObject)
	{
		ObClickable component = gameObject.GetComponent<ObClickable>();
		if (component != null)
		{
			component._Active = false;
		}
		MyRoomItem component2 = gameObject.GetComponent<MyRoomItem>();
		if (component2 != null)
		{
			component2.enabled = false;
		}
	}

	private void FarmSlotsConversionFailed()
	{
		mFarmSlotConversionDone = true;
		if (mDictRoomData.ContainsKey(pCurrentFarmData.RoomID))
		{
			mDictRoomData[pCurrentFarmData.RoomID].RecreateRoomItems();
		}
	}

	private void FarmSlotsConversionComplete(bool save = true, bool recreateItems = true)
	{
		if (save)
		{
			ProductData.pPairData.SetValue("SlotsConverted", "1");
			ProductData.Save();
		}
		mFarmSlotConversionDone = true;
		if (recreateItems && mDictRoomData.ContainsKey(pCurrentFarmData.RoomID))
		{
			mDictRoomData[pCurrentFarmData.RoomID].RecreateRoomItems();
		}
	}

	public override void OnUserItemPositionEvent(WsServiceType inType, WsServiceEvent inEvent, string inRoomID)
	{
		switch (inType)
		{
		case WsServiceType.GET_USER_ITEM_POSITION_LIST:
			if (inEvent == WsServiceEvent.COMPLETE)
			{
				if (!mFarmSlotConversionDone && pCurrentFarmData != null && pCurrentFarmData.RoomID.Equals(""))
				{
					mPairSlotDataRequested = true;
				}
				else
				{
					base.OnUserItemPositionEvent(inType, inEvent, inRoomID);
				}
			}
			break;
		case WsServiceType.SET_USER_ITEM_POSITION_LIST:
			if (inEvent == WsServiceEvent.COMPLETE)
			{
				base.OnUserItemPositionEvent(inType, inEvent, inRoomID);
			}
			break;
		}
	}

	public MyRoomData GetRoomData()
	{
		MyRoomData[] rooms = _Rooms;
		foreach (MyRoomData myRoomData in rooms)
		{
			if (myRoomData._RoomID == base.pCurrentRoomID)
			{
				return myRoomData;
			}
		}
		return null;
	}

	private void AddMissingItem(int inItemID, UserItemPositionListData uipData)
	{
		UserItemPosition userItemPosition = null;
		UserItemPosition[] pList = uipData.pList;
		foreach (UserItemPosition userItemPosition2 in pList)
		{
			if ((userItemPosition2.ItemID.HasValue && userItemPosition2.ItemID.Value == inItemID) || (userItemPosition2.Item != null && userItemPosition2.Item.ItemID == inItemID))
			{
				userItemPosition = userItemPosition2;
				break;
			}
		}
		bool flag = userItemPosition != null;
		UserItemData userItemData = CommonInventoryData.pInstance.FindItem(inItemID);
		if (userItemData != null)
		{
			if (flag)
			{
				CommonInventoryData.pInstance.RemoveItem(inItemID, updateServer: true, userItemData.Quantity);
			}
			else if (inItemID == _WellInventoryID)
			{
				PlaceWellItemInRoom(inItemID);
			}
		}
		else if (!flag)
		{
			CommonInventoryData.pInstance.AddItem(inItemID, updateServer: false);
			CommonInventoryData.pInstance.Save(AwardMissingItemsEventHandler, inItemID);
		}
	}

	private void PlaceWellItemInRoom(int inItemID)
	{
		if (inItemID != _WellInventoryID)
		{
			return;
		}
		UserItemData userItemData = CommonInventoryData.pInstance.FindItem(inItemID);
		if (userItemData != null)
		{
			AddRoomObject(new GameObject(), userItemData, _WellPosition, _WellOrientation, null, isUpdateLocalList: true);
			CommonInventoryData.pInstance.RemoveItem(userItemData, userItemData.Quantity);
			if (UserItemPositionList.Save(base.pCurrentRoomID, OnAddDefaultUserItemPositionEvent))
			{
				MyRoomsIntMain.pDisableBuildMode = true;
			}
		}
	}

	public void AwardMissingItemsEventHandler(bool inSaveSuccess, object inUserData)
	{
		if (inSaveSuccess)
		{
			int num = (int)inUserData;
			if (num == _WellInventoryID)
			{
				PlaceWellItemInRoom(num);
			}
		}
	}

	private void OnAddDefaultUserItemPositionEvent(WsServiceType inType, WsServiceEvent inEvent, string inRoomID)
	{
		switch (inType)
		{
		case WsServiceType.SET_USER_ITEM_POSITION_LIST:
			switch (inEvent)
			{
			case WsServiceEvent.ERROR:
				MyRoomsIntMain.pDisableBuildMode = false;
				break;
			case WsServiceEvent.COMPLETE:
				MyRoomsIntMain.pDisableBuildMode = false;
				UserItemPositionList.Init(UserInfo.pInstance.UserID, inRoomID, OnAddDefaultUserItemPositionEvent);
				break;
			}
			break;
		case WsServiceType.GET_USER_ITEM_POSITION_LIST:
			if ((inEvent == WsServiceEvent.COMPLETE || inEvent == WsServiceEvent.ERROR) && base.pDictRoomData.ContainsKey(inRoomID))
			{
				base.pDictRoomData[inRoomID].RecreateRoomItems();
			}
			break;
		}
	}

	private void Deserialize()
	{
		mIsDeserialized = true;
		string[] separator = new string[1] { "#" };
		string[] separator2 = new string[1] { ";" };
		string[] array = null;
		string empty = string.Empty;
		bool flag = false;
		if (mFarmPairData != null)
		{
			Pair pair = mFarmPairData.FindByKey("FarmItems");
			if (pair != null)
			{
				empty = pair.PairValue;
				if (!empty.Equals(string.Empty))
				{
					array = empty.Split(separator, StringSplitOptions.None);
					if (array != null && array.Length != 0 && array[0] == mPairDataHeaderV3)
					{
						string[] array2 = array[1].Split(separator2, StringSplitOptions.RemoveEmptyEntries);
						if (array2 != null && array2.Length != 0)
						{
							int num = Convert.ToInt32(array2[0]);
							int num2 = Convert.ToInt32(array2[1]);
							if (num != _Removables.Count || num2 != _NumberofDynamicRemovables)
							{
								InitializeRemovables();
								flag = true;
							}
						}
						if (array.Length > 2 && !string.IsNullOrEmpty(array[2]))
						{
							DeserializeRemovables(array[2]);
							flag = true;
						}
					}
				}
			}
		}
		if (!flag)
		{
			InitializeRemovables();
		}
	}

	private void DeserializeRemovables(string inRemovableData)
	{
		string[] separator = new string[1] { "$" };
		string[] separator2 = new string[1] { ";" };
		string[] array = inRemovableData.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			string[] array2 = text.Split(separator2, StringSplitOptions.RemoveEmptyEntries);
			if (array2 == null || array2.Length == 0)
			{
				continue;
			}
			int num = Convert.ToInt32(array2[0]);
			bool flag = false;
			foreach (RemovableInfo removable in _Removables)
			{
				if (removable._ID == num)
				{
					flag = true;
					removable.pDeserializeString = text;
					removable.Load(this);
					break;
				}
			}
			if (!flag)
			{
				int num2 = Convert.ToInt32(array2[1]);
				float x = Convert.ToSingle(array2[2]);
				float y = Convert.ToSingle(array2[3]);
				float z = Convert.ToSingle(array2[4]);
				RemovableInfo removableInfo = null;
				removableInfo = ((num2 != 1) ? new RemovableInfo(RemovableType.ROCK, num, new Vector3(x, y, z)) : new RemovableInfo(RemovableType.TREE, num, new Vector3(x, y, z)));
				removableInfo.pDeserializeString = text;
				removableInfo.Load(this);
			}
		}
	}

	private void GetSavedFarmItemsInfo()
	{
		PairData.Load(_PairID, FarmItemInfoServerCallback, null);
	}

	private void FarmItemInfoServerCallback(bool success, PairData inData, object inUserData)
	{
		mFarmInfoReady = true;
		mFarmPairData = inData;
	}

	private void SetParentChildRelation()
	{
		UserItemPosition[] list = UserItemPositionList.GetList(pCurrentFarmID);
		if (list == null)
		{
			return;
		}
		UserItemPosition[] array = list;
		foreach (UserItemPosition userItemPosition in array)
		{
			if (userItemPosition.Item == null || userItemPosition.Item.AssetName.Split('/').Length != 3 || !userItemPosition.ParentID.HasValue || userItemPosition._GameObject == null)
			{
				continue;
			}
			FarmItem component = userItemPosition._GameObject.GetComponent<FarmItem>();
			UserItemPosition[] list2 = UserItemPositionList.GetList(pCurrentFarmID);
			foreach (UserItemPosition userItemPosition2 in list2)
			{
				if (userItemPosition.ParentID.Value != userItemPosition2.UserItemPositionID)
				{
					continue;
				}
				GameObject gameObject = userItemPosition2._GameObject;
				if (gameObject == null)
				{
					break;
				}
				userItemPosition._GameObject.transform.parent = gameObject.transform;
				if (!(component == null))
				{
					FarmItem component2 = gameObject.GetComponent<FarmItem>();
					if (component != null)
					{
						component.SetParent(component2);
					}
				}
				break;
			}
		}
	}

	public void SaveRemovables()
	{
		string empty = string.Empty;
		Transform obj = base.transform.Find("Removables");
		empty += mPairDataHeaderV3;
		empty += "#";
		empty = empty + _Removables.Count + ";" + _NumberofDynamicRemovables;
		empty += "#";
		foreach (Transform item in obj)
		{
			Removable component = item.GetComponent<Removable>();
			empty = empty + "$" + component.Serialize();
		}
		if (mFarmPairData != null)
		{
			mFarmPairData.SetValueAndSave("FarmItems", empty);
		}
	}

	private void UpdateFarmArea(bool buildMode)
	{
		if (_FarmGameArea != null)
		{
			_FarmGameArea.SetActive(buildMode);
		}
	}

	public override void OnBuildModeChanged(bool inBuildMode)
	{
		UpdateFarmArea(inBuildMode);
		_GridManager.SetBuildMode(inBuildMode);
		if (mFarmItems != null && mFarmItems.Count > 0)
		{
			foreach (FarmItem mFarmItem in mFarmItems)
			{
				if (mFarmItem != null)
				{
					mFarmItem.UpdateContextIcon();
					mFarmItem.OnBuildModeChanged(inBuildMode);
				}
			}
		}
		if (_BuildModeAvatarMarker != null)
		{
			AvAvatar.SetPosition(_BuildModeAvatarMarker);
		}
	}

	public void RemoveItemFromInventory(int inItemID)
	{
		if (CommonInventoryData.pInstance.RemoveItem(inItemID, updateServer: false) != 0 || pBuilder == null || pBuilder.pBuildModeMenu == null)
		{
			return;
		}
		for (int i = 0; i < pBuilder.pBuildModeMenu.GetNumItems(); i++)
		{
			KAWidget kAWidget = pBuilder.pBuildModeMenu.FindItemAt(i);
			if (kAWidget != null && kAWidget.GetUserData() != null)
			{
				MyRoomBuilderItemData myRoomBuilderItemData = (MyRoomBuilderItemData)kAWidget.GetUserData();
				if (myRoomBuilderItemData != null && myRoomBuilderItemData._ItemID == inItemID)
				{
					pBuilder.pBuildModeMenu.RemoveWidget(kAWidget);
					break;
				}
			}
		}
	}

	public void ShowDialog(string assetName, string dbName, LocaleString title, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, LocaleString text, GameObject messageObject)
	{
		ShowDialog(assetName, dbName, title, yesMessage, noMessage, okMessage, closeMessage, destroyDB, text.GetLocalizedString(), messageObject);
	}

	public void ShowDialog(string assetName, string dbName, LocaleString title, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, string text, GameObject messageObject)
	{
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB.Destroy();
		}
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB(assetName, dbName);
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB.SetMessage(messageObject, yesMessage, noMessage, okMessage, closeMessage);
			mKAUIGenericDB.SetDestroyOnClick(destroyDB);
			mKAUIGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(yesMessage), !string.IsNullOrEmpty(noMessage), !string.IsNullOrEmpty(okMessage), !string.IsNullOrEmpty(closeMessage));
			mKAUIGenericDB.SetText(text, interactive: false);
			mKAUIGenericDB.SetTitle(title.GetLocalizedString());
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
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

	public void OnStoreLoaded(List<StoreData> inStoreData, object inUserData)
	{
		mStoreDataList = inStoreData;
	}

	private void OnOK()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
	}

	public void UpdateAchievementUI()
	{
		mRefreshRankUI = true;
		UserAchievementInfo userAchievementInfo = null;
		if (UserRankData.pIsReady)
		{
			userAchievementInfo = UserRankData.GetUserAchievementInfoByType(9);
			if (userAchievementInfo != null)
			{
				mRefreshRankUI = mFarmToolbar.UpdateUIData(_GridManager.pCurrentLevel.ToString(), userAchievementInfo.AchievementPointTotal.ToString());
			}
			else
			{
				mRefreshRankUI = mFarmToolbar.UpdateUIData("1", "1");
			}
		}
	}

	public void UpdateUIOnEnable()
	{
		UserAchievementInfo userAchievementInfo = null;
		if (UserRankData.pIsReady)
		{
			userAchievementInfo = UserRankData.GetUserAchievementInfoByType(9);
		}
		if (userAchievementInfo != null)
		{
			mFarmToolbar.UpdateUIData(_GridManager.pCurrentLevel.ToString(), userAchievementInfo.AchievementPointTotal.ToString());
		}
	}

	public void PlayActionSFX(string inActionName, string stageName = null)
	{
		FarmingSFX[] farmingActionSFX = _FarmingActionSFX;
		foreach (FarmingSFX farmingSFX in farmingActionSFX)
		{
			bool flag = (farmingSFX._CheckForExactMatch ? farmingSFX._ActionName.Equals(inActionName) : inActionName.Contains(farmingSFX._ActionName));
			if (flag && !string.IsNullOrEmpty(stageName) && !string.IsNullOrEmpty(farmingSFX._StageName))
			{
				flag = farmingSFX._StageName.Equals(stageName);
			}
			if (flag)
			{
				SnChannel.Play(farmingSFX._Clip, "SFX_Pool", inForce: true);
				break;
			}
		}
	}

	public void PlayTimerSFX(bool playSfx)
	{
		if (playSfx)
		{
			SnChannel.Play(_StatusTimerSFX, "TimerStatus_Pool", inForce: true);
		}
		else
		{
			SnChannel.StopPool("TimerStatus_Pool");
		}
		mIsPlayingTimerSFX = playSfx;
	}

	public bool HandleFirstTimeAction(string inActionName, FarmItem callbackObj)
	{
		if (mFarmPairData != null && !base.pIsBuildMode)
		{
			FirstTimeAction[] farmingActions = _FarmingActions;
			foreach (FirstTimeAction firstTimeAction in farmingActions)
			{
				if (firstTimeAction == null || mFarmPairData.GetBoolValue(firstTimeAction._PairDataName, defaultVal: false))
				{
					continue;
				}
				ActionData[] actions = firstTimeAction._Actions;
				foreach (ActionData actionData in actions)
				{
					if (!actionData._Name.Equals(inActionName))
					{
						continue;
					}
					bool skipStep = false;
					if (actionData._SkipOnConsumableIDs.Count > 0)
					{
						List<int> consumableIDs = actionData._SkipOnConsumableIDs;
						OnParseConsumableInventoryData parseDelegate = delegate(UserItemData userItemData, ItemStateCriteriaConsumable consumable)
						{
							if (userItemData != null && consumableIDs.Contains(userItemData.Item.ItemID))
							{
								skipStep = true;
							}
						};
						callbackObj.ParseConsumablesInventoryData(parseDelegate);
					}
					if (!skipStep)
					{
						DisplayBoxData dBData = firstTimeAction._DBData;
						string text = (string.IsNullOrEmpty(dBData._BundleName) ? _DialogAssetName : dBData._BundleName);
						ShowDialog(text, text, _FarmingDBTitleText, dBData._YesMessage, dBData._NoMessage, dBData._OkMessage, dBData._CloseMessage, dBData._DestroyOnClick, firstTimeAction._DisplayText, callbackObj.gameObject);
					}
					mFarmPairData.SetValueAndSave(firstTimeAction._PairDataName, true.ToString());
					return !skipStep;
				}
			}
		}
		return false;
	}

	public string GetItemHelpText(int itemID)
	{
		FarmItemHelp[] farmingItemHelpTexts = _FarmingItemHelpTexts;
		foreach (FarmItemHelp farmItemHelp in farmingItemHelpTexts)
		{
			if (farmItemHelp._ID == itemID)
			{
				return farmItemHelp._Text.GetLocalizedString();
			}
		}
		return string.Empty;
	}

	public override void OnWaitListCompleted()
	{
		if (_UserNotifyObject != null)
		{
			_UserNotifyObject.SetActive(value: false);
		}
	}

	public void CheckHarvestAchievement(int harvestedItemID)
	{
		HarvestAchievement harvestAchievement = _HarvestAchievementData.Find((HarvestAchievement data) => data._ItemID == harvestedItemID);
		if (harvestAchievement != null)
		{
			AchievementTask achievementTask = new AchievementTask(harvestAchievement._AchievementID);
			UserAchievementTask.Set(achievementTask, UserProfile.pProfileData.GetGroupAchievement(harvestAchievement._ClanAchievementID));
		}
	}

	public List<ContextData> GetDataList(FarmItemStage[] stages)
	{
		List<ContextData> list = new List<ContextData>();
		foreach (FarmItemStage farmItemStage in stages)
		{
			if (farmItemStage == null)
			{
				continue;
			}
			foreach (ContextSensitiveState farmItemContextDatum in farmItemStage.GetFarmItemContextData())
			{
				string[] currentContextNamesList = farmItemContextDatum._CurrentContextNamesList;
				foreach (string contextName in currentContextNamesList)
				{
					ContextData contextData = _DataList.Find((ContextData data) => data._Name.Equals(contextName));
					if (contextData != null)
					{
						list.Add(contextData.Clone() as ContextData);
					}
				}
			}
		}
		return list;
	}

	public ContextData GetContextData(string contextName)
	{
		ContextData contextData = _DataList.Find((ContextData data) => data._Name.Equals(contextName));
		if (contextData == null)
		{
			return contextData;
		}
		return contextData.Clone() as ContextData;
	}

	public override void OnUserItemSaveComplete(bool saveSuccess)
	{
		if (_BuildmodeTutorial != null && InteractiveTutManager._CurrentActiveTutorialObject != null && InteractiveTutManager._CurrentActiveTutorialObject == _BuildmodeTutorial.gameObject && CommonInventoryData.pInstance != null)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "BuildModeExit");
		}
		base.OnUserItemSaveComplete(saveSuccess);
		if (pBuilder != null)
		{
			pBuilder.OnOutOfBuildMode(saveSuccess);
		}
		if (!saveSuccess)
		{
			AvAvatar.pState = AvAvatarState.IDLE;
		}
	}

	public override void OnUserItemSaveError()
	{
		ShowDialog(_DialogAssetName, "SaveError", _FarmingDBTitleText, string.Empty, string.Empty, "OnSaveErrorDBClose", string.Empty, destroyDB: true, _UserItemSaveErrorText, base.gameObject);
	}
}
