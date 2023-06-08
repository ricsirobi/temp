using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public class PromoPackage
{
	[XmlElement(ElementName = "TriggerData")]
	public PromoPackageTrigger[] TriggerData;

	[XmlElement(ElementName = "StartDate")]
	public DateTime StartDate;

	[XmlElement(ElementName = "EndDate")]
	public DateTime EndDate;

	[XmlElement(ElementName = "Priority")]
	public int Priority;

	[XmlElement(ElementName = "ActionType")]
	public PromoActionType? ActionType;

	[XmlElement(ElementName = "ImageRes")]
	public string ImageRes;

	[XmlElement(ElementName = "IconRes")]
	public string IconRes;

	[XmlElement(ElementName = "ItemID")]
	public int ItemID;

	[XmlElement(ElementName = "StoreID")]
	public string StoreID;

	[XmlElement(ElementName = "CategoryID")]
	public string CategoryID;

	[XmlElement(ElementName = "Scene")]
	public string Scene;

	[XmlElement(ElementName = "Duration")]
	public int? Duration;

	[XmlElement(ElementName = "HideInfo")]
	public bool HideInfo;

	[XmlElement(ElementName = "FullView")]
	public bool FullView;

	[XmlElement(ElementName = "Title")]
	public LocaleString Title;

	[XmlElement(ElementName = "Description")]
	public LocaleString Description;

	[XmlElement(ElementName = "Gender", IsNullable = true)]
	public Gender? GenderType;

	[XmlElement(ElementName = "DaysOlder", IsNullable = true)]
	public int? DaysOlder;

	[XmlElement(ElementName = "Member", IsNullable = true)]
	public MembershipType? MemberType;

	[XmlIgnore]
	public string _GenderFilter = "U";

	[XmlIgnore]
	private PackageCallback mCallback;

	[XmlIgnore]
	private RsResourceLoadEvent mLoadEvent;

	[XmlIgnore]
	private bool isInvalid;

	public bool pOfferShowcased { get; set; }

	public void LoadPackageContent(PackageCallback inCallback, bool forceLoad = false)
	{
		mCallback = (PackageCallback)Delegate.Combine(mCallback, inCallback);
		if (forceLoad)
		{
			mLoadEvent = RsResourceLoadEvent.NONE;
		}
		switch (mLoadEvent)
		{
		case RsResourceLoadEvent.NONE:
			mLoadEvent = RsResourceLoadEvent.PROGRESS;
			ItemData.Load(ItemID, ItemReady, null);
			break;
		case RsResourceLoadEvent.COMPLETE:
			if (mCallback != null)
			{
				mCallback(ItemID, isInvalid);
				mCallback = null;
			}
			break;
		}
	}

	private void ItemReady(int itemId, ItemData itemData, object userData)
	{
		if (itemData != null)
		{
			if (itemData.IsBundleItem())
			{
				itemData.LoadBundledItems(ItemDataList);
				return;
			}
			mLoadEvent = RsResourceLoadEvent.COMPLETE;
			isInvalid = !ValidateItemData(itemData);
			mCallback(itemId, isInvalid);
			mCallback = null;
		}
		else
		{
			UtDebug.LogError("@@@@ Item data does not exist");
			mLoadEvent = RsResourceLoadEvent.NONE;
			mCallback = null;
		}
	}

	private void ItemDataList(List<ItemData> itemData, int inItemID)
	{
		if (itemData != null)
		{
			mLoadEvent = RsResourceLoadEvent.COMPLETE;
		}
		else
		{
			mLoadEvent = RsResourceLoadEvent.NONE;
		}
		if (mCallback != null)
		{
			mCallback(inItemID, isValidPackage: false);
			mCallback = null;
		}
		else
		{
			UtDebug.LogError("@@@@ Callback is null ");
		}
	}

	private bool ValidateItemData(ItemData item)
	{
		if (item.IsOutdated())
		{
			return false;
		}
		UserItemData userItemData = CommonInventoryData.pInstance.FindItem(item.ItemID);
		if (userItemData != null && userItemData.Quantity >= item.InventoryMax && item.InventoryMax > 0)
		{
			return false;
		}
		return true;
	}

	public string GetOfferStartTime()
	{
		string result = null;
		if (ProductData.pPairData.FindByKey("PromoPackageOffer" + ItemID) != null)
		{
			result = ProductData.pPairData.GetValue("PromoPackageOffer" + ItemID);
		}
		return result;
	}

	public void UserPurchasedOffer()
	{
		if (ProductData.pPairData.FindByKey("PromoPackageOffer" + ItemID) != null)
		{
			ProductData.pPairData.SetValueAndSave("PromoPackageOffer" + ItemID, "PURCHASED");
		}
	}

	public bool IsNewPackage()
	{
		if (GetOfferStartTime() == null)
		{
			return true;
		}
		return false;
	}

	public void StartPackage()
	{
		if (GetOfferStartTime() == null)
		{
			ProductData.pPairData.SetValue("PromoPackageOffer" + ItemID, ServerTime.pCurrentTime.ToString());
		}
	}
}
