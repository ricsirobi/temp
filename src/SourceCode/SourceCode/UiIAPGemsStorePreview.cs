using UnityEngine;

public class UiIAPGemsStorePreview : KAUI
{
	public GameObject _ChooseMenu;

	private KAWidget mGemsBuyBtn;

	private KAWidget mMembershipBuyBtn;

	private KAWidget mTitleTxt;

	private KAWidget mDescriptionTxt;

	private KAWidget mPreviewTexture;

	private bool mInitialised;

	private IAPItemData mMembershipItemData;

	private void Init()
	{
		mGemsBuyBtn = FindItem("GemsBuyBtn");
		mMembershipBuyBtn = FindItem("MembershipBuyBtn");
		mTitleTxt = FindItem("TitleTxt");
		mDescriptionTxt = FindItem("DescriptionTxt");
		mPreviewTexture = FindItem("GemsPreview");
		mInitialised = true;
	}

	public void Preview(IAPItemWidgetUserData iapItemData)
	{
		if (!mInitialised)
		{
			Init();
		}
		IAPItemData[] dataListInCategory = IAPManager.pIAPStoreData.GetDataListInCategory(2);
		for (int i = 0; i < dataListInCategory.Length; i++)
		{
			if (dataListInCategory[i].AppStoreID == iapItemData._IAPItemData.SubscriptionOffer)
			{
				mMembershipItemData = dataListInCategory[i];
				break;
			}
		}
		if (mMembershipItemData != null)
		{
			mMembershipBuyBtn.GetLabel().text = mMembershipItemData.FormattedPrice;
			mTitleTxt.SetText(iapItemData._IAPItemData.ItemName.GetLocalizedString());
			mDescriptionTxt.SetText(iapItemData._IAPItemData.SubscriptionOfferDescription.GetLocalizedString());
			CoBundleItemData coBundleItemData = new CoBundleItemData(iapItemData._IAPItemData.IconName, null);
			coBundleItemData._Item = mPreviewTexture;
			coBundleItemData.LoadResource();
		}
		if (!GetVisibility())
		{
			SetVisibility(inVisible: true);
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mGemsBuyBtn)
		{
			_ChooseMenu.SendMessage("BuyCurrentItem", SendMessageOptions.DontRequireReceiver);
		}
		else if (item == mMembershipBuyBtn)
		{
			IAPItemWidgetUserData iAPItemWidgetUserData = new IAPItemWidgetUserData(mMembershipItemData.IconName, null, mMembershipItemData.PreviewAsset);
			iAPItemWidgetUserData._AppStoreID = mMembershipItemData.AppStoreID;
			iAPItemWidgetUserData._NoofCoins = mMembershipItemData.NumberOfCoins;
			iAPItemWidgetUserData._IAPItemData = mMembershipItemData;
			iAPItemWidgetUserData.PurchaseStoreType = KAStoreMenuItemData.StoreType.IAPStore;
			item.SetUserData(iAPItemWidgetUserData);
			_ChooseMenu.SendMessage("BuyIAPOfferItem", item, SendMessageOptions.DontRequireReceiver);
		}
	}
}
