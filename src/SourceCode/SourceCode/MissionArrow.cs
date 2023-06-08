using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class MissionArrow : MonoBehaviour
{
	public static int _PairDataID = -1;

	public TextAsset _SceneMapData;

	public float _VerticalOffsetScreen = 60f;

	public float _ForwardOffsetWorld = 2f;

	public float _RepathRate = 0.5f;

	public float _AvailableTaskRefreshRate = 2f;

	public float _TurnSpeed = 5f;

	public KAWidget _QuestTextWidget;

	public LocaleString _MeetNPCMessageText = new LocaleString("{{npc}} would like to talk to you");

	public LocaleString _NewQuestMessageText = new LocaleString("New Quest");

	private GameObject mCurrentTargetGO;

	private bool mCurrentTarget;

	private Renderer mRenderer;

	private bool mIsSetup;

	private float mLastRepathTime;

	private Task mCurrentActiveTask;

	private string mQuestText;

	private Mission mLoadingMission;

	private NavMeshPath mNavMeshPath;

	private bool mForceStraightArrow;

	public Task pCurrentActiveTask => mCurrentActiveTask;

	private void Start()
	{
		mRenderer = GetComponentInChildren<Renderer>();
		MissionManager.AddMissionEventHandler(OnMissionEvent);
		if (AvAvatar.pObject != null)
		{
			SetupTarget();
		}
		MissionManagerDO._QuestArrow = this;
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		if (AvAvatar.pObject != null)
		{
			SetupTarget();
		}
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		mIsSetup = false;
	}

	private void OnDestroy()
	{
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
	}

	private void LateUpdate()
	{
		if (Camera.main == null || mRenderer == null)
		{
			return;
		}
		if (!mIsSetup || (mCurrentTargetGO == null && mCurrentTarget))
		{
			SetupTarget();
		}
		bool flag = true;
		if (mCurrentActiveTask != null && mCurrentActiveTask.GetObjectiveValue<bool>("HideArrow"))
		{
			flag = false;
		}
		flag = flag && UiOptions.pShowQuestArrow;
		if (_QuestTextWidget != null && _QuestTextWidget.GetVisibility() != UiOptions.pShowQuestArrow)
		{
			_QuestTextWidget.SetVisibility(UiOptions.pShowQuestArrow);
		}
		if (flag && mCurrentTargetGO != null && InteractiveTutManager._CurrentActiveTutorialObject == null)
		{
			mRenderer.enabled = true;
			mCurrentTarget = true;
			if (Time.time - mLastRepathTime >= _RepathRate)
			{
				mLastRepathTime = Time.time;
				if (mNavMeshPath == null)
				{
					mNavMeshPath = new NavMeshPath();
				}
				NavMesh.CalculatePath(AvAvatar.position, mCurrentTargetGO.transform.position, 1, mNavMeshPath);
			}
			ShowArrow();
		}
		else
		{
			mRenderer.enabled = false;
		}
	}

	private void ShowArrow()
	{
		if (!(Camera.main == null) && !(mRenderer == null))
		{
			Vector3 vector = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0f + _VerticalOffsetScreen, _ForwardOffsetWorld));
			if (!AvAvatar.IsFlying() && !mForceStraightArrow && mNavMeshPath != null && mNavMeshPath.corners != null && mNavMeshPath.corners.Length > 2)
			{
				base.transform.position = vector;
				Vector3 vector2 = mNavMeshPath.corners[1];
				vector2.y = vector.y;
				Debug.DrawLine(vector, base.transform.position, Color.white);
				Vector3 forward = vector2 - base.transform.position;
				Quaternion rotation = base.transform.rotation;
				Quaternion b = Quaternion.LookRotation(forward);
				rotation = Quaternion.Slerp(rotation, b, _TurnSpeed * Time.fixedDeltaTime);
				base.transform.rotation = rotation;
			}
			else if (mCurrentTargetGO != null)
			{
				base.transform.position = vector;
				base.transform.LookAt(mCurrentTargetGO.transform.position);
			}
		}
	}

	public void SetupTarget()
	{
		CancelInvoke();
		mCurrentTargetGO = null;
		mCurrentTarget = false;
		mCurrentActiveTask = null;
		mForceStraightArrow = false;
		if (MissionManager.pIsReady)
		{
			mIsSetup = true;
			mCurrentActiveTask = MissionManagerDO.GetPlayerActiveTask();
			if (mCurrentActiveTask != null)
			{
				mCurrentTargetGO = MissionManagerDO.GetTargetForTask(mCurrentActiveTask);
				if (mCurrentTargetGO == null)
				{
					mCurrentTargetGO = MissionManagerDO.GetPortalForTask(mCurrentActiveTask);
				}
				if (mCurrentTargetGO == null)
				{
					mCurrentTargetGO = MissionManagerDO.GetCollectiblesForTask(mCurrentActiveTask);
				}
			}
			else
			{
				if (mCurrentTargetGO == null)
				{
					mCurrentTargetGO = MissionManagerDO.GetTaskTargetInCurrentScene(out mCurrentActiveTask);
				}
				if (mCurrentTargetGO == null)
				{
					List<Task> tasksAccessibleFromCurrentScene = MissionManagerDO.GetTasksAccessibleFromCurrentScene();
					if (tasksAccessibleFromCurrentScene != null && tasksAccessibleFromCurrentScene.Count > 0)
					{
						mCurrentActiveTask = tasksAccessibleFromCurrentScene[0];
						mCurrentTargetGO = MissionManagerDO.GetPortalForTask(mCurrentActiveTask);
					}
				}
				if (mCurrentTargetGO == null && mCurrentActiveTask == null)
				{
					mCurrentActiveTask = MissionManagerDO.GetNextActiveTask();
				}
				if (mCurrentTargetGO == null && mCurrentActiveTask == null)
				{
					mCurrentTargetGO = MissionManagerDO.GetNearestNPCWithAvailableTask();
					if (mCurrentTargetGO == null)
					{
						Task newQuest = MissionManagerDO.GetNewQuest();
						if (newQuest != null)
						{
							string sceneForNPC = MissionManagerDO.GetSceneForNPC(newQuest._Mission.GroupID);
							if (!string.IsNullOrEmpty(sceneForNPC))
							{
								if (sceneForNPC != RsResourceManager.pCurrentLevel)
								{
									string portalToScene = MissionManagerDO.GetPortalToScene(RsResourceManager.pCurrentLevel, sceneForNPC);
									if (!string.IsNullOrEmpty(portalToScene))
									{
										mCurrentTargetGO = GameObject.Find(portalToScene);
									}
									else
									{
										UtDebug.LogError("Portal to target scene not found");
									}
								}
								else
								{
									string nPCObjectName = MissionManagerDO.GetNPCObjectName(newQuest._Mission.GroupID);
									if (!string.IsNullOrEmpty(nPCObjectName))
									{
										mCurrentTargetGO = GameObject.Find(nPCObjectName);
									}
								}
							}
						}
					}
				}
			}
			if (mCurrentTargetGO != null && mCurrentTargetGO.name.Contains("StraightArrow"))
			{
				mForceStraightArrow = true;
			}
			SetupText();
		}
		if (mLoadingMission != null)
		{
			Mission mission = mLoadingMission;
			mission.OnSetupComplete = (Mission.SetupComplete)Delegate.Remove(mission.OnSetupComplete, new Mission.SetupComplete(SetupTarget));
			mLoadingMission = null;
		}
		if (mCurrentTargetGO == null && mCurrentActiveTask != null && mCurrentActiveTask._Mission != null && mCurrentActiveTask._Mission.pData != null && mCurrentActiveTask._Mission.pData.Setups != null && !mCurrentActiveTask._Mission.pIsReady)
		{
			UtDebug.Log("Mission Arrow : Mission setup is pending for MI" + mCurrentActiveTask._Mission.MissionID);
			mLoadingMission = mCurrentActiveTask._Mission;
			Mission mission2 = mLoadingMission;
			mission2.OnSetupComplete = (Mission.SetupComplete)Delegate.Combine(mission2.OnSetupComplete, new Mission.SetupComplete(SetupTarget));
		}
	}

	public void SetupText()
	{
		if (!(_QuestTextWidget != null))
		{
			return;
		}
		if (mCurrentActiveTask != null && mCurrentActiveTask.pData.Title != null)
		{
			mQuestText = MissionManager.pInstance.FormatText(mCurrentActiveTask.pData.Title.ID, mCurrentActiveTask.pData.Title.Text);
			if (mCurrentActiveTask.pData.Type == "Collect" && mCurrentActiveTask.pData.Objectives.Count > 0)
			{
				foreach (TaskObjective objective in mCurrentActiveTask.pData.Objectives)
				{
					int num = objective.Get<int>("Quantity");
					if (num > 1)
					{
						mQuestText = mQuestText + " (" + objective._Collected + " / " + num + ")";
					}
				}
			}
			_QuestTextWidget.SetText(mQuestText);
		}
		else
		{
			mQuestText = GetAvailableTaskMessage(mCurrentTargetGO);
			_QuestTextWidget.SetText(mQuestText);
			Invoke("RefreshAvailableTask", _AvailableTaskRefreshRate);
		}
	}

	protected virtual string GetAvailableTaskMessage(GameObject inNPCObj)
	{
		string result = "";
		if (inNPCObj != null)
		{
			NPCAvatar component = inNPCObj.GetComponent<NPCAvatar>();
			if (component != null)
			{
				result = _MeetNPCMessageText.GetLocalizedString();
				result = result.Replace("{{npc}}", MissionManagerDO.GetNPCName(component._Name));
			}
			else
			{
				result = _NewQuestMessageText.GetLocalizedString();
			}
		}
		return result;
	}

	protected void RefreshAvailableTask()
	{
		if (mCurrentTargetGO != null && mCurrentActiveTask == null)
		{
			mIsSetup = false;
		}
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		switch (inEvent)
		{
		case MissionEvent.TASK_STARTED:
		case MissionEvent.TASK_COMPLETE:
		case MissionEvent.MISSION_COMPLETE:
			mIsSetup = false;
			break;
		case MissionEvent.COLLECTED:
			if ((Task)inObject == mCurrentActiveTask)
			{
				SetupText();
			}
			break;
		}
	}

	public void Collect(GameObject inCollectedGO)
	{
		if (inCollectedGO == mCurrentTargetGO && mCurrentTargetGO != null)
		{
			mIsSetup = false;
		}
	}

	public GameObject GetNearestTarget(List<GameObject> targets)
	{
		GameObject result = null;
		float num = 100000f;
		foreach (GameObject target in targets)
		{
			float num2 = Vector3.Distance(AvAvatar.position, target.transform.position);
			if (num2 < num)
			{
				num = num2;
				result = target;
			}
		}
		return result;
	}
}
