using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "Task", Namespace = "")]
public class Task
{
	public delegate void SetTaskFailed(Task task);

	[XmlElement(ElementName = "I")]
	public int TaskID;

	[XmlElement(ElementName = "N")]
	public string Name;

	[XmlElement(ElementName = "S")]
	public string Static;

	[XmlElement(ElementName = "C")]
	public int Completed;

	[XmlElement(ElementName = "F")]
	public bool Failed;

	[XmlElement(ElementName = "P")]
	public string Payload;

	[XmlIgnore]
	public Mission _Mission;

	[XmlIgnore]
	public bool _Active;

	[NonSerialized]
	private TaskStatic mData;

	[NonSerialized]
	private float mTaskTime;

	[NonSerialized]
	private float mTimeSpent;

	private Mission.SaveTask mCurrentSavedTask;

	public static SetTaskFailed OnSetTaskFailed;

	[NonSerialized]
	private TaskPayload mPayload;

	[NonSerialized]
	private bool? mIsTimedTask;

	[XmlIgnore]
	public TaskStatic pData
	{
		get
		{
			if (mData == null || !string.IsNullOrEmpty(Static))
			{
				if (string.IsNullOrEmpty(Static))
				{
					UtDebug.LogWarning("Task has no static data! " + TaskID + " :: Is Static Data ready " + _Mission.pStaticDataReady);
					mData = new TaskStatic();
				}
				else
				{
					mData = UtUtilities.DeserializeFromXml(Static, typeof(TaskStatic)) as TaskStatic;
					Static = null;
				}
			}
			return mData;
		}
	}

	[XmlIgnore]
	public TaskPayload pPayload
	{
		get
		{
			if (mPayload == null)
			{
				if (!string.IsNullOrEmpty(Payload))
				{
					mPayload = UtUtilities.DeserializeFromXml(Payload, typeof(TaskPayload)) as TaskPayload;
				}
				else
				{
					mPayload = new TaskPayload();
				}
			}
			return mPayload;
		}
	}

	public bool pStarted => pPayload.Started;

	public bool pIsReady
	{
		get
		{
			if (!_Mission.pIsReady)
			{
				return false;
			}
			List<TaskSetup> setups = GetSetups();
			if (setups != null)
			{
				foreach (TaskSetup item in setups)
				{
					if (!item.pIsReady)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	[XmlIgnore]
	public float pRemainingTime
	{
		get
		{
			float num = mTaskTime - mTimeSpent;
			if (num < 0f)
			{
				num = 0f;
			}
			return num;
		}
	}

	[XmlIgnore]
	public bool pIsTimedTask
	{
		get
		{
			if (!mIsTimedTask.HasValue)
			{
				string objectiveValue = GetObjectiveValue<string>("StartObject");
				mIsTimedTask = !string.IsNullOrEmpty(objectiveValue);
			}
			return mIsTimedTask.Value;
		}
	}

	public bool pCompleted
	{
		get
		{
			if (Completed > 0)
			{
				return true;
			}
			for (Mission mission = _Mission; mission != null; mission = mission._Parent)
			{
				if (mission.Completed > 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool pActivateOnClick
	{
		get
		{
			if (!(pData.Type == "Escort") && !(pData.Type == "Follow") && !(pData.Type == "Chase"))
			{
				return pIsTimedTask;
			}
			return true;
		}
	}

	public List<TaskSetup> GetSetups()
	{
		List<TaskSetup> list = null;
		if (pData != null)
		{
			list = new List<TaskSetup>();
			if (pData.Setups != null)
			{
				list.AddRange(pData.Setups);
			}
			if (pData.RandomSetups != null)
			{
				list.Add(pData.RandomSetups);
			}
		}
		return list;
	}

	public void Start()
	{
		UtDebug.Log("Task " + TaskID + " started.  Type is " + pData.Type, Mission.LOG_MASK);
		if (!pPayload.Started)
		{
			pPayload.Started = true;
			Save();
		}
		if (pData.Type == "Collect")
		{
			foreach (TaskObjective objective in pData.Objectives)
			{
				string text = objective.Get<string>("Name");
				objective._Collected = pPayload.Get<int>(text + "CollectedCount");
			}
		}
		else if (pData.Type == "Delivery")
		{
			bool flag = false;
			foreach (TaskObjective objective2 in pData.Objectives)
			{
				if (objective2.Get<bool>("Free"))
				{
					int itemID = objective2.Get<int>("ItemID");
					int num = objective2.Get<int>("Quantity") - CommonInventoryData.pInstance.GetQuantity(itemID);
					if (num > 0)
					{
						flag = true;
						CommonInventoryData.pInstance.AddItem(itemID, num);
					}
				}
			}
			if (flag)
			{
				CommonInventoryData.pInstance.Save();
			}
		}
		if (pData.Objectives != null)
		{
			mTaskTime = GetObjectiveValue<float>("Time");
		}
		_Active = !pActivateOnClick;
		Setup();
		mTimeSpent = 0f;
	}

	public void Save()
	{
		Save(completed: false, null);
	}

	public void Save(bool completed, MissionCompleteEventHandler callback)
	{
		Mission.SaveTask saveTask = new Mission.SaveTask(this, completed, callback);
		_Mission.pTasksSaving.Add(saveTask);
		if (_Mission.pTasksSaving.Count == 1)
		{
			SetTaskState(saveTask);
		}
	}

	private void SetTaskState(Mission.SaveTask saveTask)
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		string payload = "";
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using StreamWriter textWriter = new StreamWriter(memoryStream, uTF8Encoding);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(TaskPayload));
			XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
			xmlSerializerNamespaces.Add("", "");
			xmlSerializer.Serialize(textWriter, pPayload, xmlSerializerNamespaces);
			payload = uTF8Encoding.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length);
		}
		CommonInventoryRequest[] inventoryRequest = null;
		if (pData.Type == "Delivery" && saveTask._Completed)
		{
			List<CommonInventoryRequest> list = new List<CommonInventoryRequest>();
			foreach (TaskObjective objective in pData.Objectives)
			{
				CommonInventoryRequest commonInventoryRequest = new CommonInventoryRequest();
				commonInventoryRequest.ItemID = objective.Get<int>("ItemID");
				commonInventoryRequest.Quantity = -objective.Get<int>("Quantity");
				list.Add(commonInventoryRequest);
			}
			inventoryRequest = list.ToArray();
		}
		if (Mission.pFail)
		{
			UtBehaviour.RunCoroutine(SimulateSetTaskStateFailed(saveTask));
		}
		else if (Mission.pSave)
		{
			if (_Mission.pTimedMission)
			{
				WsWebService.SetTimedMissionTaskState(UserInfo.pInstance.UserID, _Mission.MissionID, TaskID, saveTask._Completed, payload, 1, inventoryRequest, SetTaskStateEventHandler, saveTask);
			}
			else
			{
				WsWebService.SetTaskState(UserInfo.pInstance.UserID, _Mission.MissionID, TaskID, saveTask._Completed, payload, 1, inventoryRequest, SetTaskStateEventHandler, saveTask);
			}
		}
		else
		{
			UtBehaviour.RunCoroutine(SimulateSetTaskState(saveTask));
		}
	}

	private IEnumerator SimulateSetTaskState(Mission.SaveTask saveTask)
	{
		yield return new WaitForEndOfFrame();
		SetTaskStateResult setTaskStateResult = new SetTaskStateResult();
		setTaskStateResult.Success = true;
		setTaskStateResult.CommonInvRes = new CommonInventoryResponse();
		setTaskStateResult.CommonInvRes.Success = true;
		SetTaskStateEventHandler(WsServiceType.SET_TASK_STATE, WsServiceEvent.COMPLETE, 1f, setTaskStateResult, saveTask);
	}

	private IEnumerator SimulateSetTaskStateFailed(Mission.SaveTask saveTask)
	{
		yield return new WaitForEndOfFrame();
		SetTaskStateResult setTaskStateResult = new SetTaskStateResult();
		setTaskStateResult.Success = false;
		SetTaskStateEventHandler(WsServiceType.SET_TASK_STATE, WsServiceEvent.COMPLETE, 1f, setTaskStateResult, saveTask);
	}

	private void SetTaskStateEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != WsServiceEvent.COMPLETE && inEvent != WsServiceEvent.ERROR)
		{
			return;
		}
		SetTaskStateResult setTaskStateResult = ((inType != WsServiceType.SET_TIMED_MISSION_TASK_STATE) ? ((SetTaskStateResult)inObject) : ((SetTimedMissionTaskStateResult)inObject));
		Mission.SaveTask saveTask = (Mission.SaveTask)inUserData;
		if (Mission.pSyncDB && ((inEvent == WsServiceEvent.ERROR && GameDataConfig.pInstance.ShowTaskSyncDBOnError) || (inEvent == WsServiceEvent.COMPLETE && GameDataConfig.pInstance.ShowTaskSyncDBOnFail && (setTaskStateResult == null || !setTaskStateResult.Success))))
		{
			mCurrentSavedTask = saveTask;
			OnSetTaskFailed(this);
			return;
		}
		if (saveTask._Completed && setTaskStateResult != null && setTaskStateResult.Success)
		{
			if (pData.Type == "Delivery" && setTaskStateResult.CommonInvRes != null && setTaskStateResult.CommonInvRes.Success)
			{
				foreach (TaskObjective objective in pData.Objectives)
				{
					int itemID = objective.Get<int>("ItemID");
					int inQuantity = objective.Get<int>("Quantity");
					CommonInventoryData.pInstance.RemoveItem(itemID, updateServer: false, inQuantity);
				}
			}
			if (pData.RemoveItem != null)
			{
				foreach (MissionRemoveItem item in pData.RemoveItem)
				{
					CommonInventoryData.pInstance.RemoveItem(item.ItemID, updateServer: false, item.Quantity);
				}
			}
			CommonInventoryData.pInstance.ClearSaveCache();
		}
		if (saveTask._Callback != null)
		{
			saveTask._Callback(setTaskStateResult);
		}
		if (_Mission.pTasksSaving.Count > 0 && _Mission.pTasksSaving[0]._Task == this)
		{
			_Mission.pTasksSaving.RemoveAt(0);
			if (_Mission.pTasksSaving.Count > 0)
			{
				_Mission.pTasksSaving[0]._Task.SetTaskState(_Mission.pTasksSaving[0]);
			}
		}
	}

	public void SaveTaskState()
	{
		SetTaskState(mCurrentSavedTask);
	}

	public List<MissionAction> GetOffers(bool unplayed)
	{
		List<MissionAction> list = _Mission.GetOffers(unplayed);
		if (pData.Offers != null)
		{
			List<MissionAction> list2 = pData.Offers.FindAll((MissionAction offer) => offer.Type != MissionActionType.Rebus && (!unplayed || !offer._Played));
			if (list2 != null)
			{
				List<MissionAction> list3 = list2.OrderBy((MissionAction offer) => offer.Priority).ToList();
				if (list != null)
				{
					list.AddRange(list3);
				}
				else
				{
					list = list3;
				}
			}
		}
		return list;
	}

	public MissionAction GetOffer(MissionActionType type, string npcName, bool unplayed)
	{
		MissionAction offer = _Mission.GetOffer(type, npcName, unplayed);
		if (offer != null)
		{
			return offer;
		}
		if (pData.Offers != null)
		{
			foreach (MissionAction offer2 in pData.Offers)
			{
				if (offer2.Type == type && (string.IsNullOrEmpty(npcName) || (!string.IsNullOrEmpty(offer2.NPC) && offer2.NPC.StartsWith(npcName))) && (!unplayed || !offer2._Played))
				{
					return offer2;
				}
			}
		}
		return null;
	}

	public List<MissionAction> GetEnds()
	{
		if (pData.Ends != null)
		{
			return pData.Ends.OrderBy((MissionAction end) => end.Priority).ToList();
		}
		return null;
	}

	public void Setup()
	{
		if (!_Mission.pIsReady)
		{
			Mission mission = _Mission;
			mission.OnSetupComplete = (Mission.SetupComplete)Delegate.Combine(mission.OnSetupComplete, new Mission.SetupComplete(OnSetupComplete));
			_Mission.Setup();
			return;
		}
		List<TaskSetup> setups = GetSetups();
		if (setups == null)
		{
			return;
		}
		foreach (TaskSetup item in setups)
		{
			item.Setup(this);
		}
	}

	public void FinishSetup()
	{
		if (!pIsReady)
		{
			return;
		}
		List<TaskSetup> setups = GetSetups();
		if (setups == null)
		{
			return;
		}
		foreach (TaskSetup item in setups)
		{
			item.FinishSetup();
		}
	}

	public void OnSetupComplete()
	{
		Mission mission = _Mission;
		mission.OnSetupComplete = (Mission.SetupComplete)Delegate.Remove(mission.OnSetupComplete, new Mission.SetupComplete(OnSetupComplete));
		Setup();
	}

	public void ObjectActivated(string inObjectName)
	{
		if (pIsReady)
		{
			return;
		}
		_Mission.ObjectActivated(inObjectName);
		List<TaskSetup> setups = GetSetups();
		if (setups == null)
		{
			return;
		}
		foreach (TaskSetup item in setups)
		{
			if (item.pObject == null && !string.IsNullOrEmpty(item.Asset) && !item.Asset.Contains('/') && inObjectName == item.Asset)
			{
				item.Setup(this);
			}
		}
	}

	public void OnLevelLoaded()
	{
		_Mission.OnLevelLoaded();
		List<TaskSetup> setups = GetSetups();
		if (setups != null)
		{
			foreach (TaskSetup item in setups)
			{
				item.pIsReady = false;
			}
		}
		if (_Active && pData != null && (pData.Type == "Escort" || pData.Type == "Chase" || pData.Type == "Follow"))
		{
			Failed = true;
		}
	}

	public bool IsTaskObject(GameObject inClickedObject)
	{
		string objectiveValue = GetObjectiveValue<string>("StartObject");
		if (!string.IsNullOrEmpty(objectiveValue) && objectiveValue == inClickedObject.name)
		{
			return true;
		}
		string objectiveValue2 = GetObjectiveValue<string>("NPC");
		if (!string.IsNullOrEmpty(objectiveValue2) && objectiveValue2 == inClickedObject.name)
		{
			return true;
		}
		return false;
	}

	public void Update(float deltaTime)
	{
		if (!_Active || Completed > 0 || Failed)
		{
			return;
		}
		mTimeSpent += deltaTime;
		if (pIsTimedTask && mTimeSpent > mTaskTime)
		{
			Failed = true;
		}
		else if (pData.Type == "Visit" || pData.Type == "Chase" || pData.Type == "Follow" || pData.Type == "Escort")
		{
			int i = 0;
			for (int count = pData.Objectives.Count; i < count; i++)
			{
				TaskObjective taskObjective = pData.Objectives[i];
				if (!RsResourceManager.pCurrentLevel.Equals(taskObjective.Get<string>("Scene"), StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				bool flag = true;
				if (taskObjective._Object == null)
				{
					taskObjective._Object = GameObject.Find(taskObjective.Get<string>("Name"));
				}
				if (taskObjective._Object != null)
				{
					float num = taskObjective.Get<float>("Range");
					float magnitude = (AvAvatar.position - taskObjective._Object.transform.position).magnitude;
					flag = !(num > 0f) || magnitude < num;
				}
				else if (pData.Type == "Chase")
				{
					flag = false;
				}
				bool flag2 = AvAvatar.pToolbar != null && AvAvatar.pToolbar.activeInHierarchy;
				if (flag && flag2)
				{
					bool flag3 = true;
					if (pData.Type == "Escort" || pData.Type == "Follow")
					{
						flag3 = taskObjective._WithinProximity;
					}
					if (flag3)
					{
						UtDebug.Log("Task " + TaskID + " marked for completion by Update.", Mission.LOG_MASK);
						Completed++;
						return;
					}
				}
			}
		}
		float objectiveValue = GetObjectiveValue<float>("GraceTime");
		if (pData.Type == "Escort" || pData.Type == "Follow" || (pData.Type == "Chase" && mTimeSpent > objectiveValue))
		{
			Failed = CheckRange(deltaTime);
		}
		if (!Failed && mTaskTime > 0f && mTimeSpent > mTaskTime)
		{
			if (pData.Type != "Chase")
			{
				Failed = true;
			}
			else
			{
				Completed++;
			}
		}
	}

	public bool CheckRange(float deltaTime)
	{
		foreach (TaskObjective objective in pData.Objectives)
		{
			if (objective._NPC == null)
			{
				string objectiveValue = GetObjectiveValue<string>("NPC");
				if (!string.IsNullOrEmpty(objectiveValue))
				{
					objective._NPC = GameObject.Find(objectiveValue);
				}
			}
			if (!(objective._NPC != null))
			{
				continue;
			}
			float num = objective.Get<float>("Proximity");
			float num2 = objective.Get<float>("GraceTime");
			float magnitude = (AvAvatar.position - objective._NPC.transform.position).magnitude;
			bool flag = !(num > 0f) || magnitude < num;
			if (pData.Type == "Chase")
			{
				return flag;
			}
			if (!flag)
			{
				if (objective._WithinProximity)
				{
					objective._ProximityTimer = 0f;
				}
				else
				{
					objective._ProximityTimer += deltaTime;
				}
			}
			objective._WithinProximity = flag;
			int num3;
			if (!objective._WithinProximity)
			{
				num3 = ((objective._ProximityTimer > num2) ? 1 : 0);
				if (num3 != 0)
				{
					objective._ProximityTimer = 0f;
				}
			}
			else
			{
				num3 = 0;
			}
			return (byte)num3 != 0;
		}
		return false;
	}

	public bool Collect(GameObject gameObject)
	{
		bool result = false;
		if (pData.Type == "Collect")
		{
			int i = 0;
			for (int count = pData.Objectives.Count; i < count; i++)
			{
				TaskObjective taskObjective = pData.Objectives[i];
				string text = taskObjective.Get<string>("Name");
				if (!gameObject.name.StartsWith(text))
				{
					continue;
				}
				taskObjective._Collected++;
				result = true;
				int num = taskObjective.Get<int>("Quantity");
				if (num > 1 && gameObject.transform.parent != null)
				{
					string text2 = pPayload.Get<string>(text + "Collected");
					string text3 = gameObject.name.Substring(text.Length);
					pPayload.Set(text + "Collected", text2 + (string.IsNullOrEmpty(text2) ? text3 : ("|" + text3)));
				}
				if (num > 1)
				{
					pPayload.Set(text + "CollectedCount", taskObjective._Collected.ToString());
				}
				if (!CheckForCompletion("Collect", text, taskObjective._Collected, "") && !pIsTimedTask)
				{
					Save();
				}
				List<TaskSetup> setups = GetSetups();
				if (setups == null)
				{
					continue;
				}
				int j = 0;
				for (int count2 = setups.Count; j < count2; j++)
				{
					TaskSetup taskSetup = setups[j];
					if (taskSetup.pObject != null)
					{
						taskSetup.pObject.SendMessage("SetCollected", taskObjective, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}
		return result;
	}

	public bool Collect(string collectedObjectName)
	{
		bool result = false;
		if (pData.Type == "Collect")
		{
			int i = 0;
			for (int count = pData.Objectives.Count; i < count; i++)
			{
				TaskObjective taskObjective = pData.Objectives[i];
				string text = taskObjective.Get<string>("Name");
				if (!collectedObjectName.StartsWith(text))
				{
					continue;
				}
				taskObjective._Collected++;
				result = true;
				if (taskObjective.Get<int>("Quantity") > 1)
				{
					pPayload.Set(text + "CollectedCount", taskObjective._Collected.ToString());
				}
				if (!CheckForCompletion("Collect", text, taskObjective._Collected, "") && !pIsTimedTask)
				{
					Save();
				}
				List<TaskSetup> setups = GetSetups();
				if (setups == null)
				{
					continue;
				}
				int j = 0;
				for (int count2 = setups.Count; j < count2; j++)
				{
					TaskSetup taskSetup = setups[j];
					if (taskSetup.pObject != null)
					{
						taskSetup.pObject.SendMessage("SetCollected", taskObjective, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}
		return result;
	}

	public void Fail()
	{
		_Active = false;
		Reset();
		CleanUp();
	}

	public void Complete(MissionCompleteEventHandler callback)
	{
		_Active = false;
		Save(completed: true, callback);
	}

	public void CleanUp()
	{
		List<TaskSetup> setups = GetSetups();
		if (setups == null)
		{
			return;
		}
		foreach (TaskSetup item in setups)
		{
			item.CleanUp();
		}
	}

	public void Reset()
	{
		Completed = 0;
		mTimeSpent = 0f;
		Failed = false;
		Payload = null;
		mPayload = null;
		if (!(pData.Type == "Collect"))
		{
			return;
		}
		foreach (TaskObjective objective in pData.Objectives)
		{
			objective._Collected = 0;
		}
	}

	public bool CheckForCompletion(string taskType, object value1, object value2, object value3)
	{
		UtDebug.Log("CheckForTaskCompletion task " + TaskID + ": " + taskType + ", value1: " + value1.ToString() + ", value2: " + value2.ToString() + ", value3: " + value3.ToString(), Mission.LOG_MASK);
		if (Completed > 0 || !_Active || pData.Type != taskType)
		{
			return false;
		}
		if (pIsTimedTask && mTimeSpent > mTaskTime)
		{
			Failed = true;
			return false;
		}
		foreach (TaskObjective objective in pData.Objectives)
		{
			object obj = value1;
			object obj2 = value2;
			object obj3 = value3;
			if (pData.Type == "Action")
			{
				obj = objective.Get<string>("Name");
				string value4 = objective.Get<string>("ItemName");
				if (!string.IsNullOrEmpty(value4) && !value2.ToString().Contains(value4))
				{
					obj2 = "Fail";
				}
			}
			else if (pData.Type == "Build")
			{
				ItemData itemData = null;
				if (value1 != null && value1.GetType().Equals(typeof(ItemData)))
				{
					itemData = (ItemData)value1;
				}
				string text = objective.Get<string>("ItemName");
				int num = objective.Get<int>("CategoryID");
				if (!string.IsNullOrEmpty(text))
				{
					obj = text;
					if (itemData != null)
					{
						value1 = itemData.AssetName;
					}
				}
				else if (num > 0)
				{
					obj = num;
					if (itemData != null)
					{
						ItemDataCategory[] category = itemData.Category;
						for (int i = 0; i < category.Length; i++)
						{
							if (category[i].CategoryId == num)
							{
								value1 = obj;
								break;
							}
						}
					}
				}
				obj2 = objective.Get<string>("Name");
			}
			else if (pData.Type == "Buy")
			{
				ItemData itemData2 = (ItemData)value2;
				int num2 = objective.Get<int>("StoreID");
				if (num2 == 0 || value1.Equals(num2))
				{
					int num3 = objective.Get<int>("ItemID");
					int num4 = objective.Get<int>("CategoryID");
					if (num3 > 0)
					{
						if (itemData2.ItemID == num3)
						{
							if (objective.Get<int>("Quantity") > 1)
							{
								UtDebug.LogError("Quantity > 1 not supported on buy tasks!");
							}
						}
						else
						{
							obj = "";
						}
					}
					else if (num4 > 0)
					{
						obj = "";
						ItemDataCategory[] category = itemData2.Category;
						for (int i = 0; i < category.Length; i++)
						{
							if (category[i].CategoryId == num4)
							{
								obj = value1;
								break;
							}
						}
					}
				}
				else
				{
					obj = "";
				}
			}
			else if (pData.Type == "Collect")
			{
				string text2 = objective.Get<string>("Name");
				if (!string.IsNullOrEmpty(text2))
				{
					obj = text2;
				}
				else
				{
					UtDebug.LogError("No Objective item for collect task!");
				}
				obj2 = objective.Get<int>("Quantity");
				if ((int)value2 > (int)obj2)
				{
					value2 = obj2;
				}
			}
			if (pData.Type == "Delivery")
			{
				string value5 = value1.ToString();
				if (objective.Get<string>("NPC").Contains(value5))
				{
					int num5 = objective.Get<int>("ItemID");
					if (num5 > 0)
					{
						if (CommonInventoryData.pInstance.GetQuantity(num5) < objective.Get<int>("Quantity"))
						{
							obj = "";
						}
					}
					else
					{
						obj = "";
					}
				}
				else
				{
					obj = "";
				}
			}
			else if (pData.Type == "Game")
			{
				obj = objective.Get<string>("Name");
				string text3 = objective.Get<string>("Level");
				if (!string.IsNullOrEmpty(text3))
				{
					obj2 = text3;
				}
			}
			else if (pData.Type == "Meet")
			{
				string value6 = value1.ToString();
				if (!objective.Get<string>("NPC").Contains(value6))
				{
					obj = "";
				}
			}
			else if (pData.Type == "Puzzle")
			{
				obj = objective.Get<string>("Type");
			}
			else if (pData.Type == "Visit")
			{
				if (value1.Equals(objective.Get<string>("Scene")))
				{
					if (!string.IsNullOrEmpty(objective.Get<string>("Name")))
					{
						obj2 = "";
					}
				}
				else
				{
					obj = 0;
				}
			}
			else if (pData.Type == "Chase")
			{
				string text4 = objective.Get<string>("Name");
				if (!string.IsNullOrEmpty(text4))
				{
					obj = text4;
				}
				obj2 = objective.Get<int>("Quantity");
				if ((int)value2 > (int)obj2)
				{
					value2 = obj2;
				}
				if (objective._WithinProximity)
				{
					obj3 = "Fail";
				}
			}
			UtDebug.Log("goal1: " + obj.ToString() + ", goal2: " + obj2.ToString() + ", goal3: " + obj3.ToString(), Mission.LOG_MASK);
			if (!value1.Equals(obj) || !value2.Equals(obj2) || !value3.Equals(obj3))
			{
				return false;
			}
		}
		UtDebug.Log("Task " + TaskID + " marked for completion by CheckForCompletion.", Mission.LOG_MASK);
		Completed++;
		return true;
	}

	public bool Match(string taskType, string key, string value)
	{
		if (pData.Type == taskType)
		{
			if (key == null || value == null)
			{
				return true;
			}
			int i = 0;
			for (int count = pData.Objectives.Count; i < count; i++)
			{
				string text = pData.Objectives[i].Get<string>(key);
				if (text == null)
				{
					continue;
				}
				if (key == "NPC")
				{
					if (text.Contains(value))
					{
						return true;
					}
				}
				else if (text.Equals(value))
				{
					return true;
				}
			}
		}
		return false;
	}

	public TYPE GetObjectiveValue<TYPE>(string inKey)
	{
		if (pData.Objectives != null)
		{
			foreach (TaskObjective objective in pData.Objectives)
			{
				int i = 0;
				for (int count = objective.Pairs.Count; i < count; i++)
				{
					if (objective.Pairs[i].Key == inKey)
					{
						return UtStringUtil.Parse(objective.Pairs[i].Value, default(TYPE));
					}
				}
			}
		}
		return default(TYPE);
	}
}
