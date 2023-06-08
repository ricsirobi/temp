using System;
using System.Collections.Generic;
using UnityEngine;

public class KAUIIAPStore : KAUI
{
	[NonSerialized]
	public IAPStoreCategory _IAPStoreCategory;

	[NonSerialized]
	public GameObject _MessageObject;

	public LocaleString _GemsAndMembershipStoreText;

	public LocaleString _CoinsStoreText;

	public int _CoinStoreID = 102;

	public string _CoinsPreviewSize = "256,256";

	public string _TermsAndConditionsURL = "http://www.schoolofdragons.com/help/siteterms";

	public string _PrivacyPolicyURL = "http://www.schoolofdragons.com/help/privacypolicy";

	public KAStoreMenuItemData[] _MenuData;

	public KAUIMenu _CategoryMenu;

	public SceneIAPCategoryMap[] _ScenesToShowCustomIAP;

	public LocaleString _ItemExpiredText = new LocaleString("This item is currently not available.");

	private KAUIIAPStoreChooseMenu mIAPChooseMenu;

	private KAWidget mBuyBtn;

	private KAWidget mCloseBtn;

	private KAWidget mPreview;

	private KAWidget mPreviewFront;

	private KAWidget mPreviewBack;

	private KAWidget mBtnPreview;

	private KAWidget mBtnBecomeMember;

	private KAWidget mBtnUpgradeMember;

	private bool mHasToEnableInput;

	private KAWidget mSubscriptionInfoBtn;

	private KAWidget mTermsBtn;

	private KAWidget mPrivacyPolicyBtn;

	private KAWidget mTxtStoreName;

	private bool mIsPreviewFaceFront = true;

	private List<ItemData> mCoinsData;

	protected override void Start()
	{
		base.Start();
		if (IAPManager.pInstance != null)
		{
			IAPManager.pInstance.AddToMsglist(base.gameObject);
		}
		KAUI.SetExclusive(this);
		mIAPChooseMenu = base.transform.parent.GetComponentInChildren<KAUIIAPStoreChooseMenu>();
		mBuyBtn = FindItem("BtnPreviewBuy");
		mCloseBtn = FindItem("BtnClose");
		mPreview = FindItem("Ui2DPreview");
		mPreviewFront = mPreview.FindChildItem("AniIconFront");
		mPreviewBack = mPreview.FindChildItem("AniInfoBehind");
		mBtnPreview = FindItem("PreviewInfoBtn");
		mBtnBecomeMember = FindItem("BtnBecomeMember");
		mBtnUpgradeMember = FindItem("BtnUpgradeMember");
		mTxtStoreName = FindItem("TxtStoreName");
		mSubscriptionInfoBtn = FindItem("BtnSubscriptionInfo");
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
		if (_CategoryMenu != null)
		{
			KAStoreMenuItemData[] menuData2 = _MenuData;
			foreach (KAStoreMenuItemData menuData in menuData2)
			{
				int num = Array.FindIndex(_ScenesToShowCustomIAP, (SceneIAPCategoryMap X) => string.Compare(RsResourceManager.pCurrentLevel, X._Scene, StringComparison.OrdinalIgnoreCase) == 0);
				if (num == -1 || Array.Exists(_ScenesToShowCustomIAP[num]._CategoryIDsToShow, (int X) => menuData._Filter.HasCategory(X)))
				{
					KAWidget kAWidget = _CategoryMenu.AddWidget(menuData._Name);
					menuData._UserData = kAWidget;
					kAWidget.SetText(menuData._DisplayText.GetLocalizedString());
				}
			}
		}
		mIAPChooseMenu.LoadIAPItems((int)_IAPStoreCategory);
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (UiJoystick.pInstance != null && UiJoystick.pInstance.IsActive())
		{
			mHasToEnableInput = true;
			AvAvatar.EnableAllInputs(inActive: false);
		}
		if (_IAPStoreCategory == IAPStoreCategory.GEMS)
		{
			mBtnUpgradeMember.SetVisibility(IAPManager.IsMembershipUpgradeable());
			mBtnBecomeMember.SetVisibility(!SubscriptionInfo.pIsMember);
		}
		else
		{
			mBtnUpgradeMember.SetVisibility(inVisible: false);
			mBtnBecomeMember.SetVisibility(inVisible: false);
		}
		if (_IAPStoreCategory == IAPStoreCategory.COINS)
		{
			ItemStoreDataLoader.Load(_CoinStoreID, OnStoreLoaded);
		}
		if (!(mTxtStoreName == null))
		{
			if (_IAPStoreCategory == IAPStoreCategory.GEMS || _IAPStoreCategory == IAPStoreCategory.MEMBERSHIP)
			{
				mTxtStoreName.SetText(_GemsAndMembershipStoreText.GetLocalizedString());
			}
			else
			{
				mTxtStoreName.SetText(_CoinsStoreText.GetLocalizedString());
			}
		}
	}

	private void OnStoreLoaded(StoreData storeData)
	{
		if (storeData != null)
		{
			mCoinsData = new List<ItemData>();
			ItemData[] items = storeData._Items;
			foreach (ItemData itemData in items)
			{
				if (itemData.HasCategory(425))
				{
					mCoinsData.Add(itemData);
				}
			}
		}
		mIAPChooseMenu.LoadIAPItems((int)_IAPStoreCategory);
	}

	public List<ItemData> GetCoinsData()
	{
		return mCoinsData;
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mBuyBtn)
		{
			mIAPChooseMenu.BuyCurrentItem();
		}
		else if (item == mBtnBecomeMember || item == mBtnUpgradeMember)
		{
			item.SetVisibility(inVisible: false);
			_IAPStoreCategory = IAPStoreCategory.MEMBERSHIP;
			mIAPChooseMenu.LoadIAPItems((int)_IAPStoreCategory);
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
		else if (item == mCloseBtn)
		{
			Exit();
		}
		else if (item == mSubscriptionInfoBtn)
		{
			UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiSubscriptionInfo"));
		}
		else if (item == mTermsBtn)
		{
			OpenURL(_TermsAndConditionsURL);
		}
		else if (item == mPrivacyPolicyBtn)
		{
			OpenURL(_PrivacyPolicyURL);
		}
		else if (item == mBtnPreview)
		{
			if ((bool)GameConfig.pInstance)
			{
				string[] array = ((UtPlatform.IsiOS() || (UtPlatform.IsAndroid() && !UtPlatform.IsAmazon())) ? GameConfig.GetKeyData("UIMembershipBenefitsNoTrial") : GameConfig.GetKeyData("UIMembershipBenefits"))?.Split('/') ?? Array.Empty<string>();
				if (array.Length >= 3)
				{
					SetStoreStateActive(active: false);
					KAUICursorManager.SetDefaultCursor("Loading");
					RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnUIMembershipBenefitsLoaded, typeof(GameObject));
				}
			}
		}
		else
		{
			if (_MenuData == null || _MenuData.Length == 0 || !(mIAPChooseMenu != null))
			{
				return;
			}
			KAStoreMenuItemData[] menuData = _MenuData;
			foreach (KAStoreMenuItemData kAStoreMenuItemData in menuData)
			{
				if (kAStoreMenuItemData._UserData != null && (KAWidget)kAStoreMenuItemData._UserData == item)
				{
					_IAPStoreCategory = (IAPStoreCategory)kAStoreMenuItemData._Filter._CategoryIDs[0];
					if (_IAPStoreCategory == IAPStoreCategory.COINS)
					{
						ItemStoreDataLoader.Load(_CoinStoreID, OnStoreLoaded);
					}
					else
					{
						mIAPChooseMenu.LoadIAPItems((int)_IAPStoreCategory);
					}
					break;
				}
			}
		}
	}

	private void OnUIMembershipBenefitsLoaded(string inUrl, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserdata)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			UnityEngine.Object.Instantiate((GameObject)inObject);
			SetStoreStateActive(active: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		case RsResourceLoadEvent.ERROR:
			SetStoreStateActive(active: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	public void SetStoreStateActive(bool active)
	{
		SetState((!active) ? KAUIState.NOT_INTERACTIVE : KAUIState.INTERACTIVE);
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

	public void OnSelectStoreItem(KAWidget item)
	{
		if (item == null)
		{
			return;
		}
		if (item.GetUserData().GetType() == typeof(KAStoreItemData))
		{
			KAStoreItemData kAStoreItemData = (KAStoreItemData)item.GetUserData();
			StorePreview.ScalePreview(mPreviewFront, _CoinsPreviewSize);
			mPreviewFront.SetVisibility(inVisible: false);
			mPreviewFront.SetTextureFromBundle(kAStoreItemData._ItemData.IconName);
			ShowItemPreview(show: true);
			ShowBuyButton(show: true);
			mIAPChooseMenu._IAPGemsStorePreview.SetVisibility(inVisible: false);
			return;
		}
		IAPItemWidgetUserData iAPItemWidgetUserData = (IAPItemWidgetUserData)item.GetUserData();
		if (!string.IsNullOrEmpty(iAPItemWidgetUserData._IAPItemData.SubscriptionOffer) && (!SubscriptionInfo.pIsMember || SubscriptionInfo.pIsTrialMember))
		{
			ShowItemPreview(show: false);
			ShowBuyButton(show: false);
			mIAPChooseMenu._IAPGemsStorePreview.Preview(iAPItemWidgetUserData);
			return;
		}
		int num = 0;
		if (mPreviewFront != null && iAPItemWidgetUserData._ItemTextureData._Texture != null)
		{
			mPreviewFront.ResetWidget();
			mPreviewBack.SetVisibility(inVisible: false);
			mIAPChooseMenu._IAPGemsStorePreview.SetVisibility(inVisible: false);
			if (iAPItemWidgetUserData._PreviewAssetsData != null && iAPItemWidgetUserData._PreviewAssetsData.Count > num && iAPItemWidgetUserData._PreviewAssetsData[num] != null && !string.IsNullOrEmpty(iAPItemWidgetUserData._PreviewAssetsData[num].Name))
			{
				string[] array = iAPItemWidgetUserData._PreviewAssetsData[num].Name.Split('/');
				if (array != null && array.Length > 2)
				{
					StorePreview.ScalePreview(mPreviewFront, iAPItemWidgetUserData._PreviewAssetsData[num].Size);
					mPreviewFront.SetVisibility(inVisible: false);
					mPreviewFront.SetTextureFromBundle(array[0] + "/" + array[1], array[2]);
				}
			}
			else
			{
				CoBundleItemData coBundleItemData = new CoBundleItemData(iAPItemWidgetUserData._IAPItemData.IconName, null);
				coBundleItemData._Item = mPreviewFront;
				coBundleItemData.LoadResource();
			}
			ShowItemPreview(show: true);
		}
		if (!mPreviewBack)
		{
			return;
		}
		num++;
		if (iAPItemWidgetUserData._PreviewAssetsData != null && iAPItemWidgetUserData._PreviewAssetsData.Count > num && !string.IsNullOrEmpty(iAPItemWidgetUserData._PreviewAssetsData[num].Name))
		{
			mIsPreviewFaceFront = true;
			string[] array2 = iAPItemWidgetUserData._PreviewAssetsData[num].Name.Split('/');
			if (array2 != null && array2.Length > 2)
			{
				StorePreview.ScalePreview(mPreviewBack, iAPItemWidgetUserData._PreviewAssetsData[num].Size);
				mPreviewBack.SetTextureFromBundle(array2[0] + "/" + array2[1], array2[2]);
			}
		}
		if (_IAPStoreCategory == IAPStoreCategory.MEMBERSHIP)
		{
			ShowPreviewInfoBtn(show: true);
		}
		ShowBuyButton(show: true);
	}

	public void ShowItemPreview(bool show)
	{
		if (mPreviewFront != null)
		{
			mPreviewFront.SetVisibility(show);
		}
	}

	public void ShowBuyButton(bool show)
	{
		mBuyBtn.SetVisibility(show);
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

	private void ShowPreviewInfoBtn(bool show)
	{
		if (mBtnPreview != null)
		{
			mBtnPreview.SetVisibility(show);
			mBtnPreview.GetComponent<BoxCollider>().enabled = show;
		}
	}

	public void Exit()
	{
		if (mHasToEnableInput)
		{
			mHasToEnableInput = false;
			AvAvatar.EnableAllInputs(inActive: true);
		}
		KAUI.RemoveExclusive(this);
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnIAPStoreClosed", SendMessageOptions.DontRequireReceiver);
		}
		if (IAPManager.pInstance != null)
		{
			IAPManager.pInstance.RemoveFromMsglist(base.gameObject);
		}
		UnityEngine.Object.Destroy(base.transform.root.gameObject);
	}

	public void OnPurchaseSuccessful(string Category)
	{
		if (IAPManager.pIAPStoreData != null && IAPManager.pIAPStoreData.GetIAPCategoryType(Category) == IAPCategoryType.Membership)
		{
			mBtnUpgradeMember.SetVisibility(IAPManager.IsMembershipUpgradeable());
			mBtnBecomeMember.SetVisibility(inVisible: false);
		}
	}
}
