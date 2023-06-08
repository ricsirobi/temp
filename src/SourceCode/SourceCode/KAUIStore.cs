using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SquadTactics;
using UnityEngine;

public class KAUIStore : KAUIStoreBase, IAdResult
{
	public enum StoreMode
	{
		UnKnown,
		AD,
		Choose,
		CheckOut,
		Syncing
	}

	private static KAUIStore mInstance = null;

	public static bool pUpdateAvatar = true;

	private static bool mIsInItsOwnScene = false;

	public static string _EnterSelection = "";

	public static string _BackToJournalTab = string.Empty;

	public static string _EnterCategoryName = "";

	public static int _EnterItemID = 0;

	public static string _StoreExitMarker = null;

	public static int _EnterSelectionID = -1;

	public static int _EnterCategoryID = -1;

	public static bool _ShowUpsellOnStoreStart = false;

	public static int _ShowUpsellPerSessionCounter = 0;

	public static string _ExitLevelName = "";

	private const string UPSELL_KEY = "UpsellShown";

	public string _TermsAndConditionsURL = "http://www.schoolofdragons.com/help/siteterms";

	public string _PrivacyPolicyURL = "http://www.schoolofdragons.com/help/privacypolicy";

	public KAUIStoreFullPreview _StoreFullPreviewUI;

	public Transform _StorePreviewHolder;

	public GameObject _StoreBgCamera;

	public Vector3 _CameraOffset = new Vector3(0f, 0.3f, 1.3f);

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public Texture2D _DollarSign;

	public AudioClip _NoMoneyClip;

	public AudioClip _LockedClip;

	public AudioClip[] _RankLockedClip;

	public LocaleString _NonMemberText;

	public LocaleString _InsufficientGameCurrencyText = new LocaleString("You don't have enough Coins");

	public LocaleString _InsufficientCashCurrencyText = new LocaleString("You don't have enough Gems. Do you want to buy Gems?");

	public LocaleString _AvailableTilText = new LocaleString("Til XXXX");

	public LocaleString _TimeLeftMinText = new LocaleString("< XXXX min");

	public LocaleString _TimeLeftMinsText = new LocaleString("< XXXX mins");

	public LocaleString _TimeLeftHoursText = new LocaleString("< XXXX hrs");

	public LocaleString _StoreUnAvailableErrorText = new LocaleString("Store currenlty not available");

	public LocaleString _AdRewardSuccessText = new LocaleString("Item earned successfully");

	public LocaleString _AdRewardFailedText = new LocaleString("Reward failed to load. Please try again later.");

	public LocaleString _ItemExpiredText = new LocaleString("This item is currently not available.");

	public KAWidget _DragWidget;

	public string _BecomeMemberLogEventText = "BecomeMemberFromStore";

	public StoreUIMapping[] _StoreUIMappings;

	public PrereqItemUIMapping[] _PrereqItemUIMappings;

	public float _TimeToForceStoreLoad = 600f;

	public string _MysteryBoxBundlePath = "RS_DATA/PfUiClaimMysteryBoxDO.unity3d/PfUiClaimMysteryBoxDO";

	public string _MembershipUpsellPath = "RS_DATA/PfUiMembershipUpsell.unity3d/PfUiMembershipUpsell";

	public StoreLoader.Selection _CoinsStoreInfo;

	public StoreLoader.Selection _GemsStoreInfo;

	public StoreLoader.Selection _MembershipStoreInfo;

	public AdEventType _AdEventType;

	public int _AdDisplayItemCostThreshold = 5;

	public bool _CustomizationPending;

	public int _ShowUpsellPerSessionCount = 3;

	public int _ShowUpsellPerDayCount = 3;

	public int _ItemUpsellProbability = 30;

	private List<PromoPackage> mPromoPackages = new List<PromoPackage>();

	private int mItemOffersToLoad;

	[NonSerialized]
	public List<GameObject> _ExitMessageObjects = new List<GameObject>();

	[NonSerialized]
	public PurchaseItemData _CurrentPurchaseItem;

	[NonSerialized]
	public SnChannel _SoundChannel;

	[NonSerialized]
	public int _BoughtItemID = -1;

	[NonSerialized]
	public bool _ProcessingClose;

	private AvAvatarState mLastAvatarState;

	private List<StoreData> mStoreData;

	private KAStoreInfo mStoreInfo;

	private StoreMode mStoreMode;

	private string mStoreName = "";

	private int[] mStoreIDs;

	private string mCategory = "";

	private StoreFilter mFilter;

	private bool mStoreLoading;

	private bool mStoreClosing;

	private bool mInitExit;

	private bool mLoadStore;

	private bool mIsRestartTutStep;

	private static int mLocked = 0;

	private KAUIStoreCategory mCategoryUI;

	private KAUIStoreCategoryMenu mCategoryMenu;

	private KAUIStoreMainMenu mMainMenu;

	private KAUIStoreInvMenuBase mInvMenu;

	private KAUIStoreChooseMenu mChooseMenu;

	private KAUIStoreChoose mChooseUI;

	private KAUIStoreSyncPopUp mSyncPopup;

	private KAUIStoreBuyPopUp mBuyPopup;

	private UiStoreStatCompare mStatCompareMenu;

	private UiStoreDragonStat mDragonStatMenu;

	private KAWidget mCost;

	private KAWidget mCoinsTotal;

	private KAWidget mCoinsTotalBtn;

	private KAWidget mGemsTotalTxt;

	private KAWidget mGemTotal;

	private KAWidget mStoreTotal;

	private KAWidget mBuyNow;

	private KAWidget mCheckoutGrp;

	private KAWidget mBtnClose;

	private KAWidget mBtnPreviewRotateLt;

	private KAWidget mBtnPreviewRotateRt;

	private KAWidget mRotateIcon;

	private KAWidget mBtnPreviewBuy;

	private KAWidget mBtnFullPreview;

	private KAWidget mBtnBecomeMember;

	private KAWidget mBtnUpgradeMember;

	private KAWidget mBtnInvite;

	private KAWidget mPreviewLoading;

	private KAWidget mGemsDB;

	private KAWidget mSubscriptionInfoBtn;

	private KAWidget mBtnAds;

	private KAToggleButton mBtnStores;

	private GameObject mUiGenericDB;

	private KAWidget mTermsBtn;

	private KAWidget mPrivacyPolicyBtn;

	public static KAUIStore pInstance => mInstance;

	public static bool pIsInItsOwnScene => mIsInItsOwnScene;

	public KAStoreInfo pStoreInfo => mStoreInfo;

	public string pStoreName => mStoreName;

	public int[] pStoreIDs => mStoreIDs;

	public string pCategory => mCategory;

	public StoreFilter pFilter
	{
		get
		{
			return mFilter;
		}
		set
		{
			mFilter = value;
		}
	}

	public KAUIStoreMainMenu pMainMenu => mMainMenu;

	public KAUIStoreInvMenuBase pInvMenu => mInvMenu;

	public KAUIStoreCategoryMenu pCategoryMenu => mCategoryMenu;

	public KAUIStoreChooseMenu pChooseMenu => mChooseMenu;

	public KAUIStoreChoose pChooseUI => mChooseUI;

	public KAUIStoreSyncPopUp pSyncPopup => mSyncPopup;

	public KAUIStoreBuyPopUp pBuyPopup => mBuyPopup;

	public UiStoreStatCompare pStatCompareMenu => mStatCompareMenu;

	public UiStoreDragonStat pDragonStatMenu => mDragonStatMenu;

	public KAWidget pStoreTotal => mStoreTotal;

	public KAWidget pBuyNow => mBuyNow;

	public KAWidget pPreviewLoading => mPreviewLoading;

	public static bool pLocked => mLocked > 0;

	public static void LockStore()
	{
		mLocked++;
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		EnableStoreUIs(inEnable: false);
	}

	public static void UnlockStore()
	{
		mLocked = Mathf.Max(0, mLocked - 1);
		KAUICursorManager.SetDefaultCursor();
		EnableStoreUIs(inEnable: true);
	}

	public static void EnableStoreUIs(bool inEnable)
	{
		if (!(pInstance == null))
		{
			pInstance.SetInteractive(inEnable);
			if (pInstance.pChooseUI != null)
			{
				pInstance.pChooseUI.SetInteractive(inEnable);
			}
			if (pInstance.pChooseMenu != null)
			{
				pInstance.pChooseMenu.SetInteractive(inEnable);
			}
			if (pInstance.pCategoryMenu != null)
			{
				pInstance.pCategoryMenu.SetInteractive(inEnable);
			}
			if (pInstance.pMainMenu != null)
			{
				pInstance.pMainMenu.SetInteractive(inEnable);
			}
		}
	}

	public void EnableStoreMenu(bool inEnable)
	{
		if (!(mBtnStores == null) && mBtnStores.GetVisibility() && (inEnable || mMainMenu.GetVisibility()))
		{
			mBtnStores.SetChecked(inEnable);
			if (mMainMenu != null)
			{
				mMainMenu.SetVisibility(inEnable);
			}
		}
	}

	public void HideStoreUIs(bool hide)
	{
		pInstance.SetVisibility(!hide);
		if (pInstance.pChooseUI != null)
		{
			pInstance.pChooseUI.SetVisibility(!hide);
		}
		if (pInstance.pChooseMenu != null)
		{
			pInstance.pChooseMenu.SetVisibility(!hide);
		}
		if (pInstance.pCategoryMenu != null)
		{
			pInstance.pCategoryMenu.SetVisibility(!hide);
		}
		if (!(pInstance.pMainMenu != null))
		{
			return;
		}
		if (!hide)
		{
			if (mBtnStores == null || !mBtnStores.GetVisibility())
			{
				pInstance.pMainMenu.SetVisibility(!hide);
			}
		}
		else
		{
			pInstance.pMainMenu.SetVisibility(!hide);
		}
	}

	public void HideCompleteStore(bool hide)
	{
		mLoadStore = !hide;
		pInstance.SetVisibility(!hide);
		if (pInstance.pChooseUI != null)
		{
			pInstance.pChooseUI.SetVisibility(!hide);
		}
		if (pInstance.pChooseMenu != null)
		{
			pInstance.pChooseMenu.SetVisibility(!hide);
		}
		if (pInstance.pCategoryMenu != null)
		{
			pInstance.pCategoryMenu.SetVisibility(!hide);
		}
		if (pInstance.pMainMenu != null)
		{
			if (!hide)
			{
				if (mBtnStores == null || !mBtnStores.GetVisibility())
				{
					pInstance.pMainMenu.SetVisibility(!hide);
				}
			}
			else
			{
				pInstance.pMainMenu.SetVisibility(!hide);
			}
		}
		if (pInstance._StoreBgCamera != null)
		{
			pInstance._StoreBgCamera.SetActive(!hide);
		}
		if (pInstance._StoreFullPreviewUI._StoreFullPreviewCamera != null)
		{
			pInstance._StoreFullPreviewUI._StoreFullPreviewCamera.gameObject.SetActive(!hide);
		}
		if (pInstance._StoreFullPreviewUI._StorePreviewCamera != null)
		{
			pInstance._StoreFullPreviewUI._StorePreviewCamera.gameObject.SetActive(!hide);
		}
	}

	protected override void Awake()
	{
		mInstance = this;
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		if (IAPManager.pInstance != null)
		{
			IAPManager.pInstance.AddToMsglist(base.gameObject);
		}
		mLocked = 0;
		mStoreInfo = null;
		mIsInItsOwnScene = RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("StoreScene");
		mCategoryUI = base.transform.parent.GetComponentInChildren<KAUIStoreCategory>();
		mCategoryMenu = base.transform.parent.GetComponentInChildren<KAUIStoreCategoryMenu>();
		mMainMenu = base.transform.parent.GetComponentInChildren<KAUIStoreMainMenu>();
		mChooseMenu = base.transform.parent.GetComponentInChildren<KAUIStoreChooseMenu>();
		mStatCompareMenu = base.transform.parent.GetComponentInChildren<UiStoreStatCompare>();
		mDragonStatMenu = base.transform.parent.GetComponentInChildren<UiStoreDragonStat>();
		mCheckoutGrp = FindItem("CheckoutGrp");
		mBuyNow = FindItem("btnBuyNow");
		mCost = FindItem("txtCost");
		mCoinsTotal = FindItem("txtCoinTotal");
		mGemsTotalTxt = FindItem("txtGemsTotal");
		mGemTotal = FindItem("GemTotal");
		mCoinsTotalBtn = FindItem("CoinTotal");
		mStoreTotal = FindItem("txtStoreTotal");
		mBtnClose = FindItem("btnClose");
		mBtnPreviewRotateLt = FindItem("BtnPreviewRotateLt");
		mBtnPreviewRotateRt = FindItem("BtnPreviewRotateRt");
		mRotateIcon = FindItem("RotateIcon");
		mBtnStores = (KAToggleButton)FindItem("BtnStores");
		mBtnFullPreview = FindItem("BtnOpenPreview");
		mBtnPreviewBuy = FindItem("BtnPreviewBuy");
		mBtnBecomeMember = FindItem("BtnBecomeMember");
		mBtnUpgradeMember = FindItem("BtnUpgradeMember");
		mBtnInvite = FindItem("EarnGemsBtn");
		mSubscriptionInfoBtn = FindItem("BtnSubscriptionInfo");
		mBtnAds = FindItem("BtnAds");
		if (mSubscriptionInfoBtn != null)
		{
			mSubscriptionInfoBtn.SetVisibility(inVisible: false);
		}
		mTermsBtn = FindItem("BtnTermsConditions");
		if (mTermsBtn != null)
		{
			mTermsBtn.SetVisibility(inVisible: false);
		}
		mPrivacyPolicyBtn = FindItem("BtnPrivacyPolicy");
		if (mPrivacyPolicyBtn != null)
		{
			mPrivacyPolicyBtn.SetVisibility(inVisible: false);
		}
		if (mBtnInvite != null)
		{
			mBtnInvite.SetVisibility(inVisible: true);
		}
		mPreviewLoading = FindItem("PreviewLoading");
		mGemsDB = FindItem("InsufficientGemsDB");
		mSyncPopup = base.transform.parent.GetComponentInChildren<KAUIStoreSyncPopUp>();
		mBuyPopup = base.transform.parent.GetComponentInChildren<KAUIStoreBuyPopUp>();
		if (Money.pInstance == null || !Money.pIsReady)
		{
			Debug.LogError("MONEY SERVICE NOT INITIALIZED!!!");
		}
		mCoinsTotal.SetText(Money.pGameCurrency.ToString());
		if (mGemsTotalTxt != null)
		{
			mGemsTotalTxt.SetText(Money.pCashCurrency.ToString());
		}
		mChooseMenu._GenderFilter = "U";
		if (AvatarData.pInstance != null)
		{
			if (AvatarData.GetGender() == Gender.Male)
			{
				mChooseMenu._GenderFilter = "M";
			}
			else if (AvatarData.GetGender() == Gender.Female)
			{
				mChooseMenu._GenderFilter = "F";
			}
		}
		Money.AddNotificationObject(base.gameObject);
		if (mIsInItsOwnScene)
		{
			RsResourceManager.DestroyLoadScreen();
		}
		if (pUpdateAvatar)
		{
			StartCoroutine(UpdateAvatar());
		}
		mMainMenu.SetVisibility(!(mBtnStores != null) || !mBtnStores.GetVisibility());
		mMainMenu.SetState(KAUIState.INTERACTIVE);
		mMainMenu._DefaultGrid.Reposition();
		mBtnUpgradeMember.SetVisibility(IAPManager.IsMembershipUpgradeable());
		mBtnBecomeMember.SetVisibility(!SubscriptionInfo.pIsMember);
		if (UtPlatform.IsMobile())
		{
			AdManager.DisplayAd(AdEventType.STORE_ENTERED, AdOption.FULL_SCREEN);
		}
		if (StorePreview.pInstance == null)
		{
			StorePreview.Start(pPreviewLoading, null);
		}
		else
		{
			StorePreview.pInstance.mLoadingWidget = pPreviewLoading;
		}
		if (_EnterCategoryID > 0)
		{
			GetStoreAndCategoryName(_EnterSelectionID, _EnterCategoryID, out _EnterSelection, out _EnterCategoryName);
		}
		else if (_EnterSelectionID > 0)
		{
			_EnterSelection = GetStoreName(_EnterSelectionID);
		}
		_EnterSelectionID = -1;
		_EnterCategoryID = -1;
		if (_ShowUpsellOnStoreStart && _ShowUpsellPerSessionCounter < _ShowUpsellPerSessionCount && CanShowUpsellForTheDay())
		{
			ShowUpsellOnStoreStart();
		}
	}

	private IEnumerator UpdateAvatar()
	{
		yield return new WaitForEndOfFrame();
		AvAvatar.SetUIActive(inActive: false);
		mLastAvatarState = AvAvatar.pState;
		AvAvatar.pState = AvAvatarState.NONE;
		AvAvatar.SetOnlyAvatarActive(active: false);
	}

	public void LoadStore(string inEnterSelection, string inEnterCategoryName)
	{
		_EnterSelection = inEnterSelection;
		_EnterCategoryName = inEnterCategoryName;
		LoadSelectedStore();
	}

	public override void LoadSelectedStore()
	{
		base.LoadSelectedStore();
		if (!GetVisibility())
		{
			return;
		}
		if (_EnterSelection.Length > 0)
		{
			KAStoreInfo kAStoreInfo = mMainMenu.FindStore(_EnterSelection);
			SelectStore(kAStoreInfo);
			mMainMenu.SetSelectedWidget(kAStoreInfo.GetItem());
			if (mChooseUI != null)
			{
				mChooseUI.OnStoreSelected(kAStoreInfo);
			}
		}
		else if (mMainMenu._StoreData.Length != 0)
		{
			SelectStore(mMainMenu._StoreData[0]);
		}
		else
		{
			Debug.LogError("There are no stores present");
		}
		if (UiLogin.pIsGuestUser)
		{
			ShowRegisterPrompt();
		}
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "StoreOpened");
		}
	}

	public StoreUIMapping GetStoreUIMappingData(int inStoreID)
	{
		if (_StoreUIMappings == null)
		{
			return null;
		}
		StoreUIMapping[] storeUIMappings = _StoreUIMappings;
		foreach (StoreUIMapping storeUIMapping in storeUIMappings)
		{
			if (storeUIMapping != null && storeUIMapping._StoreID == inStoreID)
			{
				return storeUIMapping;
			}
		}
		return null;
	}

	public PrereqItemUIMapping GetPrereqItemUIMappingData(int inPrereqItemID)
	{
		if (_PrereqItemUIMappings == null)
		{
			return null;
		}
		PrereqItemUIMapping[] prereqItemUIMappings = _PrereqItemUIMappings;
		foreach (PrereqItemUIMapping prereqItemUIMapping in prereqItemUIMappings)
		{
			if (prereqItemUIMapping != null && prereqItemUIMapping._PrereqItemID == inPrereqItemID)
			{
				return prereqItemUIMapping;
			}
		}
		return null;
	}

	public static StorePreviewCategory GetPreviewCategory(ItemData inItemData)
	{
		if (!string.IsNullOrEmpty(AvatarData.GetItemPartType(inItemData)))
		{
			return StorePreviewCategory.Avatar;
		}
		if (RaisedPetData.GetAccessoryType(inItemData) != 0 || inItemData.GetAttribute("PetTypeID", -1) >= 0)
		{
			return StorePreviewCategory.RaisedPet;
		}
		return StorePreviewCategory.Normal3D;
	}

	public void ProcessChooseSelection(KAWidget item)
	{
		if (item != null)
		{
			int num = -1;
			StoreWidgetUserData storeWidgetUserData = (StoreWidgetUserData)item.GetUserData();
			if (storeWidgetUserData.PurchaseStoreType == KAStoreMenuItemData.StoreType.GameStore)
			{
				if (storeWidgetUserData is KAStoreItemData kAStoreItemData)
				{
					num = kAStoreItemData._StoreID;
				}
			}
			else
			{
				num = (int)GetSelectedIAPStoreCategoryType();
			}
			if (num != -1)
			{
				StoreUIMapping storeUIMappingData = GetStoreUIMappingData(num);
				if (storeUIMappingData != null)
				{
					SelectChooseUI(storeUIMappingData._PfName, storeUIMappingData._GUIName);
				}
			}
		}
		mChooseUI.ProcessChooseSelection(item);
	}

	private string GetStoreIDString()
	{
		if (mStoreIDs == null || mStoreIDs.Length == 0)
		{
			return string.Empty;
		}
		string text = string.Empty;
		int[] array = mStoreIDs;
		foreach (int num in array)
		{
			text = text + ":" + num;
		}
		return text;
	}

	private void SelectChooseUI(string inPfName, string inGUIName)
	{
		if (string.IsNullOrEmpty(inPfName) || string.IsNullOrEmpty(inGUIName))
		{
			return;
		}
		KAUIStoreChoose kAUIStoreChoose = (KAUIStoreChoose)GameObject.Find(inPfName).GetComponent(inGUIName);
		if (!(mChooseUI == kAUIStoreChoose))
		{
			if (mChooseUI.GetVisibility())
			{
				mChooseUI.SetVisibility(t: false);
			}
			mChooseUI.enabled = false;
			mChooseUI = kAUIStoreChoose;
			mChooseUI._StoreUI = this;
			ShowChooseUI(t: true);
		}
	}

	public void OnStoreSelected(KAStoreInfo st)
	{
		_EnterItemID = 0;
		EnableStoreMenu(inEnable: false);
		mChooseUI.OnStoreSelected(st);
		SelectStore(st);
	}

	public void SelectStore(KAStoreInfo st)
	{
		_BoughtItemID = -1;
		mStoreInfo = st;
		mStoreIDs = st._IDs;
		if (mChooseUI != null)
		{
			mChooseUI.SetVisibility(t: false);
		}
		Transform transform = _StorePreviewHolder.Find(st._PfName);
		mChooseUI = (KAUIStoreChoose)transform.GetComponent(st._GUIName);
		mChooseUI._StoreUI = this;
		mChooseMenu.ResetMenu();
		ProcessChooseSelection(null);
		_CurrentPurchaseItem = null;
		LoadStores();
	}

	public void SetSelectedStoreIcon(Texture tex)
	{
	}

	public void LoadStoresReal()
	{
		mLoadStore = false;
		SetInteractive(interactive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		if (!mStoreLoading)
		{
			ConnectivityMonitor.AddConnectionHandler(OnNetworkConnected);
			mStoreLoading = true;
		}
		new StoreBundledItemLoader().Load(mStoreIDs, OnMultipleStoreLoaded, null, _TimeToForceStoreLoad);
		mStoreInfo._LoadedTime = Time.time;
	}

	private void OnNetworkConnected()
	{
		if (mStoreLoading)
		{
			LoadStoresReal();
		}
	}

	public void LoadStores()
	{
		mLoadStore = true;
	}

	public void OnMultipleStoreLoaded(List<StoreData> inStoreData, object inUserData)
	{
		if (inStoreData == null)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			ShowDialog(_StoreUnAvailableErrorText.GetLocalizedString(), "CloseStore");
			return;
		}
		List<int> list = new List<int>();
		foreach (StoreData inStoreDatum in inStoreData)
		{
			if (inStoreDatum != null && !list.Contains(inStoreDatum._ID))
			{
				list.Add(inStoreDatum._ID);
			}
		}
		if (UtUtilities.IsSameList(list.ToArray(), mStoreIDs))
		{
			mStoreData = inStoreData;
			bool flag = false;
			KAStoreMenuItemData[] menuData = mStoreInfo._MenuData;
			foreach (KAStoreMenuItemData kAStoreMenuItemData in menuData)
			{
				StoreFilter filter = kAStoreMenuItemData._Filter;
				foreach (StoreData inStoreDatum2 in inStoreData)
				{
					if (inStoreDatum2.FindCategoryData(filter) != null)
					{
						continue;
					}
					StoreCategoryData storeCategoryData = null;
					List<ItemData> list2 = StoreFilter.Filter(inStoreDatum2._Items, filter);
					if (list2 != null && list2.Count > 0)
					{
						if (storeCategoryData == null)
						{
							storeCategoryData = new StoreCategoryData();
						}
						storeCategoryData.AddData(list2);
						inStoreDatum2.AddCategoryData(filter, storeCategoryData);
					}
				}
				kAStoreMenuItemData._IsEnabled = false;
				if (kAStoreMenuItemData._StoreType == KAStoreMenuItemData.StoreType.IAPStore)
				{
					kAStoreMenuItemData._IsEnabled = true;
				}
				else
				{
					bool flag2 = false;
					foreach (StoreData inStoreDatum3 in inStoreData)
					{
						if (flag2)
						{
							break;
						}
						StoreCategoryData storeCategoryData2 = inStoreDatum3.FindCategoryData(kAStoreMenuItemData._Filter);
						if (storeCategoryData2?._Items == null || storeCategoryData2._Items.Count <= 0)
						{
							continue;
						}
						foreach (ItemData item in storeCategoryData2._Items)
						{
							flag2 = kAStoreMenuItemData._Filter != null && kAStoreMenuItemData._Filter.CanFilter(item) && pChooseMenu.CanAddInMenu(item);
							if (flag2)
							{
								break;
							}
						}
					}
					if (flag2)
					{
						kAStoreMenuItemData._IsEnabled = true;
					}
					else
					{
						KAWidget kAWidget = (KAWidget)kAStoreMenuItemData._UserData;
						if (kAWidget != null)
						{
							kAWidget.SetInteractive(isInteractive: false);
						}
					}
				}
				if (!string.IsNullOrEmpty(mStoreName) && mStoreName == mStoreInfo._Name && !string.IsNullOrEmpty(mCategory) && mCategory == kAStoreMenuItemData._Name && kAStoreMenuItemData._IsEnabled)
				{
					flag = true;
				}
			}
			if (!string.IsNullOrEmpty(_EnterCategoryName))
			{
				GetCategoryData();
			}
			else if (!flag)
			{
				KAStoreMenuItemData nextStoreMenuItemData = GetNextStoreMenuItemData();
				if (nextStoreMenuItemData != null)
				{
					mCategory = nextStoreMenuItemData._Name;
					mFilter = nextStoreMenuItemData._Filter;
					mStoreName = mStoreInfo._Name;
				}
				else
				{
					UtDebug.Log("No Category button exists!!!!!");
				}
			}
			if (KAUIStoreCategory.pInstance != null && pStoreInfo != null)
			{
				KAUIStoreCategory.pInstance._Menu.SetCategories(pStoreInfo._MenuData);
			}
			mChooseMenu.OnMultipleStoreLoaded(inStoreData);
			SetStoreMode(StoreMode.Choose, update: true, mStoreName, mCategory, mFilter);
			mMainMenu.OnStoreSelected(mStoreName);
		}
		if (mIsRestartTutStep)
		{
			mIsRestartTutStep = false;
			FarmStoreTutorial component = InteractiveTutManager._CurrentActiveTutorialObject.GetComponent<FarmStoreTutorial>();
			if (component != null)
			{
				component.HighlightSeed();
			}
		}
		SetInteractive(interactive: true);
		if (mStoreLoading)
		{
			mStoreLoading = false;
			ConnectivityMonitor.RemoveConnectionHandler(OnNetworkConnected);
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	private void GetCategoryData()
	{
		KAStoreInfo kAStoreInfo = mStoreInfo;
		string text = kAStoreInfo._DefaultCatName;
		StoreFilter storeFilter = kAStoreInfo.GetDefaultFilter();
		if (UtPlatform.IsMobile() && string.IsNullOrEmpty(_EnterCategoryName))
		{
			_EnterCategoryName = kAStoreInfo._MobileDefaultCatName;
		}
		if (!string.IsNullOrEmpty(_EnterCategoryName))
		{
			bool flag = false;
			KAStoreMenuItemData[] menuData = kAStoreInfo._MenuData;
			foreach (KAStoreMenuItemData kAStoreMenuItemData in menuData)
			{
				if (!(kAStoreMenuItemData._Name == _EnterCategoryName))
				{
					continue;
				}
				if (kAStoreMenuItemData._IsEnabled)
				{
					text = _EnterCategoryName;
					storeFilter = kAStoreMenuItemData._Filter;
					flag = true;
					break;
				}
				KAStoreMenuItemData nextStoreMenuItemData = GetNextStoreMenuItemData();
				if (nextStoreMenuItemData != null)
				{
					text = nextStoreMenuItemData._Name;
					storeFilter = nextStoreMenuItemData._Filter;
					flag = true;
				}
				break;
			}
			if (!flag)
			{
				Debug.Log("Category [" + _EnterCategoryName + "] specified for entering store not found!");
			}
			_EnterCategoryName = "";
		}
		mCategory = text;
		mFilter = storeFilter;
		mStoreName = kAStoreInfo._Name;
	}

	private bool IsCategoryExists(string inCategoryName)
	{
		KAStoreMenuItemData[] menuData = mStoreInfo._MenuData;
		foreach (KAStoreMenuItemData kAStoreMenuItemData in menuData)
		{
			if (kAStoreMenuItemData._Name == inCategoryName)
			{
				return kAStoreMenuItemData._IsEnabled;
			}
		}
		return false;
	}

	public IAPStoreCategory GetSelectedIAPStoreCategoryType()
	{
		if (!pFilter.HasCategory(1))
		{
			return IAPStoreCategory.MEMBERSHIP;
		}
		return IAPStoreCategory.GEMS;
	}

	private KAStoreMenuItemData GetNextStoreMenuItemData()
	{
		KAStoreMenuItemData kAStoreMenuItemData = Array.Find(mStoreInfo._MenuData, (KAStoreMenuItemData md) => md._Name == mStoreInfo._DefaultCatName);
		if (kAStoreMenuItemData != null && kAStoreMenuItemData._IsEnabled)
		{
			return kAStoreMenuItemData;
		}
		KAStoreMenuItemData[] menuData = mStoreInfo._MenuData;
		foreach (KAStoreMenuItemData kAStoreMenuItemData2 in menuData)
		{
			if (kAStoreMenuItemData2._IsEnabled)
			{
				return kAStoreMenuItemData2;
			}
		}
		if (kAStoreMenuItemData != null)
		{
			kAStoreMenuItemData._IsEnabled = true;
		}
		return kAStoreMenuItemData;
	}

	private string GetStoreName(int inStoreID)
	{
		KAStoreInfo kAStoreInfo = mMainMenu.FindStore(inStoreID);
		if (kAStoreInfo == null)
		{
			return "";
		}
		return kAStoreInfo._Name;
	}

	private void GetStoreAndCategoryName(int inStoreID, int inCategoryID, out string storeName, out string categoryName)
	{
		KAStoreInfo[] storeData = mMainMenu._StoreData;
		foreach (KAStoreInfo kAStoreInfo in storeData)
		{
			if (kAStoreInfo._IDs.All((int storeInfoID) => storeInfoID != inStoreID))
			{
				continue;
			}
			KAStoreMenuItemData[] menuData = kAStoreInfo._MenuData;
			foreach (KAStoreMenuItemData kAStoreMenuItemData in menuData)
			{
				if (!kAStoreMenuItemData._Filter._CategoryIDs.All((int catagoryID) => catagoryID != inCategoryID))
				{
					storeName = kAStoreInfo._Name;
					categoryName = kAStoreMenuItemData._Name;
					return;
				}
			}
		}
		storeName = string.Empty;
		categoryName = string.Empty;
	}

	private StoreData GetStoreData(int inStoreID)
	{
		if (mStoreData == null)
		{
			return null;
		}
		foreach (StoreData mStoreDatum in mStoreData)
		{
			if (mStoreDatum != null && mStoreDatum._ID == inStoreID)
			{
				return mStoreDatum;
			}
		}
		return null;
	}

	public int GetPrice(int itemID, int inStoreID)
	{
		StoreData storeData = GetStoreData(inStoreID);
		if (storeData == null)
		{
			return -1;
		}
		return storeData.FindItem(itemID)?.GetFinalCost() ?? (-1);
	}

	private void ShowChooseUI(bool t)
	{
		if (mInvMenu != null)
		{
			mInvMenu.SetVisibility(t);
		}
		mChooseMenu.SetVisibility(t);
		if (mChooseUI == null)
		{
			return;
		}
		if ((t && !mChooseUI.GetVisibility()) || !t)
		{
			if (t)
			{
				mChooseUI.ResetPreview();
			}
			mChooseUI.SetVisibility(t);
		}
		mChooseUI.enabled = t;
		mChooseUI.SetState(KAUIState.INTERACTIVE);
		mChooseMenu.SetState(KAUIState.INTERACTIVE);
	}

	private void MinimizeMainMenu()
	{
		mMainMenu.SetVisibility(inVisible: false);
	}

	public void SetStoreMode(StoreMode sMode, bool update)
	{
		mStoreMode = sMode;
		if (update)
		{
			UpdateStore(mStoreMode, mStoreName, mCategory, mFilter);
		}
	}

	public void SetStoreMode(StoreMode sMode, bool update, string storeName, string category, StoreFilter filter)
	{
		mStoreMode = sMode;
		if (update)
		{
			UpdateStore(mStoreMode, storeName, category, filter);
		}
	}

	public void UpdateStore(StoreMode sMode, string storeName, string category, int categoryID)
	{
		UpdateStore(sMode, storeName, category, new StoreFilter(categoryID));
	}

	public void UpdateStore(StoreMode sMode, string storeName, string category, StoreFilter filter)
	{
		mCategory = category;
		mFilter = filter;
		mStoreName = storeName;
		KAWidget kAWidget = FindItem("TxtStoreName");
		if (kAWidget != null)
		{
			KAStoreInfo[] storeData = mMainMenu._StoreData;
			foreach (KAStoreInfo kAStoreInfo in storeData)
			{
				if (kAStoreInfo._Name == storeName)
				{
					kAWidget.SetText(kAStoreInfo._DisplayText.GetLocalizedString());
					break;
				}
			}
		}
		if (mCategory != "Membership")
		{
			mBtnUpgradeMember.SetVisibility(IAPManager.IsMembershipUpgradeable());
			mBtnBecomeMember.SetVisibility(!SubscriptionInfo.pIsMember);
			ShowSubscriptionInfoBtn(show: false);
			ShowTermsAndPolicyBtn(show: false);
		}
		else
		{
			mBtnUpgradeMember.SetVisibility(inVisible: false);
			mBtnBecomeMember.SetVisibility(inVisible: false);
		}
		switch (sMode)
		{
		case StoreMode.AD:
			mCategoryUI._IdleMgr.SetIdleVOs(mCategoryUI._BaseIdleVOs);
			mCategoryUI._IdleMgr.StartIdles();
			ShowArrows(t: true);
			mMainMenu.SetVisibility(inVisible: false);
			ShowChooseUI(t: false);
			mChooseUI.SetVisibility(t: true);
			mChooseUI.enabled = true;
			mChooseUI.ShowAD(t: true);
			if (mMainMenu != null)
			{
				KAStoreInfo kAStoreInfo3 = mMainMenu.FindStore(storeName);
				if (kAStoreInfo3 != null)
				{
					mChooseUI.SetStore(storeName, mFilter, kAStoreInfo3.GetCategory(category));
				}
			}
			ShowCash(t: false);
			break;
		case StoreMode.Choose:
			mCategoryUI._IdleMgr.SetIdleVOs(mCategoryUI._BaseIdleVOs);
			mCategoryUI._IdleMgr.StartIdles();
			ShowChooseUI(t: true);
			if (mInvMenu != null)
			{
				mInvMenu.ChangeCategory(mFilter);
				if (_BoughtItemID >= 0)
				{
					int numItems = mInvMenu.GetNumItems();
					for (int j = 0; j < numItems; j++)
					{
						if (((KAStoreInventoryItemData)mInvMenu.FindItemAt(j).GetUserData())._ItemID != _BoughtItemID)
						{
							continue;
						}
						int numItemsPerPage = mInvMenu.GetNumItemsPerPage();
						if (numItems > numItemsPerPage)
						{
							if (j > numItems - numItemsPerPage)
							{
								j = numItems - numItemsPerPage;
							}
							mInvMenu.SetTopItemIdx(j);
						}
						break;
					}
				}
			}
			mChooseMenu._GenderFilter = "U";
			if (AvatarData.pInstance != null)
			{
				if (AvatarData.GetGender() == Gender.Male)
				{
					mChooseMenu._GenderFilter = "M";
				}
				else if (AvatarData.GetGender() == Gender.Female)
				{
					mChooseMenu._GenderFilter = "F";
				}
			}
			mCategoryMenu.OnCategorySelected(mFilter);
			mChooseMenu.ChangeCategory(mFilter);
			mChooseUI.ChangeCategory(mFilter);
			mChooseUI.ShowAD(t: false);
			ShowCash(t: true);
			if (!(mMainMenu == null))
			{
				KAStoreInfo kAStoreInfo2 = mMainMenu.FindStore(storeName);
				if (kAStoreInfo2 != null)
				{
					mChooseUI.SetStore(storeName, mFilter, kAStoreInfo2.GetCategory(category));
				}
			}
			break;
		case StoreMode.Syncing:
			mChooseUI.SetState(KAUIState.DISABLED);
			mChooseMenu.SetState(KAUIState.NOT_INTERACTIVE);
			SetState(KAUIState.NOT_INTERACTIVE);
			mSyncPopup.SetVisibility(inVisible: true);
			break;
		}
	}

	protected override void Update()
	{
		if (mStoreClosing && !_ProcessingClose && !mInitExit && !AvatarData.pIsSaving && AvatarData.pIsReady)
		{
			ExitStore();
		}
		if (mLoadStore && SanctuaryManager.pIsReady && !_CustomizationPending)
		{
			LoadStoresReal();
		}
		if (Input.GetMouseButtonDown(0) && KAUIManager.pInstance.pSelectedWidget == null)
		{
			EnableStoreMenu(inEnable: false);
		}
		base.Update();
	}

	public void ShowBuyPopup(bool t, KAStoreItemData itemData)
	{
		if (t)
		{
			mBuyPopup.SetItemData(itemData, 1);
			mBuyPopup.SetVisibility(t: true);
		}
		else
		{
			mBuyPopup.SetVisibility(t: false);
		}
	}

	public void ShowDialog(int id, string text)
	{
		ShowDialog(StringTable.GetStringData(id, text));
	}

	public void ShowDialog(string text, string closeMessage = "OnClose")
	{
		if (!(mUiGenericDB != null))
		{
			mUiGenericDB = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
			KAUIGenericDB component = mUiGenericDB.GetComponent<KAUIGenericDB>();
			component._MessageObject = base.gameObject;
			component._CloseMessage = closeMessage;
			component.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: true);
			component.SetText(text, interactive: false);
			KAUI.SetExclusive(component);
		}
	}

	public void CheckItemOffersAvailableOnPurchase(int itemID)
	{
		mPromoPackages.Clear();
		if (DailyBonusAndPromo.pInstance == null)
		{
			return;
		}
		List<PromoPackage> list = DailyBonusAndPromo.pInstance.CheckForPromoPackageOffers(PromoPackageTriggerType.StorePurchase, itemID);
		if (list == null || list.Count <= 0)
		{
			return;
		}
		KAUICursorManager.SetDefaultCursor("Loading");
		mItemOffersToLoad = list.Count;
		foreach (PromoPackage item in list)
		{
			item.StartPackage();
			item.LoadPackageContent(ItemsInPackage);
		}
	}

	private bool CheckItemOffersAvailable()
	{
		mPromoPackages.Clear();
		if (DailyBonusAndPromo.pInstance == null)
		{
			return false;
		}
		List<PromoPackage> list = DailyBonusAndPromo.pInstance.CheckForPromoPackageOffers(PromoPackageTriggerType.StoreEnter, null);
		if (list == null || list.Count <= 0)
		{
			return false;
		}
		KAUICursorManager.SetDefaultCursor("Loading");
		mItemOffersToLoad = list.Count;
		foreach (PromoPackage item in list)
		{
			item.StartPackage();
			item.LoadPackageContent(ItemsInPackage);
		}
		return true;
	}

	public void ItemsInPackage(int itemID, bool userHasItem)
	{
		mItemOffersToLoad--;
		if (!userHasItem)
		{
			PromoPackage[] promoPackages = DailyBonusAndPromo.pInstance.PromoPackages;
			foreach (PromoPackage package in promoPackages)
			{
				if (package.ItemID == itemID && mPromoPackages.Find((PromoPackage p) => p.ItemID == package.ItemID) == null)
				{
					mPromoPackages.Add(package);
				}
			}
		}
		if (mItemOffersToLoad <= 0)
		{
			if (mPromoPackages.Count > 0)
			{
				string[] array = GameConfig.GetKeyData("PromoOfferStore").Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnOfferBundleLoaded, typeof(GameObject));
			}
			else
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
			}
		}
	}

	private void OnOfferBundleLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			UnityEngine.Object.Instantiate((GameObject)inObject).GetComponent<UiPromoOfferStore>().PackagesToShow(mPromoPackages);
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	public bool CheckIfHasItem(int itemID)
	{
		if ((ParentData.pIsReady && ParentData.pInstance.HasItem(itemID)) || CommonInventoryData.pInstance.FindItem(itemID) != null)
		{
			return true;
		}
		return ExpansionUnlock.pInstance.IsExpansionCompleted(itemID);
	}

	public void ShowUpsell(KAStoreItemData inStoreItemData)
	{
		PrereqItemUIMapping prereqItemUIMappingData = GetPrereqItemUIMappingData(inStoreItemData.GetPrereqItemIfNotInInventory());
		if (prereqItemUIMappingData != null && !string.IsNullOrEmpty(prereqItemUIMappingData._UpsellPath))
		{
			SetInteractive(interactive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = prereqItemUIMappingData._UpsellPath.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnUpsellScreenLoaded, typeof(GameObject));
		}
	}

	public void ShowUpsellOnStoreStart()
	{
		_ShowUpsellOnStoreStart = false;
		bool flag = false;
		bool flag2 = false;
		UtDebug.Log("CurrentActiveTutorialObject : " + InteractiveTutManager._CurrentActiveTutorialObject == null);
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			UtDebug.Log("CurrentActiveTutorialObject name : " + InteractiveTutManager._CurrentActiveTutorialObject.name);
		}
		if (InteractiveTutManager._CurrentActiveTutorialObject != null && InteractiveTutManager._CurrentActiveTutorialObject.name == "PfFarmStoreTutorial")
		{
			return;
		}
		PrereqItemUIMapping prereqItemUIMapping = null;
		if (!SubscriptionInfo.pIsMember)
		{
			if (_ShowUpsellPerSessionCounter == 0)
			{
				OnUpsellShown();
				KAUICursorManager.SetDefaultCursor("Loading");
				string[] array = _MembershipUpsellPath.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnUpsellScreenLoaded, typeof(GameObject));
				return;
			}
			PrereqItemUIMapping[] array2 = Array.FindAll(_PrereqItemUIMappings, (PrereqItemUIMapping element) => !CheckIfHasItem(element._PrereqItemID));
			if (array2 != null && array2.Length != 0)
			{
				float num = 0f;
				for (int i = 0; i < array2.Length; i++)
				{
					num += (float)array2[i]._Weight;
				}
				float num2 = UnityEngine.Random.value * num;
				num = 0f;
				for (int j = 0; j < array2.Length; j++)
				{
					num += (float)array2[j]._Weight;
					if (num2 <= num)
					{
						prereqItemUIMapping = array2[j];
						flag2 = true;
						break;
					}
				}
			}
		}
		if (!flag2 || UnityEngine.Random.Range(0, 100) <= _ItemUpsellProbability)
		{
			flag = CheckItemOffersAvailable();
			if (flag)
			{
				OnUpsellShown();
			}
		}
		if (!flag && flag2)
		{
			OnUpsellShown();
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array3 = prereqItemUIMapping._UpsellPath.Split('/');
			RsResourceManager.LoadAssetFromBundle(array3[0] + "/" + array3[1], array3[2], OnUpsellScreenLoaded, typeof(GameObject));
		}
	}

	public void OnUpsellShown()
	{
		_ShowUpsellPerSessionCounter++;
		if (_ShowUpsellPerDayCount <= 0)
		{
			return;
		}
		int num = -1;
		DateTime t = ServerTime.pCurrentTime;
		for (int i = 0; i < _ShowUpsellPerDayCount; i++)
		{
			string @string = PlayerPrefs.GetString("UpsellShown" + UserInfo.pInstance.UserID + i, "");
			if (!string.IsNullOrEmpty(@string))
			{
				DateTime dateTime = DateTime.Parse(@string, UtUtilities.GetCultureInfo("en-US"));
				if (DateTime.Compare(dateTime, t) < 0)
				{
					num = i;
					t = dateTime;
				}
				continue;
			}
			num = i;
			break;
		}
		if (num != -1)
		{
			string value = ServerTime.pCurrentTime.ToString(UtUtilities.GetCultureInfo("en-US"));
			PlayerPrefs.SetString("UpsellShown" + UserInfo.pInstance.UserID + num, value);
		}
	}

	public bool CanShowUpsellForTheDay()
	{
		if (_ShowUpsellPerDayCount < 0)
		{
			return true;
		}
		for (int i = 0; i < _ShowUpsellPerDayCount; i++)
		{
			string @string = PlayerPrefs.GetString("UpsellShown" + UserInfo.pInstance.UserID + i, "");
			if (string.IsNullOrEmpty(@string))
			{
				return true;
			}
			DateTime dateTime = DateTime.Parse(@string, UtUtilities.GetCultureInfo("en-US"));
			if ((ServerTime.pCurrentTime - dateTime).Days > 0)
			{
				PlayerPrefs.SetString("UpsellShown" + UserInfo.pInstance.UserID + i, "");
				return true;
			}
		}
		return false;
	}

	private void OnUpsellScreenLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = obj.name.Replace("(Clone)", "");
			UiUpsellDB component = obj.GetComponent<UiUpsellDB>();
			component.SetSource(ItemPurchaseSource.EXPANSION_STORE.ToString());
			component.OnUpsellComplete = UpsellCompleteCallback;
			component.pActiveStore = this;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			SetInteractive(interactive: true);
			break;
		}
	}

	private void UpsellCompleteCallback(bool purchased)
	{
		LoadStores();
		SetInteractive(interactive: true);
		if (InteractiveTutManager._CurrentActiveTutorialObject != null && InteractiveTutManager._CurrentActiveTutorialObject.name == "PfFarmStoreTutorial")
		{
			mIsRestartTutStep = true;
		}
	}

	public void OnClose()
	{
		if (!(mUiGenericDB == null))
		{
			KAUI.RemoveExclusive(mUiGenericDB.GetComponent<KAUIGenericDB>());
			UnityEngine.Object.Destroy(mUiGenericDB);
			mUiGenericDB = null;
		}
	}

	private void CloseStore()
	{
		OnClose();
		SetInteractive(interactive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		mStoreClosing = true;
		_ProcessingClose = false;
		if (mChooseUI != null)
		{
			mChooseUI.transform.parent.gameObject.BroadcastMessage("OnStoreExit", true, SendMessageOptions.DontRequireReceiver);
		}
		CharacterDatabase.pInstance.DeleteInstance();
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (pLocked)
		{
			return;
		}
		EnableStoreMenu(item == mBtnStores && !mMainMenu.GetVisibility());
		if (item == mBtnClose)
		{
			CloseStore();
		}
		else if (item == mBtnFullPreview)
		{
			_StoreFullPreviewUI.ShowFullPreview();
		}
		else if (item == mBtnPreviewBuy)
		{
			mChooseMenu.BuyCurrentItem();
		}
		else if (item == mGemTotal || item.name == "BtnGemsDBPurchase")
		{
			if (mGemsDB.GetVisibility())
			{
				EnableStoreUIs(inEnable: true);
				mGemsDB.SetVisibility(inVisible: false);
			}
			if (_GemsStoreInfo != null && !pCategory.Equals(_GemsStoreInfo._Category))
			{
				LoadStore(_GemsStoreInfo._Store, _GemsStoreInfo._Category);
			}
		}
		else if (item == mCoinsTotalBtn)
		{
			if (_CoinsStoreInfo != null && !pCategory.Equals(_CoinsStoreInfo._Category))
			{
				LoadStore(_CoinsStoreInfo._Store, _CoinsStoreInfo._Category);
			}
		}
		else if (item == mBtnInvite || item.name == "BtnGemsDBEarn")
		{
			if (mGemsDB.GetVisibility())
			{
				SetInteractive(interactive: false);
				mGemsDB.SetVisibility(inVisible: false);
			}
			else
			{
				EnableStoreUIs(inEnable: false);
			}
			UiEarnGems.Show(base.gameObject);
		}
		else if (item == mBtnBecomeMember || item == mBtnUpgradeMember)
		{
			if (_MembershipStoreInfo != null)
			{
				LoadStore(_MembershipStoreInfo._Store, _MembershipStoreInfo._Category);
			}
		}
		else if (item.name == "BtnGemsDBClose")
		{
			EnableStoreUIs(inEnable: true);
			mGemsDB.SetVisibility(inVisible: false);
		}
		else if (item == mSubscriptionInfoBtn)
		{
			UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiSubscriptionInfo"));
		}
		else if (item == mBtnAds && AdManager.pInstance.AdAvailable(_AdEventType, AdType.REWARDED_VIDEO))
		{
			SetInteractive(interactive: false);
			AdManager.DisplayAd(_AdEventType, AdType.REWARDED_VIDEO, base.gameObject);
		}
		else if (item == mTermsBtn)
		{
			OpenURL(_TermsAndConditionsURL);
		}
		else if (item == mPrivacyPolicyBtn)
		{
			OpenURL(_PrivacyPolicyURL);
		}
		if (item == mGemTotal || item == mCoinsTotalBtn || item == mBtnBecomeMember)
		{
			if (pStatCompareMenu != null)
			{
				pStatCompareMenu.RemoveStatPreview();
			}
			if (pDragonStatMenu != null)
			{
				pDragonStatMenu.RemoveStatPreview();
			}
		}
	}

	private void OpenURL(string url)
	{
		if (Application.isEditor || !UtUtilities.IsConnectedToWWW())
		{
			return;
		}
		if (UtPlatform.IsMobile())
		{
			UniWebView uniWebView = base.gameObject.GetComponent<UniWebView>();
			if (uniWebView == null)
			{
				uniWebView = base.gameObject.AddComponent<UniWebView>();
			}
			uniWebView.SetShowToolbar(show: true);
			uniWebView.Frame = new Rect(0f, 0f, Screen.width, Screen.height);
			uniWebView.Load(url);
			uniWebView.Show();
		}
		else
		{
			Application.OpenURL(url);
		}
	}

	public void OnAdWatched()
	{
		AdManager.pInstance.LogAdWatchedEvent(_AdEventType, pChooseUI.pChosenItemData._ItemData.ItemID.ToString());
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		CommonInventoryData.pInstance.AddItem(pChooseUI.pChosenItemData._ItemData.ItemID, updateServer: false);
		CommonInventoryData.pInstance.Save(InventorySaveEventHandler, null);
	}

	public void OnAdFailed()
	{
		SetInteractive(interactive: true);
		UtDebug.LogError("OnAdFailed for event:- " + _AdEventType);
	}

	public void OnAdSkipped()
	{
		SetInteractive(interactive: true);
	}

	public void OnAdClosed()
	{
	}

	public void OnAdFinished(string eventDataRewardString)
	{
	}

	public void OnAdCancelled()
	{
	}

	private void InventorySaveEventHandler(bool success, object inUserData)
	{
		SetInteractive(interactive: true);
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		AdManager.pInstance.SyncAdAvailableCount(_AdEventType, success);
		pChooseMenu.ChangeCategory(mFilter, forceChange: true);
		if (success)
		{
			ShowDialog(_AdRewardSuccessText.GetLocalizedString());
			return;
		}
		ShowDialog(_AdRewardFailedText.GetLocalizedString());
		CommonInventoryData.pInstance.RemoveItem(pChooseUI.pChosenItemData._ItemData.ItemID, updateServer: false);
	}

	public override void OnPressRepeated(KAWidget item, bool inPressed)
	{
		if (item == mBtnPreviewRotateLt)
		{
			mChooseUI.RotatePreview();
		}
		else if (item == mBtnPreviewRotateRt)
		{
			mChooseUI.RotatePreview(rotateLeft: false);
		}
	}

	public override void OnPress(KAWidget item, bool inPressed)
	{
		if (item == mBtnPreviewRotateLt || item == mBtnPreviewRotateRt)
		{
			EnableStoreMenu(inEnable: false);
		}
		base.OnPress(item, inPressed);
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		base.OnDrag(inWidget, inDelta);
		if (!(inWidget != _DragWidget))
		{
			float rotationDelta = (0f - mChooseUI._RotationSpeed) * inDelta.x;
			mChooseUI.RotatePreview(rotationDelta);
			EnableStoreMenu(inEnable: false);
		}
	}

	public override void SelectCategory(KAStoreMenuItemData pd)
	{
		_EnterItemID = 0;
		_BoughtItemID = -1;
		mChooseMenu.ResetMenu();
		EnableStoreMenu(inEnable: false);
		UpdateStore(StoreMode.Choose, mStoreName, pd._Name, pd._Filter);
		mCategoryUI.SetBackGroundTexture(pd._Bkg);
		mChooseUI.SendMessage("OnCategorySelected", pd, SendMessageOptions.DontRequireReceiver);
	}

	private void ShowRegisterPrompt()
	{
	}

	private void OnBecomeMemberYes()
	{
		RegisterMsg registerMsg = MonetizationData.GetRegisterMsg("Store");
		if (registerMsg != null)
		{
			UiLogin._RewardForRegistering = registerMsg.mCredits;
		}
		UiLogin.pShowRegistrationPage = true;
		base.gameObject.SendMessageUpwards("ExitPDA");
	}

	public void OnMoneyUpdated()
	{
		mCoinsTotal.SetText(Money.pGameCurrency.ToString());
		if (mGemsTotalTxt != null)
		{
			mGemsTotalTxt.SetText(Money.pCashCurrency.ToString());
		}
	}

	private void ShowCash(bool t)
	{
		mCoinsTotal.SetVisibility(t);
		if (t)
		{
			if (mStoreMode == StoreMode.CheckOut)
			{
				mCheckoutGrp.SetVisibility(inVisible: true);
				mBuyNow.SetState(KAUIState.INTERACTIVE);
			}
			else
			{
				mCheckoutGrp.SetVisibility(inVisible: false);
			}
		}
		else
		{
			mCheckoutGrp.SetVisibility(t);
			mCoinsTotal.SetVisibility(t);
		}
	}

	public void ShowArrows(bool t)
	{
	}

	public void ShowPreviewRotateButtons(bool visible)
	{
		mBtnPreviewRotateLt.SetVisibility(visible);
		mBtnPreviewRotateRt.SetVisibility(visible);
		if (mRotateIcon != null)
		{
			mRotateIcon.SetVisibility(visible);
		}
	}

	public void ShowPreviewBuyButton(bool visible)
	{
		mBtnPreviewBuy.SetVisibility(visible);
	}

	public void ShowAdsButton(bool visible)
	{
		if (mBtnAds != null && mBtnAds.GetVisibility() != visible && (!visible || AdManager.pInstance.AdSupported(_AdEventType, AdType.REWARDED_VIDEO)))
		{
			mBtnAds.SetVisibility(visible);
		}
	}

	public void SetupFullPreviewInfo(Transform previewTransform, StorePreviewCategory category, bool isZoomed = false)
	{
		_StoreFullPreviewUI.SetFullPreviewInfo(previewTransform, category, isZoomed);
		mBtnFullPreview.SetVisibility(inVisible: true);
	}

	public void ShowFullPreviewButton(bool visible)
	{
		mBtnFullPreview.SetVisibility(visible);
	}

	public void ExitStore()
	{
		if (IAPManager.pInstance != null)
		{
			IAPManager.pInstance.RemoveFromMsglist(base.gameObject);
		}
		mInitExit = true;
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (mChooseUI != null)
		{
			mChooseUI.SetVisibility(t: false);
		}
		Input.ResetInputAxes();
		SnChannel.StopPool("VO_Pool");
		if (pUpdateAvatar)
		{
			UpdateAvatarState();
		}
		mInstance = null;
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "Close_Store");
		}
		if (pIsInItsOwnScene)
		{
			AvAvatar.pStartLocation = (string.IsNullOrEmpty(_StoreExitMarker) ? AvAvatar.pSpawnAtSetPosition : _StoreExitMarker);
			if (_ExitLevelName == GameConfig.GetKeyData("JournalScene") || RsResourceManager.pLastLevel == GameConfig.GetKeyData("JournalScene"))
			{
				UiJournal.EnterSelection = _BackToJournalTab;
			}
			if (!string.IsNullOrEmpty(_ExitLevelName))
			{
				RsResourceManager.LoadLevel(_ExitLevelName);
				_ExitLevelName = "";
			}
			else
			{
				RsResourceManager.LoadLevel(RsResourceManager.pLastLevel);
			}
			return;
		}
		UnityEngine.Object.Destroy(base.transform.root.gameObject);
		if (UtPlatform.IsMobile())
		{
			RsResourceManager.Unload(UtMobileUtilities.IsWideDisplay() ? GameConfig.GetKeyData("StoreAsset") : GameConfig.GetKeyData("StoreAsset4x3"));
			RsResourceManager.UnloadUnusedAssets();
		}
		if (!string.IsNullOrEmpty(_BackToJournalTab))
		{
			JournalLoader.Load(_BackToJournalTab, "", setDefaultMenuItem: true, null, resetLastSceneRef: false);
			return;
		}
		foreach (GameObject exitMessageObject in _ExitMessageObjects)
		{
			if (exitMessageObject != null)
			{
				exitMessageObject.SendMessage("OnStoreClosed", null, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void UpdateAvatarState()
	{
		AvAvatar.SetActive(inActive: true);
		AvAvatar.pState = mLastAvatarState;
	}

	private void OnIAPStoreClosed()
	{
		if (SubscriptionInfo.pIsMember && mCategory != "Membership")
		{
			mBtnUpgradeMember.SetVisibility(IAPManager.IsMembershipUpgradeable());
			mBtnBecomeMember.SetVisibility(inVisible: false);
		}
	}

	public void OnInviteFriendClosed()
	{
		EnableStoreUIs(inEnable: true);
	}

	public void OnEarnGemsClose()
	{
		EnableStoreUIs(inEnable: true);
	}

	public void ShowInsufficientGemsDB()
	{
		EnableStoreUIs(inEnable: false);
		SetInteractive(interactive: true);
		mGemsDB.SetVisibility(inVisible: true);
	}

	public void OnPurchaseSuccessful(string Category)
	{
		if (IAPManager.pIAPStoreData != null && IAPManager.pIAPStoreData.GetIAPCategoryType(Category) == IAPCategoryType.Membership)
		{
			mBtnUpgradeMember.SetVisibility(IAPManager.IsMembershipUpgradeable());
			mBtnBecomeMember.SetVisibility(inVisible: false);
		}
	}

	public void ShowSubscriptionInfoBtn(bool show)
	{
		if (mSubscriptionInfoBtn != null)
		{
			mSubscriptionInfoBtn.SetVisibility(show);
		}
	}

	public void ShowTermsAndPolicyBtn(bool show)
	{
		if (mTermsBtn != null)
		{
			mTermsBtn.SetVisibility(show);
		}
		if (mPrivacyPolicyBtn != null)
		{
			mPrivacyPolicyBtn.SetVisibility(show);
		}
	}
}
