using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

public class FarmItem : FarmItemBase, IAdResult
{
	public class LoadItemCallBackData
	{
		public StringBuilder StringBuilder;

		public string HelpTexts;
	}

	public const string _NoInteractionStageName = "NoInteraction";

	public string _SpeedupActionName = "Boost";

	public string _ExpiryTimerCSMActionText = "ExpiryTimer";

	public LevelProgressInfo _LevelProgressInfo;

	public FarmItemStage[] _Stages;

	public ContextSensitiveState _StatusContextSensitiveState;

	public ContextSensitiveState _LoadingContextSensitiveState;

	public List<StateDetails> _StateDetails = new List<StateDetails>();

	public int _SpeedupStoreID = 90;

	public int[] _DependentItemIDs;

	public bool _CanMoveToInventory = true;

	public GameObject _DefaultMeshObject;

	public DefaultFarmItem[] _DefaultItems;

	public bool _SnapToGrids = true;

	public bool _CanPlaceOnNotPurchasedSlot;

	public bool _CanPlaceOnPurchasedSlot;

	public int[] _StackableItems;

	public ContextActionFx[] _ContextActionsFx;

	public SnChannel _AmbientSFXChannel;

	private FarmItemStage mCurrentStage;

	protected DateTime mStageStartTime;

	private bool mIsRuleSetInitialized;

	private UserItemState mCurrentItemState;

	private bool mIsStateSet;

	private List<ItemState> mItemStates;

	protected List<FarmItem> mChildren = new List<FarmItem>();

	protected bool mInitialized;

	private bool mIsWaitingForWsCall;

	protected FarmItemState mFarmItemState = new FarmItemState();

	protected bool mIsSpeedUp;

	protected List<FarmItemClickable> mFarmItemClickables = new List<FarmItemClickable>();

	protected Dictionary<string, MethodInfo> mContextItemBasedUpdates = new Dictionary<string, MethodInfo>();

	private bool mAdWatched;

	public FarmItemStage pCurrentStage
	{
		get
		{
			return mCurrentStage;
		}
		set
		{
			mCurrentStage = value;
		}
	}

	public bool pIsRuleSetInitialized
	{
		get
		{
			return mIsRuleSetInitialized;
		}
		set
		{
			mIsRuleSetInitialized = value;
		}
	}

	public UserItemState pCurrentItemState
	{
		get
		{
			return mCurrentItemState;
		}
		set
		{
			mCurrentItemState = value;
		}
	}

	public bool pIsStateSet
	{
		get
		{
			return mIsStateSet;
		}
		set
		{
			mIsStateSet = value;
		}
	}

	public List<ItemState> pItemStates
	{
		get
		{
			return mItemStates;
		}
		set
		{
			mItemStates = value;
		}
	}

	public List<FarmItem> pChildren => mChildren;

	public bool pInitialized => mInitialized;

	protected bool pIsWaitingForWSCall
	{
		get
		{
			return mIsWaitingForWsCall;
		}
		set
		{
			mIsWaitingForWsCall = value;
		}
	}

	protected bool pIsBuildMode
	{
		get
		{
			if (!(base.pFarmManager == null))
			{
				return base.pFarmManager.pIsBuildMode;
			}
			return true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		mFarmItemClickables.Add(GetComponent<FarmItemClickable>());
	}

	protected override void Start()
	{
		base.Start();
		Initialize();
		UpdateCollider();
	}

	public virtual void InitializeRuleSet()
	{
		foreach (ItemState itemState in base.pUserItemData.Item.ItemStates)
		{
			StateDetails stateDetails = new StateDetails();
			stateDetails._Name = itemState.Name;
			stateDetails._ID = itemState.ItemStateID;
			stateDetails._Order = itemState.Order;
			stateDetails._CompletionAction = itemState.Rule.CompletionAction;
			foreach (ItemStateCriteria criteria in itemState.Rule.Criterias)
			{
				if (criteria.Type == ItemStateCriteriaType.Length)
				{
					ItemStateCriteriaLength itemStateCriteriaLength = (stateDetails._CriteriaLength = (ItemStateCriteriaLength)criteria);
					if (pIsStateSet && pCurrentStage != null && itemState.ItemStateID == pCurrentStage._ID)
					{
						pCurrentStage._Duration = itemStateCriteriaLength.Period;
					}
				}
				else if (criteria.Type == ItemStateCriteriaType.ConsumableItem)
				{
					ItemStateCriteriaConsumable item = (ItemStateCriteriaConsumable)criteria;
					stateDetails._CriteriaConsumables.Add(item);
				}
				else if (criteria.Type == ItemStateCriteriaType.ReplenishableItem)
				{
					ItemStateCriteriaReplenishable criteriaReplenishable = (ItemStateCriteriaReplenishable)criteria;
					stateDetails._CriteriaReplenishable = criteriaReplenishable;
				}
				else if (criteria.Type == ItemStateCriteriaType.StateExpiry)
				{
					ItemStateCriteriaExpiry criteriaExpiry = (ItemStateCriteriaExpiry)criteria;
					stateDetails._CriteriaExpiry = criteriaExpiry;
				}
				pIsRuleSetInitialized = true;
			}
			_StateDetails.Add(stateDetails);
		}
		Refresh();
	}

	public virtual void Initialize()
	{
		if (base.pFarmManager == null)
		{
			base.pFarmManager = MyRoomsIntMain.pInstance as FarmManager;
		}
		if (pCurrentStage == null && _Stages != null && _Stages.Length != 0)
		{
			pCurrentStage = _Stages[0];
			SwapStageMesh();
			mStageStartTime = ServerTime.pCurrentTime;
			FillContextData();
		}
		mInitialized = true;
	}

	protected void FillContextData()
	{
		FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
		if (!farmManager)
		{
			return;
		}
		foreach (ContextData cd in farmManager.GetDataList(_Stages))
		{
			if (_DataList.Find((ContextData dle) => dle._Name == cd._Name) == null)
			{
				_DataList.Add(cd);
			}
		}
		string[] currentContextNamesList = _StatusContextSensitiveState._CurrentContextNamesList;
		foreach (string actionName in currentContextNamesList)
		{
			if (_DataList.Find((ContextData d) => d._Name == actionName) == null)
			{
				_DataList.Add(farmManager.GetContextData(actionName));
			}
		}
		currentContextNamesList = _LoadingContextSensitiveState._CurrentContextNamesList;
		foreach (string actionName2 in currentContextNamesList)
		{
			if (_DataList.Find((ContextData d) => d._Name == actionName2) == null)
			{
				_DataList.Add(farmManager.GetContextData(actionName2));
			}
		}
		if (!string.IsNullOrEmpty(_ExpiryTimerCSMActionText) && _DataList.Find((ContextData d) => d._Name == _ExpiryTimerCSMActionText) == null)
		{
			_DataList.Add(farmManager.GetContextData(_ExpiryTimerCSMActionText));
		}
		UpdateChildrenData();
		ConfigureAdBtns();
	}

	protected void ConfigureAdBtns()
	{
		if (base.pFarmManager != null && AdManager.pInstance.AdSupported(base.pFarmManager._AdEventType, AdType.REWARDED_VIDEO))
		{
			foreach (FarmManager.AdContextData adContextData in base.pFarmManager._AdContextDataList)
			{
				if (adContextData == null || adContextData._ContextData == null || _DataList.Exists((ContextData x) => x._Name == adContextData._ContextData._Name))
				{
					continue;
				}
				FarmItemStage[] array = Array.FindAll(_Stages, (FarmItemStage x) => x._PropName == adContextData._PropName);
				if (array.Length == 0)
				{
					continue;
				}
				FarmItemStage[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					foreach (FarmItemContextData item in array2[i]._ContextData.FindAll((FarmItemContextData x) => !x._IsBuildMode))
					{
						item._ContextSensitiveState._CurrentContextNamesList = item._ContextSensitiveState._CurrentContextNamesList.Add(adContextData._ContextData._Name);
					}
				}
				_DataList.Add(adContextData._ContextData);
			}
		}
		_StatusContextSensitiveState._CurrentContextNamesList = _StatusContextSensitiveState._CurrentContextNamesList.Add("AdBtn");
	}

	protected override void Update()
	{
		base.Update();
		ProcessCurrentStage();
		if (!(base.pFarmManager != null) || !base.pFarmManager._PlayStatusTimerSFX)
		{
			return;
		}
		if (base.pShowStatus && base.pUI != null)
		{
			if (!base.pFarmManager.pIsPlayingTimerSFX)
			{
				base.pFarmManager.pFarmItemWithStatusTimer = this;
				base.pFarmManager.PlayTimerSFX(playSfx: true);
			}
		}
		else if (base.pFarmManager.pIsPlayingTimerSFX && base.pFarmManager.pFarmItemWithStatusTimer == this)
		{
			base.pFarmManager.PlayTimerSFX(playSfx: false);
		}
	}

	protected override void UpdateData(ref ContextSensitiveState[] inStatesArrData)
	{
		if (!CanProcessUpdateData())
		{
			return;
		}
		float timeLeftInCurrentStage = GetTimeLeftInCurrentStage();
		if (base.pFarmManager != null)
		{
			if (pIsWaitingForWSCall)
			{
				ShowLoading(ref inStatesArrData);
			}
			else
			{
				List<ContextSensitiveState> farmItemContextData = pCurrentStage.GetFarmItemContextData(base.pFarmManager.pIsBuildMode);
				inStatesArrData = GetSensitiveData(farmItemContextData, timeLeftInCurrentStage);
			}
		}
		UpdateScaleData(ref inStatesArrData);
	}

	protected virtual bool CanProcessUpdateData()
	{
		if (pIsRuleSetInitialized)
		{
			return pIsStateSet;
		}
		return false;
	}

	public virtual void ProcessClick()
	{
	}

	protected override void OnActivate()
	{
		if (pCurrentStage != null && !base.pFarmManager.HandleFirstTimeAction(pCurrentStage._Name, this))
		{
			base.OnActivate();
			FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
			if (farmManager.pIsBuildMode)
			{
				farmManager.pBuilder.HighlightSelectedFarmItem();
			}
			SnChannel.Play(base.pFarmManager._OnCSMActivationSFX, "SFX_Pool", inForce: true);
		}
	}

	public virtual void ContextItemBasedUpdate(string name)
	{
		if (!mContextItemBasedUpdates.ContainsKey(name))
		{
			mContextItemBasedUpdates[name] = base.pFarmManager.GetMethod(this, name);
		}
		if (mContextItemBasedUpdates[name] != null)
		{
			mContextItemBasedUpdates[name].Invoke(this, null);
		}
	}

	public virtual void ProcessCurrentStage()
	{
		if (pCurrentStage == null || pCurrentStage._UserItemState == null || pIsWaitingForWSCall)
		{
			return;
		}
		float timeLeftInCurrentStage = GetTimeLeftInCurrentStage();
		if (base.pShowStatus)
		{
			int i = 0;
			for (int num = _StatusContextSensitiveState._CurrentContextNamesList.Length; i < num; i++)
			{
				ContextItemBasedUpdate(_StatusContextSensitiveState._CurrentContextNamesList[i]);
			}
		}
		else
		{
			int j = 0;
			for (int count = pCurrentStage._ContextData.Count; j < count; j++)
			{
				FarmItemContextData farmItemContextData = pCurrentStage._ContextData[j];
				int k = 0;
				for (int num2 = farmItemContextData._ContextSensitiveState._CurrentContextNamesList.Length; k < num2; k++)
				{
					ContextItemBasedUpdate(farmItemContextData._ContextSensitiveState._CurrentContextNamesList[k]);
				}
			}
		}
		if (base.pShowStatus)
		{
			if (base.pUI != null)
			{
				base.pUI.SetText("Status", GetStatusText());
			}
			ProcessShowStatus();
		}
		else
		{
			ProcessExpiry();
		}
		if (pCurrentStage._Name == "NoInteraction" && timeLeftInCurrentStage <= 0f)
		{
			GotoNextStage();
		}
	}

	protected virtual void ProcessExpiry()
	{
		if (pCurrentStage == null || pCurrentStage._UserItemState == null || pIsWaitingForWSCall)
		{
			return;
		}
		StateDetails stateDetails = GetStateDetails(pCurrentStage);
		if (stateDetails != null && stateDetails._CriteriaExpiry != null && stateDetails._CriteriaExpiry.EndStateID > 0)
		{
			float num = ((stateDetails._CriteriaLength != null) ? stateDetails._CriteriaLength.Period : 0);
			DateTime stateChangeDate = pCurrentStage._UserItemState.StateChangeDate;
			TimeSpan timeSpan = ServerTime.pCurrentTime.Subtract(stateChangeDate);
			float num2 = (float)stateDetails._CriteriaExpiry.Period + num - (float)timeSpan.TotalSeconds;
			if (num2 <= 0f)
			{
				GotoNextStage();
			}
			else if (!string.IsNullOrEmpty(_ExpiryTimerCSMActionText) && base.pUI != null)
			{
				timeSpan = TimeSpan.FromSeconds(num2);
				base.pUI.SetText(_ExpiryTimerCSMActionText, $"{timeSpan.Hours:d2}:{timeSpan.Minutes:d2}:{timeSpan.Seconds:d2}");
			}
		}
	}

	protected StateDetails GetStateDetails(FarmItemStage inStage)
	{
		return _StateDetails.Find((StateDetails s) => s._ID == inStage._ID);
	}

	public virtual void ProcessShowStatus()
	{
		if (GetTimeLeftInCurrentStage() <= 0f)
		{
			base.pShowStatus = false;
			Refresh();
		}
	}

	public virtual void UpdateItemWithStage(FarmItemStage inStage)
	{
	}

	public virtual void Upgrade()
	{
	}

	protected virtual UserItemPosition GetUserItemPosition()
	{
		return UserItemPositionList.GetInstance(MyRoomsIntMain.pInstance.pCurrentRoomID).GetUserItemPositionData(base.gameObject);
	}

	public virtual void GotoNextStage(bool speedup = false)
	{
		pIsWaitingForWSCall = true;
		Refresh();
		mIsSpeedUp = speedup;
		SetNextItemStateRequest setNextItemStateRequest = new SetNextItemStateRequest();
		setNextItemStateRequest.OverrideStateCriteria = speedup;
		if (!mAdWatched)
		{
			setNextItemStateRequest.StoreID = _SpeedupStoreID;
		}
		mAdWatched = false;
		setNextItemStateRequest.CommonInventoryConsumables = GetConsumables();
		UserItemPosition userItemPosition = GetUserItemPosition();
		if (userItemPosition != null)
		{
			if (userItemPosition.UserInventoryCommonID.HasValue)
			{
				setNextItemStateRequest.UserInventoryCommonID = userItemPosition.UserInventoryCommonID.Value;
			}
			if (userItemPosition.UserItemPositionID.HasValue)
			{
				setNextItemStateRequest.UserItemPositionID = userItemPosition.UserItemPositionID.Value;
			}
		}
		WsWebService.SetNextItemState(setNextItemStateRequest, SetNextItemStateEventHandler, null);
	}

	protected virtual void RemoveUsedConsumables(UserItemData userItemData, ItemStateCriteriaConsumable consumable)
	{
		if (userItemData != null)
		{
			CommonInventoryData.pInstance.RemoveItem(userItemData, consumable.Amount);
		}
	}

	protected virtual void OnSetNextItemStateSuccess()
	{
		base.pFarmManager.PlayActionSFX("Success", pCurrentStage._Name);
		if (!string.IsNullOrEmpty(pCurrentStage._PropName))
		{
			AvAvatar.UseProp(pCurrentStage._PropName);
		}
	}

	protected virtual bool CanDoHarvest(SetNextItemStateResult result)
	{
		if (!pCurrentStage._Name.Equals(GetHarvestStageName()) || result.ErrorCode == ItemStateChangeError.ItemStateExpired)
		{
			if (pCurrentStage._Name.Equals("Withered") && result.UserItemState.ItemStateID == pCurrentStage._ID)
			{
				return result.Success;
			}
			return false;
		}
		return true;
	}

	protected virtual void SetNextItemStateEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			pIsWaitingForWSCall = false;
			Refresh();
			if (inObject == null)
			{
				break;
			}
			SetNextItemStateResult setNextItemStateResult = (SetNextItemStateResult)inObject;
			bool flag = mIsSpeedUp;
			mIsSpeedUp = false;
			if (setNextItemStateResult.Success)
			{
				OnSetNextItemStateSuccess();
				bool flag2 = CanDoHarvest(setNextItemStateResult);
				if (flag2)
				{
					DoHarvest();
					if (setNextItemStateResult.UserItemState != null && pCurrentStage._Name.Equals(GetHarvestStageName()))
					{
						base.pFarmManager.CheckHarvestAchievement(setNextItemStateResult.UserItemState.ItemID);
					}
				}
				if (flag)
				{
					Money.UpdateMoneyFromServer();
				}
				else
				{
					ParseConsumablesInventoryData(RemoveUsedConsumables);
				}
				if (setNextItemStateResult.Rewards != null && setNextItemStateResult.Rewards.Length != 0)
				{
					SpawnRewards(setNextItemStateResult.Rewards);
					GameUtilities.AddRewards(setNextItemStateResult.Rewards, inUseRewardManager: false);
					(MyRoomsIntMain.pInstance as FarmManager).UpdateAchievementUI();
				}
				if ((!CanDestroyOnHarvest() || !flag2) && setNextItemStateResult.UserItemState != null && pCurrentStage != null)
				{
					SetState(setNextItemStateResult.UserItemState);
				}
			}
			else
			{
				OnSetNextItemStateError(setNextItemStateResult);
			}
			break;
		}
		case WsServiceEvent.ERROR:
			pIsWaitingForWSCall = false;
			Refresh();
			base.pFarmManager.pIsWsErrorReceived = true;
			base.pFarmManager.ShowDialog(base.pFarmManager._DialogAssetName, "PfUiFarmingDB", base.pFarmManager._FarmingDBTitleText, "OnYes", "OnNo", string.Empty, string.Empty, destroyDB: true, base.pFarmManager._WebServiceErrorText, base.gameObject);
			break;
		}
	}

	private void UpdateSpeedupCost()
	{
	}

	protected virtual void OnSetNextItemStateError(SetNextItemStateResult inResult)
	{
		ItemStateChangeError errorCode = inResult.ErrorCode;
		if (errorCode == ItemStateChangeError.UsesLessThanRequired || errorCode == ItemStateChangeError.ItemNotInUserInventory)
		{
			ShowUsesLessThanRequiredDB();
		}
		else
		{
			Debug.LogError("SetNextItemState Error: " + inResult.ErrorCode);
		}
	}

	protected virtual void ShowUsesLessThanRequiredDB()
	{
		string lItemsList = "";
		List<int> itemIDs = new List<int>();
		string itemHelpTexts = "";
		OnParseConsumableInventoryData parseDelegate = delegate(UserItemData userItemData, ItemStateCriteriaConsumable consumable)
		{
			int amount = consumable.Amount;
			if (userItemData != null)
			{
				string attribute = userItemData.Item.GetAttribute("ConsumableDisplayText", "");
				lItemsList = lItemsList + (string.IsNullOrEmpty(lItemsList) ? "" : ", ") + amount + " " + ((!string.IsNullOrEmpty(attribute)) ? attribute : userItemData.Item.ItemName);
			}
			else
			{
				lItemsList = lItemsList + (string.IsNullOrEmpty(lItemsList) ? "" : ", ") + amount + " %i" + consumable.ItemID;
				itemIDs.Add(consumable.ItemID);
			}
			itemHelpTexts = itemHelpTexts + "\n" + base.pFarmManager.GetItemHelpText(consumable.ItemID);
		};
		ParseConsumablesInventoryData(parseDelegate);
		if (itemIDs.Count > 0)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			StringBuilder stringBuilder = new StringBuilder(lItemsList);
			LoadItemCallBackData loadItemCallBackData = new LoadItemCallBackData();
			loadItemCallBackData.StringBuilder = stringBuilder;
			loadItemCallBackData.HelpTexts = itemHelpTexts;
			{
				foreach (int item in itemIDs)
				{
					ItemData.Load(item, OnLoadItemDataReady, loadItemCallBackData);
				}
				return;
			}
		}
		string text = (itemHelpTexts.Contains("%items_list%") ? itemHelpTexts : (base.pFarmManager._ConsumableInsufficientText.GetLocalizedString() + itemHelpTexts));
		text = text.Replace("%items_list%", lItemsList);
		SetupUsesLessThanRequiredDB(text);
	}

	protected virtual void SetupUsesLessThanRequiredDB(string inMessage)
	{
		base.pFarmManager.ShowDialog(base.pFarmManager._DialogAssetName, "PfUiNoFeed", base.pFarmManager._FarmingDBTitleText, string.Empty, string.Empty, "OnOK", string.Empty, destroyDB: true, inMessage, base.gameObject);
	}

	public virtual void ParseConsumablesInventoryData(OnParseConsumableInventoryData parseDelegate)
	{
		StateDetails stateDetails = _StateDetails.Find((StateDetails s) => s._ID == pCurrentStage._ID);
		if (stateDetails == null || stateDetails._CriteriaConsumables == null)
		{
			return;
		}
		foreach (ItemStateCriteriaConsumable criteriaConsumable in stateDetails._CriteriaConsumables)
		{
			if (criteriaConsumable.ItemID > 0)
			{
				UserItemData userItemData = CommonInventoryData.pInstance.FindItem(criteriaConsumable.ItemID);
				parseDelegate(userItemData, criteriaConsumable);
			}
		}
	}

	private void OnLoadItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		string text = "";
		StringBuilder stringBuilder = null;
		if (inUserData is LoadItemCallBackData loadItemCallBackData)
		{
			stringBuilder = loadItemCallBackData.StringBuilder;
			if (stringBuilder != null)
			{
				string attribute = dataItem.GetAttribute("ConsumableDisplayText", "");
				stringBuilder.Replace("%i" + itemID, (!string.IsNullOrEmpty(attribute)) ? attribute : dataItem.ItemName);
				text = stringBuilder.ToString();
			}
			if (!text.Contains("%i"))
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				string text2 = (loadItemCallBackData.HelpTexts.Contains("%items_list%") ? loadItemCallBackData.HelpTexts : (base.pFarmManager._ConsumableInsufficientText.GetLocalizedString() + loadItemCallBackData.HelpTexts));
				text2 = text2.Replace("%items_list%", text);
				SetupUsesLessThanRequiredDB(text2);
			}
		}
	}

	protected virtual void DoHarvest()
	{
	}

	protected virtual void SpawnRewards(AchievementReward[] inRewards)
	{
		if (pCurrentStage == null || pCurrentStage._HarvestedObject == null || pCurrentStage._HarvestCollectableCount == 0)
		{
			return;
		}
		foreach (AchievementReward achievementReward in inRewards)
		{
			if (achievementReward.PointTypeID == 6 && achievementReward.Amount.HasValue)
			{
				ObBouncyCoinEmitter component = GetComponent<ObBouncyCoinEmitter>();
				if (component == null && base.pFarmManager._FarmHarvestEmitter != null)
				{
					GameObject obj = UnityEngine.Object.Instantiate(base.pFarmManager._FarmHarvestEmitter);
					obj.transform.position = base.transform.position;
					component = obj.GetComponent<ObBouncyCoinEmitter>();
					obj.name = "FarmHarvestEmitter_" + ((base.pUserItemData != null) ? base.pUserItemData.UserInventoryID.ToString() : "");
				}
				if (component != null)
				{
					component._Coin = pCurrentStage._HarvestedObject;
					int max = ((pCurrentStage._HarvestCollectableCount > 0) ? pCurrentStage._HarvestCollectableCount : achievementReward.Amount.Value);
					component._CoinsToEmit._Min = (component._CoinsToEmit._Max = max);
					component.GenerateCoins();
				}
				break;
			}
		}
	}

	protected virtual bool CanDestroyOnHarvest()
	{
		bool result = true;
		StateDetails stateDetails = _StateDetails.Find((StateDetails s) => s._ID == pCurrentStage._ID);
		if (stateDetails != null)
		{
			result = stateDetails._CompletionAction.Transition == StateTransition.Deletion;
		}
		return result;
	}

	protected virtual string GetHarvestStageName()
	{
		return "";
	}

	protected virtual List<CommonInventoryConsumable> GetConsumables()
	{
		return null;
	}

	public virtual void SwapStageMesh(int inCurrentStage)
	{
		if (inCurrentStage < 0 || inCurrentStage >= _Stages.Length)
		{
			UtDebug.LogWarning("Incorrect stage sent for setup: " + inCurrentStage);
			return;
		}
		for (int i = 0; i < _Stages.Length; i++)
		{
			if (_Stages[i]._StageObject != null)
			{
				_Stages[i]._StageObject.SetActive(value: false);
			}
		}
		if (_Stages[inCurrentStage]._StageObject != null)
		{
			_Stages[inCurrentStage]._StageObject.SetActive(value: true);
		}
	}

	public virtual void SwapStageMesh()
	{
		if (((!mInitialized || !pIsRuleSetInitialized || !pIsStateSet) && _DefaultMeshObject != null) || pCurrentStage == null)
		{
			return;
		}
		if (_DefaultMeshObject != null)
		{
			_DefaultMeshObject.SetActive(value: false);
		}
		GameObject gameObject = null;
		FarmItemStage[] stages = _Stages;
		foreach (FarmItemStage farmItemStage in stages)
		{
			if (pCurrentStage._Name.Equals(farmItemStage._Name))
			{
				if (farmItemStage._StageObject != null)
				{
					farmItemStage._StageObject.SetActive(value: true);
				}
				gameObject = farmItemStage._StageObject;
				AddStageChangeFx(pCurrentStage, activate: true);
			}
			else if (farmItemStage._StageObject != null && farmItemStage._StageObject != gameObject)
			{
				farmItemStage._StageObject.SetActive(value: false);
			}
		}
	}

	public virtual GameObject GetCurrentStageMesh()
	{
		if ((!mInitialized || !pIsRuleSetInitialized || !pIsStateSet) && _DefaultMeshObject != null)
		{
			return _DefaultMeshObject;
		}
		if (pCurrentStage != null)
		{
			if (_DefaultMeshObject != null)
			{
				_DefaultMeshObject.SetActive(value: false);
			}
			FarmItemStage[] stages = _Stages;
			foreach (FarmItemStage farmItemStage in stages)
			{
				if (pCurrentStage._Name.Equals(farmItemStage._Name))
				{
					return farmItemStage._StageObject;
				}
			}
		}
		return null;
	}

	public FarmItemStage GetStageFromStageID(int inStageID)
	{
		if (_Stages == null || _Stages.Length == 0)
		{
			return null;
		}
		FarmItemStage[] stages = _Stages;
		foreach (FarmItemStage farmItemStage in stages)
		{
			if (farmItemStage._UserItemState.ItemStateID == inStageID)
			{
				return farmItemStage;
			}
		}
		return null;
	}

	protected FarmItemStage GetStageFromIndex(int inIndex)
	{
		if (_Stages == null || _Stages.Length == 0)
		{
			Debug.LogError("Invalid stages for " + base.name);
			return null;
		}
		if (_Stages.Length <= inIndex)
		{
			Debug.LogError("Invalid index for the stage of " + base.name);
			return null;
		}
		return _Stages[inIndex];
	}

	public int GetStageIndex(FarmItemStage inStage)
	{
		if (inStage == null || _Stages == null || _Stages.Length == 0)
		{
			return -1;
		}
		int i;
		for (i = 0; i < _Stages.Length; i++)
		{
			FarmItemStage farmItemStage = _Stages[i];
			if (farmItemStage != null && inStage._UserItemState.ItemStateID == farmItemStage._UserItemState.ItemStateID)
			{
				break;
			}
		}
		if (i >= _Stages.Length)
		{
			return -1;
		}
		return i;
	}

	public void OnMouseOver()
	{
		if (base.pFarmManager != null && base.pFarmManager.pIsReady && !MyRoomsIntMain.pInstance.pIsBuildMode)
		{
			base.pFarmManager.OnMouseEnterOnFarmItem(this);
		}
	}

	public void OnMouseExit()
	{
		if (base.pFarmManager != null && base.pFarmManager.pIsReady)
		{
			base.pFarmManager.OnMouseExitFromFarmItem(this);
		}
	}

	public void OnMouseDown()
	{
		ProcessClick();
	}

	public virtual string Serialize()
	{
		string result = string.Empty;
		int? userItemPositionID = UserItemPositionList.GetUserItemPositionID(MyRoomsIntMain.pInstance.pCurrentRoomID, base.gameObject);
		if (userItemPositionID.HasValue)
		{
			mFarmItemState.ID = userItemPositionID.Value;
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(FarmItemState));
			StringWriter stringWriter = new StringWriter();
			xmlSerializer.Serialize(stringWriter, mFarmItemState);
			result = stringWriter.ToString();
		}
		return result;
	}

	public virtual void Deserialize(FarmItemState inFarmItemState)
	{
		mFarmItemState = inFarmItemState;
		mIsDeserialized = true;
		Initialize();
	}

	public virtual bool AddChild(FarmItem inFarmItem)
	{
		if (inFarmItem == null)
		{
			return false;
		}
		if (mChildren.Contains(inFarmItem))
		{
			return false;
		}
		mChildren.Add(inFarmItem);
		return true;
	}

	public virtual void RemoveChild(FarmItem inFarmItem)
	{
		if (mChildren.Contains(inFarmItem))
		{
			mChildren.Remove(inFarmItem);
		}
	}

	public virtual void ClearChildren(FarmItem inFarmItem)
	{
		mChildren.Clear();
	}

	public virtual void SetParent(FarmItem inFarmItem)
	{
		if (!(inFarmItem == null))
		{
			inFarmItem.AddChild(this);
			mParent = inFarmItem;
		}
	}

	public bool IsDependent()
	{
		if (_DependentItemIDs != null)
		{
			return _DependentItemIDs.Length != 0;
		}
		return false;
	}

	public bool IsDependentOnFarmItem(FarmItem inFarmItem)
	{
		if (inFarmItem == null || inFarmItem.pUserItemData == null || inFarmItem.pUserItemData.Item == null)
		{
			return false;
		}
		return IsDependentOnItem(inFarmItem.pUserItemData.Item.ItemID);
	}

	public bool IsDependentOnItem(int inDependentItemID)
	{
		if (_DependentItemIDs == null || _DependentItemIDs.Length == 0)
		{
			return false;
		}
		int[] dependentItemIDs = _DependentItemIDs;
		for (int i = 0; i < dependentItemIDs.Length; i++)
		{
			if (dependentItemIDs[i] == inDependentItemID)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void UpdateContextIcon()
	{
	}

	public virtual string GetStatusText()
	{
		return GameUtilities.FormatTime(TimeSpan.FromSeconds(GetTimeTillHarvest()));
	}

	public virtual bool CanPlaceDependentObject(FarmItem inDependentObject)
	{
		if (inDependentObject == null || !inDependentObject.IsDependentOnFarmItem(this))
		{
			return false;
		}
		return true;
	}

	public float GetTimeLeftInCurrentStage()
	{
		return GetTimeLeftInCurrentStage(ServerTime.pCurrentTime);
	}

	public float GetTimeLeftInCurrentStage(DateTime inCurrentTime)
	{
		if (pCurrentStage == null || pCurrentStage._UserItemState == null)
		{
			return -1f;
		}
		float num = 0f;
		foreach (StateDetails stateDetail in _StateDetails)
		{
			if (stateDetail._ID == pCurrentStage._ID && stateDetail._CriteriaLength != null)
			{
				num = stateDetail._CriteriaLength.Period;
				break;
			}
		}
		return num - (float)inCurrentTime.Subtract(pCurrentStage._UserItemState.StateChangeDate).TotalSeconds;
	}

	public void AddDefaultFarmItems()
	{
		if (_DefaultItems == null || _DefaultItems.Length == 0 || base.pFarmManager == null || base.pFarmManager.pBuilder == null)
		{
			return;
		}
		DefaultFarmItem[] defaultItems = _DefaultItems;
		foreach (DefaultFarmItem defaultFarmItem in defaultItems)
		{
			if (defaultFarmItem == null)
			{
				continue;
			}
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(defaultFarmItem._ItemID);
			if (userItemData == null || userItemData.Item == null || string.IsNullOrEmpty(userItemData.Item.AssetName))
			{
				continue;
			}
			for (int j = 0; j < defaultFarmItem._Count; j++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromBundle(userItemData.Item.AssetName));
				if (!(gameObject == null))
				{
					gameObject.transform.parent = base.transform;
					if (defaultFarmItem._Marker != null && defaultFarmItem._Marker.Length > j && (bool)defaultFarmItem._Marker[j])
					{
						gameObject.transform.localPosition = defaultFarmItem._Marker[j].localPosition;
						gameObject.transform.localRotation = defaultFarmItem._Marker[j].localRotation;
					}
					else
					{
						gameObject.transform.localPosition = Vector3.zero;
					}
					FarmItem component = gameObject.GetComponent<FarmItem>();
					if (component != null)
					{
						component.pUserItemData = userItemData;
						component.SetParent(this);
						component.pFarmManager = base.pFarmManager;
					}
				}
			}
		}
	}

	protected float GetTotalTimeTillHarvest()
	{
		float num = 0f;
		foreach (StateDetails stateDetail in _StateDetails)
		{
			if (stateDetail._CriteriaLength != null)
			{
				num += (float)stateDetail._CriteriaLength.Period;
			}
		}
		return num;
	}

	protected virtual float GetTimeTillHarvest()
	{
		float num = GetTimeLeftInCurrentStage();
		if (num < 0f)
		{
			num = 0f;
		}
		foreach (StateDetails stateDetail in _StateDetails)
		{
			if (stateDetail._CriteriaLength != null && stateDetail._Order > pCurrentStage.pOrder)
			{
				num += (float)stateDetail._CriteriaLength.Period;
			}
		}
		return num;
	}

	protected FarmItemStage GetStage(int inID)
	{
		FarmItemStage[] stages = _Stages;
		foreach (FarmItemStage farmItemStage in stages)
		{
			if (farmItemStage._ID == inID)
			{
				return farmItemStage;
			}
		}
		return null;
	}

	protected int GetCurrentStageDuration()
	{
		if (_StateDetails != null)
		{
			StateDetails stateDetails = GetStateDetails(pCurrentStage);
			if (stateDetails != null && stateDetails._CriteriaLength != null)
			{
				return stateDetails._CriteriaLength.Period;
			}
		}
		return 0;
	}

	protected int GetOrder(int stateID)
	{
		int result = 0;
		if (_StateDetails != null)
		{
			foreach (StateDetails stateDetail in _StateDetails)
			{
				if (stateDetail._ID == stateID)
				{
					result = stateDetail._Order;
					break;
				}
			}
		}
		return result;
	}

	public override void SetState(UserItemState state)
	{
		if (state != null)
		{
			pCurrentItemState = state;
			FarmItemStage farmItemStage = pCurrentStage;
			AddStageChangeFx(farmItemStage, activate: false);
			pCurrentStage = GetStage(state.ItemStateID);
			if (pCurrentStage != null)
			{
				pIsStateSet = true;
				pCurrentStage._ID = state.ItemStateID;
				pCurrentStage.pOrder = GetOrder(state.ItemStateID);
				pCurrentStage._UserItemState = state;
				base.pShowStatus = GetTimeLeftInCurrentStage() > 0f;
				pCurrentStage._Duration = GetCurrentStageDuration();
				SwapStageMesh();
				Refresh();
			}
			OnChangeStage(farmItemStage, pCurrentStage);
		}
	}

	protected void ShowLoading(ref ContextSensitiveState[] inStatesArrData)
	{
		SetInteractiveEnabledData("Loading", isEnabled: false);
		ContextSensitiveState contextSensitiveState = (ContextSensitiveState)_LoadingContextSensitiveState.Clone();
		List<string> list = new List<string>(contextSensitiveState._CurrentContextNamesList);
		contextSensitiveState._CurrentContextNamesList = list.ToArray();
		inStatesArrData[1] = contextSensitiveState;
	}

	protected virtual ContextSensitiveState[] GetSensitiveData(List<ContextSensitiveState> csData, float timeLeft = 0f)
	{
		List<ContextSensitiveState> list = new List<ContextSensitiveState>();
		if (base.pShowStatus && timeLeft > 0f)
		{
			if (!base.pFarmManager.pIsBuildMode)
			{
				SetInteractiveEnabledData("Status", isEnabled: false);
				ContextSensitiveState contextSensitiveState = (ContextSensitiveState)_StatusContextSensitiveState.Clone();
				List<string> list2 = new List<string>(contextSensitiveState._CurrentContextNamesList);
				contextSensitiveState._CurrentContextNamesList = list2.ToArray();
				list.Add(contextSensitiveState);
			}
		}
		else if (csData != null && csData.Count > 0)
		{
			ContextSensitiveState expiryContextSensitiveState = GetExpiryContextSensitiveState();
			int i = 0;
			for (int count = csData.Count; i < count; i++)
			{
				ContextSensitiveState contextSensitiveState2 = csData[i];
				ContextSensitiveState contextSensitiveState3 = (ContextSensitiveState)contextSensitiveState2.Clone();
				contextSensitiveState3._CurrentContextNamesList = (string[])contextSensitiveState2._CurrentContextNamesList.Clone();
				List<string> menuItemNames = ((timeLeft > 0f) ? new List<string>() : new List<string>(contextSensitiveState3._CurrentContextNamesList));
				if (expiryContextSensitiveState != null && contextSensitiveState2._MenuType == expiryContextSensitiveState._MenuType)
				{
					int j = 0;
					for (int num = expiryContextSensitiveState._CurrentContextNamesList.Length; j < num; j++)
					{
						menuItemNames.Insert(0, expiryContextSensitiveState._CurrentContextNamesList[j]);
					}
				}
				ProcessSensitiveData(ref menuItemNames);
				if (contextSensitiveState3._CurrentContextNamesList.Length != menuItemNames.Count)
				{
					contextSensitiveState3._CurrentContextNamesList = menuItemNames.ToArray();
				}
				else
				{
					int k = 0;
					for (int count2 = menuItemNames.Count; k < count2; k++)
					{
						contextSensitiveState3._CurrentContextNamesList[k] = menuItemNames[k];
					}
				}
				list.Add(contextSensitiveState3);
			}
		}
		return list.ToArray();
	}

	protected virtual ContextSensitiveState GetExpiryContextSensitiveState()
	{
		ContextSensitiveState contextSensitiveState = null;
		StateDetails stateDetails = GetStateDetails(pCurrentStage);
		if (!string.IsNullOrEmpty(_ExpiryTimerCSMActionText) && stateDetails != null && stateDetails._CriteriaExpiry != null && stateDetails._CriteriaExpiry.EndStateID > 0)
		{
			float num = ((stateDetails._CriteriaLength != null) ? stateDetails._CriteriaLength.Period : 0);
			TimeSpan timeSpan = ServerTime.pCurrentTime.Subtract(pCurrentStage._UserItemState.StateChangeDate);
			if ((float)stateDetails._CriteriaExpiry.Period + num - (float)timeSpan.TotalSeconds > 0f)
			{
				contextSensitiveState = (ContextSensitiveState)_StatusContextSensitiveState.Clone();
				SetInteractiveEnabledData(_ExpiryTimerCSMActionText, isEnabled: false);
				List<string> list = new List<string>();
				list.Add(_ExpiryTimerCSMActionText);
				contextSensitiveState._CurrentContextNamesList = list.ToArray();
			}
		}
		return contextSensitiveState;
	}

	public virtual void CreateRoomObject(GameObject inObject, UserItemData inUserItemData)
	{
		if (inObject != null && inUserItemData != null)
		{
			MyRoomObject component = inObject.GetComponent<MyRoomObject>();
			if (component != null)
			{
				component.pUserItemData = inUserItemData;
			}
		}
	}

	protected override void OnContextAction(string inActionName)
	{
		if (base.pFarmManager.HandleFirstTimeAction(inActionName, this))
		{
			return;
		}
		base.OnContextAction(inActionName);
		ContextActionFx[] contextActionsFx = _ContextActionsFx;
		foreach (ContextActionFx contextActionFx in contextActionsFx)
		{
			if (contextActionFx._Name.Equals(inActionName) && contextActionFx._Fx != null)
			{
				PlayOneShotFx(contextActionFx._Fx, contextActionFx._Fx.transform.parent);
			}
		}
		base.pFarmManager.PlayActionSFX(inActionName, pCurrentStage._Name);
	}

	protected virtual void PlayOneShotFx(GameObject fxPrefab, Transform parentT, bool addToParent = true, Vector3 offset = default(Vector3))
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(fxPrefab, parentT.position + offset, Quaternion.identity);
		if ((bool)gameObject)
		{
			if (addToParent)
			{
				gameObject.transform.parent = parentT;
			}
			gameObject.SetActive(value: true);
			ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
			if ((bool)component)
			{
				UnityEngine.Object.Destroy(gameObject, component.main.duration);
			}
			AddToSkipRenderer(component.GetComponent<Renderer>());
		}
	}

	protected virtual void AddToSkipRenderer(Renderer r)
	{
		foreach (FarmItemClickable mFarmItemClickable in mFarmItemClickables)
		{
			if (mFarmItemClickable != null && mFarmItemClickable._SkipRenderers != null)
			{
				mFarmItemClickable._SkipRenderers.Add(r);
			}
		}
	}

	protected override void OnMenuActive(ContextSensitiveStateType inMenuType)
	{
		base.OnMenuActive(inMenuType);
		if (pCurrentStage != null && pCurrentStage._StageFx != null)
		{
			pCurrentStage._StageFx.SetActive(value: false);
		}
	}

	protected override void DestroyMenu(bool checkProximity)
	{
		base.DestroyMenu(checkProximity);
		if (pCurrentStage != null && pCurrentStage._StageFx != null)
		{
			pCurrentStage._StageFx.SetActive(value: true);
		}
	}

	protected virtual void AddStageChangeFx(FarmItemStage stage, bool activate)
	{
		if (base.pFarmManager != null && base.pFarmManager.pFarmItems != null && base.pFarmManager.pFarmItems.Contains(this) && stage != null && stage._StageFx != null)
		{
			stage._StageFx.SetActive(activate);
		}
	}

	protected virtual void OnChangeStage(FarmItemStage prevStage, FarmItemStage curStage)
	{
	}

	public virtual void AddToClickableList(List<FarmItemClickable> clickables)
	{
		foreach (FarmItemClickable clickable in clickables)
		{
			mFarmItemClickables.Add(clickable);
		}
	}

	public virtual void SetSpeedupCostText()
	{
		if (!(base.pUI != null))
		{
			return;
		}
		KAWidget kAWidget = base.pUI.FindItem(_SpeedupActionName);
		if (kAWidget != null)
		{
			UILabel uILabel = kAWidget.FindChildNGUIItem("Text") as UILabel;
			if (uILabel != null)
			{
				int speedupCost = GetSpeedupCost();
				uILabel.text = ((speedupCost > 0) ? GetSpeedupCost().ToString() : "");
			}
		}
	}

	protected int GetSpeedupCost()
	{
		if (GetSpeedupCriteria() is ItemStateCriteriaSpeedUpItem itemStateCriteriaSpeedUpItem && CommonInventoryData.pInstance != null)
		{
			if (CommonInventoryData.pInstance.FindItem(itemStateCriteriaSpeedUpItem.ItemID) != null)
			{
				return 0;
			}
			ItemData itemData = base.pFarmManager.GetItemData(itemStateCriteriaSpeedUpItem.ItemID);
			if (itemData != null)
			{
				return itemData.FinalCashCost;
			}
		}
		return -1;
	}

	protected bool CheckGemsAvailable(int inReqdGems)
	{
		if (Money.pCashCurrency < inReqdGems)
		{
			LocaleString localeString = new LocaleString("");
			localeString._Text = base.pFarmManager._InsufficientGemsText._Text.Replace("%gems_count%", inReqdGems.ToString());
			localeString._ID = base.pFarmManager._InsufficientGemsText._ID;
			base.pFarmManager.ShowDialog(base.pFarmManager._DialogAssetName, "PfUiFarmingDB", base.pFarmManager._FarmingDBTitleText, "OpenGemsStore", "OnNo", string.Empty, string.Empty, destroyDB: true, localeString, base.gameObject);
			return false;
		}
		return true;
	}

	protected virtual void OpenGemsStore()
	{
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	protected virtual void OnIAPStoreClosed()
	{
		OnOK();
	}

	protected virtual void OnOK()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
	}

	protected virtual void OnYes()
	{
		OnOK();
		GotoNextStage(mIsSpeedUp);
	}

	protected virtual void OnNo()
	{
		OnOK();
	}

	public ItemStateCriteria GetSpeedupCriteria()
	{
		if (pCurrentItemState != null)
		{
			ItemData itemData = null;
			if (base.pUserItemData != null)
			{
				itemData = base.pUserItemData.Item;
			}
			if (itemData != null)
			{
				ItemState itemState = itemData.ItemStates.Find((ItemState st) => st.ItemStateID == pCurrentItemState.ItemStateID);
				if (itemState != null && itemState.Rule != null && itemState.Rule.Criterias != null)
				{
					return itemState.Rule.Criterias.Find((ItemStateCriteria cr) => cr.Type == ItemStateCriteriaType.SpeedUpItem);
				}
			}
		}
		return null;
	}

	public double TimeSinceLastUpdate()
	{
		if (MyRoomsIntMain.pInstance != null)
		{
			UserItemPositionListData instance = UserItemPositionList.GetInstance(MyRoomsIntMain.pInstance.pCurrentRoomID);
			if (instance != null)
			{
				UserItemPosition userItemPositionData = instance.GetUserItemPositionData(base.gameObject);
				if (userItemPositionData != null)
				{
					return (ServerTime.pCurrentTime - userItemPositionData.InvLastModifiedDate).TotalSeconds;
				}
			}
		}
		return -1.0;
	}

	public void OnAdWatched()
	{
		AdManager.pInstance.LogAdWatchedEvent(base.pFarmManager._AdEventType, "Farming");
		AdManager.pInstance.SyncAdAvailableCount(base.pFarmManager._AdEventType, isConsumed: true);
		mAdWatched = true;
		GotoNextStage(speedup: true);
	}

	public void OnAdFailed()
	{
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
