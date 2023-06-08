using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

[XmlRoot(ElementName = "Mission", Namespace = "")]
public class Mission
{
	public delegate void SetupComplete();

	public class SaveTask
	{
		[XmlIgnore]
		public Task _Task;

		[XmlIgnore]
		public bool _Completed;

		[XmlIgnore]
		public MissionCompleteEventHandler _Callback;

		public SaveTask(Task task, bool completed, MissionCompleteEventHandler callback)
		{
			_Task = task;
			_Completed = completed;
			_Callback = callback;
		}
	}

	[XmlElement(ElementName = "I")]
	public int MissionID;

	[XmlElement(ElementName = "N")]
	public string Name;

	[XmlElement(ElementName = "G")]
	public int GroupID;

	[XmlElement(ElementName = "P", IsNullable = true)]
	public int? ParentID;

	[XmlElement(ElementName = "S")]
	public string Static;

	[XmlElement(ElementName = "A")]
	public bool Accepted;

	[XmlElement(ElementName = "C")]
	public int Completed;

	[XmlElement(ElementName = "R")]
	public string Rule;

	[XmlElement(ElementName = "MR")]
	public MissionRule MissionRule;

	[XmlElement(ElementName = "V")]
	public int VersionID;

	[XmlElement(ElementName = "AID")]
	public int AchievementID;

	[XmlElement(ElementName = "AAID")]
	public int AcceptanceAchievementID;

	[XmlElement(ElementName = "M")]
	public List<Mission> Missions;

	[XmlElement(ElementName = "Task")]
	public List<Task> Tasks;

	[XmlElement(ElementName = "AR")]
	public List<AchievementReward> Rewards;

	[XmlElement(ElementName = "AAR")]
	public List<AchievementReward> AcceptanceRewards;

	[XmlElement(ElementName = "RPT")]
	public bool Repeatable;

	public static uint LOG_MASK = 1u;

	public static bool pLocked = true;

	public static bool pSave = true;

	public static bool pFail = false;

	public static bool pSyncDB = true;

	[XmlIgnore]
	public SetupComplete OnSetupComplete;

	[XmlIgnore]
	public bool pStaticDataReady;

	[XmlIgnore]
	public bool pTimedMission;

	[XmlIgnore]
	public Mission _Parent;

	[NonSerialized]
	private MissionStatic mData;

	[XmlIgnore]
	public List<SaveTask> pTasksSaving = new List<SaveTask>();

	public bool pMustAccept
	{
		get
		{
			bool flag = false;
			if (_Parent != null)
			{
				flag = _Parent.pMustAccept && !_Parent.Accepted;
			}
			if (!flag)
			{
				if (MissionRule.GetPrerequisite<bool>(PrerequisiteRequiredType.Accept))
				{
					return !Accepted;
				}
				return false;
			}
			return true;
		}
	}

	public bool pMemberOnly => MissionRule.GetPrerequisite<bool>(PrerequisiteRequiredType.Member);

	public bool pIsReady
	{
		get
		{
			if (_Parent != null && !_Parent.pIsReady)
			{
				return false;
			}
			if (pData.Setups != null)
			{
				foreach (MissionSetup setup in pData.Setups)
				{
					if (!setup.pIsReady)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool pStarted
	{
		get
		{
			if (pMustAccept)
			{
				return false;
			}
			foreach (Task task in Tasks)
			{
				if (task.pStarted)
				{
					return true;
				}
			}
			foreach (Mission mission in Missions)
			{
				if (mission.pStarted)
				{
					return true;
				}
			}
			return false;
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
			for (Mission parent = _Parent; parent != null; parent = parent._Parent)
			{
				if (parent.Completed > 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	[XmlIgnore]
	public MissionStatic pData
	{
		get
		{
			if (mData == null || !string.IsNullOrEmpty(Static))
			{
				if (!string.IsNullOrEmpty(Static))
				{
					mData = UtUtilities.DeserializeFromXml(Static, typeof(MissionStatic)) as MissionStatic;
					Static = null;
				}
				else
				{
					UtDebug.LogWarning("Mission has no static data! " + MissionID + " :: Is Static Data ready " + pStaticDataReady);
					mData = new MissionStatic();
				}
			}
			return mData;
		}
	}

	[XmlIgnore]
	public string pStatic
	{
		set
		{
			Static = value;
			pStaticDataReady = true;
		}
	}

	public void Setup()
	{
		if (_Parent != null && !_Parent.pIsReady)
		{
			Mission parent = _Parent;
			parent.OnSetupComplete = (SetupComplete)Delegate.Combine(parent.OnSetupComplete, new SetupComplete(OnMissionSetupComplete));
			_Parent.Setup();
			return;
		}
		if (pData.Setups != null)
		{
			foreach (MissionSetup setup in pData.Setups)
			{
				setup.Setup(this);
			}
		}
		OnDownloadDone();
	}

	public void ObjectActivated(string inObjectName)
	{
		if (_Parent != null)
		{
			_Parent.ObjectActivated(inObjectName);
		}
		if (pData.Setups == null)
		{
			return;
		}
		foreach (MissionSetup setup in pData.Setups)
		{
			if (setup.pObject == null && !string.IsNullOrEmpty(setup.Asset) && !setup.Asset.Contains('/') && inObjectName == setup.Asset)
			{
				setup.Setup(this);
			}
		}
	}

	public void OnDownloadDone()
	{
		if (pIsReady && OnSetupComplete != null)
		{
			OnSetupComplete();
		}
	}

	public void OnMissionSetupComplete()
	{
		Mission parent = _Parent;
		parent.OnSetupComplete = (SetupComplete)Delegate.Remove(parent.OnSetupComplete, new SetupComplete(OnMissionSetupComplete));
		Setup();
	}

	public void OnLevelLoaded()
	{
		if (_Parent != null)
		{
			_Parent.OnLevelLoaded();
		}
		if (pData.Setups == null)
		{
			return;
		}
		foreach (MissionSetup setup in pData.Setups)
		{
			setup.OnLevelLoaded();
		}
	}

	public List<MissionAction> GetOffers(bool unplayed)
	{
		List<MissionAction> list = null;
		if (_Parent != null)
		{
			list = _Parent.GetOffers(unplayed);
		}
		if (pData.Offers != null)
		{
			List<MissionAction> list2 = (from offer in pData.Offers.FindAll((MissionAction offer) => offer.Type != MissionActionType.Rebus && (!unplayed || !offer._Played))
				orderby offer.Priority
				select offer).ToList();
			if (list != null)
			{
				list.AddRange(list2);
			}
			else
			{
				list = list2;
			}
		}
		return list;
	}

	public MissionAction GetOffer(MissionActionType type, string npcName, bool unplayed)
	{
		if (pData.Offers != null)
		{
			foreach (MissionAction offer in pData.Offers)
			{
				if (offer.Type == type && (string.IsNullOrEmpty(npcName) || (!string.IsNullOrEmpty(offer.NPC) && offer.NPC.StartsWith(npcName))) && (!unplayed || !offer._Played))
				{
					return offer;
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

	public Mission GetMission(int missionID)
	{
		return Missions.Find((Mission m) => m.MissionID == missionID);
	}

	public Task GetTask(int taskID)
	{
		return Tasks.Find((Task t) => t.TaskID == taskID);
	}

	public void CleanUp()
	{
		if (pData.Setups == null)
		{
			return;
		}
		foreach (MissionSetup setup in pData.Setups)
		{
			setup.CleanUp();
		}
	}

	public void Reset()
	{
		Accepted = false;
		MissionRule.Reset();
		if (_Parent != null)
		{
			Completed = 0;
		}
		foreach (Mission mission in Missions)
		{
			mission.Reset();
		}
		foreach (Task task in Tasks)
		{
			task.Reset();
		}
	}
}
