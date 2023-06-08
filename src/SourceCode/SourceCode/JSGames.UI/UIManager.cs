using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JSGames.UI;

[RequireComponent(typeof(EventSystem))]
[RequireComponent(typeof(StandaloneInputModule))]
public class UIManager : Singleton<UIManager>
{
	public struct ExclusiveData
	{
		public UI _UI;

		public int _PreviousSortingOrder;

		public bool _PreviousOverrideSorting;
	}

	private const int BaseSortOrder = 20000;

	public bool _UpdatePixelDragThresold = true;

	public float _ReferenceDPI = 115f;

	public Action<UI> OnSetExclusive;

	public Action<UI> OnRemoveExclusive;

	private static List<ExclusiveData> mExclusiveList = new List<ExclusiveData>();

	public static Action OnExclusiveListUpdated;

	private Dictionary<int, UIWidget> mGlobalMouseOverItem = new Dictionary<int, UIWidget>();

	public static List<ExclusiveData> pExclusiveList => mExclusiveList;

	public Dictionary<int, UIWidget> pGlobalMouseOverItem => mGlobalMouseOverItem;

	public UIWidget GetGlobalMouseOverItem(int fingerID)
	{
		if (mGlobalMouseOverItem.ContainsKey(fingerID))
		{
			return mGlobalMouseOverItem[fingerID];
		}
		return null;
	}

	public void SetGlobalMouseOverItem(int fingetID, UIWidget uIWidget)
	{
		mGlobalMouseOverItem[fingetID] = uIWidget;
		if (Input.touchSupported)
		{
			_ = Input.touchCount;
			_ = 0;
		}
	}

	protected override void Start()
	{
		base.Start();
		UpdateThreshold();
		RsResourceManager.LoadLevelCompleted += ClearExclusiveList;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RsResourceManager.LoadLevelCompleted -= ClearExclusiveList;
	}

	private void ClearExclusiveList(string lastSceneName)
	{
		mExclusiveList.RemoveAll((ExclusiveData item) => item._UI == null);
	}

	private void UpdateThreshold()
	{
		if (_UpdatePixelDragThresold && EventSystem.current != null)
		{
			int pixelDragThreshold = EventSystem.current.pixelDragThreshold;
			EventSystem component = GetComponent<EventSystem>();
			if (component != null)
			{
				pixelDragThreshold = component.pixelDragThreshold;
			}
			_UpdatePixelDragThresold = false;
			EventSystem.current.pixelDragThreshold = (int)((float)pixelDragThreshold * Screen.dpi / _ReferenceDPI);
		}
	}

	public void EnableInput(bool enable)
	{
		EventSystem component = GetComponent<EventSystem>();
		if ((bool)component)
		{
			component.enabled = enable;
		}
	}

	public void AddToExclusiveListOnTop(UI ui)
	{
		int num = 20000;
		if (mExclusiveList.Count > 0)
		{
			num = mExclusiveList[mExclusiveList.Count - 1]._UI.GetComponent<Canvas>().sortingOrder;
		}
		Canvas component = ui.GetComponent<Canvas>();
		ExclusiveData exclusiveData = default(ExclusiveData);
		int num2 = mExclusiveList.FindIndex((ExclusiveData x) => x._UI == ui);
		if (num2 == -1)
		{
			ExclusiveData exclusiveData2 = default(ExclusiveData);
			exclusiveData2._UI = ui;
			exclusiveData2._PreviousSortingOrder = component.sortingOrder;
			exclusiveData2._PreviousOverrideSorting = component.overrideSorting;
			exclusiveData = exclusiveData2;
		}
		else
		{
			exclusiveData = mExclusiveList[num2];
			mExclusiveList.RemoveAt(num2);
		}
		mExclusiveList.Add(exclusiveData);
		OnExclusiveListUpdated?.Invoke();
		component.overrideSorting = true;
		component.sortingOrder = num + 1;
		UI._GlobalExclusiveUI = ui;
		UICamera.ignoreAllEvents = true;
		if (OnSetExclusive != null)
		{
			OnSetExclusive(ui);
		}
	}

	public void RemoveFromExclusiveList(UI ui)
	{
		int num = mExclusiveList.FindIndex((ExclusiveData x) => x._UI == ui);
		if (num != -1)
		{
			ExclusiveData exclusiveData = mExclusiveList[num];
			Canvas component = ui.GetComponent<Canvas>();
			component.overrideSorting = exclusiveData._PreviousOverrideSorting;
			component.sortingOrder = exclusiveData._PreviousSortingOrder;
			mExclusiveList.RemoveAt(num);
			OnExclusiveListUpdated?.Invoke();
			UI._GlobalExclusiveUI = ((mExclusiveList.Count > 0) ? mExclusiveList[mExclusiveList.Count - 1]._UI : null);
			if (OnRemoveExclusive != null)
			{
				OnRemoveExclusive(ui);
			}
		}
		if (mExclusiveList.Count <= 0)
		{
			UICamera.ignoreAllEvents = false;
		}
	}
}
