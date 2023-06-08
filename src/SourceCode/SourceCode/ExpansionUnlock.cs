using System;
using SOD.Event;
using UnityEngine;

public class ExpansionUnlock : MonoBehaviour, IUnlock
{
	[Serializable]
	public class ExpansionInfo
	{
		[Serializable]
		public class Scene
		{
			public string _Scene;

			public string _Fallback;
		}

		[Tooltip("Internal name of expansion.")]
		public string _Name;

		public int[] _TicketID;

		public string[] _UpsellDB;

		public bool _AvailableForTrialMembership;

		public Scene[] _Scenes;

		public int[] _CompleteMissionId;

		public bool IsUnlocked()
		{
			if (SubscriptionInfo.pIsMember && (!SubscriptionInfo.pIsTrialMember || _AvailableForTrialMembership))
			{
				return true;
			}
			if (_TicketID != null && _TicketID.Length != 0)
			{
				for (int i = 0; i < _TicketID.Length; i++)
				{
					if (ParentData.pIsReady && ParentData.pInstance.HasItem(_TicketID[i]))
					{
						return true;
					}
					if (CommonInventoryData.pIsReady && CommonInventoryData.pInstance.FindItem(_TicketID[i]) != null)
					{
						return true;
					}
					if (IsExpansionCompleted())
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool IsExpansionCompleted()
		{
			if (_CompleteMissionId != null && _CompleteMissionId.Length != 0)
			{
				return Array.Find(_CompleteMissionId, (int missionid) => missionid > 0 && !MissionManager.IsMissionCompleted(missionid)) <= 0;
			}
			return false;
		}
	}

	[Serializable]
	public class UpsellDataMissionMap
	{
		public int _MissionID;

		public LocaleString _QuestPendingText;

		public LocaleString _QuestLockedText;

		public string GetUpsellConfirmationMsg()
		{
			Mission mission = MissionManager.pInstance.GetMission(_MissionID);
			if (mission != null)
			{
				if (MissionManager.pInstance.IsLocked(mission))
				{
					return _QuestLockedText.GetLocalizedString();
				}
				if (!mission.pStarted && !mission.pCompleted)
				{
					return _QuestPendingText.GetLocalizedString();
				}
			}
			return null;
		}
	}

	private const string UpsellDBKey = "LAST_UPSELL_DB";

	private static ExpansionUnlock mInstance;

	public ExpansionInfo[] _ExpansionInfo;

	public UpsellDataMissionMap[] _UpsellInfo;

	private AvAvatarState mPrevAvatarState;

	public static ExpansionUnlock pInstance => mInstance;

	private void Start()
	{
		if (mInstance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		mInstance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		AddUnlockInfo();
	}

	public void AddUnlockInfo()
	{
		UnlockManager.Add(this);
	}

	public void RemoveUnlockInfo()
	{
		UnlockManager.Remove(this);
	}

	public bool IsSceneUnlocked(string sceneName, bool inShowUi, UnlockManager.OnSceneUnlockedCallBack onSceneUnlocked)
	{
		ExpansionInfo inExpansionInfo = null;
		ExpansionInfo[] expansionInfo = GetExpansionInfo(sceneName);
		if (expansionInfo != null)
		{
			for (int i = 0; i < expansionInfo.Length; i++)
			{
				if (expansionInfo[i].IsUnlocked())
				{
					return true;
				}
				inExpansionInfo = expansionInfo[i];
			}
			if (EventManager.IsSceneUnlocked(sceneName))
			{
				return true;
			}
			if (!inShowUi)
			{
				ShowExpansionUpsell(inExpansionInfo, onSceneUnlocked);
			}
			return false;
		}
		return true;
	}

	public string IsSceneUnlocked(string sceneName)
	{
		string result = null;
		ExpansionInfo[] expansionInfo = GetExpansionInfo(sceneName);
		if (EventManager.IsSceneUnlocked(sceneName))
		{
			return null;
		}
		if (expansionInfo != null)
		{
			for (int i = 0; i < expansionInfo.Length; i++)
			{
				if (expansionInfo[i].IsUnlocked())
				{
					return null;
				}
				result = Array.Find(expansionInfo[i]._Scenes, (ExpansionInfo.Scene s) => s._Scene == sceneName)._Fallback;
			}
		}
		return result;
	}

	public bool IsUnlocked(int ticketID)
	{
		return Array.Find(_ExpansionInfo, delegate(ExpansionInfo obj)
		{
			for (int i = 0; i < obj._TicketID.Length; i++)
			{
				if (obj._TicketID[i] == ticketID)
				{
					return true;
				}
			}
			return false;
		})?.IsUnlocked() ?? false;
	}

	public bool IsExpansionCompleted(int ticketID)
	{
		return Array.Find(_ExpansionInfo, delegate(ExpansionInfo obj)
		{
			for (int i = 0; i < obj._TicketID.Length; i++)
			{
				if (obj._TicketID[i] == ticketID)
				{
					return true;
				}
			}
			return false;
		})?.IsExpansionCompleted() ?? false;
	}

	public ExpansionInfo[] GetExpansionInfo(string inSceneName)
	{
		ExpansionInfo[] array = Array.FindAll(_ExpansionInfo, (ExpansionInfo eInfo) => Array.Exists(eInfo._Scenes, (ExpansionInfo.Scene scene) => scene._Scene == inSceneName));
		if (array.Length == 0)
		{
			array = null;
		}
		return array;
	}

	public ExpansionInfo[] GetExpansionInfo(int inTicketID)
	{
		ExpansionInfo[] array = Array.FindAll(_ExpansionInfo, (ExpansionInfo eInfo) => Array.Exists(eInfo._TicketID, (int ticketID) => ticketID == inTicketID));
		if (array.Length == 0)
		{
			array = null;
		}
		return array;
	}

	public UpsellDataMissionMap GetUpsellInfo(int inMissionID)
	{
		return Array.Find(_UpsellInfo, (UpsellDataMissionMap eInfo) => eInfo._MissionID.Equals(inMissionID));
	}

	public void ShowExpansionUpsell(ExpansionInfo inExpansionInfo, UnlockManager.OnSceneUnlockedCallBack onSceneUnlocked)
	{
		if (inExpansionInfo._UpsellDB.Length != 0)
		{
			string key = "LAST_UPSELL_DB" + inExpansionInfo._Name;
			int @int = PlayerPrefs.GetInt(key, 0);
			if (++@int >= inExpansionInfo._UpsellDB.Length)
			{
				@int = 0;
			}
			PlayerPrefs.SetInt(key, @int);
			mPrevAvatarState = AvAvatar.pState;
			if (AvAvatar.pState != AvAvatarState.PAUSED)
			{
				AvAvatar.SetUIActive(inActive: false);
				AvAvatar.pState = AvAvatarState.PAUSED;
			}
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = inExpansionInfo._UpsellDB[@int].Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnUpsellScreenLoaded, typeof(GameObject), inDontDestroy: false, onSceneUnlocked);
		}
	}

	private void OnUpsellScreenLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			UiUpsellDB component = obj.GetComponent<UiUpsellDB>();
			obj.name = obj.name.Replace("(Clone)", "");
			component.SetSource(ItemPurchaseSource.EXPANSION_UPSELL.ToString());
			component.OnUpsellComplete = delegate(bool p)
			{
				UpsellCompleteCallback(p, (UnlockManager.OnSceneUnlockedCallBack)inUserData);
			};
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
		case RsResourceLoadEvent.ERROR:
			if (mPrevAvatarState != AvAvatarState.PAUSED)
			{
				AvAvatar.SetUIActive(inActive: true);
				AvAvatar.pState = AvAvatarState.IDLE;
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	protected virtual void UpsellCompleteCallback(bool purchased, UnlockManager.OnSceneUnlockedCallBack onSceneUnlocked)
	{
		if (mPrevAvatarState != AvAvatarState.PAUSED)
		{
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = AvAvatarState.IDLE;
		}
		onSceneUnlocked?.Invoke(purchased);
	}
}
