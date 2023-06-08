using UnityEngine;

public class UiCustomizeHUD : KAUI
{
	public GameObject _UICamera;

	public Transform[] _MobileOnlyButtons;

	public Transform[] _WebOnlyButtons;

	public Color _InvalidColor = Color.red;

	public Color _NormalColor = Color.white;

	public Color _RemovedColor = new Color(1f, 1f, 1f, 0.5f);

	private KAUIDragObject[] mDragObjects;

	private KAUIDragObject mSelectedObject;

	private bool mAvatarButtonsState = true;

	private static GameObject mExitMessageObject = null;

	private static string mCallbackFunction = "OnCustomizeHUDClosed";

	private static UiCustomizeHUD mInstance = null;

	public static UiCustomizeHUD pInstance => mInstance;

	public KAUIDragObject[] pDragObjects => mDragObjects;

	public KAUIDragObject pSelectedObject
	{
		get
		{
			return mSelectedObject;
		}
		set
		{
			mSelectedObject = value;
		}
	}

	protected override void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
		}
		else
		{
			Debug.LogError("Error: There should be only one instance of UiCustomizeHUD class.");
		}
		if (_UICamera != null && string.IsNullOrEmpty(RsResourceManager.pLastLevel))
		{
			_UICamera.SetActive(value: true);
		}
		base.Awake();
		bool flag = UtPlatform.IsMobile();
		if (UtPlatform.IsWSA())
		{
			flag = !UtUtilities.IsKeyboardAttached();
		}
		if (_MobileOnlyButtons != null)
		{
			Transform[] mobileOnlyButtons = _MobileOnlyButtons;
			foreach (Transform transform in mobileOnlyButtons)
			{
				if (transform != null)
				{
					transform.gameObject.SetActive(flag);
				}
			}
		}
		if (_WebOnlyButtons != null)
		{
			Transform[] mobileOnlyButtons = _WebOnlyButtons;
			foreach (Transform transform2 in mobileOnlyButtons)
			{
				if (transform2 != null)
				{
					transform2.gameObject.SetActive(!flag);
				}
			}
		}
		if (!RsResourceManager.pCurrentLevel.Equals(GameConfig.GetKeyData("CustomizeHUDScene")) && UiAvatarControls.pInstance != null)
		{
			mAvatarButtonsState = UiAvatarControls.pInstance.gameObject.activeSelf;
			UiAvatarControls.pInstance.gameObject.SetActive(value: false);
		}
	}

	protected override void Start()
	{
		base.Start();
		mDragObjects = base.transform.GetComponentsInChildren<KAUIDragObject>();
		RsResourceManager.DestroyLoadScreen();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnBack" && mSelectedObject == null)
		{
			if (HaveUnsavedChanges())
			{
				EnableDrag(isDragEnabled: false);
				Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUICustomizeHUDExitDB"));
			}
			else
			{
				OnQuitWithoutSaving();
			}
		}
		else if (inWidget.name == "BtnApply")
		{
			Save();
		}
		else if (inWidget.name == "BtnDefaults")
		{
			SetDefaultPositions();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		mDragObjects = null;
	}

	public void SetDefaultPositions()
	{
		KAUIDragObject[] array = mDragObjects;
		foreach (KAUIDragObject kAUIDragObject in array)
		{
			if (kAUIDragObject != null)
			{
				kAUIDragObject.SetDefaultPosition();
			}
		}
	}

	public void OnQuitWithoutSaving()
	{
		UtDebug.Log("OnQuitWithoutSaving");
		if (RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("CustomizeHUDScene"))
		{
			AvAvatar.pStartLocation = AvAvatar.pSpawnAtSetPosition;
			RsResourceManager.LoadLevel(RsResourceManager.pLastLevel);
			return;
		}
		if (AvAvatar.pToolbar != null)
		{
			KAUIDragObject[] array = Resources.FindObjectsOfTypeAll(typeof(KAUIDragObject)) as KAUIDragObject[];
			foreach (KAUIDragObject kAUIDragObject in array)
			{
				if (kAUIDragObject != null && !kAUIDragObject._AllowDrag)
				{
					kAUIDragObject.enabled = true;
					kAUIDragObject.pReloadOnEnable = true;
				}
			}
		}
		Object.Destroy(base.gameObject);
		if (mExitMessageObject != null)
		{
			mExitMessageObject.SendMessage(mCallbackFunction, SendMessageOptions.DontRequireReceiver);
		}
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.gameObject.SetActive(mAvatarButtonsState);
		}
	}

	public void OnSaveAndQuit()
	{
		UtDebug.Log("OnSaveAndQuit");
		Save();
		OnQuitWithoutSaving();
	}

	public void Save()
	{
		UtDebug.Log("Saving Custom UI positions...");
		KAUIDragObject[] array = mDragObjects;
		foreach (KAUIDragObject kAUIDragObject in array)
		{
			if (kAUIDragObject != null)
			{
				kAUIDragObject.Save();
			}
		}
	}

	public void ResetSession()
	{
		UtDebug.Log("Resetting the session positions...");
		KAUIDragObject[] array = mDragObjects;
		foreach (KAUIDragObject kAUIDragObject in array)
		{
			if (kAUIDragObject != null)
			{
				kAUIDragObject.ResetSession();
			}
		}
	}

	private bool HaveUnsavedChanges()
	{
		KAUIDragObject[] array = mDragObjects;
		foreach (KAUIDragObject kAUIDragObject in array)
		{
			if (kAUIDragObject != null && kAUIDragObject.pIsDirty)
			{
				return true;
			}
		}
		return false;
	}

	public static void Load(GameObject inMessageObj, UILoadOptions inLoadOption = UILoadOptions.AUTO)
	{
		mExitMessageObject = inMessageObj;
		if (UtMobileUtilities.CanLoadInCurrentScene(UiType.CustomizeHUD, inLoadOption))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = GameConfig.GetKeyData("CustomizeHUDAsset").Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnCustomizationHUDLoaded, typeof(GameObject));
			return;
		}
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			if (component.pSubState == AvAvatarSubState.UWSWIMMING)
			{
				AvAvatar.pStartPosition = AvAvatar.GetPosition();
			}
			else if (component.IsValidLastPositionOnGround())
			{
				AvAvatar.pStartPosition = component.pLastPositionOnGround;
			}
			else
			{
				AvAvatar.pStartLocation = null;
			}
			AvAvatar.pStartRotation = AvAvatar.mTransform.rotation;
		}
		if (AvAvatar.pObject != null)
		{
			AvAvatar.pObject.transform.position = Vector3.up * -5000f;
		}
		RsResourceManager.LoadLevel(GameConfig.GetKeyData("CustomizeHUDScene"));
	}

	private static void OnCustomizationHUDLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			Object.Instantiate((GameObject)inObject).name = "PfUiCustomizeHUD";
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (mExitMessageObject != null)
			{
				mExitMessageObject.SendMessage(mCallbackFunction, SendMessageOptions.DontRequireReceiver);
			}
			break;
		}
	}

	public void EnableDrag(bool isDragEnabled)
	{
		KAUIDragObject[] array = mDragObjects;
		foreach (KAUIDragObject kAUIDragObject in array)
		{
			if (kAUIDragObject != null)
			{
				if (kAUIDragObject.collider != null)
				{
					kAUIDragObject.collider.enabled = isDragEnabled;
				}
				if (kAUIDragObject._RemoveButton != null)
				{
					kAUIDragObject._RemoveButton.SetInteractive(isDragEnabled);
				}
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (UiJoystick.pInstance != null && UiJoystick.pInstance.GetVisibility())
		{
			UiJoystick.pInstance.SetVisibility(isVisible: false);
		}
	}
}
