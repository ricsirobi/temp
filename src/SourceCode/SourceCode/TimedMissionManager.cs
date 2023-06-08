using System;
using System.Collections.Generic;
using System.Linq;
using SOD.Event;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimedMissionManager : MonoBehaviour
{
	[Serializable]
	public class DefaultSlotInfo
	{
		public SlotType _SlotType;

		public int _ItemID;

		public int _DefaultCount;

		public int _MaxCount;
	}

	private const int LOG_PRIORITY = 100;

	private const string SLOTS_COUNT_KEY_NAME = "NumSlots";

	private const string SLOTS_DATA_KEY_PREFIX = "Slot";

	private const string MISSION_STATUS_DATA_KEY = "MissionStatus";

	private const string MISSION_STATUS_CLEANED_KEY = "MissionStatusCleaned";

	public int _PairDataID = 2014;

	public LocaleString _QuestCompleteText = new LocaleString("Your Dragons have returned from the quest {{Result}}. Would you like to go back to stables?");

	public LocaleString _CoolDownOverText = new LocaleString("Cool down finished, would you like to check new quests available?");

	public List<string> _NotificationExcludeSceneList = new List<string>();

	public List<string> _NonInteractiveNotificationSceneList = new List<string>();

	public LocaleString _QuestOverNotification = new LocaleString("Your Dragons have returned from the Quest {{Result}}");

	public LocaleString _StableMissionActionText = new LocaleString(" [[input]] [-] HERE[c] to go to the Stables.");

	public LocaleString _DragonBusyText = new LocaleString("[REVIEW]-Dragon is busy with stable quest. Do you want to finish the quest using {{GEMS}} gems.");

	public LocaleString _DragonBusyAfterQuestCompleteText = new LocaleString("[REVIEW]-Dragon is now free.Do you want to claim?");

	public LocaleString _DragonBusyHeaderText = new LocaleString("[REVIEW]-Dragon is busy!");

	public LocaleString _DragonFreeHeaderText = new LocaleString("[REVIEW]-Dragon is free!");

	public LocaleString _NotEnoughFeeText = new LocaleString("You do not have enough gems to pay, Please buy more!");

	public LocaleString _InsufficientGemsText = new LocaleString("Insufficient Gems");

	public LocaleString _StableQuestBundleFailedText = new LocaleString("[REVIEW]-Sorry, Stable quest bundle couldn't be loaded");

	public LocaleString _ForceCompleteMissionFailText = new LocaleString("[REVIEW] Failed to complete mission instantly");

	private static TimedMissionManager mInstance;

	private static bool mIsReady;

	private TimedMissionCompletion mCompletionCallback;

	private TimedMissionCoolDownPurchase mCoolDownPurchaseCallback;

	public string _ConfigPath = "RS_DATA/TimedMission.xml";

	private bool mConfigLoaded;

	private int mCheatPreferredMissionID = -1;

	public List<DefaultSlotInfo> _SlotInfo;

	public int _TimePurchaseStoreID;

	public CostPerTime _CostOfCompletionList;

	public CostPerMinite _CostOfCoolDown;

	public float _SlotUpdateCheckInterval = 10f;

	public int _ProgressiveAchievementID = -1;

	public bool _CleanupStatusListClientSide;

	private List<string> mNotificationList = new List<string>();

	private bool mValidNotificationLevel;

	private bool mInteractiveNotificationLevel;

	private bool mCanRegisterNotification = true;

	private TimedMissionSlotData mSlotForPurchase;

	private int mForceCompleteCost;

	private bool mPendingPurchase;

	private bool mSlotDataUpdated;

	private PairData mPairData;

	private ItemData mCompletionItem;

	private ItemData mCoolDownItem;

	private float mUpdateSlotStateInterval;

	private TimedMissionList mTimedMissionList;

	private int mItemDataLoadCount;

	private bool mTimedMissionDataSavePending;

	private List<TimedMissionSlotData> mTimedMissionSlotList = new List<TimedMissionSlotData>();

	private TimedMissionStatusList mTimedMissionStatusList = new TimedMissionStatusList();

	private bool mFetchingServerTime;

	private List<TimedMissionSlotData> mCompletedSlots;

	private List<TimedMissionSlotData> mCoolingSlots;

	public static TimedMissionManager pInstance => mInstance;

	public static bool pIsReady => mIsReady;

	public bool pIsEnabled
	{
		get
		{
			if (mIsReady)
			{
				return !mTimedMissionList.Disabled;
			}
			return false;
		}
	}

	public int pCheatPreferredMissionID
	{
		get
		{
			return mCheatPreferredMissionID;
		}
		set
		{
			mCheatPreferredMissionID = value;
		}
	}

	public bool pValidNotificationLevel => mValidNotificationLevel;

	public bool pInteractiveNotificationLevel => mInteractiveNotificationLevel;

	public bool pCanRegisterNotification
	{
		get
		{
			return mCanRegisterNotification;
		}
		set
		{
			mCanRegisterNotification = value;
		}
	}

	public List<TimedMissionSlotData> pTimedMissionSlotList
	{
		get
		{
			return mTimedMissionSlotList;
		}
		set
		{
			mTimedMissionSlotList = value;
		}
	}

	public event TimedMissionSlotStateChange OnSlotStateStatus;

	private void Start()
	{
		if (mInstance == null)
		{
			mInstance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			ServerTime.OnServerTimeReady = (Action<WsServiceEvent>)Delegate.Combine(ServerTime.OnServerTimeReady, new Action<WsServiceEvent>(pInstance.OnGetServerTime));
			mCompletedSlots = new List<TimedMissionSlotData>();
			mCoolingSlots = new List<TimedMissionSlotData>();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Update()
	{
		if (mIsReady)
		{
			if (!mSlotDataUpdated)
			{
				UpdateSlotInfoFromItems();
			}
			if (!mTimedMissionList.Disabled)
			{
				UpdateSlotStates();
			}
			if (mValidNotificationLevel && mNotificationList.Count > 0)
			{
				MessageInfo msgData = new MessageInfo();
				UiChatHistory.AddSystemNotification(mNotificationList[0] + _StableMissionActionText.GetLocalizedString(), msgData, OnSystemMessageClicked);
				mNotificationList.RemoveAt(0);
			}
		}
	}

	private void UpdateSlotStates()
	{
		mUpdateSlotStateInterval += Time.deltaTime;
		if (!(mUpdateSlotStateInterval >= _SlotUpdateCheckInterval))
		{
			return;
		}
		mUpdateSlotStateInterval = 0f;
		for (int i = 0; i < mTimedMissionSlotList.Count; i++)
		{
			TimedMissionSlotData timedMissionSlotData = mTimedMissionSlotList[i];
			if (timedMissionSlotData == null)
			{
				continue;
			}
			switch (timedMissionSlotData.State)
			{
			case TimedMissionState.Alotted:
				if (timedMissionSlotData.pMission != null && !IsMissionValid(timedMissionSlotData.pMission))
				{
					ResetSlot(timedMissionSlotData);
				}
				break;
			case TimedMissionState.Started:
				CheckMissionCompleted(timedMissionSlotData);
				break;
			case TimedMissionState.CoolDown:
				if (GetCoolDownTime(timedMissionSlotData) < TimeSpan.Zero)
				{
					ChangeState(timedMissionSlotData, TimedMissionState.None);
				}
				break;
			}
		}
	}

	public static void Reset()
	{
		if (pInstance != null)
		{
			mIsReady = false;
			pInstance.mNotificationList.Clear();
			pInstance.mPairData = null;
			pInstance.mSlotDataUpdated = false;
			pInstance.mUpdateSlotStateInterval = 0f;
			pInstance.mTimedMissionSlotList.Clear();
			pInstance.mTimedMissionStatusList = new TimedMissionStatusList();
		}
	}

	public static void Init()
	{
		if (pInstance != null)
		{
			if (!pInstance.mConfigLoaded)
			{
				pInstance.LoadAllMissions();
			}
			else
			{
				pInstance.LoadSlotStateData();
			}
		}
	}

	private void LoadAllMissions()
	{
		RsResourceManager.Load(_ConfigPath, XmlLoadEventHandler);
	}

	public void XmlLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				mTimedMissionList = UtUtilities.DeserializeFromXml<TimedMissionList>((string)inObject);
				foreach (TimedMission mission in mTimedMissionList.Missions)
				{
					if (mission.MaxNoOfTimes > 0)
					{
						mission.pStatusTrack = true;
					}
					foreach (PrerequisiteItem prerequisite in mission.Prerequisites)
					{
						if (prerequisite.Type == PrerequisiteRequiredType.Mission)
						{
							int missionID = UtStringUtil.Parse(prerequisite.Value, 0);
							TimedMission timedMission = FindMission(missionID);
							if (timedMission != null)
							{
								timedMission.pStatusTrack = true;
							}
						}
					}
				}
			}
			mConfigLoaded = true;
			LoadSlotStateData();
			break;
		case RsResourceLoadEvent.ERROR:
			mTimedMissionList = null;
			LoadSlotStateData();
			UtDebug.LogError("Timed Mission data file missing!!!");
			break;
		}
	}

	public LocaleString[] GetMissionLogs(int slotID)
	{
		if (mTimedMissionList != null)
		{
			TimedMissionSlotData slot = GetSlotData(slotID);
			if (slot != null)
			{
				MissionLogSet missionLogSet = mTimedMissionList.LogSets.Find((MissionLogSet l) => l.LogSetID == slot.LogID);
				if (missionLogSet != null)
				{
					if (missionLogSet.IsRandom)
					{
						UnityEngine.Random.InitState(slot.LogSeed);
						List<LocaleString> list = new List<LocaleString>();
						list.AddRange(missionLogSet.LogString);
						list.RemoveRange(0, 1);
						list.RemoveRange(list.Count - 1, 1);
						List<LocaleString> list2 = new List<LocaleString>();
						for (int i = 0; i < 1; i++)
						{
							list2.Add(missionLogSet.LogString[i]);
						}
						int num = ((list.Count < missionLogSet.Count) ? list.Count : missionLogSet.Count);
						num -= 2;
						for (int j = 0; j < num; j++)
						{
							int index = UnityEngine.Random.Range(0, list.Count - 1);
							list2.Add(list[index]);
							list.Remove(list[index]);
						}
						for (int k = missionLogSet.LogString.Length - 1; k < missionLogSet.LogString.Length; k++)
						{
							list2.Add(missionLogSet.LogString[k]);
						}
						return list2.ToArray();
					}
					return missionLogSet.LogString;
				}
			}
		}
		return null;
	}

	public void AddSlot(ItemData itemData, bool save)
	{
		mTimedMissionDataSavePending = true;
		TimedMissionSlotData timedMissionSlotData = new TimedMissionSlotData();
		timedMissionSlotData.LoadFromItemData(itemData, mTimedMissionSlotList.Count);
		mTimedMissionSlotList.Add(timedMissionSlotData);
		if (save)
		{
			SaveSlotData();
		}
	}

	public void ResetSlot(int slotID)
	{
		TimedMissionSlotData slotData = GetSlotData(slotID);
		if (slotData != null)
		{
			ResetSlot(slotData);
		}
	}

	public void ResetSlot(TimedMissionSlotData slot)
	{
		if (slot != null)
		{
			slot.PetIDs = null;
			slot.StartDate = ServerTime.pCurrentTime;
			TimedMission nextMission = GetNextMission(slot);
			if (nextMission != null)
			{
				AssignMission(slot, nextMission);
			}
			else
			{
				ChangeState(slot, TimedMissionState.None);
			}
		}
	}

	private void LoadSlotStateData()
	{
		PairData.Load(_PairDataID, PairDataLoadHandler, null);
	}

	private void PairDataLoadHandler(bool success, PairData pData, object inUserData)
	{
		mPairData = pData;
		if (mPairData != null)
		{
			mPairData.Init();
			UtDebug.Log("Deserializing TimedMission data...", 100);
			int intValue = mPairData.GetIntValue("NumSlots", 0);
			string value = mPairData.GetValue("MissionStatus");
			if (!string.IsNullOrEmpty(value) && !value.Equals("___VALUE_NOT_FOUND___"))
			{
				mTimedMissionStatusList = UtUtilities.DeserializeFromXml(value, typeof(TimedMissionStatusList)) as TimedMissionStatusList;
				if (_CleanupStatusListClientSide && !mPairData.GetBoolValue("MissionStatusCleaned", defaultVal: false))
				{
					bool flag = true;
					for (int num = mTimedMissionStatusList.MissionStatusList.Count - 1; num >= 0; num--)
					{
						TimedMissionStatus timedMissionStatus = mTimedMissionStatusList.MissionStatusList[num];
						TimedMission timedMission = FindMission(timedMissionStatus.MissionID);
						if (timedMission == null || !timedMission.pStatusTrack)
						{
							mTimedMissionStatusList.MissionStatusList.Remove(timedMissionStatus);
						}
					}
					mPairData.SetValue("MissionStatusCleaned", flag.ToString());
					SaveTimedMissionStatusData();
				}
			}
			if (intValue > 0)
			{
				mTimedMissionSlotList.Clear();
				for (int i = 0; i < intValue; i++)
				{
					if (UtUtilities.DeserializeFromXml(mPairData.GetValue("Slot" + i), typeof(TimedMissionSlotData)) is TimedMissionSlotData timedMissionSlotData)
					{
						if (timedMissionSlotData.SlotID != i)
						{
							timedMissionSlotData.SlotID = i;
						}
						if (ServerTime.pCurrentTime < timedMissionSlotData.StartDate)
						{
							ResetSlot(timedMissionSlotData);
							continue;
						}
						mTimedMissionSlotList.Add(timedMissionSlotData);
						if (timedMissionSlotData.MissionID != -1)
						{
							timedMissionSlotData.pMission = FindMission(timedMissionSlotData.MissionID);
						}
						timedMissionSlotData.Init();
					}
					else
					{
						UtDebug.LogError("Could not load Timed Mission Data " + i);
					}
				}
			}
			else
			{
				mTimedMissionSlotList.Clear();
				UtDebug.Log("Timed mission slot data not avaialble!", 100);
			}
		}
		else
		{
			UtDebug.LogError("Invalid pair data for Timed mission slots!", 100);
		}
		UpdateSlotInfoFromItems();
		ItemStoreDataLoader.Load(_TimePurchaseStoreID, OnStoreLoaded);
	}

	private void AwardItemHandler(bool success, object inUserData)
	{
		CheckInventory(CommonInventoryData.pInstance);
	}

	private void UpdateSlotInfoFromItems()
	{
		if (!mSlotDataUpdated && CommonInventoryData.pIsReady)
		{
			CheckInventory(CommonInventoryData.pInstance);
			mSlotDataUpdated = true;
		}
	}

	private void CheckInventory(CommonInventoryData cid)
	{
		UserItemData[] items = cid.GetItems(529);
		mItemDataLoadCount = 1;
		foreach (DefaultSlotInfo info in _SlotInfo)
		{
			UserItemData userItemData = null;
			if (items != null)
			{
				userItemData = Array.Find(items, (UserItemData s) => s.Item.ItemID == info._ItemID);
			}
			List<TimedMissionSlotData> list = mTimedMissionSlotList.FindAll((TimedMissionSlotData s) => s.ItemID == info._ItemID);
			int num = info._DefaultCount;
			if (userItemData != null)
			{
				num += userItemData.Quantity;
			}
			if (num > info._MaxCount)
			{
				num = info._MaxCount;
			}
			if (list != null)
			{
				num -= list.Count;
			}
			if (num > 0)
			{
				mItemDataLoadCount++;
				ItemData.Load(info._ItemID, OnSlotItemDataReady, num);
			}
		}
		mItemDataLoadCount--;
		if (mTimedMissionDataSavePending && mItemDataLoadCount <= 0)
		{
			SaveSlotData();
		}
	}

	private void OnSlotItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		mItemDataLoadCount--;
		int num = (int)inUserData;
		for (int i = 0; i < num; i++)
		{
			AddSlot(dataItem, save: false);
		}
		if (mTimedMissionDataSavePending && mItemDataLoadCount <= 0)
		{
			SaveSlotData();
		}
	}

	public void SaveSlotData()
	{
		if (mPairData != null)
		{
			mTimedMissionDataSavePending = false;
			mPairData.SetValue("NumSlots", mTimedMissionSlotList.Count.ToString());
			for (int i = 0; i < mTimedMissionSlotList.Count; i++)
			{
				mPairData.SetValue("Slot" + i, UtUtilities.SerializeToXml(mTimedMissionSlotList[i], noNamespace: true));
			}
			PairData.Save(_PairDataID);
		}
		else
		{
			Debug.LogError("Invalid pair data for TimedMission slot data!");
		}
	}

	public void SaveTimedMissionStatusData()
	{
		if (mPairData != null)
		{
			mPairData.SetValue("MissionStatus", UtUtilities.SerializeToXml(mTimedMissionStatusList, noNamespace: true));
			PairData.Save(_PairDataID);
		}
		else
		{
			Debug.LogError("Invalid pair data for TimedMission slot data!");
		}
	}

	private void OnStoreLoaded(StoreData sd)
	{
		mCoolDownItem = sd.FindItem(_CostOfCoolDown.ItemID);
		foreach (CostPerMinite costItem in _CostOfCompletionList.CostItems)
		{
			costItem.pCompletionItem = sd.FindItem(costItem.ItemID);
		}
		List<TimedMission> missions = mTimedMissionList.Missions;
		for (int i = 0; i < missions.Count; i++)
		{
			if (missions[i].CostOfCompletionList == null)
			{
				continue;
			}
			missions[i].CostOfCompletionList.CostItems.Sort((CostPerMinite a, CostPerMinite b) => b.Duration - a.Duration);
			foreach (CostPerMinite costItem2 in missions[i].CostOfCompletionList.CostItems)
			{
				if (costItem2.ItemID > 0)
				{
					costItem2.pCompletionItem = sd.FindItem(costItem2.ItemID);
				}
			}
		}
		mIsReady = true;
	}

	public TimedMission GetNextMission(TimedMissionSlotData slot)
	{
		TimedMission result = null;
		if (mTimedMissionList != null)
		{
			if (mCheatPreferredMissionID != -1)
			{
				TimedMission timedMission = FindMission(mCheatPreferredMissionID);
				if (timedMission != null && timedMission.Type == slot.Type)
				{
					mCheatPreferredMissionID = -1;
					return timedMission;
				}
			}
			List<TimedMission> list = mTimedMissionList.Missions.FindAll((TimedMission m) => m.Type == slot.Type);
			list.Sort((TimedMission a, TimedMission b) => a.pProbabilityBucket - b.pProbabilityBucket);
			int num = 0;
			int lastProbabilityBucket = 0;
			int lastPlayedMissionID = slot.MissionID;
			TimedMission timedMission2 = list.Find((TimedMission m) => m.MissionID == lastPlayedMissionID);
			if (timedMission2 != null)
			{
				list.Remove(timedMission2);
				if (!IsMissionValid(timedMission2))
				{
					timedMission2 = null;
				}
			}
			int num2 = 0;
			while (num2 < list.Count)
			{
				result = list[num2];
				if (IsMissionValid(result))
				{
					if (lastProbabilityBucket != result.pProbabilityBucket)
					{
						num += result.pProbabilityBucket;
					}
					lastProbabilityBucket = result.pProbabilityBucket;
					num2++;
				}
				else
				{
					list.Remove(result);
				}
			}
			int num3 = UnityEngine.Random.Range(1, num + 1);
			lastProbabilityBucket = 0;
			num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				result = list[i];
				if (lastProbabilityBucket != result.pProbabilityBucket)
				{
					num += result.pProbabilityBucket;
				}
				lastProbabilityBucket = result.pProbabilityBucket;
				if (num3 <= num)
				{
					break;
				}
			}
			list = list.FindAll((TimedMission m) => m.pProbabilityBucket == lastProbabilityBucket);
			result = ((list == null || list.Count <= 0) ? timedMission2 : list[UnityEngine.Random.Range(0, list.Count)]);
		}
		return result;
	}

	private bool IsMissionValid(TimedMission mission)
	{
		if (mission.MaxNoOfTimes > 0 && mission.MaxNoOfTimes <= GetWinCount(mission.MissionID))
		{
			return false;
		}
		EventManager activeEvent = EventManager.GetActiveEvent();
		foreach (PrerequisiteItem prerequisite in mission.Prerequisites)
		{
			if (prerequisite.Type == PrerequisiteRequiredType.Mission)
			{
				int missionID = UtStringUtil.Parse(prerequisite.Value, 0);
				TimedMission timedMission = FindMission(missionID);
				if (timedMission != null && timedMission.pPlayedCount <= 0)
				{
					return false;
				}
			}
			else if (prerequisite.Type == PrerequisiteRequiredType.Item)
			{
				int num = UtStringUtil.Parse(prerequisite.Value, -1);
				if (num != -1 && CommonInventoryData.pInstance.GetQuantity(num) + ParentData.pInstance.pInventory.GetQuantity(num) < prerequisite.Quantity)
				{
					return false;
				}
			}
			else if (prerequisite.Type == PrerequisiteRequiredType.Rank)
			{
				string[] array = UtStringUtil.Parse<string>(prerequisite.Value, null).Split(',');
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				if (array.Length > 1)
				{
					num2 = int.Parse(array[0]);
					if (!string.IsNullOrEmpty(array[1]))
					{
						num3 = int.Parse(array[1]);
					}
					if (array.Length > 2 && !string.IsNullOrEmpty(array[2]))
					{
						num4 = int.Parse(array[2]);
					}
				}
				if (num2 <= 0)
				{
					continue;
				}
				if (num2 == 8)
				{
					bool flag = false;
					int num5 = 0;
					foreach (RaisedPetData[] value2 in RaisedPetData.pActivePets.Values)
					{
						if (value2 == null)
						{
							continue;
						}
						RaisedPetData[] array2 = value2;
						for (int i = 0; i < array2.Length; i++)
						{
							UserRank userRank = PetRankData.GetUserRank(array2[i]);
							if (userRank != null)
							{
								num5 = userRank.RankID;
								if (num5 >= num3 && (num4 == 0 || num5 <= num4))
								{
									flag = true;
									break;
								}
							}
						}
						if (flag)
						{
							break;
						}
					}
					if (!flag)
					{
						return false;
					}
				}
				else
				{
					UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(num2);
					if (num3 > 0 && (userAchievementInfoByType == null || userAchievementInfoByType.RankID < num3))
					{
						return false;
					}
					if (num4 > 0 && userAchievementInfoByType != null && userAchievementInfoByType.RankID > num4)
					{
						return false;
					}
				}
			}
			else if (prerequisite.Type == PrerequisiteRequiredType.DateRange)
			{
				string value = prerequisite.Value;
				if (!string.IsNullOrEmpty(value))
				{
					string[] array3 = value.Split(',');
					DateTime dateTime = DateTime.Parse(array3[0]);
					DateTime dateTime2 = DateTime.Parse(array3[1]);
					if (ServerTime.pCurrentTime <= dateTime || ServerTime.pCurrentTime >= dateTime2)
					{
						return false;
					}
				}
			}
			else if (prerequisite.Type == PrerequisiteRequiredType.Event)
			{
				return ((bool)activeEvent && activeEvent.EventInProgress() && !activeEvent.GracePeriodInProgress()) ? (activeEvent._EventName == prerequisite.Value) : string.IsNullOrEmpty(prerequisite.Value);
			}
		}
		return true;
	}

	private bool IsMissionLocked(TimedMission mission)
	{
		if (mission.Type == SlotType.Member && !SubscriptionInfo.pIsMember)
		{
			return true;
		}
		if (mission.Type == SlotType.Toothless && (!RaisedPetData.pActivePets.ContainsKey(17) || RaisedPetData.pActivePets[17] == null))
		{
			return true;
		}
		return false;
	}

	public int GetWinCount(int missionID)
	{
		int result = 0;
		TimedMissionStatus timedMissionStatus = null;
		if (mTimedMissionStatusList != null && mTimedMissionStatusList.MissionStatusList != null)
		{
			timedMissionStatus = mTimedMissionStatusList.MissionStatusList.Find((TimedMissionStatus m) => m.MissionID == missionID);
		}
		if (timedMissionStatus != null)
		{
			result = timedMissionStatus.WinCount;
		}
		return result;
	}

	public TimedMission FindMission(int missionID)
	{
		TimedMission result = null;
		if (mTimedMissionList != null)
		{
			result = mTimedMissionList.Missions.Find((TimedMission m) => m.MissionID == missionID);
		}
		return result;
	}

	public TimedMissionSlotData GetSlotData(int slotID)
	{
		TimedMissionSlotData result = null;
		if (mTimedMissionSlotList != null)
		{
			result = mTimedMissionSlotList.Find((TimedMissionSlotData m) => m.SlotID == slotID);
		}
		return result;
	}

	public TimedMissionSlotData GetSlotDataFromItem(int itemID)
	{
		TimedMissionSlotData result = null;
		if (mTimedMissionSlotList != null)
		{
			result = mTimedMissionSlotList.Find((TimedMissionSlotData m) => m.ItemID == itemID);
		}
		return result;
	}

	public TimedMission GetCurrentMission(int slotID)
	{
		return GetCurrentMission(GetSlotData(slotID));
	}

	public TimedMission GetCurrentMission(TimedMissionSlotData slot)
	{
		TimedMission result = null;
		if (slot != null && slot.MissionID > 0)
		{
			if (slot.pMission == null)
			{
				slot.pMission = FindMission(slot.MissionID);
			}
			result = slot.pMission;
		}
		return result;
	}

	public void AssignMission(int slotID, int missionID)
	{
		TimedMission timedMission = FindMission(missionID);
		TimedMissionSlotData slotData = GetSlotData(slotID);
		if (slotData != null && timedMission != null)
		{
			AssignMission(slotData, timedMission);
		}
	}

	public void AssignMission(TimedMissionSlotData slot, TimedMission mission)
	{
		if (mission.MaxNoOfTimes > 0 && mission.MaxNoOfTimes <= mission.pPlayedCount)
		{
			return;
		}
		slot.MissionID = mission.MissionID;
		slot.pMission = mission;
		ChangeState(slot, TimedMissionState.Alotted);
		SaveSlotData();
		if (mission.pStatusTrack)
		{
			if (mTimedMissionStatusList.MissionStatusList == null)
			{
				mTimedMissionStatusList.MissionStatusList = new List<TimedMissionStatus>();
			}
			TimedMissionStatus timedMissionStatus = mTimedMissionStatusList.MissionStatusList.Find((TimedMissionStatus m) => m.MissionID == mission.MissionID);
			if (timedMissionStatus == null)
			{
				timedMissionStatus = new TimedMissionStatus();
				timedMissionStatus.MissionID = mission.MissionID;
				mTimedMissionStatusList.MissionStatusList.Add(timedMissionStatus);
			}
			timedMissionStatus.PlayedCount++;
			mission.pPlayedCount = timedMissionStatus.PlayedCount;
			SaveTimedMissionStatusData();
		}
	}

	public float GetWinProbability(int slotID)
	{
		float num = 0f;
		TimedMissionSlotData slotData = GetSlotData(slotID);
		if (slotData != null && slotData.MissionID >= 0)
		{
			TimedMission currentMission = GetCurrentMission(slotData);
			if (currentMission != null)
			{
				num += (float)currentMission.WinFactor;
				for (int i = 0; i < slotData.PetIDs.Count; i++)
				{
					num += GetWinProbabilityForPet(currentMission, slotData.PetIDs[i]);
				}
			}
		}
		if (num > 100f)
		{
			num = 100f;
		}
		return num;
	}

	public float GetWinProbability(TimedMission mission, List<int> petIDs)
	{
		float num = 0f;
		if (mission != null)
		{
			num += (float)mission.WinFactor;
			for (int i = 0; i < petIDs.Count; i++)
			{
				num += GetWinProbabilityForPet(mission, petIDs[i]);
			}
		}
		if (num > 100f)
		{
			num = 100f;
		}
		return num;
	}

	public float GetWinProbabilityForPet(TimedMission mission, int petID)
	{
		float num = 0f;
		num += (float)mission.WinFactorPerDragon;
		RaisedPetData byID = RaisedPetData.GetByID(petID);
		if (byID != null)
		{
			SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(byID.PetTypeID);
			int[] array = new int[8];
			foreach (BonusFactor dragonFactor in mission.DragonFactors)
			{
				int type = (int)dragonFactor.Type;
				switch (dragonFactor.Type)
				{
				case FactorType.Age:
					if (int.Parse(dragonFactor.Value) == RaisedPetData.GetAgeIndex(byID.pStage) && array[type] < dragonFactor.Factor)
					{
						array[type] = dragonFactor.Factor;
					}
					break;
				case FactorType.PetType:
					if (int.Parse(dragonFactor.Value) == byID.PetTypeID && array[type] < dragonFactor.Factor)
					{
						array[type] = dragonFactor.Factor;
					}
					break;
				case FactorType.DragonClass:
				{
					DragonClass dragonClass = sanctuaryPetTypeInfo._DragonClass;
					if (int.Parse(dragonFactor.Value) == (int)dragonClass && array[type] < dragonFactor.Factor)
					{
						array[type] = dragonFactor.Factor;
					}
					break;
				}
				case FactorType.Rank:
				{
					int rankID = PetRankData.GetUserRank(byID).RankID;
					if (int.Parse(dragonFactor.Value) <= rankID && array[type] < dragonFactor.Factor)
					{
						array[type] = dragonFactor.Factor;
					}
					break;
				}
				case FactorType.ItemEquipped:
				{
					int num4 = int.Parse(dragonFactor.Value);
					RaisedPetAccessory[] accessories = byID.Accessories;
					foreach (RaisedPetAccessory ac in accessories)
					{
						if (num4 == byID.GetAccessoryItemID(ac))
						{
							array[type] += dragonFactor.Factor;
						}
					}
					break;
				}
				case FactorType.PrimaryType:
					if (dragonFactor.Value.Equals(sanctuaryPetTypeInfo.pPrimaryType._PrimaryType) && array[type] < dragonFactor.Factor)
					{
						array[type] = dragonFactor.Factor;
					}
					break;
				case FactorType.SecondaryType:
					if (dragonFactor.Value.Equals(sanctuaryPetTypeInfo.pSecondaryType._SecondaryType) && array[type] < dragonFactor.Factor)
					{
						array[type] = dragonFactor.Factor;
					}
					break;
				case FactorType.Energy:
				{
					int num2 = int.Parse(dragonFactor.Value);
					RaisedPetState raisedPetState = byID.FindStateData(SanctuaryPetMeterType.ENERGY.ToString());
					float num3 = 0f;
					if (raisedPetState != null)
					{
						num3 = raisedPetState.Value;
					}
					if ((float)num2 <= num3 && array[type] < dragonFactor.Factor)
					{
						array[type] = dragonFactor.Factor;
					}
					break;
				}
				}
			}
			for (int j = 0; j < 8; j++)
			{
				num += (float)array[j];
			}
		}
		return num;
	}

	public bool IsPetValid(TimedMission mission, int petID, int inFactorType = -1)
	{
		RaisedPetData byID = RaisedPetData.GetByID(petID);
		if (byID == null)
		{
			return false;
		}
		if (mission != null && mission.Qualify != null)
		{
			if (mission.Qualify.IsIncludeList)
			{
				bool[] array = new bool[8];
				bool[] array2 = new bool[8];
				foreach (QualifyFactor qualify in mission.Qualify.QualifyList)
				{
					int type = (int)qualify.Type;
					if (array[type] || (type != inFactorType && inFactorType != -1))
					{
						continue;
					}
					array2[type] = true;
					switch (qualify.Type)
					{
					case FactorType.Age:
						if (int.Parse(qualify.Value) == RaisedPetData.GetAgeIndex(byID.pStage))
						{
							array[type] = true;
						}
						break;
					case FactorType.PetType:
						if (int.Parse(qualify.Value) == byID.PetTypeID)
						{
							array[type] = true;
						}
						break;
					case FactorType.DragonClass:
					{
						DragonClass dragonClass = SanctuaryData.FindSanctuaryPetTypeInfo(byID.PetTypeID)._DragonClass;
						if (int.Parse(qualify.Value) == (int)dragonClass)
						{
							array[type] = true;
						}
						break;
					}
					case FactorType.Rank:
					{
						int rankID = PetRankData.GetUserRank(byID).RankID;
						if (int.Parse(qualify.Value) <= rankID)
						{
							array[type] = true;
						}
						break;
					}
					case FactorType.ItemEquipped:
					{
						int num3 = int.Parse(qualify.Value);
						RaisedPetAccessory[] accessories = byID.Accessories;
						foreach (RaisedPetAccessory ac in accessories)
						{
							if (num3 == byID.GetAccessoryItemID(ac))
							{
								array[type] = true;
							}
						}
						break;
					}
					case FactorType.PrimaryType:
					{
						SanctuaryPetTypeInfo sanctuaryPetTypeInfo2 = SanctuaryData.FindSanctuaryPetTypeInfo(byID.PetTypeID);
						if (qualify.Value.Equals(sanctuaryPetTypeInfo2.pPrimaryType._PrimaryType))
						{
							array[type] = true;
						}
						break;
					}
					case FactorType.SecondaryType:
					{
						SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(byID.PetTypeID);
						if (qualify.Value.Equals(sanctuaryPetTypeInfo.pSecondaryType._SecondaryType))
						{
							array[type] = true;
						}
						break;
					}
					case FactorType.Energy:
					{
						int num = int.Parse(qualify.Value);
						RaisedPetState raisedPetState = byID.FindStateData(SanctuaryPetMeterType.ENERGY.ToString());
						float num2 = 0f;
						if (raisedPetState != null)
						{
							num2 = raisedPetState.Value;
						}
						if ((float)num <= num2)
						{
							array[type] = true;
						}
						break;
					}
					}
				}
				for (int j = 0; j < 8; j++)
				{
					if (!array[j] && array2[j])
					{
						return false;
					}
				}
			}
			else
			{
				foreach (QualifyFactor qualify2 in mission.Qualify.QualifyList)
				{
					if (qualify2.Type != (FactorType)inFactorType && inFactorType != -1)
					{
						continue;
					}
					switch (qualify2.Type)
					{
					case FactorType.Age:
						if (int.Parse(qualify2.Value) == RaisedPetData.GetAgeIndex(byID.pStage))
						{
							return false;
						}
						break;
					case FactorType.PetType:
						if (int.Parse(qualify2.Value) == byID.PetTypeID)
						{
							return false;
						}
						break;
					case FactorType.DragonClass:
					{
						DragonClass dragonClass2 = SanctuaryData.FindSanctuaryPetTypeInfo(byID.PetTypeID)._DragonClass;
						if (int.Parse(qualify2.Value) == (int)dragonClass2)
						{
							return false;
						}
						break;
					}
					case FactorType.Rank:
					{
						int rankID2 = PetRankData.GetUserRank(byID).RankID;
						if (int.Parse(qualify2.Value) > rankID2)
						{
							return false;
						}
						break;
					}
					case FactorType.ItemEquipped:
					{
						int num6 = int.Parse(qualify2.Value);
						RaisedPetAccessory[] accessories = byID.Accessories;
						foreach (RaisedPetAccessory ac2 in accessories)
						{
							if (num6 == byID.GetAccessoryItemID(ac2))
							{
								return false;
							}
						}
						break;
					}
					case FactorType.PrimaryType:
					{
						SanctuaryPetTypeInfo sanctuaryPetTypeInfo4 = SanctuaryData.FindSanctuaryPetTypeInfo(byID.PetTypeID);
						if (qualify2.Value.Equals(sanctuaryPetTypeInfo4.pPrimaryType._PrimaryType))
						{
							return false;
						}
						break;
					}
					case FactorType.SecondaryType:
					{
						SanctuaryPetTypeInfo sanctuaryPetTypeInfo3 = SanctuaryData.FindSanctuaryPetTypeInfo(byID.PetTypeID);
						if (qualify2.Value.Equals(sanctuaryPetTypeInfo3.pSecondaryType._SecondaryType))
						{
							return false;
						}
						break;
					}
					case FactorType.Energy:
					{
						int num4 = int.Parse(qualify2.Value);
						RaisedPetState raisedPetState2 = byID.FindStateData(SanctuaryPetMeterType.ENERGY.ToString());
						float num5 = 0f;
						if (raisedPetState2 != null)
						{
							num5 = raisedPetState2.Value;
						}
						if ((float)num4 > num5)
						{
							return false;
						}
						break;
					}
					}
				}
			}
		}
		return true;
	}

	public List<object> GetPetValidityRequirement(TimedMission mission, int petID, FactorType inFactorType)
	{
		List<object> list = new List<object>();
		if (inFactorType == FactorType.Age && !mission.Qualify.IsIncludeList)
		{
			for (int i = 0; i <= RaisedPetData.GetAgeIndex(RaisedPetStage.TITAN); i++)
			{
				list.Add(i);
			}
		}
		foreach (QualifyFactor qualify in mission.Qualify.QualifyList)
		{
			if (qualify.Type == inFactorType && (qualify.Type == FactorType.Age || qualify.Type == FactorType.Rank || qualify.Type == FactorType.Energy))
			{
				int num = int.Parse(qualify.Value);
				if (qualify.Type == FactorType.Age && !mission.Qualify.IsIncludeList)
				{
					list.Remove(num);
				}
				else
				{
					list.Add(num);
				}
			}
		}
		return list;
	}

	public bool StartMission(int slotID, List<int> petIDs)
	{
		bool result = false;
		TimedMissionSlotData slot = GetSlotData(slotID);
		if (slot != null && slot.MissionID >= 0)
		{
			TimedMission currentMission = GetCurrentMission(slot);
			if (currentMission != null)
			{
				if (slot.PetIDs == null)
				{
					slot.PetIDs = new List<int>();
				}
				slot.PetIDs.Clear();
				slot.PetIDs.AddRange(petIDs);
				result = true;
				slot.StartDate = ServerTime.pCurrentTime;
				if (currentMission.LogSetIDs != null && currentMission.LogSetIDs.Count > 0)
				{
					int index = UnityEngine.Random.Range(0, currentMission.LogSetIDs.Count - 1);
					slot.LogID = currentMission.LogSetIDs[index];
					MissionLogSet missionLogSet = mTimedMissionList.LogSets.Find((MissionLogSet l) => l.LogSetID == slot.LogID);
					if (missionLogSet != null && missionLogSet.IsRandom)
					{
						slot.LogSeed = UnityEngine.Random.Range(1, 1000);
					}
				}
				ChangeState(slot, TimedMissionState.Started);
				string localizedString = _QuestOverNotification.GetLocalizedString();
				localizedString = localizedString.Replace("{{Result}}", slot.pMission.Title.GetLocalizedString());
				NotificationData.SetNotification(DateTime.Now.AddMinutes(slot.pMission.Duration), UserInfo.pInstance.UserID + slot.MissionID, localizedString);
				SaveSlotData();
			}
		}
		return result;
	}

	public void CheckMissionCompleted(int slotID)
	{
		CheckMissionCompleted(GetSlotData(slotID));
	}

	public void CheckMissionCompleted(TimedMissionSlotData slot)
	{
		if (slot != null && slot.MissionID >= 0)
		{
			TimedMission currentMission = GetCurrentMission(slot);
			if (currentMission != null && (slot.State == TimedMissionState.Ended || (slot.State == TimedMissionState.Started && slot.StartDate.AddMinutes(currentMission.Duration) < ServerTime.pCurrentTime)))
			{
				CheckQuestEnd(slot);
			}
		}
	}

	public void ChangeState(TimedMissionSlotData slot, TimedMissionState newState)
	{
		if (slot == null || newState == slot.State)
		{
			return;
		}
		TimedMissionState state = slot.State;
		slot.State = newState;
		if ((newState == TimedMissionState.Ended || newState == TimedMissionState.Started || newState == TimedMissionState.Alotted) && this.OnSlotStateStatus != null)
		{
			this.OnSlotStateStatus(state, newState);
		}
		if (mCanRegisterNotification)
		{
			if (newState == TimedMissionState.Ended)
			{
				string localizedString = slot.pMission.Title.GetLocalizedString();
				string text = (mInteractiveNotificationLevel ? _QuestCompleteText.GetLocalizedString() : _QuestOverNotification.GetLocalizedString());
				text = text.Replace("{{Result}}", localizedString);
				mNotificationList.Add(text);
			}
			else if (state == TimedMissionState.CoolDown)
			{
				string localizedString2 = _CoolDownOverText.GetLocalizedString();
				mNotificationList.Add(localizedString2);
			}
		}
	}

	private void OnSystemMessageClicked(object messageInfo)
	{
		if (mInteractiveNotificationLevel)
		{
			UserNotifyLoadStableQuest.StartStableQuestOnStableLoad = true;
			StableManager.LoadStableWithJobBoard(0);
		}
	}

	public bool CheckMissionSuccess(int slotID)
	{
		bool result = false;
		TimedMissionSlotData slotData = GetSlotData(slotID);
		if (slotData != null && slotData.MissionID >= 0)
		{
			TimedMission currentMission = GetCurrentMission(slotData);
			if (currentMission != null)
			{
				if (slotData.State == TimedMissionState.Ended || (slotData.State == TimedMissionState.Started && slotData.StartDate.AddMinutes(currentMission.Duration) < ServerTime.pCurrentTime))
				{
					float winProbability = GetWinProbability(slotID);
					if ((float)UnityEngine.Random.Range(0, 100) <= winProbability)
					{
						ChangeState(slotData, TimedMissionState.Won);
						result = true;
					}
					else
					{
						ChangeState(slotData, TimedMissionState.Lost);
					}
				}
				else if (slotData.State == TimedMissionState.Won)
				{
					result = true;
				}
			}
		}
		return result;
	}

	private int GetAchievementID(TimedMission mission, int petCount, bool win)
	{
		int num = 0;
		List<AchievementsPerPet> list = mission.WinAchievements;
		if (!win)
		{
			list = mission.LoseAchievements;
		}
		if (list != null && list.Count > 0)
		{
			int num2 = Math.Abs(petCount - list[0].PetCount);
			num = list[0].AchID;
			for (int i = 1; i < list.Count; i++)
			{
				AchievementsPerPet achievementsPerPet = list[i];
				if (Math.Abs(petCount - achievementsPerPet.PetCount) < num2)
				{
					num2 = Math.Abs(petCount - achievementsPerPet.PetCount);
					num = achievementsPerPet.AchID;
				}
			}
		}
		if (num == 0)
		{
			num = ((!win) ? mission.LoseAchID : mission.WinAchID);
		}
		return num;
	}

	public void CompleteMission(int slotID, TimedMissionCompletion callback)
	{
		CompleteMission(GetSlotData(slotID), callback);
	}

	public void CompleteMission(TimedMissionSlotData slot, TimedMissionCompletion callback)
	{
		mCompletionCallback = callback;
		TimedMission mission = GetCurrentMission(slot);
		ChangeState(slot, TimedMissionState.Ended);
		int achievementID = GetAchievementID(mission, slot.PetIDs.Count, CheckMissionSuccess(slot.SlotID));
		if (achievementID != 0)
		{
			List<Guid?> list = new List<Guid?>();
			for (int i = 0; i < slot.PetIDs.Count; i++)
			{
				RaisedPetData byID = RaisedPetData.GetByID(slot.PetIDs[i]);
				if (byID != null)
				{
					list.Add(byID.EntityID);
				}
			}
			WsWebService.SetAchievementByEntityIDs(achievementID, list.ToArray(), "", ServiceEventHandler, slot);
		}
		else
		{
			bool winStatus = slot.State == TimedMissionState.Won;
			ChangeState(slot, TimedMissionState.None);
			SaveSlotData();
			if (mCompletionCallback != null)
			{
				mCompletionCallback(success: true, winStatus, null);
				mCompletionCallback = null;
			}
		}
		if (_ProgressiveAchievementID != -1)
		{
			UserAchievementTask.Set(_ProgressiveAchievementID);
		}
		if (mission.pStatusTrack)
		{
			if (mTimedMissionStatusList.MissionStatusList == null)
			{
				mTimedMissionStatusList.MissionStatusList = new List<TimedMissionStatus>();
			}
			TimedMissionStatus timedMissionStatus = mTimedMissionStatusList.MissionStatusList.Find((TimedMissionStatus m) => m.MissionID == mission.MissionID);
			if (timedMissionStatus == null)
			{
				timedMissionStatus = new TimedMissionStatus();
				timedMissionStatus.MissionID = mission.MissionID;
				mTimedMissionStatusList.MissionStatusList.Add(timedMissionStatus);
			}
			timedMissionStatus.WinCount++;
			mission.pWinCount = timedMissionStatus.WinCount;
			SaveTimedMissionStatusData();
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			if (inObject != null)
			{
				AchievementReward[] array = (AchievementReward[])inObject;
				if (array != null)
				{
					GameUtilities.AddRewards(array, inUseRewardManager: false, inImmediateShow: false);
				}
				TimedMissionSlotData timedMissionSlotData = (TimedMissionSlotData)inUserData;
				bool winStatus = timedMissionSlotData.State == TimedMissionState.Won;
				ChangeState(timedMissionSlotData, TimedMissionState.None);
				SaveSlotData();
				if (mCompletionCallback != null)
				{
					mCompletionCallback(success: true, winStatus, array);
					mCompletionCallback = null;
				}
			}
			break;
		case WsServiceEvent.ERROR:
			if (mCompletionCallback != null)
			{
				mCompletionCallback(success: false, winStatus: false, null);
				mCompletionCallback = null;
			}
			break;
		}
	}

	public void TrashMission(int slotID)
	{
		TrashMission(GetSlotData(slotID));
	}

	public void TrashMission(TimedMissionSlotData slot)
	{
		if (slot != null)
		{
			ChangeState(slot, TimedMissionState.CoolDown);
			slot.StartDate = ServerTime.pCurrentTime;
			slot.PetIDs = null;
			SaveSlotData();
		}
	}

	public TimedMissionSlotData GetSlotFromEngagedPet(int petID)
	{
		TimedMissionSlotData result = null;
		if (!mTimedMissionList.Disabled)
		{
			result = mTimedMissionSlotList.Find((TimedMissionSlotData m) => m.State >= TimedMissionState.Started && m.PetIDs != null && m.PetIDs.Any((int p) => p == petID));
		}
		return result;
	}

	public bool IsPetEngaged(int petID)
	{
		bool result = false;
		if (!mTimedMissionList.Disabled && GetSlotFromEngagedPet(petID) != null)
		{
			result = true;
		}
		return result;
	}

	public TimeSpan GetPetEngageTime(int petID)
	{
		TimeSpan result = TimeSpan.Zero;
		TimedMissionSlotData timedMissionSlotData = mTimedMissionSlotList.Find((TimedMissionSlotData m) => m.PetIDs != null && m.PetIDs.Any((int p) => p == petID));
		if (timedMissionSlotData != null)
		{
			TimedMission currentMission = GetCurrentMission(timedMissionSlotData);
			result = timedMissionSlotData.StartDate.AddMinutes(currentMission.Duration) - ServerTime.pCurrentTime;
		}
		return result;
	}

	public TimeSpan GetCompletionTime(TimedMissionSlotData slot)
	{
		TimeSpan result = TimeSpan.Zero;
		if (slot != null)
		{
			TimedMission currentMission = GetCurrentMission(slot);
			result = slot.StartDate.AddMinutes(currentMission.Duration) - ServerTime.pCurrentTime;
		}
		return result;
	}

	public TimeSpan GetCoolDownTime(TimedMissionSlotData slot)
	{
		TimeSpan result = TimeSpan.Zero;
		if (slot != null)
		{
			result = slot.StartDate.AddMinutes(slot.pCoolDownDuration) - ServerTime.pCurrentTime;
		}
		return result;
	}

	public int GetCostForCompletion(int slotID)
	{
		TimedMissionSlotData slotData = GetSlotData(slotID);
		return GetCostForCompletion(slotData);
	}

	public int GetCostForCompletion(TimedMissionSlotData slot)
	{
		int result = 0;
		if (slot != null && slot.MissionID >= 0)
		{
			TimedMission currentMission = GetCurrentMission(slot);
			if (currentMission != null)
			{
				int duration = (int)Mathf.Ceil((float)GetCompletionTime(slot).TotalMinutes);
				result = ((currentMission.CostOfCompletionList != null && currentMission.CostOfCompletionList.CostItems != null && currentMission.CostOfCompletionList.CostItems.Count != 0) ? currentMission.CostOfCompletionList.GetCost(duration) : _CostOfCompletionList.GetCost(duration));
			}
		}
		return result;
	}

	public void ForceComplete(TimedMissionSlotData slot, TimedMissionCompletion callback)
	{
		mCompletionCallback = callback;
		bool flag = false;
		if (slot != null && slot.MissionID >= 0)
		{
			TimedMission currentMission = GetCurrentMission(slot);
			if (currentMission != null)
			{
				flag = true;
				mSlotForPurchase = slot;
				int duration = (int)Mathf.Ceil((float)GetCompletionTime(slot).TotalMinutes);
				if (currentMission.CostOfCompletionList == null || currentMission.CostOfCompletionList.CostItems == null || currentMission.CostOfCompletionList.CostItems.Count == 0)
				{
					mForceCompleteCost = _CostOfCompletionList.GetCost(duration);
				}
				else
				{
					mForceCompleteCost = currentMission.CostOfCompletionList.GetCost(duration);
				}
				mPendingPurchase = true;
				if (!mFetchingServerTime)
				{
					mFetchingServerTime = true;
					ServerTime.Init(inForceInit: true);
				}
			}
		}
		if (!flag)
		{
			mCompletionCallback(success: false, winStatus: false, null);
			mCompletionCallback = null;
		}
	}

	private void CostItemPurchaseHandler(CommonInventoryResponse ret)
	{
		if (ret != null && ret.Success)
		{
			CompleteMission(mSlotForPurchase, mCompletionCallback);
			NotificationData.RemoveNotification(UserInfo.pInstance.UserID + mSlotForPurchase.MissionID);
		}
		else
		{
			mCompletionCallback(success: false, winStatus: false, null);
			mCompletionCallback = null;
		}
	}

	public int GetCostForCoolDown(int slotID)
	{
		TimedMissionSlotData slotData = GetSlotData(slotID);
		return GetCostForCoolDown(slotData);
	}

	public int GetCostForCoolDown(TimedMissionSlotData slot)
	{
		int result = 0;
		if (slot != null)
		{
			int num = (int)Mathf.Ceil((float)GetCoolDownTime(slot).TotalMinutes / (float)_CostOfCoolDown.Duration);
			result = mCoolDownItem.FinalCashCost * num;
		}
		return result;
	}

	public void BuyCoolDownTime(TimedMissionSlotData slot, TimedMissionCoolDownPurchase callback)
	{
		mCoolDownPurchaseCallback = callback;
		bool flag = false;
		if (slot != null)
		{
			CostPerMinite costOfCoolDown = _CostOfCoolDown;
			int amount = (int)Mathf.Ceil((float)GetCoolDownTime(slot).TotalMinutes / (float)costOfCoolDown.Duration);
			flag = true;
			mSlotForPurchase = slot;
			CommonInventoryData.pInstance.AddPurchaseItem(costOfCoolDown.ItemID, amount, ItemPurchaseSource.SQUAD_TACTICS.ToString());
			CommonInventoryData.pInstance.DoPurchase(2, _TimePurchaseStoreID, CoolDownItemPurchaseHandler);
		}
		if (!flag)
		{
			mCoolDownPurchaseCallback(success: false);
			mCoolDownPurchaseCallback = null;
		}
	}

	private void CoolDownItemPurchaseHandler(CommonInventoryResponse ret)
	{
		if (ret != null && ret.Success)
		{
			mCoolDownPurchaseCallback(success: true);
		}
		else
		{
			mCoolDownPurchaseCallback(success: false);
		}
		mCoolDownPurchaseCallback = null;
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		mValidNotificationLevel = !_NotificationExcludeSceneList.Contains(RsResourceManager.pCurrentLevel);
		mInteractiveNotificationLevel = !_NonInteractiveNotificationSceneList.Contains(RsResourceManager.pCurrentLevel);
	}

	public string GetNextNotification()
	{
		if (mNotificationList.Count > 0)
		{
			return mNotificationList[0];
		}
		return null;
	}

	public void RemoveNotification()
	{
		mNotificationList.RemoveAt(0);
	}

	public void CheckQuestEnd(TimedMissionSlotData slot)
	{
		if (!mFetchingServerTime)
		{
			mFetchingServerTime = true;
			ServerTime.Init(inForceInit: true);
		}
		if (!mCompletedSlots.Contains(slot))
		{
			mCompletedSlots.Add(slot);
		}
	}

	public void CheckCoolDownEnd(TimedMissionSlotData slot)
	{
		if (!mFetchingServerTime)
		{
			mFetchingServerTime = true;
			ServerTime.Init(inForceInit: true);
		}
		if (!mCoolingSlots.Contains(slot))
		{
			mCoolingSlots.Add(slot);
		}
	}

	private void OnGetServerTime(WsServiceEvent inEvent)
	{
		if (inEvent == WsServiceEvent.COMPLETE && ServerTime.pServerTime.HasValue)
		{
			if (mCompletedSlots.Count > 0)
			{
				for (int num = mCompletedSlots.Count - 1; num >= 0; num--)
				{
					if (GetCompletionTime(mCompletedSlots[num]) <= TimeSpan.Zero)
					{
						ChangeState(mCompletedSlots[num], TimedMissionState.Ended);
					}
				}
			}
			if (mCoolingSlots.Count > 0)
			{
				for (int num2 = mCoolingSlots.Count - 1; num2 >= 0; num2--)
				{
					if (GetCoolDownTime(mCoolingSlots[num2]) <= TimeSpan.Zero)
					{
						ResetSlot(mCoolingSlots[num2]);
					}
				}
			}
			if (mPendingPurchase)
			{
				ProcessPendingPurchase();
			}
		}
		else if (inEvent == WsServiceEvent.ERROR && mPendingPurchase && mCompletionCallback != null)
		{
			mCompletionCallback(success: false, winStatus: false, null);
			mCompletionCallback = null;
		}
		mFetchingServerTime = false;
		mCompletedSlots.Clear();
		mCoolingSlots.Clear();
		mForceCompleteCost = 0;
		mPendingPurchase = false;
	}

	private void ProcessPendingPurchase()
	{
		if (mSlotForPurchase != null)
		{
			TimedMission currentMission = GetCurrentMission(mSlotForPurchase);
			if (currentMission != null)
			{
				int duration = (int)Mathf.Ceil((float)GetCompletionTime(mSlotForPurchase).TotalMinutes);
				int num = 0;
				num = ((currentMission.CostOfCompletionList != null && currentMission.CostOfCompletionList.CostItems != null && currentMission.CostOfCompletionList.CostItems.Count != 0) ? currentMission.CostOfCompletionList.GetCost(duration) : _CostOfCompletionList.GetCost(duration));
				if (num == mForceCompleteCost)
				{
					Dictionary<int, int> dictionary = new Dictionary<int, int>();
					dictionary = ((currentMission.CostOfCompletionList != null && currentMission.CostOfCompletionList.CostItems != null && currentMission.CostOfCompletionList.CostItems.Count != 0) ? currentMission.CostOfCompletionList.GetCostItems(duration) : _CostOfCompletionList.GetCostItems(duration));
					if (dictionary.Count > 0)
					{
						foreach (KeyValuePair<int, int> item in dictionary)
						{
							CommonInventoryData.pInstance.AddPurchaseItem(item.Key, item.Value, ItemPurchaseSource.SQUAD_TACTICS.ToString());
						}
						CommonInventoryData.pInstance.DoPurchase(2, _TimePurchaseStoreID, CostItemPurchaseHandler);
						return;
					}
				}
			}
		}
		if (mCompletionCallback != null)
		{
			mCompletionCallback(success: false, winStatus: false, null);
			mCompletionCallback = null;
		}
	}

	private void OnDestroy()
	{
		ServerTime.OnServerTimeReady = (Action<WsServiceEvent>)Delegate.Remove(ServerTime.OnServerTimeReady, new Action<WsServiceEvent>(OnGetServerTime));
	}
}
