using System;
using System.Collections.Generic;
using SquadTactics;
using UnityEngine;

public class UiDragonCustomization : KAUISelectDragon
{
	[Serializable]
	public class DragonPartCat
	{
		public string _PartName = string.Empty;

		public string _BtnName = string.Empty;

		public LocaleString _ToolTipText = new LocaleString("");

		public Texture _Icon;
	}

	public const string PD_PRICOLOR = "Color_PrimaryDragonC";

	public const string PD_SECCOLOR = "Color_SecondaryDragonC";

	public const string PD_TERCOLOR = "Color_TertiaryDragonC";

	public GameObject _ColorSelectionObject;

	public GameObject _MessageObject;

	public LocaleString _PetTeenText = new LocaleString("You need TEEN dragon atleast to open the talent page!");

	public LocaleString _PetNameInvalidText = new LocaleString("Name is invalid please try some other names");

	public LocaleString _PetNameConfirmationText = new LocaleString("Are you sure you want this to be your Pet name?");

	public LocaleString _ApplyGlowConfirmationText = new LocaleString("[REVIEW]Are you sure you want to apply this potion?");

	public LocaleString _OverwriteGlowConfirmationText = new LocaleString("[REVIEW]Using this potion shall overwrite the existing glow on your dragon. Continue?");

	public LocaleString _RemoveGlowConfirmationText = new LocaleString("[REVIEW]Are you sure you want to remove glow?");

	public LocaleString _UnauthorizedText = new LocaleString("Your account is still unauthorized. Please ask your parent to activate it.");

	public LocaleString _UnauthorizedTitleText = new LocaleString("Authorization");

	public int _MaxNumberOfTry = 2;

	public string _AgeUpUiTrigger = "Journal";

	private Color mPrimaryColor = Color.white;

	private Color mSecondaryColor = Color.white;

	private Color mTertiaryColor = Color.white;

	private bool mRebuildTexture;

	private bool mReloadDragon;

	private KAUIGenericDB mUiGenericDB;

	private UiStatPopUp mUiStats;

	private KAWidget mStatsBtn;

	private ItemData mGlowItemData;

	private KAWidget mGlowEffectCountDown;

	private KAWidget mGlowCountDownProgressBar;

	private KAWidget mTxtGlowCountDownTime;

	private KAWidget mGlowRemoveButton;

	public UIWidget[] _GlowColorTintChildList;

	public CountDownTimer mGlowCountDown = new CountDownTimer();

	private static bool mIsBusy;

	public List<DragonPartCat> _DragonPartCategory;

	public bool _DisablePetMoodParticles = true;

	public string _EquipmentAssetPath = string.Empty;

	public string _DragWidgetName = "DragWidget";

	public LocaleString _EnterValidNameText = new LocaleString("Please Enter Valid Name");

	public LocaleString _LevelLabelText = new LocaleString("Lvl");

	public Camera _AvatarRenderCamera;

	private KAWidget mDragonBtn;

	private KAWidget mEquipmentBtn;

	private KAWidget mColorPalette;

	private KAWidget mColorSelector;

	private KAToggleButton mGlowPreviewTogBtn;

	private KAWidget mGlowUseBtn;

	private KAWidget mGlowTitleTxt;

	private KAWidget mGlowDescTxt;

	private KAWidget mGlowTypeIcon;

	private KAWidget mBackBtn;

	private KAWidget mOKBtn;

	private KAWidget mNameLabel;

	private KAEditBox mEditLabel;

	private KAWidget mTalentTreeBtn;

	private KAWidget mAutoFillBtn;

	private KAWidget mCustomizeLockBtn;

	private KAWidget mResetBtn;

	private KAWidget mPrimaryColorBtn;

	private KAWidget mSecondaryColorBtn;

	private KAWidget mTertiaryColorBtn;

	private KAToggleButton mToggleBtnFemale;

	private KAToggleButton mToggleBtnMale;

	private KAWidget mSelectedColorBtn;

	private KAWidget mMeters;

	private KAWidget mStaminaMeter;

	private KAWidget mFireMeter;

	private KAWidget mSpeedMeter;

	private KAWidget mAgeUpBtn;

	private KAWidget mDragonNameLabel;

	private KAWidget mDragonNameBackground;

	private KAWidget mDragonLevelLabel;

	private KAWidget mDragonTypeLabel;

	private KAWidget mDragonClassIcon;

	private KAWidget mDragonClassLabel;

	private KAWidget mDragonPrimaryTypeIcon;

	private KAWidget mDragonPrimaryTypeLabel;

	private KAWidget mDragonSecondaryTypeIcon;

	private KAWidget mDragonSecondaryTypeLabel;

	private KAWidget mBtnChangeName;

	private KAWidget mGlowDial;

	private Vector3[] mTextureCoords = new Vector3[3];

	private static UiDragonCustomization mInstance;

	private string mDragonName;

	private bool mDragonMale = true;

	private bool mInitApplyProperties;

	private bool mIsExiting;

	private bool mIsUsedInJournal;

	private bool mIsSavingPicture;

	private bool mIsResetAvailable;

	private RaisedPetAccessory mPetSkinAccessory = new RaisedPetAccessory();

	private Color[] mPetColors = new Color[3];

	private RaisedPetStage mLastStage;

	private bool mUiRefresh;

	private bool mInitialized;

	private bool mIsCreationUI;

	private UiJournalCustomization mUiJournalCustomization;

	private UiSelectProfile mUiSelectProfile;

	private AvPhotoManager mPhotoManager;

	private int mPetAge = -1;

	private bool mFreeCustomization;

	public string _FBAuthDBAssetPath = "RS_DATA/PfUiFBAuthorizeDBDO.unity3d/PfUiFBAuthorizeDBDO";

	[Header("Glow Effect")]
	public bool _IsFirstTime;

	public float _GlowFlashTime = 5f;

	public float _GlowFlashInterval = 0.2f;

	private float mTimeFlashCounter;

	private int mFTUEDragonNameInvalidCount;

	public static bool pIsBusy => mIsBusy;

	public static bool pEnableAvatarInputOnClose { get; set; }

	public static UiDragonCustomization pInstance => mInstance;

	public bool pIsUsedInJournal
	{
		get
		{
			return mIsUsedInJournal;
		}
		set
		{
			mIsUsedInJournal = value;
		}
	}

	public UiJournalCustomization pUiJournalCustomization
	{
		set
		{
			mUiJournalCustomization = value;
		}
	}

	public UiSelectProfile pUiSelectProfile
	{
		get
		{
			return mUiSelectProfile;
		}
		set
		{
			mUiSelectProfile = value;
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		mInstance = this;
		mDragonBtn = FindItem("CustomizeBtn");
		mEquipmentBtn = FindItem("EquipmentBtn");
		mEquipmentBtn.SetVisibility(inVisible: false);
		mColorPalette = FindItem("ColorPicker");
		mColorSelector = FindItem("ColorSelector");
		mGlowPreviewTogBtn = (KAToggleButton)FindItem("GlowPreviewTogBtn");
		mGlowUseBtn = FindItem("GlowUseBtn");
		mGlowTitleTxt = FindItem("GlowTitleTxt");
		mGlowDescTxt = FindItem("GlowDescTxt");
		mGlowTypeIcon = FindItem("IcoGlowType");
		mBackBtn = FindItem("BackBtn");
		mOKBtn = FindItem("DoneBtn");
		mNameLabel = FindItem("BgEditDragonName");
		mEditLabel = (KAEditBox)FindItem("TxtEditDragonName");
		mTalentTreeBtn = FindItem("BtnSkillTree");
		mAutoFillBtn = FindItem("BtnAutoFill");
		mCustomizeLockBtn = FindItem("ColorLockBtn");
		mResetBtn = FindItem("ResetBtn");
		mToggleBtnMale = (KAToggleButton)FindItem("ToggleBtnMale");
		mToggleBtnFemale = (KAToggleButton)FindItem("ToggleBtnFemale");
		mPrimaryColorBtn = FindItem("SkinColorBtn");
		mMeters = FindItem("Meters");
		mStaminaMeter = mMeters.FindChildItem("StaminaMeterBar");
		mFireMeter = mMeters.FindChildItem("FireMeterBar");
		mSpeedMeter = mMeters.FindChildItem("SpeedMeterBar");
		mSelectedColorBtn = mPrimaryColorBtn;
		mGlowDial = FindItem("GlowDial");
		mAgeUpBtn = FindItem("BtnAgeUp");
		mDragonClassIcon = FindItem("DragonIco");
		mDragonClassLabel = FindItem("TxtDragonClass");
		mDragonPrimaryTypeIcon = FindItem("PrimaryTypeIco");
		mDragonPrimaryTypeLabel = FindItem("TxtDragonPrimaryType");
		mDragonSecondaryTypeIcon = FindItem("SecondaryTypeIco");
		mDragonSecondaryTypeLabel = FindItem("TxtDragonSecondaryType");
		mDragonNameBackground = FindItem("BkgDragonName");
		mDragonNameLabel = FindItem("TxtDragonName");
		mDragonTypeLabel = FindItem("TxtDragonType");
		mDragonLevelLabel = FindItem("TxtDragonLvl");
		mStatsBtn = FindItem("StatsBtn");
		mUiStats = (UiStatPopUp)_UiList[0];
		mGlowEffectCountDown = FindItem("PotionTimer");
		mGlowCountDownProgressBar = FindItem("MeterBar");
		mTxtGlowCountDownTime = FindItem("TxtTimer");
		mGlowRemoveButton = FindItem("BtnRemove");
		InitColorPickerCoords();
		if (pIsUsedInJournal || (mPetData != null && mPetData.pIsNameCustomized))
		{
			mEditLabel.SetVisibility(inVisible: false);
			mNameLabel.SetVisibility(inVisible: false);
			mEditLabel.SetText(mPetData.Name);
		}
		if (SanctuaryData.IsNameChangeAllowed(mPetData) && mPetData != null && !mPetData.pIsNameCustomized)
		{
			mDragonLevelLabel.SetVisibility(inVisible: false);
			mDragonNameBackground.SetVisibility(inVisible: false);
			mDragonNameLabel.SetVisibility(inVisible: false);
		}
		mBtnChangeName = FindItem("BtnChangeName");
		if (mBtnChangeName != null)
		{
			mBtnChangeName.SetVisibility(pIsUsedInJournal);
		}
		mSecondaryColorBtn = FindItem("PatternColorBtn");
		mTertiaryColorBtn = FindItem("DetailColorBtn");
		mPetAge = RaisedPetData.GetAgeIndex(mPetData.pStage);
		if (_IsFirstTime)
		{
			_ColorSelectionObject.SetActive(value: true);
		}
		else
		{
			DragonPartTab[] tabs = _Tabs;
			foreach (DragonPartTab dragonPartTab in tabs)
			{
				if (dragonPartTab._SelectionPanel != null)
				{
					dragonPartTab._SelectionPanel.SetActive(value: false);
				}
			}
		}
		OnClick(mDragonBtn);
		if (mPetAge == 0)
		{
			mMeters.SetVisibility(inVisible: false);
			mDragonBtn.SetVisibility(inVisible: false);
		}
		mUiRefresh = true;
		mInitialized = true;
		UpdateCustomizationUI();
		if (!pIsUsedInJournal)
		{
			KAUI.SetExclusive(this, new Color(0f, 0f, 0f, 0f));
		}
		RaisedPetAttribute raisedPetAttribute = mPetData.FindAttrData("_LastCustomizedStage");
		mFreeCustomization = mFreeCustomization || raisedPetAttribute == null || string.IsNullOrEmpty(raisedPetAttribute.Value);
		SetMeters(null);
		StartGlowTimer();
	}

	private bool AllowCustomizingPet()
	{
		if (mPetData != null && PetIsToothless())
		{
			return false;
		}
		return true;
	}

	private void UpdateCustomizationUI()
	{
		if (!mInitialized)
		{
			return;
		}
		mSelectedColorBtn = mPrimaryColorBtn;
		if (SanctuaryData.GetPetCustomizationType(mPetData) == PetCustomizationType.Default)
		{
			return;
		}
		if (!SanctuaryData.IsNameChangeAllowed(mPetData))
		{
			mEditLabel.SetText(SanctuaryData.GetPetDefaultName(mPetData));
			mEditLabel.SetVisibility(inVisible: false);
			mNameLabel.SetVisibility(inVisible: false);
			if (mBtnChangeName != null)
			{
				mBtnChangeName.SetVisibility(inVisible: false);
			}
			mNameLabel.SetInteractive(isInteractive: false);
			mEditLabel.SetInteractive(isInteractive: false);
		}
		if (!SanctuaryData.IsColorChangeAllowed(mPetData))
		{
			mPrimaryColorBtn.SetInteractive(isInteractive: false);
			mSecondaryColorBtn.SetInteractive(isInteractive: false);
			mTertiaryColorBtn.SetInteractive(isInteractive: false);
			mCustomizeLockBtn.SetVisibility(inVisible: true);
			mCustomizeLockBtn.SetInteractive(isInteractive: false);
			mColorPalette.SetInteractive(isInteractive: false);
			mColorSelector.SetInteractive(isInteractive: false);
		}
		mToggleBtnMale.SetInteractive(isInteractive: false);
		mToggleBtnFemale.SetInteractive(isInteractive: false);
		mMeters.SetVisibility(inVisible: false);
	}

	private void SetMeters(ItemData inItem)
	{
		if (mStaminaMeter != null)
		{
			float num = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.ENERGY, mPetData);
			RaisedPetState raisedPetState = mPetData.FindStateData(SanctuaryPetMeterType.ENERGY.ToString());
			float num2 = 0f;
			if (raisedPetState != null)
			{
				num2 = raisedPetState.Value;
			}
			if (num == 0f)
			{
				num = 1f;
			}
			float fillAmount = num2 / num;
			mStaminaMeter.GetProgressBar().fillAmount = fillAmount;
		}
		if (mFireMeter != null)
		{
			float num3 = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.HAPPINESS, mPetData);
			RaisedPetState raisedPetState2 = mPetData.FindStateData(SanctuaryPetMeterType.HAPPINESS.ToString());
			float num4 = 0f;
			if (raisedPetState2 != null)
			{
				num4 = raisedPetState2.Value;
			}
			if (num3 == 0f)
			{
				num3 = 1f;
			}
			float fillAmount2 = num4 / num3;
			mFireMeter.GetProgressBar().fillAmount = fillAmount2;
		}
		if (!(mSpeedMeter != null))
		{
			return;
		}
		ItemData itemData = inItem;
		if (inItem == null)
		{
			int accessoryItemID = mPetData.GetAccessoryItemID(RaisedPetAccType.Saddle);
			if (accessoryItemID > 0)
			{
				UserItemData userItemData = CommonInventoryData.pInstance.FindItem(accessoryItemID);
				if (userItemData != null)
				{
					itemData = userItemData.Item;
				}
			}
		}
		float num5 = 0f;
		if (itemData != null)
		{
			num5 = itemData.GetAttribute("FlightSpeed", 0f);
		}
		UISprite progressBar = mSpeedMeter.GetProgressBar();
		progressBar.fillAmount = 0.5f;
		KAWidget kAWidget = mSpeedMeter.FindChildItem("SpeedMeterBarIncrease");
		KAWidget kAWidget2 = mSpeedMeter.FindChildItem("SpeedMeterBarDecrease");
		if (num5 >= 0f)
		{
			kAWidget.SetVisibility(inVisible: true);
			kAWidget2.SetVisibility(inVisible: false);
			kAWidget.GetProgressBar().fillAmount = progressBar.fillAmount + progressBar.fillAmount * num5;
		}
		else
		{
			kAWidget.SetVisibility(inVisible: false);
			kAWidget2.SetVisibility(inVisible: true);
			kAWidget2.GetProgressBar().fillAmount = progressBar.fillAmount;
			progressBar.fillAmount += progressBar.fillAmount * num5;
		}
	}

	public override void OnSelectMenuItem(KAWidget widget)
	{
		ItemData itemData = ((KAUISelectItemData)widget.GetUserData())._ItemData;
		if (itemData.HasCategory(643))
		{
			UITexture uITexture = widget.FindChildNGUIItem("Background") as UITexture;
			Texture iconTexture = null;
			if (uITexture != null)
			{
				iconTexture = uITexture.mainTexture;
			}
			SetGlowItem(visible: true, itemData, iconTexture);
		}
	}

	public override void SetAccessoryItem(KAWidget widget, RaisedPetAccType type)
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)widget.GetUserData();
		SetMeters(kAUISelectItemData._ItemData);
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", "EquipItem", type.ToString());
		}
		base.SetAccessoryItem(widget, type);
	}

	protected override void OnPetReady(SanctuaryPet pet)
	{
		base.OnPetReady(pet);
		if (mPet != null)
		{
			mUiRefresh = true;
			if (mPet.pMoodPrtEffect == null && !_DisablePetMoodParticles)
			{
				mPet.PlayPetMoodParticle(SanctuaryPetMeterType.HAPPINESS, isForcePlay: true);
			}
			if (_DisablePetMoodParticles)
			{
				mPet.SetMoodParticleIgnore(_DisablePetMoodParticles);
				mPet.DisableAllMoodParticles();
			}
			if (SanctuaryData.GetPetCustomizationType(mPetData) != 0)
			{
				mPet.transform.localScale = Vector3.one * mPet.pCurAgeData._UiScale;
			}
			UpdateCustomizationUI();
			mInitApplyProperties = false;
		}
	}

	public void SetUiForJournal(bool isJournal)
	{
		mIsUsedInJournal = isJournal;
		mUiRefresh = true;
	}

	private void RefreshUI()
	{
		FindItem("BkgSkills").SetVisibility(inVisible: false);
		FindItem("Skills").SetVisibility(inVisible: false);
		mBackBtn.SetVisibility(inVisible: false);
		mOKBtn.SetVisibility(!mIsUsedInJournal);
		if (mPet != null)
		{
			if (mMenu._CurrentTab._PrtTypeName != "Glow")
			{
				mPet.pIsGlowDisabled = true;
				mPet.RemoveGlowEffect();
			}
			else
			{
				mPet.pIsGlowDisabled = false;
				mPet.RefreshGlowEffect(runCoroutineOnRemove: false);
			}
		}
		RaisedPetAttribute raisedPetAttribute = mPetData.FindAttrData("_LastCustomizedStage");
		mIsCreationUI = raisedPetAttribute == null || string.IsNullOrEmpty(raisedPetAttribute.Value);
		bool flag = SanctuaryData.GetPetCustomizationType(mPetData) == PetCustomizationType.Default;
		mNameLabel.SetVisibility(mIsCreationUI);
		mEditLabel.SetVisibility(mIsCreationUI);
		mToggleBtnMale.SetVisibility(mIsCreationUI && flag);
		mToggleBtnFemale.SetVisibility(mIsCreationUI && flag);
		base.gameObject.transform.Find("Backdrop").gameObject.SetActive(!mIsUsedInJournal);
		mUiRefresh = false;
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(mPetData.PetTypeID);
		if (mAgeUpBtn != null && (mIsCreationUI || mPetAge >= sanctuaryPetTypeInfo._AgeData.Length - 1))
		{
			mAgeUpBtn.SetVisibility(inVisible: false);
		}
		if (!mIsUsedInJournal)
		{
			mCustomizeLockBtn.SetVisibility(inVisible: false);
			AvAvatar.SetUIActive(inActive: false);
		}
		else
		{
			RefreshLockBtn(isUnlockedNow: false);
		}
		if (mDragonNameLabel != null)
		{
			mDragonNameLabel.SetText(mPetData.Name);
		}
		if (mDragonTypeLabel != null)
		{
			mDragonTypeLabel.SetText(SanctuaryData.GetDisplayTextFromPetAge(mPetData.pStage) + " " + SanctuaryData.FindSanctuaryPetTypeInfo(mPetData.PetTypeID)._NameText.GetLocalizedString());
		}
		if (mDragonLevelLabel != null)
		{
			mDragonLevelLabel.SetText(_LevelLabelText.GetLocalizedString() + " " + PetRankData.GetUserRank(mPetData).RankID);
		}
		if (mDragonClassIcon != null)
		{
			string iconSprite = SanctuaryData.GetDragonClassInfo(sanctuaryPetTypeInfo._DragonClass)._IconSprite;
			if (!string.IsNullOrEmpty(iconSprite))
			{
				mDragonClassIcon.pBackground.UpdateSprite(iconSprite);
				mDragonClassIcon.SetVisibility(inVisible: true);
			}
			else
			{
				mDragonClassIcon.SetVisibility(inVisible: false);
			}
		}
		if (mDragonClassLabel != null)
		{
			string text = sanctuaryPetTypeInfo._DragonClass.ToString();
			if (!string.IsNullOrEmpty(text))
			{
				mDragonClassLabel.SetText(text);
				mDragonClassLabel.SetVisibility(inVisible: true);
			}
			else
			{
				mDragonClassLabel.SetVisibility(inVisible: false);
			}
		}
		if (sanctuaryPetTypeInfo.pPrimaryType != null)
		{
			if (mDragonPrimaryTypeIcon != null)
			{
				string iconSprite2 = sanctuaryPetTypeInfo.pPrimaryType._IconSprite;
				if (!string.IsNullOrEmpty(iconSprite2))
				{
					mDragonPrimaryTypeIcon.pBackground.UpdateSprite(iconSprite2);
					mDragonPrimaryTypeIcon.SetVisibility(inVisible: true);
				}
				else
				{
					mDragonPrimaryTypeIcon.SetVisibility(inVisible: false);
				}
			}
			if (mDragonPrimaryTypeLabel != null)
			{
				string localizedString = sanctuaryPetTypeInfo.pPrimaryType._DisplayText.GetLocalizedString();
				if (!string.IsNullOrEmpty(localizedString))
				{
					mDragonPrimaryTypeLabel.SetText(localizedString);
					mDragonPrimaryTypeLabel.SetVisibility(inVisible: true);
				}
				else
				{
					mDragonPrimaryTypeLabel.SetVisibility(inVisible: false);
				}
			}
		}
		else
		{
			if (mDragonPrimaryTypeIcon != null)
			{
				mDragonPrimaryTypeIcon.SetVisibility(inVisible: false);
			}
			if (mDragonPrimaryTypeLabel != null)
			{
				mDragonPrimaryTypeLabel.SetVisibility(inVisible: false);
			}
		}
		if (sanctuaryPetTypeInfo.pSecondaryType != null)
		{
			if (mDragonSecondaryTypeIcon != null)
			{
				string iconSprite3 = sanctuaryPetTypeInfo.pSecondaryType._IconSprite;
				if (!string.IsNullOrEmpty(iconSprite3))
				{
					mDragonSecondaryTypeIcon.pBackground.UpdateSprite(iconSprite3);
					mDragonSecondaryTypeIcon.SetVisibility(inVisible: true);
				}
				else
				{
					mDragonSecondaryTypeIcon.SetVisibility(inVisible: false);
				}
			}
			if (mDragonSecondaryTypeLabel != null)
			{
				string localizedString2 = sanctuaryPetTypeInfo.pSecondaryType._DisplayText.GetLocalizedString();
				if (!string.IsNullOrEmpty(localizedString2))
				{
					mDragonSecondaryTypeLabel.SetText(localizedString2);
					mDragonSecondaryTypeLabel.SetVisibility(inVisible: true);
				}
				else
				{
					mDragonSecondaryTypeLabel.SetVisibility(inVisible: false);
				}
			}
		}
		else
		{
			if (mDragonSecondaryTypeIcon != null)
			{
				mDragonSecondaryTypeIcon.SetVisibility(inVisible: false);
			}
			if (mDragonSecondaryTypeLabel != null)
			{
				mDragonSecondaryTypeLabel.SetVisibility(inVisible: false);
			}
		}
	}

	private bool PetIsToothless()
	{
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo("NightFury");
		if (sanctuaryPetTypeInfo != null)
		{
			return mPetData.PetTypeID == sanctuaryPetTypeInfo._TypeID;
		}
		return false;
	}

	public void RefreshLockBtn(bool isUnlockedNow)
	{
		mCustomizeLockBtn.SetVisibility(mUiJournalCustomization.IsDragonCustomizationLocked() && !mFreeCustomization);
		if (isUnlockedNow)
		{
			RemoveDragonSkin();
		}
	}

	public override void OnPressRepeated(KAWidget inWidget, bool inPressed)
	{
		base.OnPressRepeated(inWidget, inPressed);
		if (inWidget == mColorPalette && (!mIsUsedInJournal || mFreeCustomization || mUiJournalCustomization.VerifyDragonCustomizationTicket(inPressed)) && inPressed)
		{
			Transform transform = inWidget.transform.Find("Background");
			UITexture component = transform.GetComponent<UITexture>();
			UISlicedSprite obj = mColorSelector.pBackground as UISlicedSprite;
			Vector3 vector = Vector3.zero;
			if (component != null)
			{
				vector = new Vector3(component.width, component.height, 1f);
			}
			Vector2 vector2 = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			Vector3 position = transform.position - vector * 0.5f;
			Vector3 position2 = transform.position + vector * 0.5f;
			position = UICamera.currentCamera.WorldToScreenPoint(position);
			position2 = UICamera.currentCamera.WorldToScreenPoint(position2);
			float b = (vector2.x - position.x) / (position2.x - position.x);
			float b2 = (vector2.y - position.y) / (position2.y - position.y);
			float num = (float)obj.width / (position2.x - position.x);
			float num2 = (float)obj.height / (position2.y - position.y);
			b = Mathf.Min(Mathf.Max(num * 0.5f, b), 1f - num * 0.5f);
			b2 = Mathf.Min(Mathf.Max(num2 * 0.5f, b2), 1f - num2 * 0.5f);
			Vector3 vector3 = UICamera.currentCamera.ScreenToWorldPoint(new Vector3(position.x + b * (position2.x - position.x), position.y + b2 * (position2.y - position.y), 0f));
			mColorSelector.transform.position = new Vector3(vector3.x, vector3.y, mColorSelector.transform.position.z);
			Color pixelBilinear = ((Texture2D)inWidget.GetTexture()).GetPixelBilinear(b, b2);
			mSelectedColorBtn.pBackground.color = pixelBilinear;
			if (mSelectedColorBtn == mPrimaryColorBtn)
			{
				mPrimaryColor = pixelBilinear;
				mRebuildTexture = true;
				mTextureCoords[0] = vector3;
			}
			else if (mSelectedColorBtn == mSecondaryColorBtn)
			{
				mSecondaryColor = pixelBilinear;
				mRebuildTexture = true;
				mTextureCoords[1] = vector3;
			}
			else if (mSelectedColorBtn == mTertiaryColorBtn)
			{
				mTertiaryColor = pixelBilinear;
				mRebuildTexture = true;
				mTextureCoords[2] = vector3;
			}
			if (mRebuildTexture)
			{
				RemoveDragonSkin();
				mIsResetAvailable = true;
				RefreshResetBtn();
			}
			mMenu.mModified = true;
		}
	}

	private void InitColorPickerCoords()
	{
		mTextureCoords[0] = (mTextureCoords[1] = (mTextureCoords[2] = Vector3.one * -1f));
	}

	private void SetColorSelector()
	{
		Color white = Color.white;
		Vector3 vector = Vector3.one * -1f;
		if (mSelectedColorBtn == mPrimaryColorBtn)
		{
			white = mPrimaryColor;
			vector = mTextureCoords[0];
		}
		else if (mSelectedColorBtn == mSecondaryColorBtn)
		{
			white = mSecondaryColor;
			vector = mTextureCoords[1];
		}
		else if (mSelectedColorBtn == mTertiaryColorBtn)
		{
			white = mTertiaryColor;
			vector = mTextureCoords[2];
		}
		if (vector != Vector3.one * -1f)
		{
			mColorSelector.transform.position = new Vector3(vector.x, vector.y, mColorSelector.transform.position.z);
			return;
		}
		Transform obj = mColorPalette.transform.Find("Background");
		UITexture component = obj.GetComponent<UITexture>();
		Texture2D texture2D = (Texture2D)mColorPalette.GetTexture();
		_ = mColorSelector.pBackground;
		Vector3 vector2 = Vector3.zero;
		if (component != null)
		{
			vector2 = new Vector3(texture2D.width, texture2D.height, 1f);
		}
		Vector3 position = obj.position - vector2 * 0.5f;
		Vector3 position2 = obj.position + vector2 * 0.5f;
		position = UICamera.currentCamera.WorldToScreenPoint(position);
		position2 = UICamera.currentCamera.WorldToScreenPoint(position2);
		Color[] pixels = ((Texture2D)mColorPalette.GetTexture()).GetPixels();
		int num = -1;
		float num2 = 0.005f;
		for (int i = 0; i < pixels.Length; i++)
		{
			float r = white.r;
			float g = white.g;
			float b = white.b;
			float r2 = pixels[i].r;
			float g2 = pixels[i].g;
			float b2 = pixels[i].b;
			if (Mathf.Abs(r - r2) <= num2 && Mathf.Abs(g - g2) <= num2 && Mathf.Abs(b - b2) <= num2)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			num = 0;
		}
		Vector2 vector3 = new Vector2((float)num % vector2.x, (float)num / vector2.x) - new Vector2(0.5f / vector2.x, 0.5f / vector2.y);
		((Texture2D)mColorPalette.GetTexture()).GetPixel((int)vector3.x, (int)vector3.y);
		float num3 = vector3.x / vector2.x;
		float num4 = vector3.y / vector2.y;
		Vector3 vector4 = UICamera.currentCamera.ScreenToWorldPoint(new Vector3(position.x + num3 * (position2.x - position.x), position.y + num4 * (position2.y - position.y), 0f));
		mColorSelector.transform.position = new Vector3(vector4.x, vector4.y, mColorSelector.transform.position.z);
	}

	private void RefreshResetBtn()
	{
		if (mIsResetAvailable)
		{
			if (mResetBtn != null)
			{
				mResetBtn.SetVisibility(inVisible: true);
			}
			return;
		}
		RaisedPetAccessory accessory = mPetData.GetAccessory(RaisedPetAccType.Materials);
		if (accessory != null)
		{
			mPetSkinAccessory.Geometry = accessory.Geometry;
			mPetSkinAccessory.Type = accessory.Type;
			mPetSkinAccessory.Texture = accessory.Texture;
			mPetSkinAccessory.UserItemData = accessory.UserItemData;
			mPetSkinAccessory.UserInventoryCommonID = accessory.UserInventoryCommonID;
		}
		if (mPetData != null && (mPetData.Colors == null || mPetData.Colors.Length < 3))
		{
			mPetColors[0] = GetPairDataColor("Color_PrimaryDragonC");
			mPetColors[1] = GetPairDataColor("Color_SecondaryDragonC");
			mPetColors[2] = GetPairDataColor("Color_TertiaryDragonC");
		}
		else
		{
			mPetColors[0] = mPetData.GetColor(0);
			mPetColors[1] = mPetData.GetColor(1);
			mPetColors[2] = mPetData.GetColor(2);
		}
		if (mResetBtn != null)
		{
			mResetBtn.SetVisibility(inVisible: false);
		}
	}

	private void ResetCustomization()
	{
		mPrimaryColor = mPetColors[0];
		mSecondaryColor = mPetColors[1];
		mTertiaryColor = mPetColors[2];
		mPrimaryColorBtn.pBackground.color = mPrimaryColor;
		mSecondaryColorBtn.pBackground.color = mSecondaryColor;
		mTertiaryColorBtn.pBackground.color = mTertiaryColor;
		mSelectedColorBtn = mPrimaryColorBtn;
		InitColorPickerCoords();
		mPet.UpdateData(SanctuaryManager.pCurPetInstance.pData, noHat: false);
		mPet.SetColors(mPrimaryColor, mSecondaryColor, mTertiaryColor, bSaveData: false);
		if (SanctuaryManager.pCurPetInstance != null && mPetData.RaisedPetID == SanctuaryManager.pCurPetData.RaisedPetID)
		{
			SanctuaryManager.pCurPetInstance.SetColors(mPrimaryColor, mSecondaryColor, mTertiaryColor, bSaveData: false);
		}
		string text = mPetSkinAccessory.Geometry.Split('&')[0];
		if (!string.IsNullOrEmpty(text))
		{
			int accessoryItemID = mPetData.GetAccessoryItemID(mPetSkinAccessory);
			mPetData.SetAccessory(RaisedPetAccType.Materials, text, mPetSkinAccessory.Texture, accessoryItemID);
			GameObject accessoryObject = SanctuaryManager.pCurPetInstance.GetAccessoryObject(RaisedPetAccType.Materials);
			if (accessoryObject != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(accessoryObject);
				if (gameObject != null)
				{
					mPet.SetAccessory(RaisedPetAccType.Materials, gameObject, null);
				}
			}
		}
		mMenu.mModified = true;
		mIsResetAvailable = false;
		RefreshResetBtn();
		SetColorSelector();
	}

	private void BuyCustomization()
	{
		mUiJournalCustomization.VerifyDragonCustomizationTicket(inStatus: false);
	}

	private void RemoveDragonSkin()
	{
		mPet.SetAccessory(RaisedPetAccType.Materials, null, null);
		mPetData.SetAccessory(RaisedPetAccType.Materials, "NULL", "", -1);
		mMenu.mModified = true;
	}

	public string GetDragonRendererName()
	{
		if (mPet != null)
		{
			return mPet.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().name;
		}
		return "";
	}

	public void ShowColorPalette(bool inStatus)
	{
		mColorPalette.SetVisibility(inStatus);
		mColorSelector.SetVisibility(inStatus);
	}

	public void SetGlowItem(bool visible, ItemData item = null, Texture iconTexture = null)
	{
		mGlowTitleTxt.SetVisibility(visible);
		mGlowDescTxt.SetVisibility(visible);
		mGlowPreviewTogBtn.SetVisibility(visible);
		mGlowUseBtn.SetVisibility(visible);
		mGlowTypeIcon.SetVisibility(visible);
		mGlowItemData = item;
		UtDebug.LogError("Close called, Reset dragon!!!");
		mGlowPreviewTogBtn.SetChecked(isChecked: false);
		mPet.RefreshGlowEffect(runCoroutineOnRemove: false);
		if (!visible)
		{
			mMenu.SetSelectedItem(null);
		}
		if (item != null)
		{
			mGlowTitleTxt.SetText(item.ItemName);
			mGlowDescTxt.SetText(item.Description);
			mGlowTypeIcon.SetTexture(iconTexture);
		}
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		base.OnDrag(inWidget, inDelta);
		if (!string.IsNullOrEmpty(_DragWidgetName) && inWidget.name == _DragWidgetName)
		{
			float num = (0f - _RotationSpeed) * Time.deltaTime * inDelta.x;
			if (num != 0f)
			{
				mPet.transform.Rotate(0f, num, 0f);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mUiRefresh && mInitialized)
		{
			RefreshUI();
		}
		if (_ColorSelectionObject != null && (mMenu._CurrentTab._SelectionPanel == _ColorSelectionObject || _IsFirstTime) && !mInitApplyProperties && SubstanceCustomization.pInstance != null && SubstanceCustomization.pInstance.pPairData != null && (bool)mPet)
		{
			mInitApplyProperties = true;
			Color white = Color.white;
			Color white2 = Color.white;
			Color white3 = Color.white;
			if (mPetData != null && (mPetData.Colors == null || mPetData.Colors.Length < 3))
			{
				white = GetPairDataColor("Color_PrimaryDragonC");
				white2 = GetPairDataColor("Color_SecondaryDragonC");
				white3 = GetPairDataColor("Color_TertiaryDragonC");
				mMenu.mModified = true;
			}
			else
			{
				white = mPetData.GetColor(0);
				white2 = mPetData.GetColor(1);
				white3 = mPetData.GetColor(2);
			}
			mPrimaryColor = white;
			mPrimaryColorBtn.pBackground.color = white;
			mSecondaryColor = white2;
			mSecondaryColorBtn.pBackground.color = white2;
			mTertiaryColor = white3;
			mTertiaryColorBtn.pBackground.color = white3;
			mRebuildTexture = true;
			mIsResetAvailable = mMenu.mModified;
			RefreshResetBtn();
			SetColorSelector();
		}
		if (mRebuildTexture)
		{
			mPet.SetColors(mPrimaryColor, mSecondaryColor, mTertiaryColor, bSaveData: false);
			if (SanctuaryManager.pCurPetInstance != null && mPetData.RaisedPetID == SanctuaryManager.pCurPetData.RaisedPetID)
			{
				SanctuaryManager.pCurPetInstance.SetColors(mPrimaryColor, mSecondaryColor, mTertiaryColor, bSaveData: false);
			}
			if (mMenu._CurrentTab._PrtTypeName != "Glow" && !mPet.pIsGlowDisabled)
			{
				mPet.pIsGlowDisabled = true;
				mPet.RemoveGlowEffect();
			}
			mRebuildTexture = false;
		}
		if (!mIsUsedInJournal && mIsExiting && !mIsBusy)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			KAUI.RemoveExclusive(this);
			UnityEngine.Object.Destroy(base.gameObject);
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage("OnDragonCustomizationClosed", null, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				AvAvatar.SetActive(inActive: true);
				AvAvatar.pState = AvAvatarState.IDLE;
				AvAvatar.SetUIActive(inActive: true);
			}
			if (mPetData.RaisedPetID == SanctuaryManager.pCurPetData.RaisedPetID)
			{
				if (SanctuaryManager.pInstance.pPetMeter != null)
				{
					SanctuaryManager.pInstance.pPetMeter.SetPetName(mPetData.Name);
				}
				if (SanctuaryManager.pInstance.OnNameSelectionDone != null)
				{
					SanctuaryManager.pInstance.OnNameSelectionDone();
				}
			}
		}
		if (!(mGlowEffectCountDown != null) || !(SanctuaryManager.pCurPetInstance != null))
		{
			return;
		}
		mGlowCountDown.UpdateCountDown();
		if (mGlowCountDown.pIsTimerRunning)
		{
			if (mGlowCountDown.pTimeInSecs <= (double)_GlowFlashTime)
			{
				mTimeFlashCounter += Time.deltaTime;
				if (mTimeFlashCounter > _GlowFlashInterval)
				{
					mGlowDial.SetVisibility(!mGlowDial.GetVisibility());
					mTimeFlashCounter = 0f;
				}
			}
			if (!mGlowEffectCountDown.GetVisibility())
			{
				mGlowEffectCountDown.SetVisibility(inVisible: true);
				SetGlowCountDownColor(mPetData.pGlowEffect.GlowColor);
			}
			if (mGlowCountDown.pIsTimerUIMarkDirty)
			{
				if (mGlowCountDownProgressBar != null)
				{
					mGlowCountDownProgressBar.SetProgressLevel(mGlowCountDown.pPercentRemain);
				}
				if (mTxtGlowCountDownTime != null)
				{
					mTxtGlowCountDownTime.SetText(mGlowCountDown.pTimeInHHMMSS);
				}
				mGlowCountDown.pIsTimerUIMarkDirty = false;
			}
		}
		else if (mGlowEffectCountDown.GetVisibility())
		{
			mGlowEffectCountDown.SetVisibility(inVisible: false);
		}
	}

	private void SetGlowCountDownColor(string tintColor)
	{
		for (int i = 0; i < _GlowColorTintChildList.Length; i++)
		{
			_GlowColorTintChildList[i].color = GlowManager.pInstance.GetColor(tintColor);
		}
	}

	public override void OnClick(KAWidget inItem)
	{
		base.OnClick(inItem);
		if (inItem == mEditLabel)
		{
			mEditLabel.OnSelect(inSelected: true);
		}
		else if (inItem == mDragonBtn)
		{
			_MenuList[0].ClearItems();
			if (mMenu != null && mMenu.GetItemCount() > 0)
			{
				mMenu.ClearItems();
			}
			mDragonBtn.SetDisabled(isDisabled: true);
			foreach (DragonPartCat item in _DragonPartCategory)
			{
				KAWidget kAWidget = DuplicateWidget("CatTemplateBtn");
				kAWidget.SetVisibility(inVisible: true);
				kAWidget.gameObject.name = item._BtnName;
				kAWidget.SetTexture(item._Icon);
				kAWidget._TooltipInfo._Text = item._ToolTipText;
				_MenuList[0].AddWidget(kAWidget);
			}
			if (_MenuList[0].GetNumItems() > 0)
			{
				KAWidget itemAt = _MenuList[0].GetItemAt(0);
				OnClick(itemAt);
			}
		}
		else
		{
			if (inItem == mEquipmentBtn)
			{
				return;
			}
			if (inItem == mToggleBtnMale)
			{
				mDragonMale = true;
				return;
			}
			if (inItem == mToggleBtnFemale)
			{
				mDragonMale = false;
				return;
			}
			if (inItem == mOKBtn)
			{
				mDragonName = mEditLabel.GetText().Trim();
				if (mIsCreationUI && SanctuaryData.IsNameChangeAllowed(mPetData))
				{
					if (string.IsNullOrEmpty(mDragonName) || !mEditLabel.IsValidText() || mDragonName == mEditLabel._DefaultText.GetLocalizedString())
					{
						mDragonName = null;
						InvalidName();
					}
					else if (mPetData.pIsNameCustomized)
					{
						OnNameConfirmed();
					}
					else
					{
						ShowNameConfirmation();
					}
				}
				else
				{
					OnNameConfirmed();
				}
				return;
			}
			if (inItem == mPrimaryColorBtn || inItem == mSecondaryColorBtn || inItem == mTertiaryColorBtn)
			{
				if (mIsUsedInJournal)
				{
					if (mFreeCustomization || mUiJournalCustomization.VerifyDragonCustomizationTicket(inStatus: false))
					{
						mSelectedColorBtn = inItem;
					}
				}
				else
				{
					mSelectedColorBtn = inItem;
				}
				SetColorSelector();
				return;
			}
			if (inItem == mTalentTreeBtn)
			{
				if (mPetAge < 2)
				{
					ShowPetMessage(_PetTeenText.GetLocalizedString());
				}
				return;
			}
			if (inItem == mAutoFillBtn)
			{
				if (mPetAge < 2)
				{
					ShowPetMessage(_PetTeenText.GetLocalizedString());
				}
				return;
			}
			if (inItem == mAgeUpBtn)
			{
				mLastStage = mPetData.pStage;
				mAgeUpBtn.SetInteractive(isInteractive: false);
				DragonAgeUpConfig.Trigger(_AgeUpUiTrigger, OnDragonAgeUpDone, mPetData);
				return;
			}
			if (inItem == mBtnChangeName)
			{
				if (SanctuaryManager.pInstance != null)
				{
					SanctuaryManager.pInstance.InitNameChange(mPetData, base.gameObject);
				}
				return;
			}
			if (inItem == mStatsBtn)
			{
				ShowDragonStats();
				return;
			}
			if (inItem == mGlowPreviewTogBtn)
			{
				OnGlowPreviewToggled();
				return;
			}
			if (inItem == mGlowUseBtn)
			{
				ShowApplyGlowConfirmation();
				return;
			}
			if (inItem == mGlowEffectCountDown)
			{
				if (!mTxtGlowCountDownTime.GetVisibility())
				{
					mTxtGlowCountDownTime.SetVisibility(inVisible: true);
				}
				return;
			}
			if (inItem == mGlowRemoveButton)
			{
				ShowRemoveGlowConfirmation();
				return;
			}
			if (mResetBtn != null && inItem == mResetBtn)
			{
				ResetCustomization();
				return;
			}
			if (inItem == mCustomizeLockBtn)
			{
				BuyCustomization();
				return;
			}
			for (int i = 0; i < _MenuList[0].GetNumItems(); i++)
			{
				KAWidget kAWidget2 = _MenuList[0].FindItemAt(i);
				if (!(kAWidget2 == null))
				{
					kAWidget2.SetDisabled(kAWidget2.name == inItem.name);
				}
			}
		}
	}

	private void OnGlowPreviewToggled()
	{
		if (!(mGlowPreviewTogBtn == null))
		{
			UtDebug.LogError("GLOW PREVIEW CALLED " + mGlowPreviewTogBtn.IsChecked());
			if (mGlowPreviewTogBtn.IsChecked())
			{
				mPet.ApplyGlowEffect(mGlowItemData.GetAttribute("Color", ""));
			}
			else
			{
				mPet.RefreshGlowEffect(runCoroutineOnRemove: false);
			}
		}
	}

	protected override void SelectTab(DragonPartTab partTab)
	{
		base.SelectTab(partTab);
		if (mGlowItemData != null)
		{
			SetGlowItem(visible: false);
		}
		if (mPet == null)
		{
			return;
		}
		if (partTab._PrtTypeName != "Glow")
		{
			if (!mPet.pIsGlowDisabled)
			{
				mPet.pIsGlowDisabled = true;
				mPet.RemoveGlowEffect();
			}
		}
		else
		{
			mPet.pIsGlowDisabled = false;
			mPet.RefreshGlowEffect(runCoroutineOnRemove: false);
		}
	}

	private void ShowRemoveGlowConfirmation()
	{
		mUiGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		mUiGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		mUiGenericDB.SetText(_RemoveGlowConfirmationText.GetLocalizedString(), interactive: false);
		mUiGenericDB._MessageObject = base.gameObject;
		mUiGenericDB._YesMessage = "OnRemoveGlowDone";
		mUiGenericDB._NoMessage = "KillGenericDB";
		KAUI.SetExclusive(mUiGenericDB);
	}

	private void StartGlowTimer()
	{
		if (mPetData != null && mPetData.IsGlowAvailable())
		{
			mGlowDial.SetVisibility(inVisible: true);
			mTimeFlashCounter = 0f;
			mGlowCountDown.StartTimer(mPetData.pGlowEffect.Duration, mPetData.pGlowEffect.EndTime);
			SetGlowCountDownColor(mPetData.pGlowEffect.GlowColor);
			mGlowCountDown.SetTickInterval(0.9f);
			mGlowCountDown.SetCountDownEndCallback(OnRemoveGlowDone);
			SanctuaryManager.pCurPetInstance.StartGlowTimer();
		}
	}

	private void OnRemoveGlowDone()
	{
		KillGenericDB();
		mGlowCountDown.StartTimer(0.0, DateTime.MinValue);
		mPet.RemoveGlowEffect();
		mPetData.pGlowEffect.EndTime = DateTime.MinValue;
		mPetData.SaveGlowData();
		SanctuaryManager.pCurPetInstance.ResetGlowTimerCallback();
		SetMMORaisedPetData();
		mGlowPreviewTogBtn.SetChecked(isChecked: false);
	}

	private void ShowApplyGlowConfirmation()
	{
		mUiGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		mUiGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		string text = "";
		text = (mPetData.IsGlowRunning() ? _OverwriteGlowConfirmationText.GetLocalizedString() : _ApplyGlowConfirmationText.GetLocalizedString());
		mUiGenericDB.SetText(text, interactive: false);
		mUiGenericDB._MessageObject = base.gameObject;
		mUiGenericDB._YesMessage = "OnApplyGlowDone";
		mUiGenericDB._NoMessage = "KillGenericDB";
		KAUI.SetExclusive(mUiGenericDB);
	}

	private void OnApplyGlowDone()
	{
		KillGenericDB();
		if (mGlowItemData != null && CommonInventoryData.pIsReady)
		{
			UtDebug.Log("Glow applied and saving: " + mGlowItemData.ItemName);
			KAUICursorManager.SetDefaultCursor("Loading");
			SetInteractive(interactive: false);
			string attribute = mGlowItemData.GetAttribute("Color", "");
			string attribute2 = mGlowItemData.GetAttribute("Duration", "");
			mPetData.SaveGlowData(attribute2, attribute);
			mPetData.SaveDataReal(OnGlowSaveDone);
		}
		else
		{
			UtDebug.LogError("Glow item is null or Commoninventory is null!!");
		}
	}

	public void OnGlowSaveDone(SetRaisedPetResponse response)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
		if (response != null && response.RaisedPetSetResult == RaisedPetSetResult.Success)
		{
			UtDebug.Log("Glow Save done");
			CommonInventoryData.pInstance.RemoveItem(mGlowItemData.ItemID, updateServer: true);
			SetGlowItem(visible: false);
			mMenu.ChangeCategory(mMenu._CurrentTab._PrtTypeName, forceChange: true);
			StartGlowTimer();
			SetMMORaisedPetData();
			SanctuaryManager.pCurPetInstance.StopGlowCouroutine();
		}
		else
		{
			UtDebug.LogError("Glow Save failed");
		}
	}

	private void OnNameChangeDone()
	{
		base.transform.root.gameObject.BroadcastMessage("RefreshUI", SendMessageOptions.DontRequireReceiver);
	}

	public void OnFBAuthDBLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		if (inLoadEvent == RsResourceLoadEvent.COMPLETE)
		{
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			UnityEngine.Object.Instantiate((GameObject)inObject).name = "PfUiFBAuthorizeDBDO";
		}
	}

	private void ShowNameConfirmation()
	{
		mUiGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		mUiGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		mUiGenericDB.SetText(_PetNameConfirmationText.GetLocalizedString(), interactive: false);
		mUiGenericDB._MessageObject = base.gameObject;
		mUiGenericDB._YesMessage = "OnNameConfirmed";
		mUiGenericDB._NoMessage = "KillGenericDB";
		KAUI.SetExclusive(mUiGenericDB);
	}

	private void KillGenericDB()
	{
		if (!(mUiGenericDB == null))
		{
			UnityEngine.Object.Destroy(mUiGenericDB.gameObject);
			mUiGenericDB = null;
		}
	}

	private void OnNameConfirmed()
	{
		KillGenericDB();
		mMenu.mModified = true;
		SetState(KAUIState.NOT_INTERACTIVE);
		KAUICursorManager.SetDefaultCursor("Loading");
		OnCloseCustomization();
	}

	private void ShowPetMessage(string inText)
	{
		mUiGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Dragon Age Confirmation");
		mUiGenericDB._MessageObject = base.gameObject;
		mUiGenericDB._OKMessage = "OnPetAgeMessageOkClick";
		mUiGenericDB.SetText(inText, interactive: false);
		mUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(mUiGenericDB);
	}

	private void OnPetAgeMessageOkClick()
	{
		UnityEngine.Object.Destroy(mUiGenericDB.gameObject);
		mUiGenericDB = null;
	}

	public void OnEnable()
	{
		if (mReloadDragon)
		{
			if (mPet != null)
			{
				UnityEngine.Object.Destroy(mPet.gameObject);
			}
			LoadDragon();
			mReloadDragon = false;
		}
		SubstanceCustomization.Init("Dragon");
		SetMeters(null);
	}

	public void OnDisable()
	{
		if (mGlowItemData != null)
		{
			SetGlowItem(visible: false);
		}
		mReloadDragon = true;
	}

	private void InvalidName()
	{
		if (UnityAnalyticsAgent.pNewUser)
		{
			mFTUEDragonNameInvalidCount++;
		}
		KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		kAUIGenericDB.SetText(_EnterValidNameText.GetLocalizedString(), interactive: false);
		kAUIGenericDB._MessageObject = base.gameObject;
		kAUIGenericDB._OKMessage = "DestroyMessageDB";
		kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(kAUIGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
	}

	public void SaveDragonData()
	{
		mMenu.mModified = false;
		if (!string.IsNullOrEmpty(mDragonName))
		{
			mPetData.pIsNameCustomized = true;
			mPetData.Name = (SanctuaryData.IsNameChangeAllowed(mPetData) ? mDragonName : SanctuaryData.GetPetDefaultName(mPetData));
		}
		mPetData.Gender = (mDragonMale ? Gender.Male : Gender.Female);
		SetPairDataColor("Color_PrimaryDragonC", mPrimaryColor);
		SetPairDataColor("Color_SecondaryDragonC", mSecondaryColor);
		SetPairDataColor("Color_TertiaryDragonC", mTertiaryColor);
		RaisedPetData raisedPetData = mPetData;
		int pStage = (int)mPetData.pStage;
		raisedPetData.SetAttrData("_LastCustomizedStage", pStage.ToString() ?? "", DataType.INT);
		mPetData.SaveDataReal(PetSaveEventHandler);
		SanctuaryManager.pPendingMMOPetCheck = true;
	}

	public void OnPetPictureDone(object inPicture)
	{
		UtDebug.Log("Pet picture save success: " + (inPicture != null));
		if (mPet != null && mPet.gameObject != null)
		{
			UnityEngine.Object.Destroy(mPet.gameObject);
		}
		mIsSavingPicture = false;
		RsResourceManager.UnloadUnusedAssets();
		mIsBusy = false;
		mIsExiting = true;
	}

	public void PetSaveEventHandler(SetRaisedPetResponse response)
	{
		if (response != null)
		{
			switch (response.RaisedPetSetResult)
			{
			case RaisedPetSetResult.Success:
				if (SanctuaryManager.pInstance != null && mPet != null)
				{
					if (mUiJournalCustomization != null)
					{
						SanctuaryManager.pInstance.TakePicture(mPet.gameObject, mUiJournalCustomization.gameObject);
					}
					else
					{
						SanctuaryManager.pInstance.TakePicture(mPet.gameObject, base.gameObject);
					}
					if (SanctuaryManager.pInstance.pPetMeter != null)
					{
						SanctuaryManager.pInstance.pPetMeter.RefreshAll();
					}
					mIsSavingPicture = true;
				}
				if (FUEManager.pIsFUERunning)
				{
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("failCount", mFTUEDragonNameInvalidCount);
					AnalyticAgent.LogFTUEEvent(FTUEEvent.DRAGON_NAME_ACCEPTED, dictionary);
				}
				AllSaveDone(status: true);
				break;
			case RaisedPetSetResult.Failure:
				if (UnityAnalyticsAgent.pNewUser)
				{
					mFTUEDragonNameInvalidCount++;
				}
				AllSaveDone(status: false);
				Debug.LogError(response.ErrorMessage);
				break;
			case RaisedPetSetResult.Invalid:
				if (UnityAnalyticsAgent.pNewUser)
				{
					mFTUEDragonNameInvalidCount++;
				}
				AllSaveDone(status: false);
				Debug.LogError(response.ErrorMessage);
				break;
			case RaisedPetSetResult.InvalidPetName:
				if (!mIsUsedInJournal)
				{
					if (mPetData != null)
					{
						mPetData.pIsNameCustomized = false;
					}
					SetState(KAUIState.INTERACTIVE);
					KAUICursorManager.SetDefaultCursor("Arrow");
					ShowPetMessage(_PetNameInvalidText.GetLocalizedString());
					if (UnityAnalyticsAgent.pNewUser)
					{
						mFTUEDragonNameInvalidCount++;
					}
				}
				else
				{
					mDragonName = null;
					mPetData.Name = null;
					SaveDragonData();
				}
				break;
			}
		}
		else
		{
			AllSaveDone(status: false);
		}
	}

	private void AllSaveDone(bool status)
	{
		SetState(KAUIState.NOT_INTERACTIVE);
		if (!mIsSavingPicture)
		{
			RsResourceManager.UnloadUnusedAssets();
			mIsBusy = false;
			mIsExiting = true;
			if (mPet != null)
			{
				UnityEngine.Object.Destroy(mPet.gameObject);
			}
		}
		if (mIsUsedInJournal)
		{
			mUiJournalCustomization.DragonCustomizationDone(status);
		}
	}

	public void OnCloseCustomization()
	{
		mIsBusy = true;
		if (_MenuList != null && _MenuList.Length > 1)
		{
			((KAUISelectDragonMenu)_MenuList[1]).SaveSelection();
		}
		if (mMenu.mModified)
		{
			mPet.UpdateData(mPetData, noHat: false);
			mPet.SetColors(mPrimaryColor, mSecondaryColor, mTertiaryColor, bSaveData: true);
			if (mPetData.RaisedPetID == SanctuaryManager.pCurPetData.RaisedPetID)
			{
				SanctuaryManager.pCurPetInstance.UpdateData(SanctuaryManager.pCurPetInstance.pData, noHat: false);
				SanctuaryManager.pCurPetInstance.SetColors(mPrimaryColor, mSecondaryColor, mTertiaryColor, bSaveData: true);
			}
			SaveDragonData();
			SetMMORaisedPetData();
			if (!mIsUsedInJournal && SubstanceCustomization.pInstance != null)
			{
				SubstanceCustomization.pInstance.SaveData();
			}
		}
		else
		{
			AllSaveDone(status: false);
		}
	}

	private void SetMMORaisedPetData()
	{
		if (!(MainStreetMMOClient.pInstance == null) && MainStreetMMOClient.pInstance.IsLevelMMO(RsResourceManager.pCurrentLevel))
		{
			SanctuaryPet pCurPetInstance = SanctuaryManager.pCurPetInstance;
			int mount = (int)(pCurPetInstance.pIsMounted ? pCurPetInstance.pCurrentSkillType : ((PetSpecialSkillType)(-1)));
			MainStreetMMOClient.pInstance.SetRaisedPet(SanctuaryManager.pCurPetInstance.pData, mount);
		}
	}

	private void OnCloseDB()
	{
		SetState(KAUIState.INTERACTIVE);
	}

	private Color GetPairDataColor(string key)
	{
		Color color = Color.white;
		if (SubstanceCustomization.pInstance == null || SubstanceCustomization.pInstance.pPairData == null)
		{
			return color;
		}
		string value = SubstanceCustomization.pInstance.pPairData.GetValue(key);
		if (!value.Equals("LIST_NOT_VALID") && !value.Equals("___VALUE_NOT_FOUND___"))
		{
			HexUtil.HexToColor(value, out color);
		}
		return color;
	}

	private void SetPairDataColor(string key, Color c)
	{
		if (SubstanceCustomization.pInstance != null && SubstanceCustomization.pInstance.pPairData != null)
		{
			string inValue = HexUtil.ColorToHex(c);
			SubstanceCustomization.pInstance.pPairData.SetValue(key, inValue);
			SubstanceCustomization.pInstance.mIsDirty = true;
		}
	}

	private void OnDragonAgeUpDone()
	{
		mGlowPreviewTogBtn.SetChecked(isChecked: false);
		if (mPetData.pStage != mLastStage)
		{
			mPetAge = RaisedPetData.GetAgeIndex(mPetData.pStage);
			if (mIsUsedInJournal)
			{
				mFreeCustomization = true;
				RefreshLockBtn(isUnlockedNow: true);
			}
			if (mPet != null)
			{
				if (mPetData.RaisedPetID == SanctuaryManager.pCurPetData.RaisedPetID)
				{
					SanctuaryManager.pCurPetInstance.SetAvatar(mPet.mAvatar, SpawnTeleportEffect: false);
				}
				UnityEngine.Object.Destroy(mPet.gameObject);
				LoadDragon();
			}
			SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(mPetData.PetTypeID);
			if (mAgeUpBtn != null && mPetAge >= sanctuaryPetTypeInfo._AgeData.Length - 1)
			{
				mAgeUpBtn.SetVisibility(inVisible: false);
			}
			else
			{
				mAgeUpBtn.SetInteractive(isInteractive: true);
			}
			mMenu.mModified = true;
		}
		else if (mAgeUpBtn != null)
		{
			mAgeUpBtn.SetInteractive(isInteractive: true);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (pEnableAvatarInputOnClose)
		{
			AvAvatar.pInputEnabled = true;
			pEnableAvatarInputOnClose = false;
		}
		mInstance = null;
	}

	private void ShowDragonStats()
	{
		if (mPetData != null)
		{
			SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(mPetData.PetTypeID);
			CharacterData characterData = new CharacterData(CharacterDatabase.pInstance.GetCharacter(sanctuaryPetTypeInfo._Name, mPetData));
			mUiStats.SetVisibility(inVisible: true);
			mUiStats.ShowCombatStats(characterData, mPetData.pRank, sanctuaryPetTypeInfo._NameText.GetLocalizedString());
			mUiStats.ShowRacingStats(sanctuaryPetTypeInfo);
		}
	}
}
