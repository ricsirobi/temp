using UnityEngine;
using UnityEngine.AI;

public class KAUISelectDragon : KAUISelect
{
	public static bool _DoNotRestoreDefault = false;

	public static bool _ResetAnim = true;

	public DragonPartTab[] _Tabs;

	public SanctuaryPet mPet;

	public string _CategoryWidgetName = "TxtCatName";

	public string _DefaultTab = "War Paint";

	public GameObject _CloseMsgObject;

	public string _CloseMsg = "DragonCreatorClosed";

	public string _ExitToScene = "";

	public string _ReturnMarker = "";

	public Transform _DragonStartMarker;

	public string _DragonIdleAnimName = "Idle";

	public float _RotationSpeed = 40f;

	public string _LeftRotateBtn = "BtnAmAvatarScrollLt";

	public string _RightRotateBtn = "BtnAmAvatarScrollRt";

	public bool _StandAlone = true;

	public string _StoreName = "";

	public string _ZoomInBtn = "BtnDragonZoomIn";

	public string _ZoomOutBtn = "BtnDragonZoomOut";

	public float _DragonZoomMax = 2f;

	public float _DragonZoomMin = 0.5f;

	public float _DragonZoomInc = 0.2f;

	public float _DragonAverageZoom = 1f;

	public float _DragonUiScale = 1f;

	private float mCurrentZoomVal;

	protected KAWidget mTxtCategory;

	protected GameObject mKAUIGenericDBGen;

	protected KAUISelectDragonMenu mMenu;

	protected bool mSyncCheck;

	protected bool mInitDragon;

	protected bool mWaitingForSave;

	private bool mLoadingData;

	private GameObject mObject;

	private Texture2D mTexture;

	private ItemPrefabResData mPrefabResourceData = new ItemPrefabResData();

	private ItemTextureResData mTextureResourceData = new ItemTextureResData();

	private RaisedPetAccType mSelectedItemType;

	protected RaisedPetData mPetData;

	private Transform mParent;

	private Vector3 mDragonOldPos;

	private Quaternion mDragonOldRot;

	public RaisedPetData pPetData
	{
		get
		{
			return mPetData;
		}
		set
		{
			mPetData = value;
		}
	}

	public Transform pParent => mParent;

	public Vector3 pDragonOldPos
	{
		get
		{
			return mDragonOldPos;
		}
		set
		{
			mDragonOldPos = value;
		}
	}

	public Quaternion pDragonOldRot
	{
		get
		{
			return mDragonOldRot;
		}
		set
		{
			mDragonOldRot = value;
		}
	}

	protected virtual void LoadDragon()
	{
		if (mPetData != null)
		{
			SanctuaryManager.CreatePet(mPetData, SanctuaryManager.pInstance._PetOffScreenPosition, Quaternion.identity, base.gameObject, "Full");
		}
	}

	protected virtual void OnPetReady(SanctuaryPet pet)
	{
		if (pet != null)
		{
			mPet = pet;
			mPet._IdleAnimName = mPet._AnimNameIdle;
			if (_DragonStartMarker != null)
			{
				mPet.transform.position = _DragonStartMarker.position;
				mPet.transform.rotation = _DragonStartMarker.rotation;
				mPet.SetFollowAvatar(follow: false);
				mPet.enabled = false;
			}
			mPet.SetState(Character_State.idle);
			mCurrentZoomVal = _DragonAverageZoom;
			mPet.transform.localScale = Vector3.one * _DragonUiScale * mPet.pCurAgeData._UiScale * mCurrentZoomVal;
			mPet.StopLookAtObject();
			if ((bool)mPet.animation["IdleStand"])
			{
				mPet.animation.Play("IdleStand");
			}
			LODGroup componentInChildren = mPet.gameObject.GetComponentInChildren<LODGroup>();
			if (componentInChildren != null)
			{
				componentInChildren.ForceLOD(0);
			}
			AIBehavior_PrefabRef componentInChildren2 = mPet.gameObject.GetComponentInChildren<AIBehavior_PrefabRef>();
			if (componentInChildren2 != null)
			{
				mPet.AIActor.SetState(AISanctuaryPetFSM.CUSTOM);
				componentInChildren2.gameObject.SetActive(value: false);
			}
			NavMeshAgent component = mPet.gameObject.GetComponent<NavMeshAgent>();
			if (component != null)
			{
				component.enabled = false;
			}
			Transform transform = mPet.transform.Find(pet._RootBone);
			if (transform != null)
			{
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
			}
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		mMenu = (KAUISelectDragonMenu)GetMenu("KAUISelectDragonMenu");
		if (!string.IsNullOrEmpty(_CategoryWidgetName))
		{
			mTxtCategory = FindItem(_CategoryWidgetName);
		}
		mSyncCheck = false;
		mInitDragon = false;
		if (base.transform.parent != null)
		{
			mParent = base.transform.parent;
		}
		else
		{
			mParent = base.transform;
		}
		if (!AvatarData.pInitializedFromPreviousSave)
		{
			KAWidget kAWidget = FindItem("BtnAmThreadz");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
		if (!AvatarData.pInitializedFromPreviousSave)
		{
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			SetInteractive(interactive: false);
			mSyncCheck = true;
		}
		LoadDragon();
	}

	public override void OnOpen()
	{
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SetBusy(busy: true);
		}
		SetVisibility(inVisible: true);
		PlayIdleAnim();
		RsResourceManager.DestroyLoadScreen();
		mParent.gameObject.SetActive(value: true);
		if (mIdleManager != null)
		{
			mIdleManager.StartIdles();
		}
		if (_DefaultTab == null)
		{
			return;
		}
		DragonPartTab[] tabs = _Tabs;
		foreach (DragonPartTab dragonPartTab in tabs)
		{
			if (dragonPartTab._PrtTypeName == _DefaultTab)
			{
				SelectTab(dragonPartTab);
				LocaleString text = FindItem(dragonPartTab._BtnName)._TooltipInfo._Text;
				if (mTxtCategory != null && !string.IsNullOrEmpty(text._Text))
				{
					mTxtCategory.SetTextByID(text._ID, text._Text);
				}
				break;
			}
		}
	}

	public override void OnClose()
	{
		mPetData.SaveData();
		SanctuaryManager.pPendingMMOPetCheck = true;
		if (mMenu != null)
		{
			mMenu.SaveSelection();
		}
		Input.ResetInputAxes();
		SnChannel.StopPool("VO_Pool");
		SetState(KAUIState.NOT_INTERACTIVE);
		KAUICursorManager.SetDefaultCursor("Loading");
		mWaitingForSave = true;
	}

	private void PlayIdleAnim()
	{
	}

	public virtual void OnSelectNoChange()
	{
		Object.Destroy(mKAUIGenericDBGen);
		Input.ResetInputAxes();
		SetState(KAUIState.INTERACTIVE);
	}

	protected virtual void SelectTab(DragonPartTab td)
	{
		if (!(mMenu == null))
		{
			if (mMenu._CurrentTab != null)
			{
				mMenu._CurrentTab.OnSelected(t: false, this);
			}
			mMenu._CurrentTab = td;
			mMenu._WHSize = mMenu._CurrentTab._WHSize;
			mMenu._CurrentTab.OnSelected(t: true, this);
			mMenu.ChangeCategory(td._PrtTypeName, forceChange: false);
			_ResetAnim = true;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mLoadingData && mPrefabResourceData.IsDataLoaded() && (string.IsNullOrEmpty(mTextureResourceData._ResFullName) || mTextureResourceData.IsDataLoaded()))
		{
			mLoadingData = false;
			OnDataReady();
		}
		if (mSyncCheck)
		{
			SyncDragon();
		}
		if (_ResetAnim)
		{
			_ResetAnim = false;
			PlayIdleAnim();
		}
		if (mWaitingForSave)
		{
			Exit();
		}
	}

	public virtual void SyncDragon()
	{
		SetState(KAUIState.INTERACTIVE);
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		mSyncCheck = false;
	}

	private void Exit()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		mCameraSwitched = false;
		AvAvatar.SetActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		mPet.SetFollowAvatar(follow: true);
		mPet.SetAvatar(AvAvatar.mTransform);
		mPet.MoveToAvatar();
		mPet._Move2D = true;
		mPet._ActionDoneMessageObject = null;
		mPet.RestoreScale();
		mPet.StopLookAtObject();
		Object.Destroy(mParent);
		if (_CloseMsgObject != null)
		{
			KAUI.RemoveExclusive(this);
			_CloseMsgObject.SendMessage(_CloseMsg);
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SetBusy(busy: false);
		}
	}

	public override void OnPressRepeated(KAWidget inWidget, bool inPressed)
	{
		base.OnPressRepeated(inWidget, inPressed);
		if (!inPressed)
		{
			return;
		}
		float num = 0f;
		if (!string.IsNullOrEmpty(_LeftRotateBtn) && inWidget.name == _LeftRotateBtn)
		{
			num = _RotationSpeed * Time.deltaTime;
		}
		else if (!string.IsNullOrEmpty(_RightRotateBtn) && inWidget.name == _RightRotateBtn)
		{
			num = (0f - _RotationSpeed) * Time.deltaTime;
		}
		if (num != 0f)
		{
			mPet.transform.Rotate(0f, num, 0f);
		}
		if (!string.IsNullOrEmpty(_ZoomInBtn) && inWidget.name == _ZoomInBtn)
		{
			mCurrentZoomVal += _DragonZoomInc * Time.deltaTime;
			if (mCurrentZoomVal > _DragonZoomMax)
			{
				mCurrentZoomVal = _DragonZoomMax;
			}
		}
		else if (!string.IsNullOrEmpty(_ZoomOutBtn) && inWidget.name == _ZoomOutBtn)
		{
			mCurrentZoomVal -= _DragonZoomInc * Time.deltaTime;
			if (mCurrentZoomVal < _DragonZoomMin)
			{
				mCurrentZoomVal = _DragonZoomMin;
			}
		}
	}

	public virtual void OnSelectMenuItem(KAWidget widget)
	{
	}

	public virtual void SetAccessoryItem(KAWidget widget, RaisedPetAccType type)
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)widget.GetUserData();
		string attribute = mPetData.pStage.ToString()[0] + mPetData.pStage.ToString().Substring(1).ToLower() + "Mesh";
		kAUISelectItemData._PrefResName = kAUISelectItemData._ItemData.GetAttribute(attribute, kAUISelectItemData._PrefResName).ToString();
		attribute = mPetData.pStage.ToString()[0] + mPetData.pStage.ToString().Substring(1).ToLower() + "Tex";
		kAUISelectItemData._TextResName = kAUISelectItemData._ItemData.GetAttribute(attribute, kAUISelectItemData._TextResName).ToString();
		mSelectedItemType = type;
		Object.Destroy(mPet.GetAccessoryObject(type));
		if (kAUISelectItemData._UserItemData != null)
		{
			mPetData.SetAccessory(mSelectedItemType, kAUISelectItemData._PrefResName, kAUISelectItemData._TextResName, kAUISelectItemData._UserItemData);
		}
		else
		{
			mPetData.SetAccessory(mSelectedItemType, kAUISelectItemData._PrefResName, kAUISelectItemData._TextResName, kAUISelectItemData._ItemID, kAUISelectItemData._UserInventoryID);
		}
		if (!string.IsNullOrEmpty(kAUISelectItemData._PrefResName) && !kAUISelectItemData._PrefResName.Equals("NULL"))
		{
			mPrefabResourceData.Init(kAUISelectItemData._PrefResName);
			mPrefabResourceData.LoadData();
		}
		else if ((mSelectedItemType == RaisedPetAccType.Saddle || mSelectedItemType == RaisedPetAccType.Materials) && (string.IsNullOrEmpty(kAUISelectItemData._PrefResName) || kAUISelectItemData._PrefResName.Equals("NULL")))
		{
			mPet.SetAccessory(mSelectedItemType, null, null);
			return;
		}
		mTextureResourceData.Init(kAUISelectItemData._TextResName);
		mTextureResourceData.LoadData();
		mLoadingData = true;
	}

	private void OnDataReady()
	{
		mTexture = (Texture2D)mTextureResourceData._Texture;
		if (mPrefabResourceData._Prefab != null && mPrefabResourceData._Prefab.gameObject != null)
		{
			mObject = Object.Instantiate(mPrefabResourceData._Prefab.gameObject);
			if (mTextureResourceData._Texture != null)
			{
				UtUtilities.SetObjectTexture(mObject, 0, mTextureResourceData._Texture);
			}
			mPet.SetAccessory(mSelectedItemType, mObject, mTexture);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnAmThreadz")
		{
			if (string.IsNullOrEmpty(_StoreName))
			{
				Debug.LogError("No Store defined!");
				return;
			}
			SetVisibility(inVisible: false);
			mParent.gameObject.SetActive(value: false);
			base.gameObject.SetActive(value: true);
			GameObject gameObject = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiStores"));
			gameObject.name = "PfUiStores";
			if (!_StandAlone)
			{
				Component[] componentsInChildren = gameObject.GetComponentsInChildren(typeof(AudioListener));
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					((AudioListener)componentsInChildren[i]).enabled = false;
				}
			}
			return;
		}
		DragonPartTab[] tabs = _Tabs;
		foreach (DragonPartTab dragonPartTab in tabs)
		{
			if (dragonPartTab._BtnName == inWidget.name)
			{
				SelectTab(dragonPartTab);
				LocaleString text = inWidget._TooltipInfo._Text;
				if (mTxtCategory != null && !string.IsNullOrEmpty(text._Text))
				{
					mTxtCategory.SetTextByID(text._ID, text._Text);
				}
				break;
			}
		}
	}
}
