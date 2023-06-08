using System;
using System.Collections.Generic;
using UnityEngine;

public class ComposterQuestFarmItem : ObContextSensitive
{
	[Serializable]
	public class TaskStageMap
	{
		public int _TaskID;

		public int _StageIndex;

		public int _NextStageIndex;
	}

	public string _DeliveryTaskNPCName = "PfDWQuestComposterDO";

	public string _BoostComposterQuestAction = "BoostComposter";

	public string _HarvestComposterQuestAction = "HarvestComposter";

	public string _DialogAssetName = "PfKAUIGenericDBSm";

	public string _CompostFeedAction = "CompostFeed";

	public List<TaskStageMap> _TaskStageMap = new List<TaskStageMap>();

	public AudioClip _OnCSMActivationSFX;

	public AudioClip _StatusTimerSFX;

	public FarmItemStage[] _Stages;

	public ContextSensitiveState _StatusContextSensitiveState;

	public List<ItemStateCriteriaConsumable> _CompostConsumables;

	public LocaleString _FarmingDBTitleText = new LocaleString("Farming");

	public LocaleString _FeedInsufficientText = new LocaleString("You need %reqd_amount% %reqd_item% to get the compostor started, you only have %available_amount%");

	public LocaleString _FeedUnavailableText = new LocaleString("You need %reqd_amount% %reqd_item% to get the compostor started, you have none");

	private KAUIGenericDB mKAUIGenericDB;

	private List<ItemData> mConsumableItems = new List<ItemData>();

	private int mNumItemDataRemaining = -1;

	private bool mItemDataLoadStarted;

	private FarmItemStage mCurrentStage;

	private bool mShowStatus;

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

	public bool pShowStatus
	{
		get
		{
			return mShowStatus;
		}
		set
		{
			mShowStatus = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		mNumItemDataRemaining = _CompostConsumables.Count;
		pCurrentStage = null;
	}

	protected override void Update()
	{
		if (pCurrentStage == null)
		{
			foreach (TaskStageMap taskMap in _TaskStageMap)
			{
				if (MissionManager.pInstance.pActiveTasks.Exists((Task t) => t.TaskID == taskMap._TaskID))
				{
					SetState(GetStage(taskMap._StageIndex));
					break;
				}
			}
			return;
		}
		base.Update();
		if (mNumItemDataRemaining > 0 && !mItemDataLoadStarted)
		{
			foreach (ItemStateCriteriaConsumable compostConsumable in _CompostConsumables)
			{
				ItemData.Load(compostConsumable.ItemID, OnLoadItemDataReady, null);
			}
			mItemDataLoadStarted = true;
		}
		ProcessCurrentStage();
	}

	private FarmItemStage GetStage(int inID)
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

	protected override void OnActivate()
	{
		if (pCurrentStage != null && mNumItemDataRemaining <= 0)
		{
			if (!pShowStatus && GetTimeLeftInCurrentStage() > 0f)
			{
				pShowStatus = true;
			}
			base.OnActivate();
			SnChannel.Play(_OnCSMActivationSFX, "SFX_Pool", inForce: true);
		}
	}

	public virtual void ProcessCurrentStage()
	{
		if (pCurrentStage != null && pShowStatus && base.pUI != null)
		{
			base.pUI.SetText("Status", GetStatusText());
		}
	}

	protected override void UpdateData(ref ContextSensitiveState[] inStatesArrData)
	{
		float timeLeftInCurrentStage = GetTimeLeftInCurrentStage();
		List<ContextSensitiveState> farmItemContextData = pCurrentStage.GetFarmItemContextData(inIsBuildMode: false);
		inStatesArrData = GetSensitiveData(farmItemContextData, timeLeftInCurrentStage);
		UpdateScaleData(ref inStatesArrData);
	}

	protected ContextSensitiveState[] GetSensitiveData(List<ContextSensitiveState> csData, float timeLeft = 0f)
	{
		List<ContextSensitiveState> list = new List<ContextSensitiveState>();
		if (pShowStatus && timeLeft > 0f)
		{
			SetInteractiveEnabledData("Status", isEnabled: false);
			ContextSensitiveState contextSensitiveState = (ContextSensitiveState)_StatusContextSensitiveState.Clone();
			List<string> list2 = new List<string>(contextSensitiveState._CurrentContextNamesList);
			contextSensitiveState._CurrentContextNamesList = list2.ToArray();
			list.Add(contextSensitiveState);
		}
		else if (csData != null && csData.Count > 0)
		{
			int i = 0;
			for (int count = csData.Count; i < count; i++)
			{
				ContextSensitiveState contextSensitiveState2 = (ContextSensitiveState)csData[i].Clone();
				List<string> inNames = ((timeLeft > 0f) ? new List<string>() : new List<string>(contextSensitiveState2._CurrentContextNamesList));
				UpdateCompostFeedMenu(ref inNames);
				if (contextSensitiveState2._CurrentContextNamesList.Length != inNames.Count)
				{
					contextSensitiveState2._CurrentContextNamesList = inNames.ToArray();
				}
				else
				{
					int j = 0;
					for (int count2 = inNames.Count; j < count2; j++)
					{
						contextSensitiveState2._CurrentContextNamesList[j] = inNames[j];
					}
				}
				list.Add(contextSensitiveState2);
			}
		}
		return list.ToArray();
	}

	public float GetTimeLeftInCurrentStage()
	{
		return (pCurrentStage != null && pCurrentStage._ID == _Stages[1]._ID) ? 1 : 0;
	}

	private void UpdateCompostFeedMenu(ref List<string> inNames)
	{
		if (pCurrentStage == null || pCurrentStage._ID != _Stages[0]._ID)
		{
			return;
		}
		ContextData contextData = GetContextData(_CompostFeedAction);
		if (contextData == null || _CompostConsumables == null)
		{
			return;
		}
		foreach (ItemStateCriteriaConsumable consumable in _CompostConsumables)
		{
			ItemData itemData = mConsumableItems.Find((ItemData itm) => itm.ItemID == consumable.ItemID);
			if (itemData != null)
			{
				inNames.Add(itemData.ItemName);
				AddChildContextDataToParent(contextData, itemData);
			}
		}
	}

	protected void UpdateScaleData(ref ContextSensitiveState[] inStatesArrData)
	{
		Vector3 zero = Vector3.zero;
		ContextSensitiveState[] array = inStatesArrData;
		foreach (ContextSensitiveState contextSensitiveState in array)
		{
			if (contextSensitiveState != null && contextSensitiveState._CurrentContextNamesList != null && contextSensitiveState._CurrentContextNamesList.Length != 0)
			{
				contextSensitiveState._UIScale = zero;
			}
		}
	}

	protected void OnContextAction(string inActionName)
	{
		if (pCurrentStage == null)
		{
			return;
		}
		DestroyMenu(checkProximity: false);
		if (!(inActionName == "Harvest"))
		{
			if (!(inActionName == "Boost"))
			{
				foreach (ItemStateCriteriaConsumable consumable in _CompostConsumables)
				{
					UserItemData userItemData = CommonInventoryData.pInstance.FindItem(consumable.ItemID);
					if (userItemData == null)
					{
						ItemData itemData = mConsumableItems.Find((ItemData itm) => itm.ItemID == consumable.ItemID);
						if (itemData != null && itemData.ItemName == inActionName)
						{
							string localizedString = _FeedUnavailableText.GetLocalizedString();
							localizedString = localizedString.Replace("%reqd_amount%", consumable.Amount.ToString());
							localizedString = localizedString.Replace("%reqd_item%", itemData.ItemName);
							ShowDialog(_DialogAssetName, "PfUiNoFeed", _FarmingDBTitleText, string.Empty, string.Empty, "OnOK", string.Empty, destroyDB: true, localizedString, base.gameObject);
						}
					}
					if (userItemData != null && userItemData.Item.ItemName == inActionName)
					{
						if (consumable.Amount <= userItemData.Quantity)
						{
							MissionManager.pInstance.CheckForTaskCompletion("Delivery", _DeliveryTaskNPCName);
							SetState(_Stages[1]);
						}
						else
						{
							string localizedString2 = _FeedInsufficientText.GetLocalizedString();
							localizedString2 = localizedString2.Replace("%reqd_amount%", consumable.Amount.ToString());
							localizedString2 = localizedString2.Replace("%reqd_item%", userItemData.Item.ItemName);
							localizedString2 = localizedString2.Replace("%available_amount%", userItemData.Quantity.ToString());
							ShowDialog(_DialogAssetName, "PfUiNoFeed", _FarmingDBTitleText, string.Empty, string.Empty, "OnOK", string.Empty, destroyDB: true, localizedString2, base.gameObject);
						}
						break;
					}
				}
				return;
			}
			MissionManager.pInstance.CheckForTaskCompletion("Action", _BoostComposterQuestAction, base.gameObject.name);
			SetState(_Stages[2]);
		}
		else
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", _HarvestComposterQuestAction, base.gameObject.name);
			SetState(_Stages[0]);
		}
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
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._OKMessage = "OnMessageDBClosed";
			mKAUIGenericDB.SetTitle(title.GetLocalizedString());
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	public void OnMessageDBClosed()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
		}
	}

	public string GetStatusText()
	{
		return "--";
	}

	public void SetState(FarmItemStage inNewStage)
	{
		if (inNewStage != null)
		{
			FarmItemStage prevStage = pCurrentStage;
			pCurrentStage = inNewStage;
			if (pCurrentStage != null)
			{
				SwapStageMesh();
				Refresh();
			}
			OnChangeStage(prevStage, pCurrentStage);
		}
	}

	public void SwapStageMesh()
	{
		if (pCurrentStage == null)
		{
			return;
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

	protected void OnChangeStage(FarmItemStage prevStage, FarmItemStage curStage)
	{
		AddStageChangeFx(prevStage, activate: false);
		AddStageChangeFx(curStage, activate: true);
	}

	protected void AddStageChangeFx(FarmItemStage stage, bool activate)
	{
		if (stage != null && stage._StageFx != null)
		{
			stage._StageFx.SetActive(activate);
		}
	}

	private void OnLoadItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		mConsumableItems.Add(dataItem);
		mNumItemDataRemaining--;
	}

	protected void AddChildContextDataToParent(ContextData parentContextData, ItemData childItemData, bool inShowInventoryCount = false)
	{
		ContextData contextData = GetContextData(childItemData.ItemName);
		if (contextData == null)
		{
			contextData = new ContextData();
		}
		contextData._Name = childItemData.ItemName;
		contextData._DisplayName = new LocaleString("");
		if (inShowInventoryCount && CommonInventoryData.pInstance != null)
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(childItemData.ItemID);
			if (userItemData != null)
			{
				contextData._DisplayName = new LocaleString(userItemData.Quantity.ToString());
			}
		}
		contextData._DeactivateOnClick = true;
		contextData._IconSpriteName = childItemData.IconName.Split('/')[^1];
		contextData._2DScaleInPixels = parentContextData._2DScaleInPixels;
		contextData._BackgroundColor = parentContextData._BackgroundColor;
		if (!_DataList.Contains(contextData))
		{
			AddIntoDataList(contextData);
		}
	}
}
