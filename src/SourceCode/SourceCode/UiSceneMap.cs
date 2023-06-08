using System;
using System.Collections.Generic;
using UnityEngine;

public class UiSceneMap : KAUI
{
	[Serializable]
	public class SceneNameMapping
	{
		public string _SceneName;

		public string _DisplayName;
	}

	public bool _ShowNpcIcons;

	public Transform _MapWidget;

	public BoxCollider _MapBounds;

	public KAWidget _GetObjectiveMarker;

	public KAWidget _ObjectiveCompleteMarker;

	public KAWidget _PlayerBlip;

	public KAWidget _NPCBlip;

	public KAWidget _PortalBlip;

	public KAWidget _FishingBlip;

	public KAWidget _FarmingBlip;

	public KAWidget _RacingBlip;

	public KAWidget _FlightSchoolBlip;

	public KAWidget _ScientificMachineBlip;

	public KAWidget _DragonTaxiBlip;

	public KAWidget _LabBlip;

	public KAWidget _TargetPracticeBlip;

	public KAWidget _StoreBlip;

	public string[] _Portals;

	public bool _ShowNPCWithQuestOnly = true;

	public string _FarmingPortalName = "PfFarmPortal";

	public string _RacingPortalName = "PfRacingPortal";

	public string _FlightSchoolPortalName = "PfFlightSchoolPortal";

	[NonSerialized]
	public UiToolbar _WorldMap;

	[NonSerialized]
	public UiToolbar _MiniMap;

	public string _WorldMapBundle = "RS_DATA/AniDWDragonsMapBerkDO.Unity3d/AniDWDragonsMapBerkDO";

	public SceneNameMapping[] _NameMapping;

	private Vector2 mPlayerPos = Vector2.zero;

	private KAWidget mItemWidget;

	private List<KAWidget> mClonedItems = new List<KAWidget>();

	private Vector3 mEulerAngles;

	private string GetDisplayName(string scnName)
	{
		for (int i = 0; i < _NameMapping.Length; i++)
		{
			if (_NameMapping[i]._SceneName == scnName)
			{
				return _NameMapping[i]._DisplayName;
			}
		}
		return "";
	}

	private void ShowQuestIcons()
	{
		NPCAvatar[] array = UnityEngine.Object.FindObjectsOfType(typeof(NPCAvatar)) as NPCAvatar[];
		if (_GetObjectiveMarker != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				bool flag = false;
				if (MissionManagerDO.IsNPCQuestRewardAvailable(array[i]._Name))
				{
					if (null != _ObjectiveCompleteMarker)
					{
						PlotOnMap(_ObjectiveCompleteMarker, array[i].gameObject.transform.position);
					}
					flag = true;
				}
				if (!flag)
				{
					List<Task> tasks = MissionManager.pInstance.GetTasks(array[i]._MissionGroupID);
					if (tasks != null && tasks.Count > 0)
					{
						for (int j = 0; j < tasks.Count; j++)
						{
							if (!tasks[j].pPayload.Started)
							{
								PlotOnMap(_GetObjectiveMarker, array[i].gameObject.transform.position);
								flag = true;
								break;
							}
						}
					}
				}
				if (_ShowNpcIcons && ((flag && _ShowNPCWithQuestOnly) || !_ShowNPCWithQuestOnly) && null != _NPCBlip)
				{
					PlotOnMap(_NPCBlip, array[i].gameObject.transform.position);
					KAWidget kAWidget = mItemWidget.FindChildItem("MapIcon");
					if (null != kAWidget)
					{
						kAWidget.PlayAnim(array[i]._Name);
						string nPCName = MissionManagerDO.GetNPCName(array[i]._Name);
						mItemWidget.SetText(nPCName);
					}
				}
			}
		}
		if (_ObjectiveCompleteMarker != null)
		{
			List<Task> tasksInCurrentScene = MissionManagerDO.GetTasksInCurrentScene();
			for (int k = 0; k < tasksInCurrentScene.Count; k++)
			{
				GameObject targetForTask = MissionManagerDO.GetTargetForTask(tasksInCurrentScene[k]);
				if (null != targetForTask)
				{
					PlotOnMap(_ObjectiveCompleteMarker, targetForTask.transform.position);
				}
			}
			List<Task> tasksAccessibleFromCurrentScene = MissionManagerDO.GetTasksAccessibleFromCurrentScene();
			for (int l = 0; l < tasksAccessibleFromCurrentScene.Count; l++)
			{
				GameObject portalForTask = MissionManagerDO.GetPortalForTask(tasksAccessibleFromCurrentScene[l]);
				if (null != portalForTask)
				{
					PlotOnMap(_ObjectiveCompleteMarker, portalForTask.transform.position);
				}
			}
		}
		HashSet<string> allPortalsInScene = MissionManagerDO.GetAllPortalsInScene(RsResourceManager.pCurrentLevel);
		for (int m = 0; m < _Portals.Length; m++)
		{
			if (!allPortalsInScene.Contains(_Portals[m]))
			{
				allPortalsInScene.Add(_Portals[m]);
			}
		}
		if (allPortalsInScene.Count <= 0)
		{
			return;
		}
		foreach (string item in allPortalsInScene)
		{
			if (string.IsNullOrEmpty(item))
			{
				continue;
			}
			GameObject gameObject = GameObject.Find(item);
			if (null == gameObject)
			{
				continue;
			}
			if (null != gameObject.GetComponent<LabPortalTrigger>())
			{
				if (null != _LabBlip)
				{
					PlotOnMap(_LabBlip, gameObject.transform.position);
				}
				continue;
			}
			if (null != gameObject.GetComponent<ObTriggerGauntlet>())
			{
				if (null != _TargetPracticeBlip)
				{
					PlotOnMap(_TargetPracticeBlip, gameObject.transform.position);
				}
				continue;
			}
			if (item == _FarmingPortalName)
			{
				if (null != _FarmingBlip)
				{
					PlotOnMap(_FarmingBlip, gameObject.transform.position);
				}
				continue;
			}
			if (item == _RacingPortalName)
			{
				if (null != _RacingBlip)
				{
					PlotOnMap(_RacingBlip, gameObject.transform.position);
				}
				continue;
			}
			if (item == "PfDragonTaxiSelectTrigger")
			{
				if (null != _DragonTaxiBlip)
				{
					PlotOnMap(_DragonTaxiBlip, gameObject.transform.position);
				}
				continue;
			}
			if (item.Contains("PfStore"))
			{
				if (null != _StoreBlip)
				{
					PlotOnMap(_StoreBlip, gameObject.transform.position);
				}
				continue;
			}
			string text = "";
			string text2 = "";
			ObTrigger component = gameObject.GetComponent<ObTrigger>();
			if (null != component)
			{
				text = component._LoadLevel;
			}
			else
			{
				ObTriggerDragonCheck component2 = gameObject.GetComponent<ObTriggerDragonCheck>();
				if (null != component2)
				{
					text = component2._LoadLevel;
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				text = component._LoadLevel;
			}
			text2 = GetDisplayName(text);
			if (text.Contains("FlightSchoolDO"))
			{
				if (null != _FlightSchoolBlip)
				{
					PlotOnMap(_FlightSchoolBlip, gameObject.transform.position, text2);
				}
			}
			else if (null != _PortalBlip)
			{
				PlotOnMap(_PortalBlip, gameObject.transform.position, text2);
			}
		}
	}

	private void ShowMiniGameIcons()
	{
		FishingZone[] array = UnityEngine.Object.FindObjectsOfType(typeof(FishingZone)) as FishingZone[];
		for (int i = 0; i < array.Length; i++)
		{
			if (null != _FishingBlip)
			{
				PlotOnMap(_FishingBlip, array[i].transform.position);
			}
		}
	}

	public Vector2 PositionOnMap(Vector3 pos)
	{
		if (_MapBounds != null && _MapWidget != null)
		{
			Vector3 vector = pos - _MapBounds.transform.position;
			Vector2 result = vector;
			result.x = _MapWidget.position.x + vector.x / _MapBounds.size.x * _MapWidget.localScale.x;
			result.y = _MapWidget.position.y + vector.z / _MapBounds.size.z * _MapWidget.localScale.y;
			return result;
		}
		Debug.LogWarning("Bounding Box Required for SceneMap & a Map Assigned");
		return Vector2.zero;
	}

	public void PlotOnMap(KAWidget item, Vector3 pos, string text = null)
	{
		if (item != null)
		{
			mItemWidget = DuplicateWidget(item);
			mItemWidget.SetVisibility(inVisible: true);
			mItemWidget.transform.parent = item.transform.parent;
			AddWidget(mItemWidget);
			mClonedItems.Add(mItemWidget);
		}
		if (_MapBounds != null && _MapWidget != null && mItemWidget != null)
		{
			Vector2 vector = PositionOnMap(pos);
			mItemWidget.SetPosition(vector.x, vector.y);
		}
		if (!string.IsNullOrEmpty(text))
		{
			mItemWidget.SetText(text);
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
			AvAvatar.pInputEnabled = false;
			if ((bool)AvAvatar.pObject)
			{
				mPlayerPos = PositionOnMap(AvAvatar.position);
				_PlayerBlip.SetPosition(mPlayerPos.x, mPlayerPos.y);
				AvAvatar.pState = AvAvatarState.PAUSED;
			}
		}
		if (show)
		{
			ShowQuestIcons();
			ShowMiniGameIcons();
			if (_PlayerBlip != null)
			{
				mEulerAngles.z = 180f - AvAvatar.mTransform.localEulerAngles.y;
				_PlayerBlip.transform.localEulerAngles = mEulerAngles;
			}
		}
		else
		{
			for (int i = 0; i < mClonedItems.Count; i++)
			{
				RemoveWidget(mClonedItems[i]);
			}
			mClonedItems.Clear();
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if ("BtnClose" == inWidget.name)
		{
			Show(show: false);
			AvAvatar.pInputEnabled = true;
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			Cleanup();
		}
		else if ("BtnToggle" == inWidget.name)
		{
			if (!string.IsNullOrEmpty(_WorldMapBundle))
			{
				Show(show: false);
				AvAvatar.SetUIActive(inActive: false);
				AvAvatar.pState = AvAvatarState.PAUSED;
				KAUICursorManager.SetDefaultCursor("Loading");
				string[] array = _WorldMapBundle.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnWorldMapLoaded, typeof(GameObject));
			}
		}
		else if ("BtnHelp" == inWidget.name)
		{
			KAWidget kAWidget = FindItem("MapLegend");
			if (null != kAWidget)
			{
				kAWidget.SetVisibility(!kAWidget.GetVisibility());
			}
			KAWidget kAWidget2 = FindItem("LegendTitle");
			if (null != kAWidget2)
			{
				kAWidget2.SetVisibility(!kAWidget2.GetVisibility());
			}
		}
		else if ("LegendTitle" == inWidget.name)
		{
			KAWidget kAWidget3 = FindItem("MapLegend");
			if (null != kAWidget3)
			{
				kAWidget3.SetVisibility(!kAWidget3.GetVisibility());
			}
		}
	}

	public void OnWorldMapLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiWorldMap";
			UiWorldMap component = obj.GetComponent<UiWorldMap>();
			if (component != null)
			{
				component._MiniMap = _MiniMap;
				component._SceneMap = this;
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	public void Cleanup()
	{
		RsResourceManager.Unload(_MiniMap._SceneMapBGTextureBundle);
	}

	public void SetBGTexture(Texture texture)
	{
		KAWidget kAWidget = FindItem("SceneMap");
		if (null != kAWidget)
		{
			kAWidget.SetTexture(texture, inPixelPerfect: true);
		}
	}
}
