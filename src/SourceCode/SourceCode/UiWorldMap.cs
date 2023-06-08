using System;
using System.Collections.Generic;
using UnityEngine;

public class UiWorldMap : KAUI
{
	[Serializable]
	public class MapItemInfo
	{
		public string _ItemName;

		public string _LevelToLoad;

		public string[] _SubLevels;

		public string _StartMarkerName;
	}

	[NonSerialized]
	public UiSceneMap _SceneMap;

	[NonSerialized]
	public UiToolbar _MiniMap;

	public GameObject _MessageObject;

	public string _MapLocationSelectedMessage = "OnLocationSelected";

	public string _MapClosedMessage = "OnMapClosed";

	public LocaleString _NewQuestText = new LocaleString("New Quest");

	public MapItemInfo[] _MapInfo;

	public List<string> _IgnoredScenesList;

	public string _DefaultPlayerMarkerScene = "HubSchoolDO";

	public KAWidget _PlayerMarker;

	public KAWidget _ObjectiveMarker;

	public KAWidget _PlayerMarkerPointer;

	public KAWidget _ObjectMarkerPointer;

	public KAWidget _ObjectiveLocationMap;

	public float _PointerMarkerEdgeOffsetX = 200f;

	public float _PointerMarkerEdgeOffsetY = 180f;

	public GameObject _WorldLocationsGrp;

	public GameObject _MapBkg;

	public UiScrollManager _UiMapScrollManager;

	public bool _MembersOnly;

	public bool _EnableInputsOnExit = true;

	public bool _AllowSameSceneLoad;

	public float _ScaleFactor = 1f;

	public LocaleString _NonMemberMessage = new LocaleString("Members can teleport to different islands! \n Do you want to become a member ?");

	public LocaleString _StoreUnavailableText = new LocaleString("Store Unavailable at this time. Please try again later.");

	public string _BecomeMemberLogEventText = "BecomeMemberFromWorldMap";

	private List<KAWidget> mMarkers = new List<KAWidget>();

	private KAUIGenericDB mMembershipDB;

	private KAUIGenericDB mKAUIGenericDB;

	protected override void Start()
	{
		base.Start();
		_WorldLocationsGrp.transform.localScale = new Vector3(_ScaleFactor, _ScaleFactor, 1f);
		_MapBkg.transform.localScale = new Vector3(_ScaleFactor, _ScaleFactor, 1f);
		Show(show: true);
		if (_UiMapScrollManager != null)
		{
			_UiMapScrollManager.OnInitDone += UpdateArrowMarkers;
		}
	}

	private void PlotMarkers()
	{
		for (int i = 0; i < _MapInfo.Length; i++)
		{
			if (!UnlockManager.IsSceneUnlocked(_MapInfo[i]._LevelToLoad, inShowUi: true))
			{
				KAWidget kAWidget = FindItem(_MapInfo[i]._ItemName).FindChildItem("IcoLock");
				if (kAWidget != null)
				{
					kAWidget.SetVisibility(inVisible: true);
				}
			}
		}
		if (MissionManager.pIsReady && MissionManager.pInstance != null)
		{
			Task task = MissionManagerDO.GetPlayerActiveTask() ?? MissionManagerDO.GetNextActiveTask();
			string taskScene = GetTaskScene(task);
			if (taskScene != null)
			{
				SetObjectiveMarker(taskScene);
			}
			else
			{
				if (MissionManagerDO.GetNearestNPCWithAvailableTask() != null)
				{
					SetObjectiveMarker(RsResourceManager.pCurrentLevel);
				}
				else
				{
					Task newQuest = MissionManagerDO.GetNewQuest();
					if (newQuest != null)
					{
						taskScene = MissionManagerDO.GetSceneForNPC(newQuest._Mission.GroupID);
						if (!string.IsNullOrEmpty(taskScene))
						{
							SetObjectiveMarker(taskScene);
						}
					}
				}
				_ObjectiveLocationMap.SetText(_NewQuestText.GetLocalizedString());
			}
		}
		PlotPlayerMarker();
	}

	public string PlotAndFocusQuestMarker(Task task)
	{
		string taskScene = GetTaskScene(task);
		if (taskScene == null)
		{
			return null;
		}
		KAWidget kAWidget = SetObjectiveMarker(taskScene);
		if (kAWidget == null)
		{
			return null;
		}
		MapItemInfo[] mapInfo = _MapInfo;
		foreach (MapItemInfo mapItemInfo in mapInfo)
		{
			KAWidget kAWidget2 = FindItem(mapItemInfo._ItemName);
			if (kAWidget2 != null)
			{
				kAWidget2.SetInteractive(kAWidget2 == kAWidget);
			}
		}
		UICenterOnChild component = GetComponent<UICenterOnChild>();
		if (component == null)
		{
			return null;
		}
		component.CenterOn(kAWidget.transform);
		return taskScene;
	}

	private string GetTaskScene(Task task)
	{
		if (task?.pData?.Objectives == null)
		{
			return null;
		}
		List<TaskSetup> setups = task.GetSetups();
		if (task.pData.Type == "Collect" && setups != null && setups.Count > 0)
		{
			foreach (TaskSetup item in setups)
			{
				string scene = item.Scene;
				if (scene != null)
				{
					return scene;
				}
			}
		}
		else
		{
			foreach (TaskObjective objective in task.pData.Objectives)
			{
				string text = objective.Get<string>("Scene");
				if (text != null)
				{
					return text;
				}
			}
		}
		return null;
	}

	private void PlotPlayerMarker()
	{
		KAWidget kAWidget = null;
		string text = string.Empty;
		if (_IgnoredScenesList != null && _IgnoredScenesList.Count > 0)
		{
			for (int i = 0; i < _IgnoredScenesList.Count; i++)
			{
				if (!(_IgnoredScenesList[i] != RsResourceManager.pCurrentLevel))
				{
					text = ProductData.GetSavedScene();
					if (string.IsNullOrEmpty(text))
					{
						text = _DefaultPlayerMarkerScene;
					}
					break;
				}
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = RsResourceManager.pCurrentLevel;
		}
		for (int j = 0; j < _MapInfo.Length; j++)
		{
			if (_MapInfo[j]._LevelToLoad == text)
			{
				kAWidget = FindItem(_MapInfo[j]._ItemName);
			}
			else
			{
				string[] subLevels = _MapInfo[j]._SubLevels;
				for (int k = 0; k < subLevels.Length; k++)
				{
					if (!(subLevels[k] != text))
					{
						kAWidget = FindItem(_MapInfo[j]._ItemName);
						break;
					}
				}
			}
			if (!(kAWidget == null))
			{
				KAWidget playerMarker = _PlayerMarker;
				playerMarker.SetVisibility(inVisible: true);
				playerMarker.transform.position = kAWidget.transform.position;
				break;
			}
		}
	}

	public KAWidget SetObjectiveMarker(string taskScene)
	{
		if (!string.IsNullOrEmpty(taskScene))
		{
			KAWidget kAWidget = null;
			for (int i = 0; i < _MapInfo.Length; i++)
			{
				if (_MapInfo[i]._LevelToLoad == taskScene)
				{
					kAWidget = FindItem(_MapInfo[i]._ItemName);
				}
				else
				{
					string[] subLevels = _MapInfo[i]._SubLevels;
					for (int j = 0; j < subLevels.Length; j++)
					{
						if (subLevels[j] == taskScene)
						{
							kAWidget = FindItem(_MapInfo[i]._ItemName);
						}
					}
				}
				if (!(kAWidget == null))
				{
					KAWidget objectiveMarker = _ObjectiveMarker;
					objectiveMarker.SetVisibility(inVisible: true);
					objectiveMarker.transform.position = kAWidget.transform.position;
					mMarkers.Add(objectiveMarker);
					return kAWidget;
				}
			}
		}
		return null;
	}

	public override void OnPress(KAWidget inWidget, bool inPressed)
	{
		base.OnPress(inWidget, inPressed);
		if ((!("BtnScrollRt" != inWidget.name) || !("BtnScrollLt" != inWidget.name) || !("BtnScrollUp" != inWidget.name) || !("BtnScrollDown" != inWidget.name)) && _UiMapScrollManager != null)
		{
			_UiMapScrollManager.OnButtonPress(inWidget, inPressed);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if ("BtnClose" == inWidget.name)
		{
			Show(show: false);
			if (_EnableInputsOnExit)
			{
				AvAvatar.pInputEnabled = true;
				AvAvatar.pState = AvAvatarState.IDLE;
				AvAvatar.SetUIActive(inActive: true);
			}
			Cleanup();
			if (_SceneMap != null)
			{
				_SceneMap.Cleanup();
			}
			if (_MiniMap != null)
			{
				_MiniMap.SetVisibility(inVisible: true);
			}
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage(_MapClosedMessage, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if ("BtnToggle" == inWidget.name)
		{
			Show(show: false);
			if (_SceneMap != null)
			{
				_SceneMap.Show(show: true);
			}
			Cleanup();
		}
		else if (inWidget == _PlayerMarkerPointer)
		{
			_UiMapScrollManager.ResetToMarker(_PlayerMarker.gameObject);
		}
		else if (inWidget == _ObjectMarkerPointer)
		{
			_UiMapScrollManager.ResetToMarker(_ObjectiveMarker.gameObject);
		}
		else
		{
			if (_MapInfo == null)
			{
				return;
			}
			for (int i = 0; i < _MapInfo.Length; i++)
			{
				if (_MapInfo[i]._ItemName != inWidget.name || (_MapInfo[i]._LevelToLoad == RsResourceManager.pCurrentLevel && !_AllowSameSceneLoad) || !UnlockManager.IsSceneUnlocked(_MapInfo[i]._LevelToLoad, inShowUi: false, delegate(bool success)
				{
					if (success)
					{
						OnClick(inWidget);
					}
				}))
				{
					continue;
				}
				if (!_MembersOnly || SubscriptionInfo.pIsMember)
				{
					Cleanup();
					if (_MessageObject != null)
					{
						_MessageObject.SendMessage(_MapLocationSelectedMessage, _MapInfo[i]._LevelToLoad, SendMessageOptions.RequireReceiver);
						continue;
					}
					if (AvAvatar.pObject != null)
					{
						AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
						if (component != null)
						{
							component.ResetAvatarState();
						}
						AvAvatar.SetActive(inActive: false);
					}
					RsResourceManager.LoadLevel(_MapInfo[i]._LevelToLoad);
				}
				else
				{
					ShowBecomeMemberDialog();
				}
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (_UiMapScrollManager != null && _UiMapScrollManager.IsScrolling)
		{
			UpdateArrowMarkers();
		}
	}

	private void UpdateMarkers()
	{
		MissionManagerDO missionManagerDO = (MissionManagerDO)MissionManager.pInstance;
		List<string> taskObjectiveScenes = missionManagerDO.GetTaskObjectiveScenes();
		for (int i = 0; i < _MapInfo.Length; i++)
		{
			string levelToLoad = _MapInfo[i]._LevelToLoad;
			_MapInfo[i]._LevelToLoad = missionManagerDO.UpdatedScene(_MapInfo[i]._LevelToLoad, taskObjectiveScenes);
			if (!(levelToLoad == _MapInfo[i]._LevelToLoad))
			{
				KAWidget kAWidget = FindItem(_MapInfo[i]._ItemName).FindChildItem("IcoLock");
				if (kAWidget != null)
				{
					kAWidget.SetVisibility(inVisible: false);
				}
			}
		}
	}

	public void Show(bool show)
	{
		if (GetVisibility() == show)
		{
			return;
		}
		SetVisibility(show);
		if (show)
		{
			SetState(KAUIState.INTERACTIVE);
		}
		else
		{
			SetState(KAUIState.NOT_INTERACTIVE);
		}
		if (show)
		{
			PlotMarkers();
			UpdateMarkers();
			if (_EnableInputsOnExit)
			{
				AvAvatar.pInputEnabled = false;
				AvAvatar.pState = AvAvatarState.PAUSED;
			}
			KAWidget kAWidget = FindItem("BtnToggle");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(_SceneMap != null);
			}
		}
		else
		{
			for (int i = 0; i < mMarkers.Count; i++)
			{
				RemoveWidget(mMarkers[i]);
			}
			mMarkers.Clear();
		}
	}

	public void Cleanup()
	{
		if (base.transform.parent == null)
		{
			if (_UiMapScrollManager != null)
			{
				_UiMapScrollManager.OnInitDone -= UpdateArrowMarkers;
			}
			SetVisibility(inVisible: false);
			UnityEngine.Object.Destroy(base.gameObject);
			if (_MiniMap != null && !string.IsNullOrEmpty(_MiniMap._WorldMapBundle))
			{
				RsResourceManager.Unload(_MiniMap._WorldMapBundle);
			}
			else if (_SceneMap != null)
			{
				RsResourceManager.Unload(_SceneMap._WorldMapBundle);
			}
		}
	}

	private void ShowBecomeMemberDialog()
	{
		mMembershipDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Become Member");
		mMembershipDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		mMembershipDB.SetText(_NonMemberMessage.GetLocalizedString(), interactive: false);
		mMembershipDB._MessageObject = base.gameObject;
		mMembershipDB._YesMessage = "BecomeMember";
		mMembershipDB._NoMessage = "OnNo";
		mMembershipDB.SetDestroyOnClick(isDestroy: true);
		KAUI.SetExclusive(mMembershipDB);
	}

	private void ShowDialog(LocaleString text, string dbTitle, string yesMessage, string noMessage, string okMessage, string closeMessage)
	{
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", dbTitle);
		mKAUIGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(yesMessage), !string.IsNullOrEmpty(noMessage), !string.IsNullOrEmpty(okMessage), !string.IsNullOrEmpty(closeMessage));
		mKAUIGenericDB.SetText(text.GetLocalizedString(), interactive: false);
		mKAUIGenericDB.SetMessage(base.gameObject, yesMessage, noMessage, okMessage, closeMessage);
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void OnNo()
	{
		UnityEngine.Object.Destroy(mMembershipDB.gameObject);
	}

	private void BecomeMember()
	{
		UnityEngine.Object.Destroy(mMembershipDB.gameObject);
		SetInteractive(interactive: false);
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
	}

	public void OnIAPStoreClosed()
	{
		SetInteractive(interactive: true);
	}

	private void KillGenericDB()
	{
		if (!(mKAUIGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	private void UpdateArrowMarkers()
	{
		UpdateArrowMarker(_ObjectiveMarker, _ObjectMarkerPointer);
		UpdateArrowMarker(_PlayerMarker, _PlayerMarkerPointer);
	}

	private void UpdateArrowMarker(KAWidget positionMarker, KAWidget pointerMarker)
	{
		if (UtUtilities.IsInView(KAUIManager.pInstance.camera, positionMarker.gameObject) || !positionMarker.GetVisibility())
		{
			if (pointerMarker.GetVisibility())
			{
				pointerMarker.SetVisibility(inVisible: false);
			}
			return;
		}
		pointerMarker.transform.position = GetIntersectPos(positionMarker.gameObject.transform.position);
		KAWidget kAWidget = pointerMarker.FindChildItem("Pointers");
		Vector3 to = positionMarker.transform.position - pointerMarker.transform.position;
		to.Normalize();
		float angle = Vector3.SignedAngle(Vector3.right, to, Vector3.forward);
		kAWidget.gameObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		if (!pointerMarker.GetVisibility())
		{
			pointerMarker.SetVisibility(inVisible: true);
		}
	}

	private Vector3 GetIntersectPos(Vector3 position)
	{
		position = UtUtilities.GetScreenPosition(position);
		float num = KAUIManager.pInstance.camera.pixelRect.width - _PointerMarkerEdgeOffsetX;
		float num2 = KAUIManager.pInstance.camera.pixelRect.height - _PointerMarkerEdgeOffsetY;
		Vector3 screenPosition = UtUtilities.GetScreenPosition(KAUIManager.pInstance.camera.transform.position);
		float num3 = position.x - screenPosition.x;
		float num4 = position.y - screenPosition.y;
		float num5;
		float num6;
		if (num * Mathf.Abs(num4) < num2 * Mathf.Abs(num3))
		{
			num5 = Mathf.Sign(num3) * (num / 2f);
			num6 = num4 * num5 / num3;
		}
		else
		{
			num6 = Mathf.Sign(num4) * (num2 / 2f);
			num5 = num3 * num6 / num4;
		}
		Vector3 position2 = new Vector3(screenPosition.x + num5, screenPosition.y + num6, position.z);
		return KAUIManager.pInstance.camera.ScreenToWorldPoint(position2);
	}
}
