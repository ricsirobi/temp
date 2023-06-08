using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoCommonLevel : MonoBehaviour
{
	public static List<string> _ActivateObjectsOnLoad = new List<string>();

	public static bool _MembershipVerified = false;

	public AvAvatarState _AvatarStartState = AvAvatarState.IDLE;

	public AvAvatarSubState _AvatarStartSubState;

	public Transform[] _AvatarStartMarker;

	[Tooltip("Start Markers for non-members.")]
	public Transform[] _AvatarStartMarkerNM;

	[Tooltip("Start Markers in case of invalid start state (i.e. flying with a flightless dragon).")]
	public Transform[] _AvatarStartMarkerFallback;

	[Tooltip("Start Markers for non-members in case of invalid start state (i.e. flying with a flightless dragon).")]
	public Transform[] _AvatarStartMarkerFallbackNM;

	public TaskSpawnPoint[] _AvatarTaskSpawnPoints;

	public NPCStartData[] _NPCStartData;

	public GameObject[] _CheckReadyStatusList;

	public GameObject[] _ObjectNotifyList;

	public bool _ForceProjectionShadow;

	[Tooltip("This will load the player at the start of the level if they load from another scene - prevents certain exploits on low memory devices")]
	public bool _IgnorePreviousPositionOnLoad;

	private bool mInitialized;

	public bool _RideAllowed = true;

	public bool _PetAllowed = true;

	public bool _SaveSceneWhenVisited = true;

	public GameObject[] _WaitList;

	public GameObject[] _WaitListCompletedNotifyList;

	public AchievementOnVisit _VisitAchievementData;

	public PoolInfo[] _TurnOffPoolInfo;

	private bool mRecalcProgress;

	private int mProgressCompleteWaitFrames = 5;

	private UtProgress mProgress = new UtProgress();

	private int mWaitIndex;

	private bool mUseFallbackMarker;

	public bool pInitialized => mInitialized;

	public bool pWaitListCompleted
	{
		get
		{
			if (_WaitList.Length != 0)
			{
				return mWaitIndex >= _WaitList.Length;
			}
			return true;
		}
	}

	public static event OnWaitListCompleted WaitListCompleted;

	public virtual void Awake()
	{
		EnEnemy._Target = null;
		ProductConfig.Init();
		ProductData.Init();
		AvAvatar.Init();
		Money.Init();
		CommonInventoryData.Init();
		ServerTime.Init();
		LocaleData.Init();
		GameDataConfig.Init();
		MainStreetMMOClient.Init();
		InitProgress();
		mWaitIndex = 0;
	}

	public virtual void InitProgress()
	{
		mProgress.AddTask("ProductConfig");
		mProgress.AddTask("ProductData");
		mProgress.AddTask("MissionManager");
		mProgress.AddTask("AvatarData");
		mProgress.AddTask("UserInfo");
		mProgress.AddTask("Money");
		mProgress.AddTask("CommonInventoryData");
		mProgress.AddTask("ServerTime");
		mProgress.AddTask("WsUserMessage");
		mProgress.AddTask("LocaleData");
		mProgress.AddTask("GameDataConfig");
		mProgress.AddTask("SanctuaryManager");
		mProgress.AddTask("MainStreetMMOClient");
		GameObject[] checkReadyStatusList = _CheckReadyStatusList;
		foreach (GameObject gameObject in checkReadyStatusList)
		{
			if (gameObject.GetComponent<ObStatus>() != null)
			{
				mProgress.AddTask(gameObject.name);
			}
		}
	}

	public virtual IEnumerator Start()
	{
		if (!UtPlatform.IsRealTimeShadowEnabled())
		{
			UtUtilities.SetRealTimeShadowDisabled();
		}
		if (AvAvatar.pObject != null)
		{
			AvAvatar.position = new Vector3(0f, -5000f, 0f);
		}
		SnChannel.SetGlobalVolumeReduction("VO_Pool", "VOChannel", 0.8f);
		SnChannel.AddToTurnOffPools(_TurnOffPoolInfo);
		MMOAvatar._IsRideAllowed = _RideAllowed;
		yield return new WaitForEndOfFrame();
		AvAvatar.SetUIActive(inActive: false);
	}

	private bool UpdateProgress(string inTask, bool inReady)
	{
		if (inReady)
		{
			mRecalcProgress = true;
			mProgress.UpdateTask(inTask, 1f, inForceRecalculate: false);
		}
		return inReady;
	}

	public virtual void Update()
	{
		if (mInitialized)
		{
			return;
		}
		RecalculateProgress();
		if (!CheckIfStatusListReady())
		{
			return;
		}
		CheckMembershipVerification();
		Vector3 startPos;
		bool flag;
		Transform transform2;
		Transform[] inTransforms;
		Transform[] inTransforms2;
		if (AllTasksReadyAndInitialized())
		{
			RsResourceManager.DestroyLoadScreen();
			mInitialized = true;
			AvAvatar.SetActive(inActive: true);
			AvAvatar.pState = _AvatarStartState;
			if (!IsSubstateValid(_AvatarStartSubState))
			{
				_AvatarStartSubState = AvAvatarSubState.NORMAL;
				mUseFallbackMarker = true;
			}
			AvAvatar.pSubState = _AvatarStartSubState;
			LoadPetsIfAllowed();
			CheckForAchievement();
			if (AvAvatar.pObject != null)
			{
				SetAvAvatarStateGrounded();
				SetAvAvatarShadowProjection();
			}
			EnEnemy._Target = AvAvatar.pObject;
			startPos = Vector3.zero;
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.LoadLevelComplete(RsResourceManager.pCurrentLevel);
			}
			if (MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.LoadToPlayer(ref startPos))
			{
				TeleportFx.PlayAt(startPos);
				goto IL_0457;
			}
			flag = false;
			GameObject gameObject = null;
			if (!_IgnorePreviousPositionOnLoad)
			{
				if (AvAvatar.pStartLocation == AvAvatar.pSpawnAtSetPosition)
				{
					flag = true;
					startPos = AvAvatar.pStartPosition;
					AvAvatar.mTransform.rotation = AvAvatar.pStartRotation;
				}
				else if (AvAvatar.pStartLocation != null && (gameObject = GameObject.Find(AvAvatar.pStartLocation)) != null)
				{
					flag = true;
					startPos = gameObject.transform.position;
					AvAvatar.mTransform.rotation = gameObject.transform.rotation;
					AvAvatar.pStartLocation = null;
				}
				else if (_AvatarTaskSpawnPoints != null && _AvatarTaskSpawnPoints.Length != 0)
				{
					Task task = MissionManagerDO.GetPlayerActiveTask();
					Mission mission = null;
					if (task == null)
					{
						task = MissionManagerDO.GetNextActiveTask();
					}
					if (task != null)
					{
						mission = MissionManager.pInstance.GetRootMission(task);
					}
					for (int num = _AvatarTaskSpawnPoints.Length - 1; num >= 0; num--)
					{
						TaskSpawnPoint taskSpawnPoint = _AvatarTaskSpawnPoints[num];
						Task task2 = ((taskSpawnPoint._EndTask._TaskID > 0) ? MissionManager.pInstance.GetTask(taskSpawnPoint._EndTask._TaskID) : null);
						if (task2 == null || ((taskSpawnPoint._EndTask._TaskStatus != 0 || (!task2.pStarted && !task2.pCompleted)) && (taskSpawnPoint._EndTask._TaskStatus != TaskState.COMPLETED || !task2.pCompleted)))
						{
							Task task3 = ((taskSpawnPoint._BeginTask._TaskID > 0) ? MissionManager.pInstance.GetTask(taskSpawnPoint._BeginTask._TaskID) : null);
							if (task3 != null && ((taskSpawnPoint._BeginTask._TaskStatus == TaskState.STARTED && (task3.pStarted || task3.pCompleted)) || (taskSpawnPoint._BeginTask._TaskStatus == TaskState.COMPLETED && task3.pCompleted)) && taskSpawnPoint._SpawnPoints != null && taskSpawnPoint._SpawnPoints.Length != 0)
							{
								flag = true;
								Transform transform = taskSpawnPoint._SpawnPoints[UnityEngine.Random.Range(0, taskSpawnPoint._SpawnPoints.Length - 1)];
								startPos = transform.position;
								AvAvatar.mTransform.rotation = transform.rotation;
								Mission rootMission = MissionManager.pInstance.GetRootMission(task3);
								Mission rootMission2 = MissionManager.pInstance.GetRootMission(task2);
								if (mission != null && ((rootMission != null && rootMission.MissionID == mission.MissionID) || (rootMission2 != null && rootMission2.MissionID == mission.MissionID)))
								{
									break;
								}
							}
						}
					}
				}
			}
			transform2 = null;
			if (!flag && !mUseFallbackMarker)
			{
				if (!SubscriptionInfo.pIsMember)
				{
					Transform[] avatarStartMarkerNM = _AvatarStartMarkerNM;
					if (avatarStartMarkerNM != null && avatarStartMarkerNM.Length != 0)
					{
						inTransforms = _AvatarStartMarkerNM;
						goto IL_0380;
					}
				}
				inTransforms = _AvatarStartMarker;
				goto IL_0380;
			}
			if (mUseFallbackMarker)
			{
				if (!SubscriptionInfo.pIsMember)
				{
					Transform[] avatarStartMarkerFallbackNM = _AvatarStartMarkerFallbackNM;
					if (avatarStartMarkerFallbackNM != null && avatarStartMarkerFallbackNM.Length != 0)
					{
						inTransforms2 = _AvatarStartMarkerFallbackNM;
						goto IL_03b9;
					}
				}
				inTransforms2 = _AvatarStartMarkerFallback;
				goto IL_03b9;
			}
			goto IL_03bf;
		}
		if (mProgress.pComplete && mProgressCompleteWaitFrames > 0)
		{
			mProgressCompleteWaitFrames--;
		}
		return;
		IL_0380:
		transform2 = GetStartMarkerTransform(inTransforms);
		goto IL_03bf;
		IL_03b9:
		transform2 = GetStartMarkerTransform(inTransforms2);
		goto IL_03bf;
		IL_0457:
		foreach (string item in _ActivateObjectsOnLoad)
		{
			if (item != null)
			{
				GameObject gameObject2 = GameObject.Find(item);
				if (gameObject2 != null)
				{
					gameObject2.SendMessage("OnActivate", null, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		if (_SaveSceneWhenVisited && ProductData.GetSavedScene() != RsResourceManager.pCurrentLevel)
		{
			ProductData.SaveScene(RsResourceManager.pCurrentLevel);
		}
		AvAvatar.SetPosition(startPos);
		GameObject[] objectNotifyList = _ObjectNotifyList;
		for (int i = 0; i < objectNotifyList.Length; i++)
		{
			objectNotifyList[i].SendMessage("OnLevelReady", null, SendMessageOptions.DontRequireReceiver);
		}
		OnWaitBegin();
		TimeHackPrevent.Set("");
		return;
		IL_03bf:
		if (transform2 != null)
		{
			startPos = transform2.position;
			AvAvatar.mTransform.rotation = transform2.rotation;
		}
		else if (!flag)
		{
			UtDebug.LogError(mUseFallbackMarker ? "No fallback start marker found!" : "No start marker found!");
		}
		if (AvAvatar.pStartLocation != AvAvatar.pSpawnAtSetPosition)
		{
			float num2 = UnityEngine.Random.Range(0, 24) * 15;
			float num3 = Mathf.Cos(num2 * (MathF.PI / 180f));
			float num4 = Mathf.Sin(num2 * (MathF.PI / 180f));
			startPos.x += num3;
			startPos.z += num4;
		}
		else
		{
			AvAvatar.pStartLocation = null;
		}
		goto IL_0457;
	}

	public bool IsSubstateValid(AvAvatarSubState inState)
	{
		if (inState == AvAvatarSubState.FLYING)
		{
			if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pAge >= SanctuaryManager.pCurPetInstance.pTypeInfo._MinAgeToMount && SanctuaryManager.pCurPetInstance.pAge >= SanctuaryManager.pCurPetInstance.pTypeInfo._MinAgeToFly)
			{
				return !SanctuaryManager.pCurPetInstance.pTypeInfo._Flightless;
			}
			return false;
		}
		return true;
	}

	private Transform GetStartMarkerTransform(Transform[] inTransforms)
	{
		Transform result = null;
		if (inTransforms != null && inTransforms.Length != 0)
		{
			result = inTransforms[UnityEngine.Random.Range(0, inTransforms.Length - 1)];
		}
		return result;
	}

	private void LoadPetsIfAllowed()
	{
		if (_PetAllowed)
		{
			PetBundleLoader.LoadPetsForAvatar(AvAvatar.pObject);
		}
		else
		{
			PetBundleLoader.DetachPetsForAvatar(AvAvatar.pObject);
		}
	}

	private void CheckMembershipVerification()
	{
		if (_MembershipVerified)
		{
			return;
		}
		if (SubscriptionInfo.pIsMember)
		{
			_MembershipVerified = true;
		}
		else if (AvatarData.pIsReady)
		{
			_MembershipVerified = true;
			bool flag = false;
			if (AvatarData.PartHasGeo(AvatarData.pPartSettings.AVATAR_PART_WING))
			{
				AvatarData.RestoreDefault();
				flag = true;
				UtDebug.LogError("Wing removed due to lost of membership");
			}
			if (AvatarData.PartHasGeo(AvatarData.pPartSettings.AVATAR_PART_TAIL))
			{
				AvatarData.RemovePartData(AvatarData.pPartSettings.AVATAR_PART_TAIL);
				UtDebug.LogError("Tail removed due to lost of membership");
				flag = true;
			}
			if (flag)
			{
				AvatarData.Save();
			}
		}
	}

	private bool AllTasksReadyAndInitialized()
	{
		if (UpdateProgress("ProductConfig", ProductConfig.pIsReady) && UpdateProgress("ProductData", ProductData.pIsReady) && UpdateProgress("MissionManager", MissionManager.pIsReady) && UpdateProgress("AvatarData", AvatarData.pIsReady) && UpdateProgress("UserInfo", UserInfo.pIsReady) && UpdateProgress("Money", Money.pIsReady) && UpdateProgress("CommonInventoryData", CommonInventoryData.pIsReady) && UpdateProgress("ServerTime", ServerTime.pIsReady) && UpdateProgress("WsUserMessage", WsUserMessage.pIsReady) && UpdateProgress("SubscriptionInfo", SubscriptionInfo.pIsReady) && UpdateProgress("LocaleData", LocaleData.pIsReady) && UpdateProgress("GameDataConfig", GameDataConfig.pIsReady) && UpdateProgress("SanctuaryManager", SanctuaryManager.pIsReady) && UpdateProgress("MainStreetMMOClient", MainStreetMMOClient.pIsMMOAvatarsReady))
		{
			return mProgressCompleteWaitFrames <= 0;
		}
		return false;
	}

	private void RecalculateProgress()
	{
		if (mRecalcProgress)
		{
			mProgress.Recalculate();
			mRecalcProgress = false;
			RsResourceManager.UpdateLoadProgress("CommonLevel", mProgress.pProgress);
		}
	}

	private bool CheckIfStatusListReady()
	{
		GameObject[] checkReadyStatusList = _CheckReadyStatusList;
		foreach (GameObject gameObject in checkReadyStatusList)
		{
			ObStatus component = gameObject.GetComponent<ObStatus>();
			if (component != null)
			{
				if (!UpdateProgress(gameObject.name, component.pIsReady))
				{
					return false;
				}
				continue;
			}
			return false;
		}
		return true;
	}

	private void SetAvAvatarStateGrounded()
	{
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (!(component == null))
		{
			component.ResetLastPositionOnGround();
			component.SetFlyingState(FlyingState.Grounded);
			if (SanctuaryManager.pCurPetInstance != null && !SanctuaryManager.pCurPetInstance.IsMountAllowed())
			{
				component.SetMounted(mounted: false);
			}
		}
	}

	private void SetAvAvatarShadowProjection()
	{
		AvAvatar.pObject.GetComponentInChildren<ActivateShadow>().pForceProjectionShadow = _ForceProjectionShadow;
	}

	public virtual void OnPropLoaderDone()
	{
	}

	public void OnWaitBegin()
	{
		if (_WaitList.Length != 0 && mWaitIndex < _WaitList.Length)
		{
			GameObject gameObject = _WaitList[mWaitIndex];
			if (gameObject != null)
			{
				UserNotify component = gameObject.GetComponent<UserNotify>();
				if (component != null)
				{
					component.OnWaitBegin(base.gameObject);
					return;
				}
				UtDebug.LogError("Null Component in _WaitList!!! " + gameObject.name);
				OnWaitEnd();
			}
			else
			{
				UtDebug.LogError("Null object in _WaitList!!! " + base.name);
				OnWaitEnd();
			}
			return;
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (_WaitListCompletedNotifyList != null)
		{
			GameObject[] waitListCompletedNotifyList = _WaitListCompletedNotifyList;
			foreach (GameObject gameObject2 in waitListCompletedNotifyList)
			{
				if (gameObject2 != null)
				{
					gameObject2.SendMessage("OnWaitListCompleted", null, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		if (CoCommonLevel.WaitListCompleted != null)
		{
			CoCommonLevel.WaitListCompleted();
		}
		RewardManager.OnWaitListCompleted();
	}

	public void OnWaitEnd()
	{
		mWaitIndex++;
		OnWaitBegin();
	}

	private void CheckForAchievement()
	{
		if (_VisitAchievementData != null && _VisitAchievementData._AchievementId > 0 && !_VisitAchievementData._IgnoreFromLevels.Contains(RsResourceManager.pLastLevel))
		{
			UserAchievementTask.Set(_VisitAchievementData._AchievementId, 1, _VisitAchievementData._RelatedID, displayRewards: false, _VisitAchievementData._AchievementInfoID);
		}
	}

	static CoCommonLevel()
	{
		CoCommonLevel.WaitListCompleted = null;
	}
}
