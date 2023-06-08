using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ObContextSensitive : KAMonoBase
{
	public delegate void BundleLoadSuccess();

	public bool _Active = true;

	public bool _CheckForRenderer = true;

	public List<ContextData> _DataList = new List<ContextData>();

	public string _UIBundle = "RS_DATA/PfUiContextSensitive.unity3d/PfUiContextSensitive";

	public ContextSensitivePriority[] _PriorityList;

	public ContextSensitiveUIStyleData _UIStyleData;

	public Vector3 _Position2D = new Vector3(0f, -256f, 0f);

	public Vector3 _SafeAreaMultiplier = Vector3.zero;

	public GameObject _ClickableRangeTargetObj;

	public float _ClickableRangeDist = -1f;

	public GameObject _UIFollowingTarget;

	public Camera _MainCamera;

	public float _ProximityRange = 2f;

	public Vector3 _ProximityOffset = Vector3.zero;

	public bool _DrawProximity;

	public bool _MenuBackgroundVisibility = true;

	public string _MenuBackgroundSpriteName;

	public Color _MenuBackgroundColor = new Color(1f, 1f, 1f, 0.4f);

	public bool _UseMenuForParentCategory;

	public bool _UseMenuForSubCategory;

	private UiContextSensitive mUI;

	private bool mBundleLoadInitialized;

	private ContextSensitiveStateType mCurrentPriority;

	protected bool mProximityAlreadyEntered;

	private float mProximityDistance;

	private ContextSensitiveManager mManager;

	private static UiContextSensitive mExclusiveUI;

	public static BundleLoadSuccess _OnBundleLoadSuccess;

	public UiContextSensitive pUI => mUI;

	public ContextSensitiveStateType pCurrentPriority => mCurrentPriority;

	public float pProximityDistance => mProximityDistance;

	public static UiContextSensitive pExclusiveUI
	{
		get
		{
			return mExclusiveUI;
		}
		set
		{
			mExclusiveUI = value;
		}
	}

	protected virtual void Awake()
	{
		GameObject gameObject = GameObject.Find("ContextSensitiveManager");
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfContextSensitiveManager"));
			gameObject.name = "ContextSensitiveManager";
		}
		mManager = gameObject.GetComponent<ContextSensitiveManager>();
	}

	protected virtual void OnEnable()
	{
		if (mManager != null)
		{
			mManager.Add(this);
		}
	}

	protected virtual void OnDisable()
	{
		if (mManager != null)
		{
			mManager.Remove(this);
		}
	}

	protected virtual void Start()
	{
		_UIFollowingTarget = base.gameObject;
		UpdateChildrenData();
		if (_MainCamera == null)
		{
			_MainCamera = Camera.main;
		}
		if (_PriorityList.Length == 0)
		{
			_PriorityList = new ContextSensitivePriority[3];
			_PriorityList[0] = new ContextSensitivePriority(ContextSensitiveStateType.ONCLICK);
			_PriorityList[1] = new ContextSensitivePriority(ContextSensitiveStateType.PROXIMITY);
			_PriorityList[2] = new ContextSensitivePriority(ContextSensitiveStateType.OBJECT_STATE);
		}
		Array.Resize(ref _PriorityList, _PriorityList.Length + 1);
		_PriorityList[_PriorityList.Length - 1] = new ContextSensitivePriority(ContextSensitiveStateType.NONE);
		if (_ClickableRangeTargetObj == null)
		{
			_ClickableRangeTargetObj = AvAvatar.pObject;
		}
	}

	protected virtual void Update()
	{
		if (!KAInput.pInstance.IsTouchInput() && KAInput.GetMouseButtonUp(0) && mUI != null && KAUIManager.pInstance != null && mCurrentPriority == ContextSensitiveStateType.ONCLICK)
		{
			ProcessForClickOutside();
		}
		if (AvAvatar.pObject != null)
		{
			mProximityDistance = (AvAvatar.position - (base.transform.position + base.transform.TransformDirection(_ProximityOffset))).magnitude;
			if (mProximityDistance > _ProximityRange)
			{
				OnProximityExit();
			}
			else if (!mProximityAlreadyEntered && (!_CheckForRenderer || IsObjectInCameraView()))
			{
				OnProximityEnter();
			}
			else if (_CheckForRenderer && !IsObjectInCameraView())
			{
				OnProximityIfEntered();
			}
		}
		if (mCurrentPriority == ContextSensitiveStateType.NONE && IsAllowed(ContextSensitiveStateType.OBJECT_STATE))
		{
			ContextSensitivePriority priority = GetPriority(ContextSensitiveStateType.OBJECT_STATE);
			UpdateStateData(priority);
		}
		if (!_Active || mBundleLoadInitialized)
		{
			return;
		}
		ContextSensitivePriority[] priorityList = _PriorityList;
		foreach (ContextSensitivePriority contextSensitivePriority in priorityList)
		{
			if (mCurrentPriority != contextSensitivePriority._Type && contextSensitivePriority.pData != null)
			{
				if (contextSensitivePriority._Type == ContextSensitiveStateType.ONCLICK)
				{
					mManager.RemoveOtherMenusOfType(ContextSensitiveStateType.ONCLICK, this);
				}
				ClearOtherPriorityData(contextSensitivePriority);
				DestroyMenu(checkProximity: false);
				SetCurrentPriority(contextSensitivePriority._Type);
				LoadBundle();
				break;
			}
		}
	}

	private void ClearOtherPriorityData(ContextSensitivePriority inPriority)
	{
		ContextSensitivePriority[] priorityList = _PriorityList;
		foreach (ContextSensitivePriority contextSensitivePriority in priorityList)
		{
			if (contextSensitivePriority != inPriority)
			{
				contextSensitivePriority.pData = null;
			}
		}
	}

	protected virtual void OnProximityEnter()
	{
		if (IsAllowed(ContextSensitiveStateType.PROXIMITY) && mManager.IsNearestProximityObject(this))
		{
			SetProximityAlreadyEntered(isEntered: true);
			ContextSensitivePriority priority = GetPriority(ContextSensitiveStateType.PROXIMITY);
			UpdateStateData(priority);
		}
	}

	protected virtual void OnProximityExit()
	{
		SetProximityAlreadyEntered(isEntered: false);
		if (mCurrentPriority == ContextSensitiveStateType.PROXIMITY)
		{
			DestroyMenu(checkProximity: false);
		}
	}

	private void OnProximityIfEntered()
	{
		if (mCurrentPriority == ContextSensitiveStateType.PROXIMITY)
		{
			DestroyMenu(checkProximity: false);
			SetProximityAlreadyEntered(isEntered: false);
		}
	}

	private bool IsObjectInCameraView()
	{
		Renderer componentInChildren = GetComponentInChildren<Renderer>();
		if (componentInChildren != null)
		{
			return componentInChildren.isVisible;
		}
		return false;
	}

	protected virtual void OnActivate()
	{
		if (!IsAllowed(ContextSensitiveStateType.ONCLICK))
		{
			return;
		}
		if (_ClickableRangeTargetObj != null)
		{
			Vector3 vector = _ClickableRangeTargetObj.transform.position - base.transform.position;
			if (_ClickableRangeDist > -1f && vector.magnitude > _ClickableRangeDist)
			{
				return;
			}
		}
		ContextSensitivePriority priority = GetPriority(ContextSensitiveStateType.ONCLICK);
		UpdateStateData(priority);
		KAInput.ResetInputAxes();
	}

	private void LoadBundle()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		mBundleLoadInitialized = true;
		string[] array = _UIBundle.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnBundleLoaded, typeof(GameObject));
	}

	private void OnBundleLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (mCurrentPriority == ContextSensitiveStateType.NONE)
			{
				mBundleLoadInitialized = false;
			}
			else if (_Active)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject, ContextSensitiveManager.pCSMOffScreenPosition, Quaternion.identity);
				gameObject.name = base.gameObject.name + "_ContextMenuUI";
				mUI = gameObject.GetComponent<UiContextSensitive>();
				mUI.pUIStyle = CreateContextSensitiveUIWithStyle(_UIStyleData);
				mUI.pCurrentStateType = mCurrentPriority;
				mUI.pContextSensitiveObj = this;
				mUI.pUseMenuForParentCategory = _UseMenuForParentCategory;
				mUI.pUseMenuForSubCategory = _UseMenuForSubCategory;
				ContextSensitivePriority priority = GetPriority(mCurrentPriority);
				ContextSensitiveState contextSensitiveState = priority?.pData;
				if (contextSensitiveState != null && contextSensitiveState._CurrentContextNamesList != null && contextSensitiveState._CurrentContextNamesList.Length != 0)
				{
					AddDataToUIByState(contextSensitiveState);
				}
				else
				{
					UtDebug.Log("No Context names provided for type:" + priority._Type.ToString() + " for object name:" + base.gameObject.name);
				}
				OnMenuActive(priority._Type);
				priority.pData = null;
				mBundleLoadInitialized = false;
				_OnBundleLoadSuccess?.Invoke();
			}
			break;
		case RsResourceLoadEvent.ERROR:
			mBundleLoadInitialized = false;
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	protected virtual void ProcessForClickOutside()
	{
		KAWidget pSelectedWidget = KAUIManager.pInstance.pSelectedWidget;
		bool flag = false;
		if (null != pSelectedWidget)
		{
			foreach (GameObject pMenu in mUI.pMenuList)
			{
				if (pMenu == null)
				{
					continue;
				}
				UiContextSensetiveMenu componentInChildren = pMenu.GetComponentInChildren<UiContextSensetiveMenu>();
				if (componentInChildren.transform.parent == null)
				{
					continue;
				}
				KAScrollBar componentInChildren2 = componentInChildren.transform.parent.GetComponentInChildren<KAScrollBar>();
				if (null != componentInChildren2)
				{
					if (pSelectedWidget == componentInChildren2._DownArrow || pSelectedWidget == componentInChildren2._UpArrow)
					{
						flag = true;
					}
					else if (null != pSelectedWidget.transform.parent && pSelectedWidget.transform.parent.gameObject.GetInstanceID() == componentInChildren2.gameObject.GetInstanceID())
					{
						flag = true;
					}
				}
				if (!flag)
				{
					ContextWidgetUserData contextWidgetUserData = (ContextWidgetUserData)pSelectedWidget.GetUserData();
					if (contextWidgetUserData?._Data != null && contextWidgetUserData._Data.pTarget == base.gameObject)
					{
						flag = true;
					}
				}
			}
		}
		if ((pSelectedWidget == null && _UIFollowingTarget != ObClickable.pMouseOverObject) || (pSelectedWidget != null && mUI != pSelectedWidget.pUI && !flag))
		{
			DestroyMenu(checkProximity: false);
		}
	}

	private void AddDataToUIByState(ContextSensitiveState inState)
	{
		if (mUI != null)
		{
			mUI.pOffsetPos = inState._OffsetPos;
			mUI.pIs3DUI = inState._Enable3DUI;
			mUI.transform.localScale = inState._UIScale;
			AddDataToUI(inState._CurrentContextNamesList, inState._ShowCloseButton);
			string layerName = (inState._Enable3DUI ? "3DNGUI" : "2DNGUI");
			UtUtilities.SetLayerRecursively(mUI.gameObject, LayerMask.NameToLayer(layerName));
		}
		else
		{
			Debug.LogError("UI that is being called is null:" + base.gameObject.name);
		}
	}

	private void AddDataToUI(string[] contextNamesArr, bool inShowCloseBtn)
	{
		if (mUI == null)
		{
			return;
		}
		List<ContextData> list = new List<ContextData>();
		foreach (string inName in contextNamesArr)
		{
			ContextData contextData = GetContextData(inName);
			if (contextData != null)
			{
				SetContextTargetMessageobject(contextData);
				list.Add(contextData);
			}
		}
		if (list.Count > 0)
		{
			mUI.pMainCamera = _MainCamera;
			mUI.gameObject.SetActive(value: true);
			mUI.AddContextDataList(list.ToArray(), enableRefreshItems: true);
			mUI.ShowClose(inShowCloseBtn);
		}
		else
		{
			Debug.LogError("No ContextData added with associated names for object named:" + base.gameObject.name);
		}
	}

	private void AddDataToUI(string inContextName)
	{
		if (!(mUI == null))
		{
			ContextData contextData = GetContextData(inContextName);
			if (contextData != null)
			{
				SetContextTargetMessageobject(contextData);
				mUI.AddContextDataIntoList(contextData, enableRefreshItems: true);
			}
		}
	}

	protected void Refresh()
	{
		ContextSensitivePriority priority = GetPriority(mCurrentPriority);
		if (priority != null)
		{
			UpdateStateData(priority);
			if (priority?.pData != null && priority.pData._CurrentContextNamesList.Length != 0)
			{
				AddDataToUIByState(priority.pData);
			}
		}
	}

	private void UpdateStateData(ContextSensitivePriority inPriorityMenuType)
	{
		if (inPriorityMenuType == null)
		{
			return;
		}
		ContextSensitiveState[] inStatesArrData = new ContextSensitiveState[3];
		for (int i = 0; i < inStatesArrData.Length; i++)
		{
			ContextSensitiveState inData = new ContextSensitiveState();
			SetDefaultInitialization(ref inData);
			inStatesArrData[i] = inData;
		}
		UpdateData(ref inStatesArrData);
		ContextSensitiveState inData2 = null;
		if (inStatesArrData != null)
		{
			int j = 0;
			for (int num = inStatesArrData.Length; j < num; j++)
			{
				ContextSensitiveState contextSensitiveState = inStatesArrData[j];
				if (contextSensitiveState._MenuType == inPriorityMenuType._Type)
				{
					inData2 = contextSensitiveState;
					break;
				}
			}
		}
		if (inData2 != null && inData2._CurrentContextNamesList != null && inData2._CurrentContextNamesList.Length != 0)
		{
			PostInitializationTemp(ref inData2);
			inPriorityMenuType.pData = inData2;
		}
		else if (mCurrentPriority != 0)
		{
			SetCurrentPriority(ContextSensitiveStateType.NONE);
			inPriorityMenuType.pData = null;
			DestroyMenu(checkProximity: false);
		}
	}

	private void SetDefaultInitialization(ref ContextSensitiveState inData)
	{
		inData._CurrentContextNamesList = null;
		inData._Enable3DUI = true;
		inData._ShowCloseButton = true;
	}

	private void PostInitializationTemp(ref ContextSensitiveState inData)
	{
		if (inData._UIScale.magnitude == 0f)
		{
			inData._UIScale = (inData._Enable3DUI ? (Vector3.one * KAUIManager.pInstance._2Dto3DScaleFactor) : Vector3.one);
		}
		else if (inData._Enable3DUI)
		{
			inData._UIScale = (Vector3.one + inData._UIScale) * KAUIManager.pInstance._2Dto3DScaleFactor;
		}
		if (inData._OffsetPos.magnitude == 0f)
		{
			inData._OffsetPos = (inData._Enable3DUI ? new Vector3(0f, 0.8f, 0f) : GetSafeAreaPosition());
		}
	}

	private Vector3 GetSafeAreaPosition()
	{
		Vector3 zero = Vector3.zero;
		zero.x = _Position2D.x + Screen.safeArea.xMin * _SafeAreaMultiplier.x;
		zero.y = _Position2D.y + Screen.safeArea.yMin * _SafeAreaMultiplier.y;
		zero.x = _Position2D.x - ((float)Screen.width - Screen.safeArea.xMax) * _SafeAreaMultiplier.z;
		return zero;
	}

	protected virtual void UpdateData(ref ContextSensitiveState[] inStatesArrData)
	{
	}

	public void CloseMenu(bool checkProximity = false)
	{
		DestroyMenu(checkProximity);
	}

	public void RemoveMenuOfType(ContextSensitiveStateType inStateType)
	{
		if (mCurrentPriority == inStateType && !(mUI == null))
		{
			GetPriority(inStateType).pData = null;
			DestroyMenu(checkProximity: false);
		}
	}

	protected virtual void DestroyMenu(bool checkProximity)
	{
		SetCurrentPriority(ContextSensitiveStateType.NONE);
		if (mUI != null)
		{
			UnityEngine.Object.Destroy(mUI.gameObject);
			mUI = null;
			pExclusiveUI = null;
		}
		if (checkProximity && (AvAvatar.position - (base.transform.position + base.transform.TransformDirection(_ProximityOffset))).magnitude < _ProximityRange)
		{
			SetProximityAlreadyEntered(isEntered: true);
		}
	}

	public void SetProximityAlreadyEntered(bool isEntered)
	{
		mProximityAlreadyEntered = isEntered;
	}

	public void SetInteractiveEnabledData(string itemName, bool isEnabled)
	{
		ContextData contextData = GetContextData(itemName);
		if (contextData != null)
		{
			contextData.pEnabled = isEnabled;
		}
	}

	protected bool GetInteractiveEnabledData(string itemName)
	{
		return GetContextData(itemName)?.pEnabled ?? false;
	}

	protected virtual void OnMenuActive(ContextSensitiveStateType inMenuType)
	{
		pExclusiveUI = mUI;
	}

	protected virtual void SetContextTargetMessageobject(ContextData inContextData)
	{
		inContextData.pTarget = base.gameObject;
		if (inContextData.pChildrenDataList == null)
		{
			return;
		}
		foreach (ContextData pChildrenData in inContextData.pChildrenDataList)
		{
			SetContextTargetMessageobject(pChildrenData);
		}
	}

	private IContextSensitiveStyle CreateContextSensitiveUIWithStyle(ContextSensitiveUIStyleData inStyleData)
	{
		if (inStyleData._Type == UI_STYLE_TYPE.DEFAULT_STYLE)
		{
			return new LabelButtonsContextSensitiveStyle(inStyleData);
		}
		Debug.LogError("No Style found");
		return null;
	}

	protected void AddIntoDataList(string inName, string inDisplayName, ContextData inTemplateData)
	{
		ContextData contextData = (ContextData)inTemplateData.Clone();
		contextData._Name = inName;
		contextData._DisplayName._Text = inDisplayName;
		AddIntoDataList(contextData);
	}

	protected void AddIntoDataList(ContextData inData)
	{
		if (!_DataList.Contains(inData) && GetContextData(inData._Name) == null)
		{
			_DataList.Add(inData);
			UpdateChildrenData();
		}
	}

	protected void AddChildIntoDataListItem(ContextData inParentData, string inChildName)
	{
		inParentData?.AddChildName(inChildName);
	}

	protected void RemoveFromDataList(string inName)
	{
		ContextData contextData = GetContextData(inName);
		if (contextData != null)
		{
			_DataList.Remove(contextData);
		}
	}

	protected void UpdateChildrenData()
	{
		foreach (ContextData data in _DataList)
		{
			data.pEnabled = true;
			data.pIsChildOpened = false;
			data.pChildrenDataList.Clear();
			if (data._ChildrenNames == null)
			{
				continue;
			}
			string[] childrenNames = data._ChildrenNames;
			foreach (string inName in childrenNames)
			{
				ContextData contextData = GetContextData(inName);
				if (contextData != null)
				{
					data.pChildrenDataList.Add(contextData);
					contextData.pParent = data;
					if (contextData.pTarget == null)
					{
						contextData.pTarget = data.pTarget;
					}
				}
			}
		}
	}

	protected ContextData GetContextData(string inName)
	{
		return _DataList.FirstOrDefault((ContextData contextData) => contextData._Name == inName);
	}

	private ContextSensitivePriority GetPriority(ContextSensitiveStateType inType)
	{
		if (_PriorityList.Length != 0)
		{
			return _PriorityList.FirstOrDefault((ContextSensitivePriority priority) => priority._Type == inType);
		}
		return null;
	}

	private void SetCurrentPriority(ContextSensitiveStateType inCurrStateType)
	{
		mCurrentPriority = inCurrStateType;
	}

	private int GetIndexInPriorityList(ContextSensitiveStateType inPriorityType)
	{
		for (int i = 0; i < _PriorityList.Length; i++)
		{
			if (_PriorityList[i]._Type == inPriorityType)
			{
				return i;
			}
		}
		return -1;
	}

	protected virtual bool IsAllowed(ContextSensitiveStateType inPriorityType)
	{
		int indexInPriorityList = GetIndexInPriorityList(mCurrentPriority);
		int indexInPriorityList2 = GetIndexInPriorityList(inPriorityType);
		if (indexInPriorityList == -1 || indexInPriorityList2 == -1)
		{
			return false;
		}
		return indexInPriorityList2 < indexInPriorityList;
	}

	protected virtual void OnDestroy()
	{
		DestroyMenu(checkProximity: false);
		if (mManager != null)
		{
			mManager.Remove(this);
		}
	}

	private void OnDrawGizmos()
	{
		if (_DrawProximity && _ProximityRange != 0f)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(base.transform.position + base.transform.TransformDirection(_ProximityOffset), _ProximityRange);
		}
	}
}
