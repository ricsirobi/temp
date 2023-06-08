using System;
using System.Collections.Generic;
using UnityEngine;

public class UiAvatarControls : KAUIPadButtons
{
	public class ReticleData
	{
		public Transform pTargetObject;

		public KAWidget pReticle;

		public float pRotationZ;

		public ReticleData(Transform inTargetObject, KAWidget inReticle)
		{
			pTargetObject = inTargetObject;
			pReticle = inReticle;
			pRotationZ = 0f;
		}
	}

	private static UiAvatarControls mInstance;

	public AudioClip _FireDisabledVO;

	public Transform _ReturnMarker;

	public LocaleString _FireDisabledText = new LocaleString("Your pet is too angry to shoot");

	public KAButton _MountButton;

	public string _MountIcon;

	public string _DismountIcon;

	public KASkinSpriteInfo _CannotShootSpriteInfo;

	public float _MinEnergyToShoot = 17f;

	public UIWidget[] _WeaponWidgets;

	private GameObject mDragObject;

	private SanctuaryPet mPet;

	protected GameObject mUiGenericDB;

	private AvAvatarController mAVController;

	private bool mIsReady;

	private float mCooldownTime;

	private float mCooldownTimer;

	private KAWidget mReticle;

	private List<ReticleData> mReticlesList = new List<ReticleData>();

	private KAWidget mFireProgressBar;

	private KAWidget mFireBtn;

	private KAWidget mShotsProgressBar;

	private KAWidget mTxtShotsAvailable;

	private Vector3 mScale;

	private bool mCanFire;

	private bool mFireBtnReady = true;

	private bool mEnableFireOnButtonUp;

	private bool mEnableFireOnButtonDown = true;

	private int mCurrPetAge = -1;

	private const uint LOG_MASK = 199u;

	public static UiAvatarControls pInstance => mInstance;

	public KAWidget pSoarBtn { get; set; }

	public KAWidget pRemoveBtn { get; set; }

	public bool pIsReady
	{
		get
		{
			return mIsReady;
		}
		set
		{
			mIsReady = value;
		}
	}

	public AvAvatarController pAVController
	{
		get
		{
			if (mAVController == null && AvAvatar.pObject != null)
			{
				mAVController = AvAvatar.pObject.GetComponent<AvAvatarController>();
			}
			return mAVController;
		}
	}

	public bool pEnableFireOnButtonDown
	{
		set
		{
			mEnableFireOnButtonDown = value;
		}
	}

	public bool pEnableFireOnButtonUp
	{
		get
		{
			return mEnableFireOnButtonUp;
		}
		set
		{
			mEnableFireOnButtonUp = value;
		}
	}

	public bool pFireBtnReady => mFireBtnReady;

	public static void EnableTiltControls(bool enable, bool recalibrate = true)
	{
		if (mInstance == null || !mInstance.mIsReady)
		{
			return;
		}
		UtDebug.Log("EnableTiltControls " + enable, 199u);
		enable &= UiOptions.pIsTiltSteer;
		if (enable)
		{
			UtDebug.Log("Accelerometer on", 199u);
			Screen.sleepTimeout = -1;
			if (recalibrate)
			{
				KAInput.pInstance.SetCalibration(UiOptions.pCalibratedX, UiOptions.pCalibratedY, UiOptions.pCalibratedZ, InputType.ACCELEROMETER);
			}
		}
		else
		{
			Screen.sleepTimeout = -2;
		}
		if (UtPlatform.IsMobile() || (UtPlatform.IsWSA() && !UtUtilities.IsKeyboardAttached()))
		{
			KAInput.ShowJoystick(JoyStickPos.BOTTOM_LEFT, !KAInput.pInstance.pAreInputsHidden && !enable);
		}
		KAInput.pInstance.EnableInputType("Horizontal", InputType.ACCELEROMETER, enable);
		KAInput.pInstance.EnableInputType("Vertical", InputType.ACCELEROMETER, enable);
		if (enable && UtPlatform.IsWSA())
		{
			if (Orientation.GetOrientation() == ScreenOrientation.Landscape)
			{
				Screen.autorotateToLandscapeRight = false;
			}
			else if (Orientation.GetOrientation() == ScreenOrientation.LandscapeRight)
			{
				Screen.autorotateToLandscapeLeft = false;
			}
		}
		else
		{
			Screen.autorotateToLandscapeLeft = !enable;
			Screen.autorotateToLandscapeRight = !enable;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		mIsReady = false;
		mInstance = this;
		DetachFromToolbar();
	}

	protected override void Start()
	{
		base.Start();
		mReticle = FindItem("Reticle");
		if (null == mReticle)
		{
			UtDebug.LogWarning("Reticle not found");
		}
		mFireProgressBar = FindItem("Fill");
		if (mFireProgressBar != null)
		{
			mFireProgressBar.SetProgressLevel(0f);
		}
		mShotsProgressBar = FindItem("TotalShotsFill");
		mTxtShotsAvailable = FindItem("TxtShotsNum");
		if (mTxtShotsAvailable != null)
		{
			mTxtShotsAvailable.SetVisibility(inVisible: false);
		}
		if (mShotsProgressBar != null)
		{
			mShotsProgressBar.SetProgressLevel(0f);
		}
		mFireBtn = FindItem("DragonFire");
		pSoarBtn = FindItem("Soar");
		pRemoveBtn = FindItem("Remove");
		KAInput.pInstance.EnableInputType("Jump", InputType.ALL, inEnable: true);
		KAInput.pInstance.EnableInputType("Drag", InputType.UI_BUTTONS, inEnable: false);
		KAInput.pInstance.EnableInputType("DragonBrake", InputType.UI_BUTTONS, inEnable: false);
		KAInput.pInstance.EnableInputType("DragonFire", InputType.UI_BUTTONS, inEnable: false);
		KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, inEnable: false);
		KAInput.pInstance.EnableInputType("Remove", InputType.UI_BUTTONS, inEnable: false);
		KAInput.pInstance.EnableInputType("UWSwimBoost", InputType.UI_BUTTONS, inEnable: false);
		KAInput.pInstance.EnableInputType("UWSwimBrake", InputType.UI_BUTTONS, inEnable: false);
		KAInput.pInstance.EnableInputType("WingFlap", InputType.UI_BUTTONS, inEnable: false);
		if (pAVController != null)
		{
			pAVController.ShowSoarButton(inShow: false);
			pAVController.SetFlightSuitData();
			if (pAVController.pPlayerMounted)
			{
				pAVController.OnPetFlyingStateChanged += OnFlyingStateChanged;
			}
		}
		if (AvAvatar.pSubState != AvAvatarSubState.FLYING && SanctuaryManager.pCurPetInstance != null)
		{
			KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, SanctuaryManager.pCurPetInstance.IsMountAllowed() && FUEManager.IsInputEnabled("DragonMount"));
		}
	}

	public void Init()
	{
		UtDebug.Log("Init UiFlying", 199u);
		SetMount(doMount: true);
		if (SanctuaryManager.pCurPetInstance != null)
		{
			if (SanctuaryManager.pCurPetInstance.pWeaponManager != null)
			{
				PetWeaponManager pWeaponManager = SanctuaryManager.pCurPetInstance.pWeaponManager;
				pWeaponManager.OnWeaponChanged = (WeaponManager.WeaponChanged)Delegate.Combine(pWeaponManager.OnWeaponChanged, new WeaponManager.WeaponChanged(WeaponChanged));
			}
			mCurrPetAge = SanctuaryManager.pCurPetInstance.pAge;
			ResetWeaponTheme();
			SanctuaryPet.AddJumpEvent(SanctuaryManager.pCurPetInstance, OnPetJump);
			if (SanctuaryManager.pCurPetInstance.IsMountAllowed())
			{
				if (!SanctuaryManager.pCurPetInstance.pIsMounted)
				{
					SanctuaryPet.AddMountEvent(SanctuaryManager.pCurPetInstance, OnDragonMount);
				}
				else
				{
					SetMount(doMount: false);
				}
			}
			EnableDragonFireButton(inEnable: true);
			ShowAvatarToggleButton(SanctuaryManager.pCurPetInstance.pIsMounted);
		}
		else
		{
			ShowAvatarToggleButton(show: false);
			KAInput.pInstance.EnableInputType("DragonFire", InputType.UI_BUTTONS, inEnable: false);
			KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, inEnable: false);
		}
		if (AvAvatar.pSubState != AvAvatarSubState.FLYING)
		{
			if (SanctuaryManager.pCurPetInstance != null)
			{
				KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, SanctuaryManager.pCurPetInstance.IsMountAllowed() && FUEManager.IsInputEnabled("DragonMount"));
			}
			else
			{
				KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, inEnable: false);
			}
			KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, AvAvatar.pSubState != AvAvatarSubState.UWSWIMMING);
			KAInput.pInstance.EnableInputType("WingFlap", InputType.UI_BUTTONS, inEnable: false);
			KAInput.pInstance.EnableInputType("DragonBrake", InputType.UI_BUTTONS, inEnable: false);
		}
		else
		{
			KAInput.pInstance.EnableInputType("WingFlap", InputType.UI_BUTTONS, inEnable: true);
			KAInput.pInstance.EnableInputType("DragonBrake", InputType.UI_BUTTONS, inEnable: true);
			KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, inEnable: false);
			KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, inEnable: false);
			KAInput.pInstance.EnableInputType("Drag", InputType.UI_BUTTONS, inEnable: false);
			pAVController.ShowSoarButton(inShow: true);
		}
		mIsReady = true;
		EnableTiltControls(AvAvatar.pSubState == AvAvatarSubState.FLYING);
	}

	private void WeaponChanged()
	{
		ResetWeaponTheme();
	}

	public void ResetWeaponTheme()
	{
		if (_WeaponWidgets == null)
		{
			return;
		}
		Color fireTint = SanctuaryManager.pCurPetInstance.pWeaponManager.GetCurrentWeapon()._FireTint;
		UIWidget[] weaponWidgets = _WeaponWidgets;
		foreach (UIWidget uIWidget in weaponWidgets)
		{
			if (uIWidget != null)
			{
				uIWidget.color = fireTint;
				uIWidget.pOrgColorTint = fireTint;
			}
		}
	}

	public float GetWeaponCooldown()
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			return SanctuaryManager.pCurPetInstance.GetWeaponCooldown();
		}
		return 1f;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		SanctuaryPet.RemoveMountEvent(SanctuaryManager.pCurPetInstance, OnDragonMount);
		SanctuaryPet.RemoveJumpEvent(SanctuaryManager.pCurPetInstance, OnPetJump);
		if (pAVController != null)
		{
			pAVController.OnPetFlyingStateChanged -= OnFlyingStateChanged;
		}
		mInstance = null;
	}

	protected override void Update()
	{
		base.Update();
		bool flag = !KAInput.pInstance.pAreInputsHidden && AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pState != 0 && !RsResourceManager.pLevelLoadingScreen;
		if (GetVisibility() != flag)
		{
			SetVisibility(flag);
		}
		if (RsResourceManager.pLevelLoadingScreen)
		{
			return;
		}
		if ((!mIsReady && AvatarData.pIsReady && AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pState != 0) || (SanctuaryManager.pCurPetInstance != null && mCurrPetAge != SanctuaryManager.pCurPetInstance.pAge))
		{
			Init();
		}
		if (AvAvatar.pInputEnabled && mFireBtnReady && ((mEnableFireOnButtonDown && KAInput.GetButtonDown("DragonFire")) || (mEnableFireOnButtonUp && KAInput.GetButtonUp("DragonFire"))))
		{
			Fire();
		}
		if (KAInput.GetButtonDown("HideAvatar"))
		{
			HideAvatar(hide: true);
		}
		if (KAInput.GetButtonDown("ShowAvatar"))
		{
			HideAvatar(hide: false);
		}
		if (AvAvatar.pInputEnabled)
		{
			if (KAInput.GetButtonDown("Drag"))
			{
				StartDrag();
			}
			if (KAInput.GetButtonUp("Drag"))
			{
				StopDrag();
			}
		}
		if (AvAvatar.pSubState == AvAvatarSubState.UWSWIMMING)
		{
			return;
		}
		bool inputTypeState = KAInput.pInstance.GetInputTypeState("TargetSwitch", InputType.UI_BUTTONS);
		if (SanctuaryManager.pCurPetInstance != null)
		{
			if (KAInput.GetKeyUp(KeyCode.P) && Application.isEditor)
			{
				SanctuaryManager.pCurPetInstance.Fire(AvAvatar.mTransform, useDirection: false, Vector3.zero);
			}
			if (mCanFire && _CannotShootSpriteInfo != null)
			{
				_CannotShootSpriteInfo.ChangeWidgetSprite(!SanctuaryManager.pCurPetInstance.pCanShoot);
			}
			if (SanctuaryManager.pCurPetInstance.pWeaponManager != null && SanctuaryManager.pCurPetInstance.pWeaponManager.pTargetList != null && SanctuaryManager.pCurPetInstance.pWeaponManager.pTargetList.Count > 1)
			{
				if (!inputTypeState)
				{
					KAInput.pInstance.EnableInputType("TargetSwitch", InputType.UI_BUTTONS, inEnable: true);
				}
			}
			else if (inputTypeState)
			{
				KAInput.pInstance.EnableInputType("TargetSwitch", InputType.UI_BUTTONS, inEnable: false);
			}
			WeaponRechargeData weaponRechargeData = SanctuaryManager.pInstance.GetWeaponRechargeData(SanctuaryManager.pCurPetInstance);
			if (weaponRechargeData != null)
			{
				if (mTxtShotsAvailable != null)
				{
					mTxtShotsAvailable.SetVisibility(inVisible: true);
					mTxtShotsAvailable.SetText(weaponRechargeData.mShotsAvailable.ToString());
				}
				if (mShotsProgressBar != null)
				{
					mShotsProgressBar.SetProgressLevel(weaponRechargeData.mTotalShotsProgress);
				}
			}
		}
		else if (inputTypeState)
		{
			KAInput.pInstance.EnableInputType("TargetSwitch", InputType.UI_BUTTONS, inEnable: false);
		}
		mCooldownTimer = Mathf.Max(0f, mCooldownTimer - Time.deltaTime);
		if (mCooldownTimer > 0f)
		{
			if (mFireProgressBar != null)
			{
				mFireProgressBar.SetProgressLevel(mCooldownTimer / mCooldownTime);
			}
		}
		else if (mCooldownTimer <= 0f && !mFireBtnReady)
		{
			mFireBtnReady = true;
			if (mFireBtn != null)
			{
				mFireBtn.SetState(KAUIState.INTERACTIVE);
			}
		}
	}

	public void HideAvatar(bool hide)
	{
		if (FUEManager.IsInputEnabled("ToggleAvatar"))
		{
			if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted)
			{
				EnableAvatarHideButton(!hide);
			}
			EnableAvatarShowButton(hide);
		}
		if (mAVController != null)
		{
			mAVController.AvatarHidden = hide;
			mAVController.EnableRenderer(!hide);
		}
	}

	public void EnableDrag(GameObject dragObj, bool enable)
	{
		if (enable)
		{
			if (mDragObject == null)
			{
				mDragObject = dragObj;
				KAInput.pInstance.EnableInputType("Drag", InputType.UI_BUTTONS, inEnable: true);
			}
		}
		else if (mDragObject == dragObj)
		{
			mDragObject = null;
			KAInput.pInstance.EnableInputType("Drag", InputType.UI_BUTTONS, inEnable: false);
			StopDrag();
		}
	}

	public void StartDrag()
	{
		if (mDragObject != null && !pAVController.pPlayerDragging)
		{
			KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, inEnable: false);
			pAVController.StartDrag(mDragObject);
		}
	}

	public void StopDrag()
	{
		if (pAVController.pPlayerDragging)
		{
			KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, inEnable: true);
			pAVController.StopDrag();
		}
	}

	public void ShowUWSwimButtons(bool isVisible)
	{
		KAInput.pInstance.EnableInputType("UWSwimBoost", InputType.UI_BUTTONS, isVisible);
		KAInput.pInstance.EnableInputType("UWSwimBrake", InputType.UI_BUTTONS, isVisible);
		KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, !isVisible);
		EnableTiltControls(isVisible);
	}

	public void UpdateMountButtonVisibility()
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, SanctuaryManager.pCurPetInstance.IsMountAllowed() && FUEManager.IsInputEnabled("DragonMount"));
		}
	}

	public void OnPetJump(bool isJump)
	{
		if (AvAvatar.pSubState == AvAvatarSubState.FLYING)
		{
			return;
		}
		if (mPet == null)
		{
			mPet = SanctuaryManager.pCurPetInstance;
		}
		if (isJump)
		{
			KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, inEnable: false);
			bool inEnable = (bool)mPet && !mPet.pTypeInfo._Flightless;
			KAInput.pInstance.EnableInputType("WingFlap", InputType.UI_BUTTONS, inEnable);
			KAInput.pInstance.EnableInputType("WingFlap", InputType.KEYBOARD, inEnable);
			if ((bool)mPet)
			{
				KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, inEnable: false);
			}
		}
		else
		{
			KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, inEnable: true);
			if ((bool)mPet)
			{
				KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, mPet.IsMountAllowed() && FUEManager.IsInputEnabled("DragonMount"));
			}
			KAInput.pInstance.EnableInputType("WingFlap", InputType.UI_BUTTONS, inEnable: false);
			KAInput.pInstance.EnableInputType("DragonBrake", InputType.UI_BUTTONS, inEnable: false);
			pAVController.ShowSoarButton(inShow: false);
		}
	}

	public void OnFlyingStateChanged(FlyingState newState)
	{
		switch (newState)
		{
		case FlyingState.TakeOff:
		case FlyingState.Normal:
		case FlyingState.Hover:
		case FlyingState.TakeOffGliding:
			KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, inEnable: false);
			if (pAVController.pPlayerMounted)
			{
				KAInput.pInstance.EnableInputType("DragonBrake", InputType.UI_BUTTONS, inEnable: true);
				KAInput.pInstance.EnableInputType("DragonBrake", InputType.KEYBOARD, inEnable: true);
				KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, inEnable: false);
				KAInput.pInstance.EnableInputType("WingFlap", InputType.UI_BUTTONS, inEnable: true);
				KAInput.pInstance.EnableInputType("WingFlap", InputType.KEYBOARD, inEnable: true);
			}
			pAVController.SetToolbarButtonsVisible(visible: false);
			EnableTiltControls(enable: true);
			break;
		case FlyingState.Grounded:
			KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, inEnable: true);
			KAInput.pInstance.EnableInputType("WingFlap", InputType.UI_BUTTONS, inEnable: false);
			KAInput.pInstance.EnableInputType("DragonBrake", InputType.UI_BUTTONS, inEnable: false);
			pAVController.ShowSoarButton(inShow: false);
			pAVController.SetToolbarButtonsVisible(visible: true);
			if (AvAvatar.pLevelState != AvAvatarLevelState.RACING)
			{
				if (mPet == null)
				{
					mPet = SanctuaryManager.pCurPetInstance;
				}
				if ((bool)mPet)
				{
					KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, mPet.IsMountAllowed() && FUEManager.IsInputEnabled("DragonMount"));
				}
			}
			EnableTiltControls(enable: false);
			break;
		}
	}

	public void SetMount(bool doMount)
	{
		if (!_MountButton)
		{
			return;
		}
		if (doMount)
		{
			if (!string.IsNullOrEmpty(_MountIcon))
			{
				_MountButton.SetSprite(_MountIcon);
			}
		}
		else if (!string.IsNullOrEmpty(_DismountIcon))
		{
			_MountButton.SetSprite(_DismountIcon);
		}
	}

	public void OnDragonMount(bool mount, PetSpecialSkillType skill)
	{
		if (mount)
		{
			pAVController.OnPetFlyingStateChanged += OnFlyingStateChanged;
			SetMount(doMount: false);
			if (SanctuaryManager.pCurPetInstance != null)
			{
				ResetWeaponTheme();
			}
		}
		else
		{
			pAVController.OnPetFlyingStateChanged -= OnFlyingStateChanged;
			KAInput.pInstance.EnableInputType("Jump", InputType.UI_BUTTONS, inEnable: true);
			KAInput.pInstance.EnableInputType("WingFlap", InputType.UI_BUTTONS, inEnable: false);
			KAInput.pInstance.EnableInputType("DragonBrake", InputType.UI_BUTTONS, inEnable: false);
			if (mPet == null)
			{
				mPet = SanctuaryManager.pCurPetInstance;
			}
			if ((bool)mPet)
			{
				KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, mPet.IsMountAllowed() && FUEManager.IsInputEnabled("DragonMount"));
			}
			SetMount(doMount: true);
		}
		EnableDragonFireButton(mount);
		ShowAvatarToggleButton(mount);
	}

	public void ShowAvatarToggleButton(bool show)
	{
		if (FUEManager.IsInputEnabled("ToggleAvatar"))
		{
			EnableAvatarHideButton(show);
			EnableAvatarShowButton(inEnable: false);
		}
		if (mAVController != null)
		{
			mAVController.AvatarHidden = false;
			mAVController.EnableRenderer(enable: true);
		}
	}

	public void DisableAllDragonControls(bool inDisable = true)
	{
		if (AvAvatar.pSubState == AvAvatarSubState.FLYING)
		{
			KAInput.pInstance.EnableInputType("DragonBrake", InputType.ALL, !inDisable);
			KAInput.pInstance.EnableInputType("WingFlap", InputType.ALL, !inDisable);
		}
		else
		{
			KAInput.pInstance.EnableInputType("Jump", InputType.ALL, !inDisable);
		}
		if (UiJoystick.pInstance != null)
		{
			if (inDisable)
			{
				KAInput.ShowJoystick(UiJoystick.pInstance.pPos, inShow: false);
			}
			else
			{
				KAInput.ShowJoystick(UiJoystick.pInstance.pPos, !UiOptions.pIsTiltSteer);
			}
		}
		if (SanctuaryManager.pCurPetInstance != null && AvAvatar.pSubState != AvAvatarSubState.FLYING)
		{
			KAInput.pInstance.EnableInputType("DragonMount", InputType.ALL, !inDisable && SanctuaryManager.pCurPetInstance.IsMountAllowed() && FUEManager.IsInputEnabled("DragonMount"));
			EnableDragonFireButton(!inDisable);
		}
	}

	public void EnableDragonControl(string inControl, bool inEnable = true)
	{
		KAInput.pInstance.EnableInputType(inControl, InputType.ALL, inEnable);
	}

	public void DetachFromToolbar()
	{
		if (base.transform.parent != null)
		{
			base.transform.parent = null;
			base.gameObject.SetActive(value: true);
		}
	}

	private void Fire()
	{
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pCanShoot)
		{
			SanctuaryManager.pCurPetInstance.pWeaponShotsAvailable--;
			SanctuaryManager.pCurPetInstance.Fire(null, useDirection: false, Vector3.zero, ignoreCoolDown: true);
			ForceCooldown(GetWeaponCooldown());
		}
	}

	public void ForceCooldown(float cooldownTime)
	{
		mCooldownTimer = (mCooldownTime = cooldownTime);
		mFireBtnReady = false;
		if (mFireBtn != null)
		{
			bool visibility = mFireBtn.GetVisibility();
			mFireBtn.ResetWidget(resetPosition: false);
			mFireBtn.SetState(KAUIState.NOT_INTERACTIVE);
			mFireBtn.SetVisibility(visibility);
		}
	}

	public void ShowHelp()
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			if (_FireDisabledVO != null)
			{
				SnChannel.Play(_FireDisabledVO, "VO_Pool", inForce: true);
			}
			OnClose();
			mUiGenericDB = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBHelpSm"));
			KAUIGenericDB component = mUiGenericDB.GetComponent<KAUIGenericDB>();
			component._MessageObject = base.gameObject;
			component._OKMessage = "OnClose";
			component.SetText(_FireDisabledText.GetLocalizedString(), interactive: false);
			component.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			if (SanctuaryManager.pInstance.pPetMeter != null)
			{
				mUiGenericDB.transform.parent = SanctuaryManager.pInstance.pPetMeter.transform;
				SanctuaryManager.pInstance.pPetMeter.FlashPointer(flash: true);
			}
		}
	}

	public void UpdateReticle(bool onTarget, Vector3 pos, Transform inTargetObject, float inRotationZSpeed)
	{
		if (!(mReticle != null) || !GetVisibility())
		{
			return;
		}
		ReticleData reticleData = mReticlesList.Find((ReticleData x) => x.pTargetObject == inTargetObject);
		if (reticleData == null)
		{
			KAWidget kAWidget = DuplicateWidget(mReticle);
			kAWidget.transform.parent = mReticle.transform.parent;
			kAWidget.transform.position = mReticle.transform.position;
			reticleData = new ReticleData(inTargetObject, kAWidget);
			mReticlesList.Add(reticleData);
		}
		if (reticleData == null)
		{
			return;
		}
		bool visibility = onTarget && mCanFire;
		reticleData.pReticle.SetVisibility(visibility);
		reticleData.pReticle.SetToScreenPosition(pos.x, (float)Screen.height - pos.y);
		reticleData.pReticle.SetRotateSpeed(inRotationZSpeed);
		if (null != SanctuaryManager.pCurPetInstance && null != SanctuaryManager.pCurPetInstance.pWeaponManager)
		{
			if (SanctuaryManager.pCurPetInstance.pWeaponManager.pCurrentTarget == inTargetObject)
			{
				reticleData.pReticle.PlayAnim("Lock");
			}
			else
			{
				reticleData.pReticle.PlayAnim("Scan");
			}
		}
	}

	public void ClearPreviousReticles(List<Transform> inTargetList)
	{
		if (inTargetList == null || inTargetList.Count == 0)
		{
			foreach (ReticleData mReticles in mReticlesList)
			{
				if (mReticles.pReticle != null)
				{
					UnityEngine.Object.Destroy(mReticles.pReticle.gameObject);
				}
			}
			mReticlesList.Clear();
			return;
		}
		List<ReticleData> list = new List<ReticleData>();
		foreach (ReticleData reticleData in mReticlesList)
		{
			if (inTargetList.Find((Transform x) => x == reticleData.pTargetObject) == null)
			{
				if (reticleData.pReticle != null)
				{
					UnityEngine.Object.Destroy(reticleData.pReticle.gameObject);
				}
			}
			else
			{
				list.Add(reticleData);
			}
		}
		mReticlesList = list;
	}

	private void OnClose()
	{
		if (SanctuaryManager.pInstance.pPetMeter != null)
		{
			SanctuaryManager.pInstance.pPetMeter.FlashPointer(flash: false);
		}
		if (mUiGenericDB != null)
		{
			mUiGenericDB.transform.parent = null;
			UnityEngine.Object.DestroyImmediate(mUiGenericDB);
		}
	}

	private void EnableAvatarHideButton(bool inEnable)
	{
		KAInput.pInstance.EnableInputType("HideAvatar", InputType.ALL, inEnable);
	}

	private void EnableAvatarShowButton(bool inEnable)
	{
		KAInput.pInstance.EnableInputType("ShowAvatar", InputType.ALL, inEnable);
	}

	public void EnableDragonFireButton(bool inEnable)
	{
		mCanFire = inEnable && SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted && FUEManager.IsInputEnabled("DragonFire");
		KAInput.pInstance.EnableInputType("DragonFire", InputType.ALL, mCanFire);
		if (mFireBtn != null && !mFireBtn.GetVisibility())
		{
			OnClose();
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible)
		{
			return;
		}
		foreach (ReticleData mReticles in mReticlesList)
		{
			if (mReticles.pReticle != null)
			{
				mReticles.pReticle.SetVisibility(inVisible: false);
			}
		}
	}
}
