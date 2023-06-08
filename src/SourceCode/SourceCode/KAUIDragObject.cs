using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KAUIDragObject : UIDragObject
{
	[Serializable]
	public class PositionData
	{
		public Vector3 _Position = Vector3.zero;

		public UIAnchor.Side _Anchor = UIAnchor.Side.Center;

		public bool _IsRemoved;
	}

	public bool _AllowOverlap;

	public bool _AllowDrag;

	public GameObject[] _AllowedToOverlapObjects;

	public KAUIDragObject[] _LinkedObjects;

	public UISlicedSprite _Background;

	public KAWidget _RemoveButton;

	protected Vector3 mPosition = Vector3.zero;

	protected Vector3 mPositionOnLaunch = Vector3.zero;

	protected BoxCollider mCollider;

	protected KAUI mUI;

	protected UIAnchor.Side mAnchor = UIAnchor.Side.Center;

	protected UIAnchor.Side mPrevAnchor = UIAnchor.Side.Center;

	protected int mOverlapCount;

	protected int mTouchCount;

	private bool mIsDirty;

	private bool mSetPosition;

	private bool mIsOverlapping;

	private bool mIsDragging;

	private bool mIsPressed;

	private bool mIsRemoved;

	private bool mReloadOnEnable;

	private bool mIsInRestrictedArea;

	private bool mIsReady;

	public bool pIsDirty
	{
		get
		{
			if (mIsDirty)
			{
				return true;
			}
			return false;
		}
	}

	public bool pIsReady => mIsReady;

	public UIAnchor.Side pAnchor
	{
		get
		{
			return mAnchor;
		}
		set
		{
			if (mAnchor != value)
			{
				mAnchor = value;
				UtUtilities.AttachToAnchor(pUI.gameObject, base.gameObject, mAnchor, createIfNotFound: true);
			}
		}
	}

	public KAUI pUI
	{
		get
		{
			if (mUI == null)
			{
				mUI = UtUtilities.GetComponentInParent(typeof(KAUI), base.gameObject) as KAUI;
			}
			return mUI;
		}
		set
		{
			mUI = value;
		}
	}

	public string pPositionKey
	{
		get
		{
			if (ParentData.pInstance == null || ParentData.pInstance.pUserInfo == null)
			{
				return "BtnPos=" + base.name;
			}
			return "BtnPos=" + ParentData.pInstance.pUserInfo.UserID + base.name;
		}
	}

	public string pDefaultDataKey
	{
		get
		{
			if (ParentData.pInstance == null || ParentData.pInstance.pUserInfo == null)
			{
				return "DefaultBtnPos--" + base.name;
			}
			return "DefaultBtnPos--" + ParentData.pInstance.pUserInfo.UserID + base.name;
		}
	}

	public bool pReloadOnEnable
	{
		get
		{
			return mReloadOnEnable;
		}
		set
		{
			mReloadOnEnable = value;
		}
	}

	private void Awake()
	{
		mUI = UtUtilities.GetComponentInParent(typeof(KAUI), base.gameObject) as KAUI;
		target = base.transform;
		if (_Background == null)
		{
			_Background = base.gameObject.GetComponentInChildren<UISlicedSprite>();
		}
		mCollider = base.gameObject.GetComponent<BoxCollider>();
		if (mCollider == null && _AllowDrag)
		{
			Debug.LogError("No collider attached. Drag n Drop will not work.");
		}
		if (mCollider != null && _AllowDrag)
		{
			Vector3 size = mCollider.size;
			size.z = 50f;
			mCollider.size = size;
		}
		if (_RemoveButton != null)
		{
			_RemoveButton.gameObject.SetActive(value: false);
		}
	}

	private void Start()
	{
		mAnchor = GetCurrentAnchorSide();
		SaveDefaultPosition();
		Load();
	}

	private void Update()
	{
		if (mSetPosition)
		{
			mIsReady = true;
			mSetPosition = false;
			base.transform.localPosition = mPosition;
		}
	}

	protected override void OnPress(bool pressed)
	{
		if (!AllowDrag())
		{
			return;
		}
		base.OnPress(pressed);
		mIsPressed = pressed;
		if (pressed)
		{
			mTouchCount++;
			if (mTouchCount > 1)
			{
				return;
			}
			if (base.rigidbody == null)
			{
				base.gameObject.AddComponent<Rigidbody>();
			}
			base.rigidbody.useGravity = false;
			mPosition = base.transform.localPosition;
			mPrevAnchor = GetCurrentAnchorSide();
			KAUIDragObject[] linkedObjects = _LinkedObjects;
			foreach (KAUIDragObject kAUIDragObject in linkedObjects)
			{
				if (kAUIDragObject != null)
				{
					kAUIDragObject.mPosition = kAUIDragObject.transform.localPosition;
					kAUIDragObject.mPrevAnchor = kAUIDragObject.GetCurrentAnchorSide();
				}
			}
		}
		else
		{
			mTouchCount--;
			mIsDragging = false;
			if (base.rigidbody != null)
			{
				UnityEngine.Object.Destroy(base.rigidbody);
			}
			if ((!_AllowOverlap && mIsOverlapping) || mIsInRestrictedArea || IsOutOfScreen())
			{
				ResetChanges();
			}
			else
			{
				SetPositionDirty();
			}
		}
		UiCustomizeHUD.pInstance.pSelectedObject = (pressed ? this : null);
	}

	public void Save()
	{
		if (mIsDirty)
		{
			mAnchor = GetCurrentAnchorSide();
			mIsDirty = false;
			string text = UtUtilities.SerializeToString(new PositionData
			{
				_Position = base.transform.localPosition,
				_Anchor = mAnchor,
				_IsRemoved = mIsRemoved
			});
			PlayerPrefs.SetString(pPositionKey, text);
			UtDebug.Log("Saving : " + base.name + " -> " + text);
		}
	}

	public void Delete()
	{
		PlayerPrefs.DeleteKey(pPositionKey);
	}

	public void Load()
	{
		if (PlayerPrefs.HasKey(pPositionKey))
		{
			if (UtUtilities.DeserializeFromXml(PlayerPrefs.GetString(pPositionKey), typeof(PositionData)) is PositionData positionData)
			{
				pAnchor = positionData._Anchor;
				mPosition = positionData._Position;
				mPositionOnLaunch = positionData._Position;
				mIsRemoved = positionData._IsRemoved;
				mSetPosition = true;
			}
		}
		else
		{
			mPosition = base.transform.localPosition;
			mIsReady = true;
		}
	}

	protected override void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		base.OnEnable();
		if (mReloadOnEnable)
		{
			mReloadOnEnable = false;
			Load();
		}
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		Load();
		base.enabled = true;
	}

	public void ResetChanges()
	{
		UtUtilities.AttachToAnchor(pUI.gameObject, base.gameObject, mPrevAnchor, createIfNotFound: true);
		base.transform.localPosition = mPosition;
		mOverlapCount = 0;
		mIsOverlapping = false;
		mIsInRestrictedArea = false;
		if (UiCustomizeHUD.pInstance.pSelectedObject == this)
		{
			KAUIDragObject[] linkedObjects = _LinkedObjects;
			foreach (KAUIDragObject kAUIDragObject in linkedObjects)
			{
				if (kAUIDragObject != null)
				{
					kAUIDragObject.ResetChanges();
				}
			}
		}
		ShowRestrictedArea(isRestricted: false);
	}

	public void ResetSession()
	{
		base.transform.localPosition = mPositionOnLaunch;
	}

	public void ShowRestrictedArea(bool isRestricted)
	{
		if (_Background != null)
		{
			if (isRestricted)
			{
				_Background.color = UiCustomizeHUD.pInstance._InvalidColor;
			}
			else
			{
				_Background.color = UiCustomizeHUD.pInstance._NormalColor;
			}
		}
		if (!(this == UiCustomizeHUD.pInstance.pSelectedObject))
		{
			return;
		}
		KAUIDragObject[] linkedObjects = _LinkedObjects;
		foreach (KAUIDragObject kAUIDragObject in linkedObjects)
		{
			if (kAUIDragObject != null)
			{
				kAUIDragObject.ShowRestrictedArea(isRestricted);
			}
		}
	}

	protected override void OnDrag(Vector2 delta)
	{
		if (!mIsPressed || !AllowDrag() || mIsRemoved || delta == Vector2.zero)
		{
			return;
		}
		base.OnDrag(delta);
		mIsDragging = true;
		pAnchor = UtUtilities.GetAnchorSide(base.transform.position);
		if (_LinkedObjects == null)
		{
			return;
		}
		KAUIDragObject[] linkedObjects = _LinkedObjects;
		foreach (KAUIDragObject kAUIDragObject in linkedObjects)
		{
			if (kAUIDragObject != null)
			{
				kAUIDragObject.pAnchor = mAnchor;
				kAUIDragObject.transform.localPosition = base.transform.localPosition;
			}
		}
	}

	private void OnTriggerEnter(Collider coll)
	{
		if (coll.gameObject.layer == base.gameObject.layer && mIsDragging && UiCustomizeHUD.pInstance != null && UiCustomizeHUD.pInstance.pSelectedObject == this && !CanOverlap(coll.gameObject))
		{
			mOverlapCount++;
			mIsOverlapping = true;
			ShowRestrictedArea(isRestricted: true);
			UtDebug.Log(base.name + " Enter : " + coll.name + ", mOverlapCount : " + mOverlapCount);
		}
	}

	private void OnTriggerStay(Collider coll)
	{
		if (coll.gameObject.layer == base.gameObject.layer && mIsDragging && UiCustomizeHUD.pInstance != null && UiCustomizeHUD.pInstance.pSelectedObject == this && !CanOverlap(coll.gameObject))
		{
			mIsInRestrictedArea = true;
		}
	}

	private void OnTriggerExit(Collider coll)
	{
		if (coll.gameObject.layer == base.gameObject.layer && UiCustomizeHUD.pInstance != null && UiCustomizeHUD.pInstance.pSelectedObject == this && !CanOverlap(coll.gameObject))
		{
			mIsInRestrictedArea = false;
			mOverlapCount--;
			if (mOverlapCount <= 0)
			{
				mIsOverlapping = false;
				ShowRestrictedArea(isRestricted: false);
			}
			UtDebug.Log(base.name + " exit : " + coll.name + ", mOverlapCount : " + mOverlapCount);
		}
	}

	private bool CanOverlap(GameObject inObj)
	{
		GameObject[] allowedToOverlapObjects = _AllowedToOverlapObjects;
		foreach (GameObject gameObject in allowedToOverlapObjects)
		{
			if (inObj == gameObject)
			{
				return true;
			}
		}
		if (_RemoveButton != null && inObj == _RemoveButton.gameObject)
		{
			return true;
		}
		KAUIDragObject[] linkedObjects = _LinkedObjects;
		foreach (KAUIDragObject kAUIDragObject in linkedObjects)
		{
			if (kAUIDragObject._RemoveButton != null && inObj == kAUIDragObject._RemoveButton.gameObject)
			{
				return true;
			}
		}
		return false;
	}

	private UIAnchor.Side GetCurrentAnchorSide()
	{
		return (UtUtilities.GetComponentInParent(typeof(UIAnchor), base.gameObject) as UIAnchor).side;
	}

	private void SetPositionDirty()
	{
		mTouchCount = 0;
		mIsDirty = true;
		KAUIDragObject[] linkedObjects = _LinkedObjects;
		for (int i = 0; i < linkedObjects.Length; i++)
		{
			linkedObjects[i].mIsDirty = true;
		}
	}

	private bool AllowDrag()
	{
		if (KAUI._GlobalExclusiveUI != null)
		{
			return false;
		}
		if (UiCustomizeHUD.pInstance.pSelectedObject == null || UiCustomizeHUD.pInstance.pSelectedObject == this)
		{
			return true;
		}
		return false;
	}

	private void SaveDefaultPosition()
	{
		if (!PlayerPrefs.HasKey(pDefaultDataKey))
		{
			string value = UtUtilities.SerializeToString(new PositionData
			{
				_Position = base.transform.localPosition,
				_Anchor = mAnchor,
				_IsRemoved = mIsRemoved
			});
			PlayerPrefs.SetString(pDefaultDataKey, value);
		}
	}

	public void SetDefaultPosition()
	{
		if (PlayerPrefs.HasKey(pDefaultDataKey))
		{
			if (!(UtUtilities.DeserializeFromXml(PlayerPrefs.GetString(pDefaultDataKey), typeof(PositionData)) is PositionData positionData))
			{
				return;
			}
			pAnchor = positionData._Anchor;
			mPosition = positionData._Position;
			mPositionOnLaunch = positionData._Position;
			mIsRemoved = positionData._IsRemoved;
			mSetPosition = true;
			if (mIsRemoved)
			{
				if (_RemoveButton != null)
				{
					_RemoveButton.SetVisibility(mIsRemoved);
					_Background.color = UiCustomizeHUD.pInstance._RemovedColor;
				}
			}
			else
			{
				if (_RemoveButton != null)
				{
					_RemoveButton.SetVisibility(inVisible: false);
				}
				_Background.color = UiCustomizeHUD.pInstance._NormalColor;
			}
			mIsDirty = true;
		}
		else
		{
			Debug.LogError("Default positions are not saved for : " + base.name);
		}
	}

	public bool IsOutOfScreen()
	{
		Vector3 screenPosition = UtUtilities.GetScreenPosition(base.transform.position);
		if (screenPosition.x > (float)Screen.width || screenPosition.x < 0f)
		{
			return true;
		}
		if (screenPosition.y > (float)Screen.height || screenPosition.y < 0f)
		{
			return true;
		}
		return false;
	}
}
