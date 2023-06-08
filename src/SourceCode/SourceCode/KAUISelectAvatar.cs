using UnityEngine;

public class KAUISelectAvatar : KAUISelect
{
	public static bool _DoNotRestoreDefault = false;

	public static bool _ResetAnim = true;

	public AvatarPartTab[] _Tabs;

	public string _GenderBtnName = "BtnAmGender";

	public string _CategoryWidgetName = "TxtCatName";

	public string _DefaultTab = "Hair";

	public GameObject _CloseMsgObject;

	public string _CloseMsg = "AvatarCreatorClosed";

	public string _ExitToScene = "";

	public string _ReturnMarker = "";

	public Transform _AvatarStartMarker;

	public bool _ForceGenderSelect = true;

	public LocaleString _GenderMessageText = new LocaleString("Please select your gender.");

	public LocaleString _BoyBtnText = new LocaleString("Boy");

	public LocaleString _GirlBtnText = new LocaleString("Girl");

	public AudioClip _GenderClip;

	public string _BoyIdleAnimName = "Idle";

	public string _GirlIdleAnimName = "Idle";

	public float _RotationSpeed = 40f;

	public string _LeftRotateBtn = "BtnAmAvatarScrollLt";

	public string _RightRotateBtn = "BtnAmAvatarScrollRt";

	public bool _StandAlone = true;

	public string _StoreName = "";

	protected KAWidget mGender;

	protected KAWidget mTxtCategory;

	protected GameObject mKAUIGenericDBGen;

	protected KAUISelectAvatarMenu mMenu;

	protected bool mSyncCheck;

	protected bool mInitAvatar;

	protected bool mWaitingForSave;

	private Transform mParent;

	private Vector3 mAvatarOldPos;

	private Quaternion mAvatarOldRot;

	public Transform pParent => mParent;

	public Vector3 pAvatarOldPos
	{
		get
		{
			return mAvatarOldPos;
		}
		set
		{
			mAvatarOldPos = value;
		}
	}

	public Quaternion pAvatarOldRot
	{
		get
		{
			return mAvatarOldRot;
		}
		set
		{
			mAvatarOldRot = value;
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		mMenu = (KAUISelectAvatarMenu)_MenuList[1];
		if (!string.IsNullOrEmpty(_GenderBtnName))
		{
			mGender = FindItem(_GenderBtnName);
		}
		if (!string.IsNullOrEmpty(_CategoryWidgetName))
		{
			mTxtCategory = FindItem(_CategoryWidgetName);
		}
		mSyncCheck = false;
		mInitAvatar = false;
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
		if (!_ForceGenderSelect && !AvatarData.pInitializedFromPreviousSave)
		{
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			SetInteractive(interactive: false);
			mSyncCheck = true;
		}
	}

	public override void OnOpen()
	{
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SetBusy(busy: true);
		}
		mAvatarOldRot = AvAvatar.mTransform.rotation;
		mAvatarOldPos = AvAvatar.position;
		if (_StandAlone && AvAvatar.pAvatarCam != null)
		{
			AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>().SetLookAt(null, null, 0f);
		}
		AvAvatar.SetActive(inActive: true);
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.SetDisplayNameVisible(inVisible: false);
		AvAvatar.pState = AvAvatarState.NONE;
		AvAvatar.pSubState = AvAvatarSubState.NORMAL;
		if (_AvatarStartMarker != null)
		{
			AvAvatar.SetPosition(_AvatarStartMarker);
		}
		PlayIdleAnim();
		RsResourceManager.DestroyLoadScreen();
		AvatarData.SetDontDestroyOnBundles(inDontDestroy: false);
		mParent.gameObject.SetActive(value: true);
		if (_ForceGenderSelect && !AvatarData.pInitializedFromPreviousSave)
		{
			ShowGenderDialog();
		}
		else
		{
			_ForceGenderSelect = false;
			PlayTutorial();
			if (mIdleManager != null)
			{
				mIdleManager.StartIdles();
			}
		}
		if (_DefaultTab == null)
		{
			return;
		}
		AvatarPartTab[] tabs = _Tabs;
		foreach (AvatarPartTab avatarPartTab in tabs)
		{
			if (avatarPartTab._PrtTypeName == _DefaultTab)
			{
				SelectTab(avatarPartTab);
				LocaleString text = FindItem(avatarPartTab._BtnName)._TooltipInfo._Text;
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
		if (mMenu != null)
		{
			mMenu.SaveSelection();
		}
		Input.ResetInputAxes();
		TutorialManager.StopTutorials();
		SnChannel.StopPool("VO_Pool");
		SetState(KAUIState.NOT_INTERACTIVE);
		KAUICursorManager.SetDefaultCursor("Loading");
		mWaitingForSave = true;
	}

	private void PlayIdleAnim()
	{
		if (AvatarData.GetGender() == Gender.Female)
		{
			AvAvatar.PlayAnim(_GirlIdleAnimName, WrapMode.Loop);
		}
		else
		{
			AvAvatar.PlayAnim(_BoyIdleAnimName, WrapMode.Loop);
		}
	}

	public virtual void ShowGenderDialog()
	{
		SetState(KAUIState.NOT_INTERACTIVE);
		mKAUIGenericDBGen = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
		mKAUIGenericDBGen.name = "GenderSelect";
		KAUIGenericDB component = mKAUIGenericDBGen.GetComponent<KAUIGenericDB>();
		KAUI.SetExclusive(component, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		component._MessageObject = base.gameObject;
		component.FindItem("YesBtn").SetTextByID(_BoyBtnText._ID, _BoyBtnText._Text);
		component.FindItem("NoBtn").SetTextByID(_GirlBtnText._ID, _GirlBtnText._Text);
		component._YesMessage = "OnSelectMale";
		component._NoMessage = "OnSelectFemale";
		component._CloseMessage = "OnSelectNoChange";
		component.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, !_ForceGenderSelect);
		component.SetTextByID(_GenderMessageText._ID, _GenderMessageText._Text, interactive: false);
		if ((bool)_GenderClip)
		{
			SnChannel.Play(_GenderClip, "VO_Pool", inForce: true);
		}
	}

	public virtual void OnSelectMale()
	{
		OnSelectGender(Gender.Male);
	}

	public virtual void OnSelectFemale()
	{
		OnSelectGender(Gender.Female);
	}

	public virtual void OnSelectNoChange()
	{
		Object.Destroy(mKAUIGenericDBGen);
		Input.ResetInputAxes();
		SetState(KAUIState.INTERACTIVE);
	}

	public virtual void OnSelectGender(Gender gender)
	{
		if (AvatarData.GetGender() == gender)
		{
			OnSelectNoChange();
			return;
		}
		AvatarData.SetGender(gender);
		mMenu.ChangeCategory(mMenu.pCategoryID, forceChange: true);
		Object.Destroy(mKAUIGenericDBGen);
		Input.ResetInputAxes();
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		SetState(KAUIState.NOT_INTERACTIVE);
		mSyncCheck = true;
		_ResetAnim = true;
		if (_ForceGenderSelect)
		{
			_ForceGenderSelect = false;
			PlayTutorial();
			if (mIdleManager != null)
			{
				mIdleManager.StartIdles();
			}
		}
	}

	protected void SelectTab(AvatarPartTab td)
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
		if (mSyncCheck)
		{
			SyncAvatar();
		}
		if (mInitAvatar && AvatarData.pIsReady)
		{
			mInitAvatar = false;
			SetState(KAUIState.INTERACTIVE);
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			return;
		}
		if (_ResetAnim)
		{
			_ResetAnim = false;
			PlayIdleAnim();
		}
		if (mWaitingForSave && !AvatarData.pIsSaving)
		{
			Exit();
		}
	}

	public virtual void SyncAvatar()
	{
		SetState(KAUIState.INTERACTIVE);
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		mSyncCheck = false;
	}

	private void Exit()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		mCameraSwitched = false;
		AvatarData.SetDontDestroyOnBundles(inDontDestroy: true);
		if (_StandAlone)
		{
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.SetDisplayNameVisible(inVisible: true);
			AvAvatar.mTransform.rotation = mAvatarOldRot;
			AvAvatar.SetPosition(mAvatarOldPos);
			if (AvAvatar.pAvatarCam != null)
			{
				AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>().SetLookAt(AvAvatar.mTransform, null, 0f);
			}
			if (mMenu != null && mMenu.mModified)
			{
				UiToolbar.pAvatarModified = true;
			}
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
		else
		{
			mParent.gameObject.SetActive(value: false);
			ExitScene();
		}
	}

	public void ExitScene()
	{
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.SetDisplayNameVisible(inVisible: true);
		if (_ReturnMarker != "")
		{
			AvAvatar.pStartLocation = _ReturnMarker;
		}
		if (_ExitToScene != "")
		{
			RsResourceManager.LoadLevel(_ExitToScene);
		}
		else
		{
			RsResourceManager.LoadLevel(RsResourceManager.pLastLevel);
		}
	}

	public override void OnPressRepeated(KAWidget inWidget, bool inPressed)
	{
		base.OnPressRepeated(inWidget, inPressed);
		if (inPressed)
		{
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
				AvAvatar.mTransform.Rotate(0f, num, 0f);
			}
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
		if (inWidget == mGender)
		{
			ShowGenderDialog();
			return;
		}
		AvatarPartTab[] tabs = _Tabs;
		foreach (AvatarPartTab avatarPartTab in tabs)
		{
			if (avatarPartTab._BtnName == inWidget.name)
			{
				SelectTab(avatarPartTab);
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
