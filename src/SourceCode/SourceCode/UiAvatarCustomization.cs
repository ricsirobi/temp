using System;
using System.Collections;
using System.Collections.Generic;
using SquadTactics;
using UnityEngine;

public class UiAvatarCustomization : KAUISelectAvatar
{
	[Serializable]
	public class AvatarPartCat
	{
		public string _PartName = string.Empty;

		public string _BtnName = string.Empty;

		public LocaleString _DisplayText = new LocaleString("");

		public LocaleString _ToolTipText = new LocaleString("");

		public Texture _Icon;

		public string _ColorBtnName = string.Empty;
	}

	[Serializable]
	public class ZoomItem
	{
		public string _ZoomItemName;

		public Vector3 _OffsetPos;

		public float _ItemZoomInValue = 2f;

		public float _ItemZoomOutValue = 0.8f;
	}

	public const string TEXTURE_TYPE_DECAL = "Decal";

	public const string AVATAR_TORSO_WARPAINT = "Torso_Decal";

	public const string AVATAR_LEGS_WARPAINT = "Legs_Decal";

	public const string AVATAR_EYE_LINER = "EyeLiner";

	public static string _SelectTab = "";

	public int _ScarCategoryID;

	public int _TorsoWarPaintCatID;

	public int _LegsWarPaintCatID;

	public int _FaceWarPaintCatID;

	public int _AvatarEyeLinerCatID;

	[HideInInspector]
	public int BattleReadyTabIndex;

	[HideInInspector]
	public int ClothesTabIndex = 1;

	[HideInInspector]
	public int AvatarTabIndex = 2;

	public string _BoySubstanceAvatarPath = "PfAvatar";

	public string _GirlSubstanceAvatarPath = "PfAvatar";

	public LocaleString _DuplicateNameText = new LocaleString("This username is already taken. Please try another.");

	public LocaleString _EnterValidNameText = new LocaleString("Please enter a valid name");

	public LocaleString _InvalidUserNameText = new LocaleString("You've entered an invalid name. Please try again.");

	public LocaleString _GeneralRegistrationText = new LocaleString("There was an error while registering this profile. Please try again later.");

	public LocaleString _GeneralWebserviceText = new LocaleString("Sorry, something went wrong! Please try again later.");

	public LocaleString _RegisterParentText = new LocaleString("Creating account...");

	public LocaleString _ProcessingText = new LocaleString("Creating profile...");

	public LocaleString _PleaseWaitText = new LocaleString("Nearly there! Please wait.");

	public LocaleString _SettingUpProfileText = new LocaleString("Setting up your profile...");

	public LocaleString _DeletedAccountText = new LocaleString("This account has been deleted and is no longer valid.");

	public LocaleString _UnauthorizedText = new LocaleString("Your account is still unauthorized. Please ask your parent to activate it.");

	public LocaleString _UnauthorizedTitleText = new LocaleString("Authorization");

	public LocaleString _RandomizePromptText = new LocaleString("Are you sure you want to randomize your character? You will lose all of your changes!");

	public LocaleString _SwitchGenderPromptText = new LocaleString("Are you sure you want to change your character's gender? You will lose all of your changes!");

	public List<ZoomItem> _ZoomInForItems = new List<ZoomItem>();

	private ZoomItem mZoomItem;

	public Transform _StartMarker;

	public Transform _ZoomMarker;

	public int _MaxNumberOfTry = 2;

	public List<AvatarPartCat> _AvatarPartCategory;

	public List<AvatarPartCat> _ClothingPartCategory;

	public string _DragWidgetName = "DragWidget";

	public Vector3 _AvatarButtonPosition = Vector3.zero;

	public Vector3 _ClothesButtonPosition = Vector3.zero;

	public Color _DefaultMaleEyeColor = Color.white;

	public Color _DefaultMaleHairColor = Color.white;

	public Color _DefaultMaleSkinColor = Color.white;

	public Color _DefaultMaleWarPaintColor = Color.white;

	public Color _DefaultFemaleEyeColor = Color.white;

	public Color _DefaultFemaleHairColor = Color.white;

	public Color _DefaultFemaleSkinColor = Color.white;

	public Color _DefaultFemaleWarPaintColor = Color.white;

	public string _FBLoginDBAssetPath = "";

	[NonSerialized]
	public KAWidget _LastChosenCategoryItem;

	public bool _AllowNameSuggestions;

	public Vector2 _AvatarTabPanelOffset;

	public float _AvatarTabPanelHeight;

	public Vector3 _AvatarTabMenuPosition;

	public Vector2 _ClothTabPanelOffset;

	public float _ClothTabPanelHeight;

	public Vector3 _ClothTabMenuPosition;

	public int _AvatarTabScrollBarHeight;

	public Vector3 _AvatarTabScrollBarDownArrowPosition;

	public int _ClothTabScrollBarHeight;

	public Vector3 _ClothTabScrollBarDownArrowPosition;

	public float _ZoomInValue = 2f;

	public float _ZoomOutValue = 0.8f;

	public GameObject _AvatarCamera;

	public string _ItemColorWidget = "CellBkg";

	private bool mSuggestedNameSelected;

	private KAWidget mColorPalette;

	private KAWidget mSkinColorPalette;

	private KAWidget mHairColorBtn;

	private KAWidget mEyeColorBtn;

	private KAWidget mSkinColorBtn;

	private KAWidget mLipColorBtn;

	private KAWidget mWarPaintColorBtn;

	private KAWidget mBackBtn;

	private KAWidget mOKBtn;

	private KAWidget mCustomizeBtn;

	private KAWidget mSelectedColorBtn;

	private KAToggleButton mToggleBtnFemale;

	private KAToggleButton mToggleBtnMale;

	private KAWidget mAvatarNameTxt;

	private KAWidget mColorSelector;

	private KAWidget mRandomizeBtn;

	private KAWidget mCategoryMenuBtn;

	private KAWidget mLastClickedBtn;

	private KAWidget mSlotsInfoTxt;

	private KAWidget mStatsBtn;

	private AvAvatarGenerator mAvGenerator;

	private AvatarData.InstanceInfo mMaleAvatarDataInstance;

	private AvatarData.InstanceInfo mFemaleAvatarDataInstance;

	private GameObject mUIobject;

	private bool mZoom;

	private bool mResetRotation;

	private string mAvatarName;

	private KAUIGenericDB mGenericDB;

	private bool mIsSaving;

	private bool mIsSavingSecondaryVersion;

	private bool mIsSavingAvatarData;

	private bool mSaveAvatarDataAfterLogin;

	private bool mGettingParentInfo;

	private UiStatPopUp mUiStats;

	private KAUIMenu mCategoryMenu;

	private KAUI mUiCategory;

	private ChildInfo mChildInfo = new ChildInfo();

	private string mUserId;

	private int mPrevSelectedTabIndex;

	private bool mIsBattleReady;

	private bool mIsNewProfile;

	private UiSelectProfile mUiSelectProfile;

	private UiJournalCustomization mUiJournalCustomization;

	private bool mInitApplyProperties;

	private AvatarCustomization mAvatarCustomization;

	private bool mSubstanceAvatarLoaded;

	private bool mInitSetPosition;

	private bool mCheckAvatarBundles;

	private bool mIsReInitCommonInventory;

	private bool mInitialized;

	private Renderer mEyeBlinkRenderer;

	private static bool mSkipLoginChild = false;

	private static string mChildToken = string.Empty;

	private bool mProceedToSave;

	private bool mIsFirstTime;

	public static Action OkButtonClicked = null;

	public KAToggleButton pToggleSuit { get; set; }

	public KAWidget pColorPalette => mColorPalette;

	public int pPrevSelectedTabIndex => mPrevSelectedTabIndex;

	public bool pIsBattleReady => mIsBattleReady;

	public bool pIsNewProfile
	{
		get
		{
			return mIsNewProfile;
		}
		set
		{
			mIsNewProfile = value;
			SetNewProfileTabs(mIsNewProfile);
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

	public UiJournalCustomization pUiJournalCustomization
	{
		get
		{
			return mUiJournalCustomization;
		}
		set
		{
			mUiJournalCustomization = value;
		}
	}

	public Vector3 pPrevPosition
	{
		set
		{
			pAvatarCustomization.pPrevPosition = value;
		}
	}

	public AvatarCustomization pAvatarCustomization => mAvatarCustomization ?? (mAvatarCustomization = new AvatarCustomization());

	public CustomAvatarState pCustomAvatar => pAvatarCustomization.pCustomAvatar;

	public static bool pSkipLoginChild
	{
		set
		{
			mSkipLoginChild = value;
		}
	}

	public static string pChildToken
	{
		set
		{
			mChildToken = value;
		}
	}

	public override void Initialize()
	{
		if (mInitialized)
		{
			return;
		}
		mMenu = (UiAvatarCustomizationMenu)GetMenu("UiAvatarCustomizationMenu");
		mUiStats = (UiStatPopUp)_UiList[0];
		mColorPalette = FindItem("ColorPicker");
		mSkinColorPalette = FindItem("SkinColorPicker");
		mHairColorBtn = FindItem("HairColorBtn");
		mEyeColorBtn = FindItem("EyeColorBtn");
		mSkinColorBtn = FindItem("SkinColorBtn");
		mLipColorBtn = FindItem("LipColorBtn");
		mWarPaintColorBtn = FindItem("WarPaintColorBtn");
		mOKBtn = FindItem("DoneBtn");
		mToggleBtnMale = (KAToggleButton)FindItem("ToggleBtnMale");
		mToggleBtnFemale = (KAToggleButton)FindItem("ToggleBtnFemale");
		mAvatarNameTxt = FindItem("TxtAvatarName");
		mColorSelector = FindItem("ColorSelector");
		mCustomizeBtn = FindItem("CustomizeBtn");
		mRandomizeBtn = FindItem("RandomizeBtn");
		mSlotsInfoTxt = FindItem("SlotsInfo");
		mStatsBtn = FindItem("StatsBtn");
		mBackBtn = FindItem("BackBtn");
		mCategoryMenuBtn = FindItem("CategoryMenuBtn");
		mUiCategory = _UiList[1];
		if (mUiCategory != null)
		{
			mCategoryMenu = mUiCategory._MenuList[0];
		}
		UpdateSlotsToggleButtons();
		mAvGenerator = GetComponent<AvAvatarGenerator>();
		ShowColorPalette(inStatus: false);
		ShowColorButtons(inStatus: false);
		SetUiForJournal();
		mProceedToSave = false;
		mAvatarNameTxt.SetVisibility(inVisible: false);
		if (pIsNewProfile)
		{
			InventoryTab inventoryTab = _InventoryTabList[1];
			InventoryTab inventoryTab2 = _InventoryTabList[2];
			_InventoryTabList = new InventoryTab[2] { inventoryTab, inventoryTab2 };
			if (!string.IsNullOrEmpty(ProductConfig.pToken))
			{
				ProductData.Reset();
				mGettingParentInfo = true;
				WsWebService.SetToken(ProductConfig.pToken);
				UserInfo.Reset();
				UserInfo.Init();
				mIsReInitCommonInventory = true;
				SetState(KAUIState.DISABLED);
				KAUICursorManager.SetDefaultCursor("Loading");
				CommonInventoryData.ReInit();
				mOKBtn.SetVisibility(inVisible: false);
				mRandomizeBtn.SetVisibility(inVisible: true);
				mStatsBtn.SetVisibility(inVisible: false);
			}
		}
		else
		{
			SetInteractive(interactive: false);
			mOKBtn.SetVisibility(inVisible: false);
			mRandomizeBtn.SetVisibility(inVisible: false);
		}
		pAvatarCustomization.SetAvatarData(AvatarData.pInstance);
		UiAvatarItemCustomization.LoadPairData();
		base.Initialize();
		mInitialized = true;
	}

	public void UpdateSlotsToggleButtons()
	{
		EquippedSlotWidget[] equippedSlots = ((UiAvatarCustomizationMenu)mMenu)._EquippedSlots;
		foreach (EquippedSlotWidget equippedSlotWidget in equippedSlots)
		{
			if (equippedSlotWidget._PartType == AvatarData.pPartSettings.AVATAR_PART_HAT)
			{
				KAToggleButton toggleButton = GetToggleButton(equippedSlotWidget);
				if (toggleButton != null)
				{
					toggleButton.SetChecked(AvatarData.pInstanceInfo.GetPartVisibility(AvatarData.pInstanceInfo.mAvatar.name, AvatarData.pPartSettings.AVATAR_PART_HAT));
				}
			}
			else
			{
				if (!(equippedSlotWidget._PartType == AvatarData.pPartSettings.AVATAR_PART_WING))
				{
					continue;
				}
				KAToggleButton toggleButton2 = GetToggleButton(equippedSlotWidget);
				if (!(toggleButton2 != null))
				{
					continue;
				}
				if (equippedSlotWidget.pEquippedItemData == null)
				{
					toggleButton2.SetChecked(AvatarData.pInstanceInfo.GetPartVisibility(AvatarData.pInstanceInfo.mAvatar.name, AvatarData.pPartSettings.AVATAR_PART_WING));
				}
				else
				{
					bool flag = AvatarData.pInstanceInfo.SuitEquipped();
					toggleButton2.SetChecked(flag);
					if (flag != AvatarData.pInstanceInfo.GetPartVisibility(AvatarData.pInstanceInfo.mAvatar.name, AvatarData.pPartSettings.AVATAR_PART_WING))
					{
						AvatarData.pInstanceInfo.UpdatePartVisibility(AvatarData.pPartSettings.AVATAR_PART_WING, flag);
						mMenu.mModified = true;
					}
				}
				pToggleSuit = toggleButton2;
			}
		}
	}

	private KAToggleButton GetToggleButton(EquippedSlotWidget slot)
	{
		KAToggleButton result = null;
		if (slot != null && slot._WidgetSlot != null)
		{
			result = slot._WidgetSlot.FindChildItem("ToggleBtn") as KAToggleButton;
		}
		return result;
	}

	private void SetUiForJournal()
	{
		ShowBackButton(pIsNewProfile && mUiSelectProfile.pChildListInfo != null);
		mToggleBtnMale.SetVisibility(pIsNewProfile);
		mToggleBtnFemale.SetVisibility(pIsNewProfile);
		if (mGender != null)
		{
			mGender.SetVisibility(pIsNewProfile);
			mGender.SetText("Female");
		}
	}

	public void ShowBackButton(bool show)
	{
		mBackBtn.SetVisibility(show);
	}

	public void ShowOkButton(bool show)
	{
		mOKBtn.SetVisibility(show);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		CheckAvatarBundlesReady();
		mMenu.transform.localPosition = _AvatarTabMenuPosition;
	}

	private void CheckAvatarBundlesReady()
	{
		mCheckAvatarBundles = true;
		SetInteractive(interactive: false);
	}

	private void LoadSubstanceAvatar()
	{
		bool inFirstTime = false;
		bool flag = AvatarData.GetGender() == Gender.Male;
		GameObject inObject = pAvatarCustomization.LoadAvatarForPreview(flag, "PfAvatarCustomization", Vector3.up * 5000f, Vector3.one * 0.8f, ref inFirstTime);
		ProcessLoadedAvatar(inObject, flag, inFirstTime);
		SetWarPaintColorBtnStatus();
		mSubstanceAvatarLoaded = true;
	}

	public bool SetWarPaintColorBtnStatus()
	{
		if (pCustomAvatar != null)
		{
			bool result = false;
			string data = pCustomAvatar.GetData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DECAL2);
			if (!string.IsNullOrEmpty(data))
			{
				string[] array = data.Split('/');
				if (array.Length > 2)
				{
					result = (array[2].Equals("Decal_Fill") ? true : false);
				}
			}
			return result;
		}
		return false;
	}

	public void ProcessLoadedAvatar(GameObject inObject, bool inMale, bool inFirstTime)
	{
		if (pCustomAvatar == null)
		{
			return;
		}
		string text = (inMale ? "M" : "F");
		Transform transform = UtUtilities.FindChildTransform(inObject, "DWAvBlinkPlane" + text);
		if (transform != null)
		{
			mEyeBlinkRenderer = (MeshRenderer)transform.gameObject.GetComponent(typeof(MeshRenderer));
		}
		if (pIsNewProfile)
		{
			SetInteractive(interactive: true);
		}
		if (mUiJournalCustomization != null)
		{
			mUiJournalCustomization.pIsLoadingAsset = false;
		}
		mInitSetPosition = false;
		if (pIsNewProfile)
		{
			if (inFirstTime)
			{
				mIsFirstTime = inFirstTime;
				if (mSkinColorBtn != null)
				{
					Color white = Color.white;
					white = ((!inMale) ? _DefaultFemaleSkinColor : _DefaultMaleSkinColor);
					mSkinColorBtn.pBackground.color = white;
				}
				if (mHairColorBtn != null)
				{
					Color white2 = Color.white;
					white2 = ((!inMale) ? _DefaultFemaleHairColor : _DefaultMaleHairColor);
					mHairColorBtn.pBackground.color = white2;
				}
				if (mWarPaintColorBtn != null)
				{
					Color white3 = Color.white;
					white3 = ((!inMale) ? _DefaultFemaleWarPaintColor : _DefaultMaleWarPaintColor);
					mWarPaintColorBtn.pBackground.color = white3;
				}
				if (mLipColorBtn != null)
				{
					mLipColorBtn.pBackground.color = Color.white;
				}
				if (mEyeColorBtn != null)
				{
					Color white4 = Color.white;
					white4 = ((!inMale) ? _DefaultFemaleEyeColor : _DefaultMaleEyeColor);
					mEyeColorBtn.pBackground.color = white4;
				}
				mIsFirstTime = false;
				pCustomAvatar.SetColor(CustomAvatarState.pCustomAvatarSettings.SKINCOLOR_INDEX, mSkinColorBtn.pBackground.color);
				pCustomAvatar.SetColor(CustomAvatarState.pCustomAvatarSettings.HAIRCOLOR_INDEX, mHairColorBtn.pBackground.color);
				pCustomAvatar.SetColor(CustomAvatarState.pCustomAvatarSettings.EYECOLOR_INDEX, mEyeColorBtn.pBackground.color);
				if ((bool)mWarPaintColorBtn)
				{
					pCustomAvatar.SetColor(CustomAvatarState.pCustomAvatarSettings.WARPAINTCOLOR_INDEX, mWarPaintColorBtn.pBackground.color);
				}
			}
			else
			{
				pCustomAvatar.SetColor(CustomAvatarState.pCustomAvatarSettings.SKINCOLOR_INDEX, mSkinColorBtn.pBackground.color);
				pCustomAvatar.SetColor(CustomAvatarState.pCustomAvatarSettings.HAIRCOLOR_INDEX, mHairColorBtn.pBackground.color);
				pCustomAvatar.SetColor(CustomAvatarState.pCustomAvatarSettings.EYECOLOR_INDEX, mEyeColorBtn.pBackground.color);
				if ((bool)mWarPaintColorBtn)
				{
					pCustomAvatar.SetColor(CustomAvatarState.pCustomAvatarSettings.WARPAINTCOLOR_INDEX, mWarPaintColorBtn.pBackground.color);
				}
			}
		}
		pCustomAvatar.mIsDirty = true;
	}

	public override void OnPressRepeated(KAWidget inWidget, bool inPressed)
	{
		base.OnPressRepeated(inWidget, inPressed);
		if (inPressed && !(mSelectedColorBtn == null) && (inWidget == mColorPalette || inWidget == mSkinColorPalette))
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
			SetColor(pixelBilinear);
		}
	}

	private void SetColor(Color inColor)
	{
		if (pCustomAvatar == null)
		{
			return;
		}
		if (mSelectedColorBtn == mEyeColorBtn || mSelectedColorBtn == mLipColorBtn)
		{
			pCustomAvatar.SetColor(CustomAvatarState.pCustomAvatarSettings.EYECOLOR_INDEX, inColor);
		}
		else if (mSelectedColorBtn == mHairColorBtn)
		{
			pCustomAvatar.SetColor(CustomAvatarState.pCustomAvatarSettings.HAIRCOLOR_INDEX, inColor);
		}
		else if (mSelectedColorBtn == mWarPaintColorBtn)
		{
			pCustomAvatar.SetColor(CustomAvatarState.pCustomAvatarSettings.WARPAINTCOLOR_INDEX, inColor);
		}
		else if (mSelectedColorBtn == mSkinColorBtn)
		{
			pCustomAvatar.SetColor(CustomAvatarState.pCustomAvatarSettings.SKINCOLOR_INDEX, inColor);
			if (mEyeBlinkRenderer != null && mEyeBlinkRenderer.materials != null && mEyeBlinkRenderer.materials.Length != 0 && mEyeBlinkRenderer.materials[0] != null)
			{
				mEyeBlinkRenderer.materials[0].SetColor("_Color", inColor);
			}
		}
		mMenu.mModified = true;
	}

	public void ShowColorPalette(bool inStatus)
	{
		mColorPalette.SetVisibility(inStatus);
		mSkinColorPalette.SetVisibility(inStatus);
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		base.OnDrag(inWidget, inDelta);
		if (!string.IsNullOrEmpty(_DragWidgetName) && inWidget.name == _DragWidgetName)
		{
			float num = (0f - _RotationSpeed) * Time.deltaTime * inDelta.x;
			if (num != 0f)
			{
				AvAvatar.mTransform.Rotate(0f, num, 0f);
			}
		}
	}

	private void ZoomInAndOut()
	{
		if (pAvatarCustomization.pPrevAvatar == null)
		{
			return;
		}
		Vector3 vector = _ZoomMarker.position;
		float num = _ZoomInValue;
		float num2 = _ZoomOutValue;
		if (mZoomItem != null)
		{
			vector = _ZoomMarker.position + mZoomItem._OffsetPos;
			num = mZoomItem._ItemZoomInValue;
			num2 = mZoomItem._ItemZoomOutValue;
		}
		Vector3 vector2 = (mZoom ? vector : _StartMarker.position);
		float b = (mZoom ? num : num2);
		if (mZoom && mResetRotation)
		{
			Vector3 eulerAngles = _StartMarker.eulerAngles;
			Vector3 eulerAngles2 = AvAvatar.mTransform.eulerAngles;
			if (eulerAngles2.y > 180f)
			{
				eulerAngles.y = 360f;
			}
			eulerAngles2 = Vector3.Lerp(eulerAngles2, eulerAngles, Time.deltaTime * 4f);
			AvAvatar.mTransform.eulerAngles = eulerAngles2;
			if ((eulerAngles - AvAvatar.mTransform.eulerAngles).magnitude <= 1f)
			{
				mResetRotation = false;
			}
		}
		Vector3 localPosition = AvAvatar.mTransform.localPosition;
		localPosition = Vector3.Lerp(localPosition, vector2, Time.deltaTime * 4f);
		if ((localPosition - vector2).magnitude > 5f)
		{
			localPosition = vector2;
		}
		AvAvatar.mTransform.localPosition = localPosition;
		float x = AvAvatar.mTransform.localScale.x;
		x = Mathf.Lerp(x, b, Time.deltaTime * 4f);
		AvAvatar.mTransform.localScale = new Vector3(x, x, x);
	}

	protected override void Update()
	{
		base.Update();
		if (!mInitialized)
		{
			return;
		}
		if (mCheckAvatarBundles && AvatarData.pIsReady)
		{
			mCheckAvatarBundles = false;
			LoadSubstanceAvatar();
		}
		if (!string.IsNullOrEmpty(_SelectTab) && mSubstanceAvatarLoaded)
		{
			OnClick(FindItem(_SelectTab));
			_SelectTab = "";
		}
		if (!pIsNewProfile && mInitApplyProperties)
		{
			ZoomInAndOut();
		}
		else if (AvatarData.pIsReady && mSubstanceAvatarLoaded && mInitSetPosition)
		{
			ZoomInAndOut();
		}
		if (mGettingParentInfo && UserInfo.pIsReady)
		{
			mGettingParentInfo = false;
			if (pIsNewProfile)
			{
				mOKBtn.SetVisibility(inVisible: true);
				mAvatarNameTxt.SetVisibility(inVisible: true);
				if (mSkipLoginChild)
				{
					string displayName = AvatarData.pInstance.DisplayName;
					if (string.IsNullOrEmpty(displayName) && mUiSelectProfile.pCurrentChild != null)
					{
						displayName = mUiSelectProfile.pCurrentChild._Name;
					}
					if (!string.IsNullOrEmpty(displayName))
					{
						mAvatarName = displayName;
					}
				}
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
		if (!pIsNewProfile && !mInitApplyProperties && mSubstanceAvatarLoaded)
		{
			mInitApplyProperties = true;
			if ((bool)mEyeColorBtn)
			{
				mEyeColorBtn.pBackground.color = pCustomAvatar.mColors[CustomAvatarState.pCustomAvatarSettings.EYECOLOR_INDEX];
			}
			if ((bool)mSkinColorBtn)
			{
				mSkinColorBtn.pBackground.color = pCustomAvatar.mColors[CustomAvatarState.pCustomAvatarSettings.SKINCOLOR_INDEX];
				if (mEyeBlinkRenderer != null && mEyeBlinkRenderer.materials.Length != 0)
				{
					mEyeBlinkRenderer.materials[0].SetColor("_Color", pCustomAvatar.mColors[CustomAvatarState.pCustomAvatarSettings.SKINCOLOR_INDEX]);
				}
			}
			if ((bool)mHairColorBtn)
			{
				mHairColorBtn.pBackground.color = pCustomAvatar.mColors[CustomAvatarState.pCustomAvatarSettings.HAIRCOLOR_INDEX];
			}
			if (mWarPaintColorBtn != null)
			{
				mWarPaintColorBtn.pBackground.color = pCustomAvatar.mColors[CustomAvatarState.pCustomAvatarSettings.WARPAINTCOLOR_INDEX];
			}
			if (mLipColorBtn != null)
			{
				mLipColorBtn.pBackground.color = pCustomAvatar.mColors[CustomAvatarState.pCustomAvatarSettings.EYECOLOR_INDEX];
			}
			AvAvatar.SetPosition(_StartMarker);
			SetInteractive(interactive: true);
			if (mUiJournalCustomization != null)
			{
				mUiJournalCustomization.SetDragonBtnState(inInteractive: true);
			}
		}
		if (pIsNewProfile)
		{
			if (AvatarData.pIsReady)
			{
				if (AvatarData.GetGender() == Gender.Male && mToggleBtnFemale.GetState() != 0)
				{
					mToggleBtnMale.SetState(KAUIState.NOT_INTERACTIVE);
					mToggleBtnFemale.SetState(KAUIState.INTERACTIVE);
				}
				else if (AvatarData.GetGender() == Gender.Female && mToggleBtnMale.GetState() != 0)
				{
					mToggleBtnMale.SetState(KAUIState.INTERACTIVE);
					mToggleBtnFemale.SetState(KAUIState.NOT_INTERACTIVE);
				}
			}
			if (!mInitSetPosition && mSubstanceAvatarLoaded)
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				SetInteractive(interactive: true);
				mInitSetPosition = true;
				AvAvatar.SetPosition(_StartMarker);
				if (WsUserMessage.pInstance != null)
				{
					WsUserMessage.pInstance.RemoveMessageWithoutShowing(24, inDelete: true);
					WsUserMessage.pInstance.ShowMessage(35, delete: true);
				}
			}
			if (UserInfo.pIsReady)
			{
				if (mIsSaving)
				{
					mIsSaving = false;
					mIsSavingAvatarData = true;
					mMenu.mModified = true;
					OnCloseCustomization(checkProcessing: true);
					AnalyticAgent.LogFTUEEvent(FTUEEvent.ONCLOSECUSTOMIZATION);
				}
				else if (mIsSavingAvatarData && !AvatarData.pIsSaving && AvatarData.pIsReady && ProductData.pIsReady)
				{
					mIsSavingAvatarData = false;
					ProfileCreated();
				}
			}
		}
		pAvatarCustomization.DoUpdate();
		if (mProceedToSave && WsTokenMonitor.pHaveCheckedToken)
		{
			mProceedToSave = false;
			ProceedToSave();
		}
		if (mIsReInitCommonInventory && CommonInventoryData.pIsReady)
		{
			mIsReInitCommonInventory = false;
			SetState(KAUIState.INTERACTIVE);
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
		if (mUiCategory != null && mUiCategory.IsActive() && Input.GetMouseButtonUp(0) && KAUIManager.pInstance.pSelectedWidget != mCategoryMenuBtn)
		{
			mUiCategory.SetVisibility(inVisible: false);
		}
	}

	private void ShowErrorDB(string inText)
	{
		KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Enter valid name");
		kAUIGenericDB.SetText(inText, interactive: false);
		kAUIGenericDB.SetMessage(base.gameObject, "", "", "OnCloseDB", "");
		kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(kAUIGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	private void OnSelectNameProcessed(UiSelectName.Status status, string name, bool suggestedNameSelected, UiSelectName uiSelectName)
	{
		mSuggestedNameSelected = suggestedNameSelected;
		switch (status)
		{
		case UiSelectName.Status.Accepted:
			KAUICursorManager.SetDefaultCursor("Loading");
			SetState(KAUIState.NOT_INTERACTIVE);
			WsTokenMonitor.ForceCheckToken();
			AnalyticAgent.LogFTUEEvent(FTUEEvent.FORCECHECKTOKEN_STARTED);
			mProceedToSave = true;
			mAvatarName = name;
			StartCoroutine("SendTokenStatusAnalytics");
			break;
		case UiSelectName.Status.Closed:
			SetState(KAUIState.INTERACTIVE);
			break;
		case UiSelectName.Status.Loaded:
			if (string.IsNullOrEmpty(name) && UtPlatform.IsMobile())
			{
				AdManager.DisplayAd(AdEventType.PROFILE_CREATION, AdOption.FULL_SCREEN);
			}
			break;
		}
	}

	private IEnumerator SendTokenStatusAnalytics()
	{
		float timeout = 30f;
		while (timeout > 0f && mProceedToSave)
		{
			timeout -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		if (mProceedToSave)
		{
			AnalyticAgent.LogFTUEEvent(FTUEEvent.FORCECHECKTOKEN_FAILED);
		}
		yield return null;
	}

	private void ProceedToSave()
	{
		if (mSkipLoginChild)
		{
			if (!mIsSavingSecondaryVersion)
			{
				WsWebService.SetToken(mChildToken);
				UserInfo.Reset();
				UserInfo.Init();
				ProductData.Init();
			}
			mIsSavingSecondaryVersion = true;
			AvatarData.pInstance.DisplayName = mAvatarName;
			AvatarData.pInstance.IsSuggestedAvatarName = mSuggestedNameSelected;
			WsWebService.SetAvatar(AvatarData.pInstanceInfo.mInstance, ServiceEventHandler, null);
		}
		else if ((UiLogin.pIsGuestUser && UiLogin.pHasUserJustRegistered) || mUiSelectProfile.pAvatarDataNull)
		{
			mSaveAvatarDataAfterLogin = true;
			WsWebService.LoginChild(ProductConfig.pToken, mUiSelectProfile.pChildInfo[0]._UserID, UtUtilities.GetLocaleLanguage(), ServiceEventHandler, null);
			AnalyticAgent.LogFTUEEvent(FTUEEvent.LOGINCHILD_STARTED);
			UiLogin.pHasUserJustRegistered = false;
		}
		else
		{
			RegisterChild();
		}
	}

	public override void OnClick(KAWidget inItem)
	{
		base.OnClick(inItem);
		if (!mInitialized)
		{
			return;
		}
		if (inItem == mToggleBtnMale || inItem == mToggleBtnFemale)
		{
			if ((mMenu.mModified && mGenericDB == null) || mLastClickedBtn == mRandomizeBtn)
			{
				mGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "MessageBox");
				mGenericDB.SetText(_SwitchGenderPromptText.GetLocalizedString(), interactive: false);
				mGenericDB.SetMessage(base.gameObject, "SwitchAvatarGender", "OnCloseDB", "", "");
				mGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
				mGenericDB.SetDestroyOnClick(isDestroy: true);
				KAUI.SetExclusive(mGenericDB, _MaskColor);
				mToggleBtnMale.SetChecked(mAvatarCustomization.pCustomAvatar.mIsMale);
				mToggleBtnFemale.SetChecked(!mAvatarCustomization.pCustomAvatar.mIsMale);
			}
			else
			{
				SwitchAvatarGender();
			}
		}
		else if (inItem == mCustomizeBtn)
		{
			UiAvatarCustomizationMenu obj = (UiAvatarCustomizationMenu)mMenu;
			obj.DisableCustomizeUI();
			obj.LoadItemCustomizationUI();
			if (mUiJournalCustomization != null)
			{
				mUiJournalCustomization.SetBlacksmithButtonVisibility(visible: false);
			}
		}
		else if (inItem == mOKBtn)
		{
			AnalyticAgent.LogFTUEEvent(FTUEEvent.CHARACTER_CUSTOMIZATION_COMPLETED);
			if (OkButtonClicked != null)
			{
				OkButtonClicked();
			}
			else
			{
				if ((UtPlatform.IsMobile() || UtPlatform.IsWSA()) && !UtUtilities.IsConnectedToWWW())
				{
					return;
				}
				UiSelectName.Init(OnSelectNameProcessed, mAvatarName, null);
				SetInteractive(interactive: false);
			}
		}
		else if (inItem == mStatsBtn)
		{
			ShowAvatarStats();
		}
		else if (inItem == mEyeColorBtn || inItem == mLipColorBtn || inItem == mHairColorBtn || inItem == mSkinColorBtn || inItem == mWarPaintColorBtn)
		{
			mZoom = inItem != mSkinColorBtn;
			mResetRotation = inItem != mSkinColorBtn;
			mSelectedColorBtn = inItem;
			SetSkinColorPickersVisibility();
			if (!mIsFirstTime)
			{
				foreach (AvatarPartCat item in _AvatarPartCategory)
				{
					if (inItem.name.Equals(item._ColorBtnName))
					{
						if (mCategoryMenu != null)
						{
							OnClick(mCategoryMenu.FindItem(item._BtnName));
						}
						break;
					}
				}
			}
			if (inItem == mWarPaintColorBtn)
			{
				mColorPalette.SetDisabled(SetWarPaintColorBtnStatus());
			}
			else
			{
				mColorPalette.SetDisabled(isDisabled: false);
			}
		}
		else if (inItem == mRandomizeBtn)
		{
			if (mMenu.mModified && mGenericDB == null)
			{
				mGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "MessageBox");
				mGenericDB.SetText(_RandomizePromptText.GetLocalizedString(), interactive: false);
				mGenericDB.SetMessage(base.gameObject, "DoRandomize", "OnCloseDB", "", "");
				mGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
				mGenericDB.SetDestroyOnClick(isDestroy: true);
				KAUI.SetExclusive(mGenericDB, _MaskColor);
			}
			else
			{
				DoRandomize();
			}
		}
		else if (Array.Exists(((UiAvatarCustomizationMenu)mMenu)._EquippedSlots, (EquippedSlotWidget x) => x._WidgetSlot == inItem))
		{
			((UiAvatarCustomizationMenu)mMenu).StartCoroutine("CheckAndShowStatsDB", inItem);
		}
		else if (inItem.name == "ToggleBtn")
		{
			KAWidget partWidget = inItem.pParentWidget;
			EquippedSlotWidget equippedSlotWidget = Array.Find(((UiAvatarCustomizationMenu)mMenu)._EquippedSlots, (EquippedSlotWidget x) => x._WidgetSlot == partWidget);
			if (equippedSlotWidget != null)
			{
				if (equippedSlotWidget._PartType == AvatarData.pPartSettings.AVATAR_PART_HAT)
				{
					KAToggleButton kAToggleButton = (KAToggleButton)inItem;
					if (kAToggleButton != null)
					{
						bool partVisibility = AvatarData.pInstanceInfo.GetPartVisibility(AvatarData.pInstanceInfo.mAvatar.name, AvatarData.pPartSettings.AVATAR_PART_HAT);
						AvatarData.pInstanceInfo.UpdatePartVisibility(AvatarData.pPartSettings.AVATAR_PART_HAT, !partVisibility);
						kAToggleButton.SetChecked(!partVisibility);
						AvatarData.RestoreCurrentPart(AvatarData.pPartSettings.AVATAR_PART_HAT);
						pCustomAvatar.mIsDirty = true;
						mMenu.mModified = true;
					}
				}
				else if (equippedSlotWidget._PartType == AvatarData.pPartSettings.AVATAR_PART_WING)
				{
					((UiAvatarCustomizationMenu)mMenu).ToggleSuit();
				}
			}
		}
		else if (inItem == mCategoryMenuBtn && mUiCategory != null)
		{
			mUiCategory.SetVisibility(!mUiCategory.GetVisibility());
		}
		else if (inItem != mSkinColorPalette && inItem != mColorPalette && mCategoryMenu != null && mCategoryMenu.GetNumItems() > 0)
		{
			bool flag = false;
			for (int i = 0; i < mCategoryMenu.GetNumItems(); i++)
			{
				KAWidget kAWidget = mCategoryMenu.FindItemAt(i);
				if (kAWidget != null && kAWidget.name == inItem.name)
				{
					if (kAWidget.name != "ShieldBtn")
					{
						_LastChosenCategoryItem = kAWidget;
					}
					flag = true;
					break;
				}
			}
			if (flag)
			{
				UpdateCategoryMenuButton(inItem);
				mZoomItem = _ZoomInForItems.Find((ZoomItem x) => x._ZoomItemName.Equals(inItem.name));
				if (mZoomItem != null)
				{
					mZoom = true;
					mResetRotation = true;
				}
				else
				{
					mZoom = false;
					mResetRotation = false;
				}
				mColorPalette.SetDisabled(isDisabled: false);
				if (inItem.name == "EyeBtn")
				{
					mSelectedColorBtn = mEyeColorBtn;
					SetSkinColorPickersVisibility();
					base.OnClick(mSelectedColorBtn);
				}
				else if (inItem.name == "HairBtn")
				{
					mSelectedColorBtn = mHairColorBtn;
					SetSkinColorPickersVisibility();
					base.OnClick(mSelectedColorBtn);
				}
				else if (inItem.name == "SkinBtn")
				{
					mSelectedColorBtn = mSkinColorBtn;
					SetSkinColorPickersVisibility();
					base.OnClick(mSelectedColorBtn);
				}
				else if (inItem.name == "FaceDecalBtn")
				{
					mSelectedColorBtn = mWarPaintColorBtn;
					SetSkinColorPickersVisibility();
					base.OnClick(mSelectedColorBtn);
					mColorPalette.SetDisabled(SetWarPaintColorBtnStatus());
				}
				else if (inItem.name == "ScarBtn")
				{
					mSelectedColorBtn = null;
				}
				if (mCategoryMenu != null)
				{
					for (int j = 0; j < mCategoryMenu.GetNumItems(); j++)
					{
						KAWidget kAWidget2 = mCategoryMenu.FindItemAt(j);
						if (kAWidget2 != null)
						{
							kAWidget2.SetDisabled(kAWidget2.name == inItem.name);
						}
					}
				}
			}
		}
		mLastClickedBtn = inItem;
	}

	private void SwitchAvatarGender()
	{
		bool flag = !pAvatarCustomization.pCustomAvatar.mIsMale;
		pAvatarCustomization.ResetCustomAvatar();
		mMaleAvatarDataInstance = null;
		mFemaleAvatarDataInstance = null;
		pAvatarCustomization.DestroyCustomAvatar();
		SetAvatarPrefab(flag);
		mMenu.mModified = false;
		mToggleBtnMale.SetChecked(flag);
		mToggleBtnFemale.SetChecked(!flag);
	}

	private void DoRandomize()
	{
		mMenu.mModified = false;
		Gender gender = (pCustomAvatar.mIsMale ? Gender.Male : Gender.Female);
		RandomizeAvatar(gender);
		SetAvatarPrefab(gender == Gender.Male);
		mSkinColorBtn.pBackground.color = GetRandomPalletColor(mSkinColorPalette, mSkinColorBtn);
		mHairColorBtn.pBackground.color = GetRandomPalletColor(mColorPalette, mHairColorBtn);
		mEyeColorBtn.pBackground.color = GetRandomPalletColor(mColorPalette, mEyeColorBtn);
		if ((bool)mWarPaintColorBtn)
		{
			mWarPaintColorBtn.pBackground.color = GetRandomPalletColor(mColorPalette, mWarPaintColorBtn);
		}
		mToggleBtnMale.SetState(KAUIState.NOT_INTERACTIVE);
		mToggleBtnFemale.SetState(KAUIState.NOT_INTERACTIVE);
		mLastClickedBtn = mRandomizeBtn;
	}

	private void UpdateCategoryMenuButton(KAWidget inItem)
	{
		if (inItem == null)
		{
			return;
		}
		bool flag = inItem.name.Equals("All", StringComparison.OrdinalIgnoreCase);
		mCategoryMenuBtn.SetText(flag ? inItem.GetText() : string.Empty);
		Transform transform = mCategoryMenuBtn.transform.Find("Icon");
		if (transform == null)
		{
			return;
		}
		UITexture component = transform.GetComponent<UITexture>();
		if (component != null)
		{
			component.enabled = !flag;
		}
		if (flag)
		{
			return;
		}
		transform = inItem.transform.Find("Icon");
		if (!(transform == null))
		{
			UITexture component2 = transform.GetComponent<UITexture>();
			if (component2 != null)
			{
				component.mainTexture = component2.mainTexture;
			}
		}
	}

	public int[] GetAllCategories()
	{
		List<int> list = new List<int>();
		int i;
		for (i = 0; i < _Tabs.Length; i++)
		{
			if (_Tabs[i]._PrtTypeName != "All" && _ClothingPartCategory.Exists((AvatarPartCat x) => x._PartName == _Tabs[i]._PrtTypeName))
			{
				list.Add(AvatarData.GetCategoryID(_Tabs[i]._PrtTypeName));
			}
		}
		return list.ToArray();
	}

	public override void OnDoubleClick(KAWidget inWidget)
	{
		if (inWidget != null && Array.Exists(((UiAvatarCustomizationMenu)mMenu)._EquippedSlots, (EquippedSlotWidget x) => x._WidgetSlot == inWidget))
		{
			mMenu.OnDoubleClick(inWidget);
		}
	}

	public Color GetRandomPalletColor(KAWidget palletWidget, KAWidget colorButton)
	{
		Color result = colorButton.pBackground.color;
		Texture2D texture2D = (Texture2D)palletWidget.GetTexture();
		if (texture2D != null)
		{
			result = texture2D.GetPixelBilinear(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
		}
		return result;
	}

	public void RandomizeAvatar(Gender forceGenderTo = Gender.Unknown)
	{
		if (!(mAvGenerator == null))
		{
			AvatarData.InstanceInfo instanceInfo = mAvGenerator.GenerateRandomAvatar(AvatarData.pInstanceInfo.mAvatar, forceGenderTo, useCustomColors: true);
			AvatarData.pInstanceInfo.mInstance = instanceInfo.mInstance;
			AvatarData.SetDontDestroyOnBundles(inDontDestroy: true);
			if (AvatarData.GetGender() == Gender.Male)
			{
				mMaleAvatarDataInstance = instanceInfo;
				mMaleAvatarDataInstance.mAvatar = instanceInfo.mAvatar;
			}
			else
			{
				mFemaleAvatarDataInstance = instanceInfo;
				mFemaleAvatarDataInstance.mAvatar = instanceInfo.mAvatar;
			}
		}
	}

	public override void ChangeCategory(KAWidget item)
	{
		if (base.pKAUiSelectTabMenu.GetItems().Count != 0)
		{
			if (base.pKAUiSelectTabMenu.GetSelectedItemIndex() == -1)
			{
				base.pKAUiSelectTabMenu.SetSelectedItem(item);
				mPrevSelectedTabIndex = -1;
			}
			mIsBattleReady = !pIsNewProfile && item == base.pKAUiSelectTabMenu.GetItemAt(BattleReadyTabIndex);
			base.ChangeCategory(item);
			if (item == base.pKAUiSelectTabMenu.GetItemAt(AvatarTabIndex))
			{
				mKAUiSelectMenu._AllowItemDrag = false;
				OpenAvatarPartCategory();
			}
			else if (item == base.pKAUiSelectTabMenu.GetItemAt(ClothesTabIndex) || mIsBattleReady)
			{
				mKAUiSelectMenu._AllowItemDrag = true;
				OpenClothesPartCategory();
			}
			mPrevSelectedTabIndex = base.pKAUiSelectTabMenu.GetSelectedItemIndex();
			UpdateCategoryMenuButton(mCategoryMenu.GetItemAt(0));
			UpdateSlotsInfo();
		}
	}

	public void UpdateSlotsInfo()
	{
		if (mSlotsInfoTxt != null)
		{
			if (pIsBattleReady && base.pKAUiSelectTabMenu.pSelectedTab.pTabData != null)
			{
				int occupiedSlots = base.pKAUiSelectTabMenu.pSelectedTab.pTabData.GetOccupiedSlots();
				int totalSlots = base.pKAUiSelectTabMenu.pSelectedTab.pTabData.GetTotalSlots();
				mSlotsInfoTxt.SetText(occupiedSlots + "/" + totalSlots);
			}
			mSlotsInfoTxt.SetVisibility(pIsBattleReady);
		}
	}

	private void ShowColorButtons(bool inStatus)
	{
		if (mEyeColorBtn != null)
		{
			mEyeColorBtn.SetVisibility(inStatus);
		}
		if (mHairColorBtn != null)
		{
			mHairColorBtn.SetVisibility(inStatus);
		}
		if (mSkinColorBtn != null)
		{
			mSkinColorBtn.SetVisibility(inStatus);
		}
		if (mLipColorBtn != null)
		{
			mLipColorBtn.SetVisibility(inStatus);
		}
		if (mWarPaintColorBtn != null)
		{
			mWarPaintColorBtn.SetVisibility(inStatus);
		}
		if (mColorSelector != null)
		{
			mColorSelector.SetVisibility(inStatus);
		}
	}

	private void SetSkinColorPickersVisibility()
	{
		if (!(mSelectedColorBtn == null))
		{
			bool flag = mSelectedColorBtn == mSkinColorBtn;
			mColorPalette.SetDisabled(isDisabled: false);
			mSkinColorPalette.SetVisibility(flag);
			mColorPalette.SetVisibility(!flag);
		}
	}

	private void SetAvatarPrefab(bool isMale)
	{
		if (pAvatarCustomization.pPrevAvatar != null)
		{
			AvAvatar.pObject = pAvatarCustomization.pPrevAvatar;
		}
		KAUICursorManager.SetDefaultCursor("Loading");
		SetInteractive(interactive: false);
		mToggleBtnMale.SetState(KAUIState.NOT_INTERACTIVE);
		mToggleBtnFemale.SetState(KAUIState.NOT_INTERACTIVE);
		AvatarData avatarData = null;
		avatarData = (isMale ? ((mMaleAvatarDataInstance != null) ? mMaleAvatarDataInstance.mInstance : AvatarData.CreateDefault(isMale ? Gender.Male : Gender.Female)) : ((mFemaleAvatarDataInstance != null) ? mFemaleAvatarDataInstance.mInstance : AvatarData.CreateDefault(isMale ? Gender.Male : Gender.Female)));
		pAvatarCustomization.SetAvatarData(avatarData);
		AvAvatar.CreateDefault(avatarData);
		int num = mPrevSelectedTabIndex;
		mPrevSelectedTabIndex = -1;
		((UiAvatarCustomizationMenu)mMenu).pInitEquipSlots = true;
		if (num == -1)
		{
			num = base.pDefaultTabIndex;
		}
		if (base.pKAUiSelectTabMenu.GetItems().Count > 0)
		{
			KAWidget itemAt = base.pKAUiSelectTabMenu.GetItemAt(num);
			KAToggleButton component = itemAt.GetComponent<KAToggleButton>();
			if (component != null)
			{
				component.SetChecked(isChecked: true);
			}
			base.pKAUiSelectTabMenu.SetSelectedItem(itemAt);
			base.pKAUiSelectTabMenu.OnClick(itemAt);
		}
		CheckAvatarBundlesReady();
	}

	private void ProfileCreated()
	{
		if (mGenericDB != null)
		{
			UnityEngine.Object.Destroy(mGenericDB.gameObject);
		}
		if (!mUiSelectProfile.pFirstLaunch)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
		mUiSelectProfile.pCurrentChild = null;
		if (!UiLogin.pIsGuestUser)
		{
			mUiSelectProfile.SelectProfile(mAvatarName);
		}
		else
		{
			mUiSelectProfile.SelectProfile();
		}
		if (!string.IsNullOrEmpty(_CloseMsg))
		{
			_CloseMsgObject.SendMessage(_CloseMsg, true);
		}
		if (!mUiSelectProfile.pFirstLaunch && !UiLogin.pIsGuestUser)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		pAvatarCustomization.DestroyPreviousAvatar();
		AnalyticAgent.LogFTUEEvent(FTUEEvent.PROFILE_CREATED);
	}

	public override void OnClose()
	{
		OkButtonClicked = null;
		if (mGenericDB != null)
		{
			UnityEngine.Object.Destroy(mGenericDB.gameObject);
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (pIsNewProfile && UiLogin.pIsGuestUser)
		{
			UiLogin.pHasUserJustRegistered = false;
			UiLogin.pGuestUserFirstLaunch = false;
			if (PlayerPrefs.HasKey("GUEST_ACC_CREATED"))
			{
				PlayerPrefs.DeleteKey("GUEST_ACC_CREATED");
			}
			RsResourceManager.UnloadUnusedAssets();
			RsResourceManager.LoadLevel(GameConfig.GetKeyData("LoginScene"));
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (!string.IsNullOrEmpty(_CloseMsg))
		{
			_CloseMsgObject.SendMessage(_CloseMsg, false);
		}
		if (!pIsNewProfile)
		{
			OnCloseCustomization(checkProcessing: false);
		}
		else
		{
			pAvatarCustomization.SwapAvatars();
			mUiSelectProfile.SelectProfile((mUiSelectProfile.pCurrentChild == null) ? null : mUiSelectProfile.pCurrentChild._Name);
		}
		UnityEngine.Object.Destroy(base.gameObject);
		RsResourceManager.UnloadUnusedAssets();
	}

	public void SaveAvatarData()
	{
		if (!(mMenu == null) && mMenu.mModified)
		{
			if (pAvatarCustomization.pCustomAvatar != null)
			{
				pAvatarCustomization.pCustomAvatar.ToAvatarData(AvatarData.pInstanceInfo);
			}
			AvatarData.Save();
			if (AvatarData.pInstanceInfo != null)
			{
				AvatarData.pInstanceInfo.RemovePart();
				AvatarData.pInstanceInfo.LoadBundlesAndUpdateAvatar();
			}
		}
	}

	public void OnCloseCustomization(bool checkProcessing)
	{
		AvatarData.SetDontDestroyOnBundles(inDontDestroy: true);
		if (mMenu != null && mMenu.mModified)
		{
			if (mUiJournalCustomization != null)
			{
				mUiJournalCustomization.pIsProcessingClose = false;
			}
			pAvatarCustomization.SaveCustomAvatar();
			UiToolbar.pAvatarModified = true;
		}
		else
		{
			if (mUiJournalCustomization != null)
			{
				mUiJournalCustomization.pIsProcessingClose = false;
			}
			pAvatarCustomization.SwapAvatars();
		}
		UiAvatarItemCustomization.ClearPairData();
	}

	public void OnAvatarEquipmentLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			DestroyUI();
			mUIobject = UnityEngine.Object.Instantiate((GameObject)inObject);
			mUIobject.transform.parent = base.transform;
			break;
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Failed to load Avatar Equipment....");
			break;
		}
	}

	private void RegisterChild()
	{
		string gender = (mToggleBtnMale.IsChecked() ? "Boy" : "Girl");
		if (mGenericDB == null)
		{
			mGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "MessageBox");
			mGenericDB.SetMessage(base.gameObject, "", "", "", "OnCloseDB");
			mGenericDB.SetDestroyOnClick(isDestroy: true);
			mGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
			KAUI.SetExclusive(mGenericDB, _MaskColor);
		}
		SetMessage(_ProcessingText.GetLocalizedString(), isError: false);
		if (_AllowNameSuggestions)
		{
			WsWebService.RegisterChild(ProductConfig.pProductName, ProductConfig.pToken, mAvatarName, DateTime.MinValue, gender, mSuggestedNameSelected, ServiceEventHandler, null);
		}
		else
		{
			WsWebService.RegisterChild(ProductConfig.pProductName, ProductConfig.pToken, mAvatarName, DateTime.MinValue, gender, ServiceEventHandler, null);
		}
		AnalyticAgent.LogFTUEEvent(FTUEEvent.REGISTERCHILD_STARTED);
	}

	private void SetMessage(string message, bool isError)
	{
		if (!(mGenericDB == null))
		{
			mGenericDB.SetText(message, interactive: false);
			if (isError)
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				mGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: true);
			}
		}
	}

	private void DestroyUI()
	{
		if (mUIobject != null)
		{
			UnityEngine.Object.DestroyImmediate(mUIobject);
		}
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.REGISTER_PARENT:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (inObject != null)
				{
					RegistrationResult registrationResult2 = (RegistrationResult)inObject;
					if (registrationResult2.Status == MembershipUserStatus.Success)
					{
						ProductConfig.pToken = registrationResult2.ApiToken;
						WsWebService.SetToken(ProductConfig.pToken);
						WsTokenMonitor.pCheckToken = true;
						RegisterChild();
					}
					else
					{
						SetMessage(_GeneralWebserviceText.GetLocalizedString(), isError: true);
					}
				}
				else
				{
					SetMessage(_GeneralWebserviceText.GetLocalizedString(), isError: true);
				}
				break;
			case WsServiceEvent.ERROR:
				SetMessage(_GeneralWebserviceText.GetLocalizedString(), isError: true);
				break;
			}
			break;
		case WsServiceType.REGISTER_CHILD:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (inObject != null)
				{
					RegistrationResult registrationResult = null;
					if (_AllowNameSuggestions)
					{
						registrationResult = (RegistrationResult)inObject;
					}
					else
					{
						registrationResult = new RegistrationResult();
						registrationResult.Status = (MembershipUserStatus)inObject;
					}
					if (registrationResult.Status == MembershipUserStatus.Success)
					{
						UiLogin.pHasUserJustRegistered = false;
						SetMessage(_PleaseWaitText.GetLocalizedString(), isError: false);
						mUserId = registrationResult.UserID;
						WsWebService.GetUserProfileByUserID(registrationResult.UserID, ServiceEventHandler, null);
						AnalyticAgent.LogFTUEEvent(FTUEEvent.REGISTERCHILD_SUCCESS);
						break;
					}
					SuggestionResult suggestionResult2 = null;
					if (registrationResult.Suggestions != null && registrationResult.Suggestions.Suggestion != null)
					{
						suggestionResult2 = registrationResult.Suggestions;
					}
					if (registrationResult.Status == MembershipUserStatus.InvalidUserName || registrationResult.Status == MembershipUserStatus.InvalidChildUserName)
					{
						if (_AllowNameSuggestions && suggestionResult2 != null && suggestionResult2.Suggestion.Length != 0)
						{
							ShowSuggestedNames(mAvatarName, suggestionResult2, UiSelectName.FailStatus.Invalid);
						}
						else
						{
							SetMessage(_InvalidUserNameText.GetLocalizedString(), isError: true);
						}
					}
					else if (registrationResult.Status == MembershipUserStatus.DuplicateUserName)
					{
						if (_AllowNameSuggestions && suggestionResult2 != null && suggestionResult2.Suggestion.Length != 0)
						{
							ShowSuggestedNames(mAvatarName, suggestionResult2, UiSelectName.FailStatus.Taken);
						}
						else
						{
							SetMessage(_DuplicateNameText.GetLocalizedString(), isError: true);
						}
					}
					else if (registrationResult.Status == MembershipUserStatus.IsDeleted)
					{
						SetMessage(_DeletedAccountText.GetLocalizedString(), isError: true);
					}
					else if (registrationResult.Status == MembershipUserStatus.TokenNotFound || registrationResult.Status == MembershipUserStatus.InvalidApiToken || registrationResult.Status == MembershipUserStatus.InvalidCulture || registrationResult.Status == MembershipUserStatus.InvalidDOB || registrationResult.Status == MembershipUserStatus.ValidationError || registrationResult.Status == MembershipUserStatus.InvalidChildDOB)
					{
						SetMessage("Error: " + registrationResult.Status, isError: true);
					}
					else
					{
						SetMessage(_GeneralRegistrationText.GetLocalizedString(), isError: true);
					}
					UtDebug.LogError("Error in registering child " + registrationResult.Status);
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary.Add("name", registrationResult.Status);
					AnalyticAgent.LogFTUEEvent(FTUEEvent.REGISTERCHILD_FAILED, dictionary);
				}
				else
				{
					UtDebug.LogError("Error in registering child. inObject was null.");
					SetMessage(_GeneralRegistrationText.GetLocalizedString(), isError: true);
				}
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("Error in registering child. WebService returned Error.");
				SetMessage(_GeneralRegistrationText.GetLocalizedString(), isError: true);
				break;
			}
			break;
		case WsServiceType.GET_USER_PROFILE_BY_USER_ID:
		{
			if (inEvent != WsServiceEvent.COMPLETE)
			{
				break;
			}
			if (inObject == null)
			{
				UtDebug.LogError("WEB SERVICE CALL GetUserProfile RETURNED NO DATA!!!");
				UtUtilities.ShowGenericValidationError();
				break;
			}
			UserProfileData userProfileData = (UserProfileData)inObject;
			UserProfileDataList userProfileDataList = new UserProfileDataList();
			if (mUiSelectProfile.pChildListInfo == null || mUiSelectProfile.pChildListInfo.UserProfiles.Length < 1)
			{
				userProfileDataList.UserProfiles = new UserProfileData[1];
				userProfileDataList.UserProfiles[0] = userProfileData;
			}
			else
			{
				userProfileDataList.UserProfiles = new UserProfileData[mUiSelectProfile.pChildListInfo.UserProfiles.Length + 1];
				for (int i = 0; i < mUiSelectProfile.pChildListInfo.UserProfiles.Length; i++)
				{
					userProfileDataList.UserProfiles[i] = mUiSelectProfile.pChildListInfo.UserProfiles[i];
				}
				userProfileDataList.UserProfiles[mUiSelectProfile.pChildListInfo.UserProfiles.Length] = userProfileData;
			}
			mUiSelectProfile.pChildListInfo = userProfileDataList;
			WsWebService.LoginChild(ProductConfig.pToken, userProfileData.ID, UtUtilities.GetLocaleLanguage(), ServiceEventHandler, null);
			AnalyticAgent.LogFTUEEvent(FTUEEvent.GETUSERPROFILEBYUSERID_COMPLETED);
			break;
		}
		case WsServiceType.LOGIN_CHILD:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (inObject != null)
				{
					WsWebService.SetToken((string)inObject);
					UserInfo.Reset();
					UserInfo.Init();
					ProductData.Init();
					AvatarData.pInstance.DisplayName = mAvatarName;
					mChildInfo._Name = mAvatarName;
					if (mSaveAvatarDataAfterLogin)
					{
						mChildInfo._UserID = mUiSelectProfile.pChildInfo[0]._UserID;
						mUiSelectProfile.pChildInfo[0] = mChildInfo;
					}
					else
					{
						mChildInfo._UserID = mUserId;
						mUiSelectProfile.pChildInfo.Add(mChildInfo);
					}
					if (pIsNewProfile)
					{
						if (mSaveAvatarDataAfterLogin)
						{
							mSaveAvatarDataAfterLogin = false;
							mSkipLoginChild = true;
							mIsSavingSecondaryVersion = true;
							WsWebService.SetAvatar(AvatarData.pInstance, ServiceEventHandler, null);
							AnalyticAgent.LogFTUEEvent(FTUEEvent.LOGINCHILD_COMPLETED);
						}
						else
						{
							mIsSaving = true;
							SetMessage(_SettingUpProfileText.GetLocalizedString(), isError: false);
						}
					}
				}
				else
				{
					UtDebug.LogError("Error in login child. inObject was null.");
					SetMessage(_GeneralWebserviceText.GetLocalizedString(), isError: true);
				}
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("Error in login child. WebService returned error.");
				SetMessage(_GeneralWebserviceText.GetLocalizedString(), isError: true);
				break;
			}
			break;
		case WsServiceType.SET_AVATAR:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				SetAvatarResult setAvatarResult = (SetAvatarResult)inObject;
				if (setAvatarResult.Success)
				{
					mIsSaving = true;
					AnalyticAgent.LogFTUEEvent(FTUEEvent.SETAVATAR_COMPLETED);
				}
				else if (setAvatarResult.StatusCode == AvatarValidationResult.AvatarDisplayNameInvalid)
				{
					SuggestionResult suggestionResult = null;
					if (setAvatarResult.Suggestions != null && setAvatarResult.Suggestions.Suggestion != null)
					{
						suggestionResult = setAvatarResult.Suggestions;
					}
					if (_AllowNameSuggestions && suggestionResult != null && suggestionResult.Suggestion.Length != 0)
					{
						ShowSuggestedNames(mAvatarName, suggestionResult, UiSelectName.FailStatus.Invalid);
					}
					else
					{
						ShowErrorDB(_InvalidUserNameText.GetLocalizedString());
					}
				}
				else
				{
					ShowErrorDB(_GeneralWebserviceText.GetLocalizedString());
				}
				break;
			}
			case WsServiceEvent.ERROR:
				ShowErrorDB(_GeneralWebserviceText.GetLocalizedString());
				break;
			}
			break;
		}
	}

	private void ShowSuggestedNames(string name, SuggestionResult result, UiSelectName.FailStatus status)
	{
		if (mGenericDB != null)
		{
			UnityEngine.Object.Destroy(mGenericDB.gameObject);
		}
		UiSelectName.Init(OnSelectNameProcessed, name, result.Suggestion, status);
	}

	private void OnCloseDB()
	{
		SetState(KAUIState.INTERACTIVE);
	}

	private void OpenAvatarPartCategory()
	{
		DestroyUI();
		ShowColorPalette(inStatus: true);
		ShowColorButtons(inStatus: true);
		if (mSelectedColorBtn == null)
		{
			mSelectedColorBtn = mSkinColorBtn;
		}
		mZoom = mSelectedColorBtn != mSkinColorBtn;
		SetSkinColorPickersVisibility();
		if (mCategoryMenu != null)
		{
			mCategoryMenu.ClearItems();
		}
		UIPanel component = mMenu.GetComponent<UIPanel>();
		component.clipOffset = _AvatarTabPanelOffset;
		Vector4 baseClipRegion = component.baseClipRegion;
		baseClipRegion.w = _AvatarTabPanelHeight;
		component.baseClipRegion = baseClipRegion;
		mMenu.transform.localPosition = _AvatarTabMenuPosition;
		mMenu.pVerticalScrollbar._DownArrow.SetPosition(_AvatarTabScrollBarDownArrowPosition.x, _AvatarTabScrollBarDownArrowPosition.y);
		mMenu.pVerticalScrollbar.pScrollbar.foregroundWidget.height = _AvatarTabScrollBarHeight;
		mMenu.pVerticalScrollbar.pScrollbar.backgroundWidget.height = _AvatarTabScrollBarHeight;
		((UiAvatarCustomizationMenu)mMenu).EnableEquippedSlots(enable: false);
		foreach (AvatarPartCat item in _AvatarPartCategory)
		{
			KAWidget kAWidget = DuplicateWidget("CatTemplateBtn");
			kAWidget.gameObject.name = item._BtnName;
			kAWidget.SetTexture(item._Icon);
			if (!string.IsNullOrEmpty(item._DisplayText._Text))
			{
				kAWidget.SetText(item._DisplayText.GetLocalizedString());
			}
			kAWidget._TooltipInfo._Text = item._ToolTipText;
			if (mCategoryMenu != null)
			{
				mCategoryMenu.AddWidget(kAWidget);
			}
			kAWidget.SetVisibility(inVisible: true);
		}
		if (!(mCategoryMenu == null) && mCategoryMenu.GetNumItems() > 0)
		{
			mCategoryMenu.GetItemAt(0).SetDisabled(isDisabled: true);
			_LastChosenCategoryItem = mCategoryMenu.GetItemAt(0);
			mZoomItem = _ZoomInForItems.Find((ZoomItem x) => x._ZoomItemName.Equals(mCategoryMenu.GetItemAt(0).name));
			if (mZoomItem != null)
			{
				mZoom = true;
				mResetRotation = true;
			}
			else
			{
				mZoom = false;
				mResetRotation = false;
			}
		}
	}

	private void OpenClothesPartCategory()
	{
		DestroyUI();
		ShowColorPalette(inStatus: false);
		ShowColorButtons(inStatus: false);
		if (mCategoryMenu != null)
		{
			mCategoryMenu.ClearItems();
		}
		UIPanel component = mMenu.GetComponent<UIPanel>();
		component.clipOffset = _ClothTabPanelOffset;
		Vector4 baseClipRegion = component.baseClipRegion;
		baseClipRegion.w = _ClothTabPanelHeight;
		component.baseClipRegion = baseClipRegion;
		mMenu.transform.localPosition = _ClothTabMenuPosition;
		mMenu.pVerticalScrollbar._DownArrow.SetPosition(_ClothTabScrollBarDownArrowPosition.x, _ClothTabScrollBarDownArrowPosition.y);
		mMenu.pVerticalScrollbar.pScrollbar.foregroundWidget.height = _ClothTabScrollBarHeight;
		mMenu.pVerticalScrollbar.pScrollbar.backgroundWidget.height = _ClothTabScrollBarHeight;
		((UiAvatarCustomizationMenu)mMenu).EnableEquippedSlots(enable: true);
		foreach (AvatarPartCat item in _ClothingPartCategory)
		{
			if (!(item._PartName != "All") || base.pKAUiSelectTabMenu.pSelectedTab.pTabData == null || base.pKAUiSelectTabMenu.pSelectedTab.pTabData.HasCategory(AvatarData.GetCategoryID(item._PartName)))
			{
				KAWidget kAWidget = DuplicateWidget("CatTemplateBtn");
				kAWidget.gameObject.name = item._BtnName;
				kAWidget.SetTexture(item._Icon, inPixelPerfect: true);
				if (!string.IsNullOrEmpty(item._DisplayText._Text))
				{
					kAWidget.SetText(item._DisplayText.GetLocalizedString());
				}
				kAWidget._TooltipInfo._Text = item._ToolTipText;
				if (mCategoryMenu != null)
				{
					mCategoryMenu.AddWidget(kAWidget);
				}
				kAWidget.SetVisibility(inVisible: true);
			}
		}
		if (!(mCategoryMenu == null) && mCategoryMenu.GetNumItems() > 0)
		{
			mCategoryMenu.GetItemAt(0).SetDisabled(isDisabled: true);
			_LastChosenCategoryItem = mCategoryMenu.GetItemAt(0);
			mZoomItem = _ZoomInForItems.Find((ZoomItem x) => x._ZoomItemName.Equals(mCategoryMenu.GetItemAt(0).name));
			if (mZoomItem != null)
			{
				mZoom = true;
				mResetRotation = true;
			}
			else
			{
				mZoom = false;
				mResetRotation = false;
			}
		}
	}

	public void SetCustomizeBtnVisiblity(bool isVisible)
	{
		if (mCustomizeBtn != null)
		{
			mCustomizeBtn.SetVisibility(isVisible);
		}
	}

	private void ShowAvatarStats()
	{
		ItemStat[] array = null;
		array = ((pCustomAvatar == null) ? AvatarData.pInstanceInfo.GetPartsCombinedStats() : AvatarData.pInstanceInfo.GetPartsCombinedStats(pCustomAvatar.GetPartUiids()));
		CharacterData characterData = new CharacterData(CharacterDatabase.pInstance.GetAvatar());
		mUiStats.SetVisibility(inVisible: true);
		int rank = ((UserRankData.pInstance == null) ? 1 : UserRankData.pInstance.RankID);
		mUiStats.ShowCombatStats(characterData, rank, AvatarData.pInstance.DisplayName, array);
	}

	public override void AddWidgetData(KAWidget inWidget, KAUISelectItemData widgetData)
	{
		base.AddWidgetData(inWidget, widgetData);
		UserItemData userItemData = widgetData?._UserItemData;
		ItemData itemData = widgetData?._ItemData;
		bool flag = userItemData?.pIsBattleReady ?? false;
		KAWidget kAWidget = inWidget.FindChildItem("BattleReadyIcon");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(flag);
		}
		KAWidget kAWidget2 = inWidget.FindChildItem("FlightReadyIcon");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(itemData?.HasAttribute("FlightSuit") ?? false);
		}
		KAWidget kAWidget3 = inWidget.FindChildItem(_ItemColorWidget);
		if (!(kAWidget3 == null))
		{
			if (flag)
			{
				UiItemRarityColorSet.SetItemBackgroundColor((itemData == null || !itemData.ItemRarity.HasValue) ? ItemRarity.Common : itemData.ItemRarity.Value, kAWidget3);
			}
			else
			{
				UiItemRarityColorSet.SetItemBackgroundColor(((UiAvatarCustomizationMenu)mMenu).pItemDefaultColor, kAWidget3);
			}
		}
	}

	public override KAWidget AddEmptySlot()
	{
		KAWidget kAWidget = mKAUiSelectMenu.AddWidget("EmptySlot");
		if (kAWidget != null)
		{
			AddWidgetData(kAWidget, null);
		}
		return kAWidget;
	}

	public override void SelectItem(KAWidget item)
	{
		base.SelectItem(item);
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
		if (kAUISelectItemData != null && kAUISelectItemData._SlotLocked)
		{
			mKAUiSelectTabMenu.pSelectedTab.pTabData.BuySlot(base.gameObject);
		}
	}

	protected override void OnItemPurchaseComplete()
	{
		base.OnItemPurchaseComplete();
		UpdateSlotsInfo();
	}

	private void OnDisable()
	{
		if (mUiCategory != null && mUiCategory.GetVisibility())
		{
			mUiCategory.SetVisibility(inVisible: false);
		}
	}

	private void SetNewProfileTabs(bool newProfile)
	{
		if (newProfile)
		{
			BattleReadyTabIndex = -1;
			ClothesTabIndex = 0;
			AvatarTabIndex = 1;
		}
		else
		{
			BattleReadyTabIndex = 0;
			ClothesTabIndex = 1;
			AvatarTabIndex = 2;
		}
	}
}
