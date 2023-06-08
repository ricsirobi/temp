using System.Collections.Generic;
using UnityEngine;

public class UiPromoOfferStore : UiPromoOffer
{
	protected override void ShowCurrentOffer()
	{
		if (mPackagesToShow.Count > 0)
		{
			mRegularPriceCoins.SetText("");
			mRegularPriceGems.SetText("");
			_OfferMenu.ClearItems();
			ItemData.Load(mPackagesToShow[mPackageIndex].ItemID, PromoItemReady, null);
		}
	}

	protected override void PromoItemReady(int itemId, ItemData itemData, object userData)
	{
		PromoPackage promoPackage = mPackagesToShow[mPackageIndex];
		mCurrentPackageItemData = itemData;
		CoBundleItemData coBundleItemData = new CoBundleItemData(mCurrentPackageItemData.IconName, null);
		mPackageBanner.SetUserData(coBundleItemData);
		coBundleItemData.LoadResource();
		SetPackageInfo();
		if (promoPackage.Description != null)
		{
			string localizedString = promoPackage.Description.GetLocalizedString();
			if (!string.IsNullOrEmpty(localizedString))
			{
				mTxtPackageDes.SetText(localizedString);
			}
		}
		KAWidget kAWidget = FindItem("TxtTitle");
		if (kAWidget != null)
		{
			string text = mCurrentPackageItemData.ItemName;
			if (promoPackage.Title != null)
			{
				string localizedString2 = promoPackage.Title.GetLocalizedString();
				if (!string.IsNullOrEmpty(localizedString2))
				{
					text = localizedString2;
				}
			}
			kAWidget.SetText(text);
		}
		ShowTimeLeft();
	}

	public override void PackagesToShow(List<PromoPackage> packages)
	{
		KAUI.SetExclusive(this);
		mPackagesToShow = packages;
		Init();
		ShowNextOffer(0);
		mPromoOfferStoreName = ItemPurchaseSource.PROMO_OFFER_STORE.ToString();
	}

	protected override void Exit()
	{
		KAUI.RemoveExclusive(this);
		Object.Destroy(base.gameObject);
	}
}
