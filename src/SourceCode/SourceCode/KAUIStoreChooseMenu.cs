using System;
using System.Collections.Generic;
using UnityEngine;

public class KAUIStoreChooseMenu : KAUIStoreInvMenuBase
{
	[Serializable]
	public class ItemStatusIcons
	{
		public string _ID;

		public Texture _IconTexture;
	}

	[Serializable]
	public class PriorityHeaderInfo
	{
		public string _Key;

		public string _SpriteName;

		public LocaleString _HeaderText;
	}

	public class ItemDataStoreIDPair
	{
		public ItemData _ItemData;

		public int _StoreID;

		public ItemDataStoreIDPair(ItemData data, int storeID)
		{
			_ItemData = data;
			_StoreID = storeID;
		}
	}

	[Serializable]
	public class ItemDataStoreFilterInfo
	{
		public StoreFilter.SortType _Filter;

		[NonSerialized]
		public List<ItemDataStoreIDPair> _ItemDataStoreIDPair = new List<ItemDataStoreIDPair>();
	}

	public class LimitedStateItemData
	{
		public KAWidget mIconInfo;

		public KAStoreItemData mData;

		public DateTime mNextUpdateTime;

		public LimitedStateItemData(KAStoreItemData inData)
		{
			mData = inData;
		}

		public void Initialize()
		{
			if (mData != null && !(mData.GetItem() == null))
			{
				mNextUpdateTime = ServerTime.pCurrentTime;
				mIconInfo = mData.GetItem().GetRootItem().FindChildItem("AniIconInfo");
			}
		}

		public void UpdateNextUpdateTime()
		{
			if (mData == null || mData._ItemData == null)
			{
				mNextUpdateTime = ServerTime.pCurrentTime;
				return;
			}
			ItemData.ItemSale currentSale = mData._ItemData.GetCurrentSale();
			if (currentSale != null)
			{
				mNextUpdateTime = FindNextUpdateTime(currentSale.mEndDate);
				return;
			}
			ItemAvailability availability = mData._ItemData.GetAvailability();
			if (availability != null && availability.EndDate.HasValue)
			{
				mNextUpdateTime = FindNextUpdateTime(availability.EndDate.Value);
			}
			else
			{
				mNextUpdateTime = ServerTime.pCurrentTime;
			}
		}

		private DateTime FindNextUpdateTime(DateTime inTime)
		{
			TimeSpan timeSpan = inTime.Subtract(ServerTime.pCurrentTime);
			double num = 0.0;
			num = ((timeSpan.TotalHours > 24.0) ? (timeSpan.TotalSeconds - 86400.0) : ((!(timeSpan.TotalHours >= 1.0)) ? ((timeSpan.TotalMinutes - Math.Floor(timeSpan.TotalMinutes)) * 60.0) : ((timeSpan.TotalHours - Math.Floor(timeSpan.TotalHours)) * 60.0 * 60.0)));
			return ServerTime.pCurrentTime.AddSeconds(Math.Max(0.0, num));
		}
	}

	public static KAUIStoreChooseMenu pInstance;

	public bool _SinglePageStore;

	public ItemStatusIcons[] _StatusIcons;

	public PriorityHeaderInfo[] _PriorityHeaderInfo;

	public List<ItemDataStoreFilterInfo> _StoreFilterOrder;

	public Color _StatPositiveColor = new Color(0.1764706f, 0.5294118f, 0.1764706f, 1f);

	public Color _StatNegativeColor = Color.red;

	public LocaleString _StoreUnavailableText = new LocaleString("Store Unavailable at this time. Please try again later.");

	public LocaleString _ItemsUnavailableText = new LocaleString("No Items currently available.");

	public Texture _DefaultItemTexture;

	[NonSerialized]
	public KAUIStore _UIStore;

	public StoreItemProperty _Description;

	public string _GenderFilter = "U";

	public Color _OfferPriceColor = Color.green;

	public Texture _IconTextureCoin;

	public Texture _IconTextureGems;

	public LocaleString _LevelRequiredText = new LocaleString("Level {0}\nRequired");

	public LocaleString _UDTPointsRequiredText = new LocaleString("{0}\nPoints Required");

	public KAUIIAPStoreSyncPopUp _IAPStoreSyncPopUp;

	public UiIAPGemsStorePreview _IAPGemsStorePreview;

	public string _ItemColorWidget = "AniIconFront";

	private Dictionary<string, Texture> mStatusIcons;

	private IAPItemWidgetUserData mCurrentIAPItemData;

	private KAStoreItemData mLastPurchasedItem;

	private List<LimitedStateItemData> mLimitedItems;

	private CommonInventoryResponseItem[] mLastReceivedItems;

	private KAScrollBar mScrollBar;

	private KAScrollButton mPageRightArrow;

	private KAScrollButton mPageLeftArrow;

	private bool mCanPageLeft;

	private bool mCanPageRight;

	private Dictionary<ItemData, int> mCachedItemData = new Dictionary<ItemData, int>();

	private int mCurrentItemIndex;

	private int mPage = 1;

	private Color mItemDefaultColor = Color.white;

	private bool mReInitInventory;

	private bool mCheckOffersPending;

	private Texture GetStatusTexture(string id)
	{
		if (mStatusIcons == null)
		{
			mStatusIcons = new Dictionary<string, Texture>();
			for (int i = 0; i < _StatusIcons.Length; i++)
			{
				mStatusIcons.Add(_StatusIcons[i]._ID, _StatusIcons[i]._IconTexture);
			}
		}
		if (!mStatusIcons.ContainsKey(id))
		{
			return null;
		}
		return mStatusIcons[id];
	}

	protected override void Start()
	{
		base.Start();
		_UIStore = base.transform.root.GetComponentInChildren<KAUIStore>();
		pInstance = this;
		if (IAPManager.pInstance != null)
		{
			IAPManager.pInstance.AddToMsglist(base.gameObject);
		}
		if (_SinglePageStore)
		{
			if (mVerticalScrollbar != null)
			{
				SetupSinglePageScrollBar(mVerticalScrollbar);
			}
			if (mHorizontalScrollbar != null)
			{
				SetupSinglePageScrollBar(mHorizontalScrollbar);
			}
			TouchManager.OnFlickEvent = (OnFlick)Delegate.Combine(TouchManager.OnFlickEvent, new OnFlick(OnFlick));
		}
		mItemDefaultColor = UiItemRarityColorSet.GetItemBackgroundColor(_Template, _ItemColorWidget);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (IAPManager.pInstance != null)
		{
			IAPManager.pInstance.RemoveFromMsglist(base.gameObject);
		}
		if (_SinglePageStore)
		{
			TouchManager.OnFlickEvent = (OnFlick)Delegate.Remove(TouchManager.OnFlickEvent, new OnFlick(OnFlick));
		}
	}

	public void InitStoreFilterList()
	{
		ClearStoreFilterList();
		if (_StoreFilterOrder.Count == 0 || _StoreFilterOrder.Find((ItemDataStoreFilterInfo x) => x._Filter == StoreFilter.SortType.None) == null)
		{
			_StoreFilterOrder.Add(new ItemDataStoreFilterInfo());
		}
	}

	public void ClearStoreFilterList()
	{
		if (_StoreFilterOrder.Count == 0)
		{
			return;
		}
		foreach (ItemDataStoreFilterInfo item in _StoreFilterOrder)
		{
			item._ItemDataStoreIDPair.Clear();
		}
	}

	public void AddInStoreFilterList(ItemData itemData, int storeID)
	{
		foreach (ItemDataStoreFilterInfo item in _StoreFilterOrder)
		{
			switch (item._Filter)
			{
			case StoreFilter.SortType.Limited:
			{
				ItemAvailability availability = itemData.GetAvailability();
				if (availability != null && availability.EndDate.HasValue && item._ItemDataStoreIDPair.Find((ItemDataStoreIDPair i) => i._ItemData.ItemID == itemData.ItemID) == null)
				{
					item._ItemDataStoreIDPair.Add(new ItemDataStoreIDPair(itemData, storeID));
					return;
				}
				break;
			}
			case StoreFilter.SortType.Sale:
				if (itemData.IsOnSale() && item._ItemDataStoreIDPair.Find((ItemDataStoreIDPair i) => i._ItemData.ItemID == itemData.ItemID) == null)
				{
					item._ItemDataStoreIDPair.Add(new ItemDataStoreIDPair(itemData, storeID));
					return;
				}
				break;
			case StoreFilter.SortType.New:
				if (itemData.IsNew && item._ItemDataStoreIDPair.Find((ItemDataStoreIDPair i) => i._ItemData.ItemID == itemData.ItemID) == null)
				{
					item._ItemDataStoreIDPair.Add(new ItemDataStoreIDPair(itemData, storeID));
					return;
				}
				break;
			case StoreFilter.SortType.Popular:
				if (itemData.pIsPopular && item._ItemDataStoreIDPair.Find((ItemDataStoreIDPair i) => i._ItemData.ItemID == itemData.ItemID) == null)
				{
					item._ItemDataStoreIDPair.Add(new ItemDataStoreIDPair(itemData, storeID));
					return;
				}
				break;
			default:
				if (item._ItemDataStoreIDPair.Find((ItemDataStoreIDPair i) => i._ItemData.ItemID == itemData.ItemID) == null)
				{
					item._ItemDataStoreIDPair.Add(new ItemDataStoreIDPair(itemData, storeID));
					return;
				}
				break;
			}
		}
	}

	public void AddStoreMenuItem(string resName, string rvo, ItemData itemData, int inStoreID, KAWidget item)
	{
		KAStoreItemData kAStoreItemData = new KAStoreItemData(resName, rvo, itemData, inStoreID, _WHSize);
		item.SetUserData(kAStoreItemData);
		kAStoreItemData._DefaultTexture = _DefaultItemTexture;
		kAStoreItemData.LoadResource();
	}

	public bool CanAddInMenu(ItemData item)
	{
		if (item == null || !ValidateItemData(item, isBundleItem: false))
		{
			return false;
		}
		if (item.pBundledItems == null || item.pBundledItems.Count <= 0)
		{
			return true;
		}
		foreach (ItemData pBundledItem in item.pBundledItems)
		{
			if (!ValidateItemData(pBundledItem, isBundleItem: true))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanAddBundleInMenu(ItemData item)
	{
		bool result = false;
		foreach (ItemData pBundledItem in item.pBundledItems)
		{
			if (pBundledItem.InventoryMax == -1)
			{
				result = true;
				break;
			}
			if (pBundledItem.InventoryMax < 0)
			{
				continue;
			}
			if (item.HasAttribute("Parent"))
			{
				UserItemData userItemData = ParentData.pInstance.pInventory.pData.FindItem(item.ItemID);
				if (userItemData != null && userItemData.Quantity < item.InventoryMax)
				{
					result = true;
					break;
				}
				continue;
			}
			UserItemData userItemData2 = ParentData.pInstance.pInventory.pData.FindItem(item.ItemID);
			UserItemData userItemData3 = CommonInventoryData.pInstance.FindItem(item.ItemID);
			int num = userItemData2?.Quantity ?? 0;
			int num2 = userItemData3?.Quantity ?? 0;
			if (num + num2 < item.InventoryMax)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private bool ValidateItemData(ItemData item, bool isBundleItem)
	{
		if (item.IsOutdated())
		{
			return false;
		}
		if (item.HasCategory(514) && ExpansionUnlock.pInstance.IsUnlocked(item.ItemID))
		{
			return false;
		}
		if (item.InventoryMax > 0 && !isBundleItem)
		{
			if (item.HasAttribute("Parent"))
			{
				UserItemData userItemData = ParentData.pInstance.pInventory.pData.FindItem(item.ItemID);
				if (userItemData != null && userItemData.Quantity >= item.InventoryMax)
				{
					return false;
				}
			}
			else
			{
				UserItemData userItemData2 = ParentData.pInstance.pInventory.pData.FindItem(item.ItemID);
				UserItemData userItemData3 = CommonInventoryData.pInstance.FindItem(item.ItemID);
				int num = userItemData2?.Quantity ?? 0;
				int num2 = userItemData3?.Quantity ?? 0;
				if (num + num2 >= item.InventoryMax)
				{
					return false;
				}
			}
		}
		string attribute = item.GetAttribute("Gender", "U");
		if (attribute != _GenderFilter && attribute != "U" && _GenderFilter != "U")
		{
			return false;
		}
		if (item.Relationship == null)
		{
			return true;
		}
		ItemDataRelationship[] relationship = item.Relationship;
		foreach (ItemDataRelationship itemDataRelationship in relationship)
		{
			string type = itemDataRelationship.Type;
			if (!(type == "Override"))
			{
				if (type == "Prereq" && CommonInventoryData.pInstance.FindItem(itemDataRelationship.ItemId) == null && ParentData.pIsReady && !ParentData.pInstance.HasItem(itemDataRelationship.ItemId) && _UIStore.GetPrereqItemUIMappingData(itemDataRelationship.ItemId) == null)
				{
					return false;
				}
				continue;
			}
			ItemData item2 = ItemStoreDataLoader.GetItem(itemDataRelationship.ItemId);
			if (item2 != null && ValidateItemData(item2, item2.IsBundleItem()))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsPetMeterType(string inKey)
	{
		string[] names = Enum.GetNames(typeof(SanctuaryPetMeterType));
		for (int i = 0; i < names.Length; i++)
		{
			if (names[i].ToLower() == inKey.ToLower())
			{
				return true;
			}
		}
		return false;
	}

	private void SetupStatsIcons(KAWidget inTemplateItem, ItemData inItemData)
	{
		if (inTemplateItem == null || inItemData == null)
		{
			return;
		}
		string text = "Stats";
		string text2 = "Ico";
		string text3 = "Txt";
		Dictionary<string, float> dictionary = new Dictionary<string, float>();
		if (inItemData.Attribute == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		KAWidget kAWidget = null;
		ItemAttribute[] attribute = inItemData.Attribute;
		foreach (ItemAttribute itemAttribute in attribute)
		{
			if (!(GetStatusTexture(text2 + itemAttribute.Key) == null))
			{
				num2++;
				KAWidget kAWidget2 = inTemplateItem.FindChildItem(text + num2.ToString("D2"));
				if (!(kAWidget2 != null))
				{
					break;
				}
				float result = 0f;
				float.TryParse(itemAttribute.Value, out result);
				if (result != 0f)
				{
					kAWidget = kAWidget2;
					dictionary.Add(itemAttribute.Key, result);
				}
			}
		}
		if (kAWidget == null)
		{
			return;
		}
		kAWidget.SetVisibility(inVisible: true);
		foreach (KeyValuePair<string, float> item in dictionary)
		{
			num++;
			KAWidget kAWidget3 = kAWidget.FindChildItem("IconStats" + num.ToString("D2"));
			if (!(kAWidget3 == null))
			{
				KAWidget kAWidget4 = kAWidget.FindChildItem(text3 + text + num.ToString("D2"));
				float num3 = ((Mathf.Abs(item.Value) < 1f) ? (item.Value * 100f) : item.Value);
				string text4 = ((Mathf.Abs(item.Value) < 1f) ? $"{num3}%" : $"{num3}");
				UILabel label;
				if (kAWidget4 != null)
				{
					kAWidget4.SetText(text4);
					label = kAWidget4.GetLabel();
				}
				else
				{
					kAWidget3.SetText(text4);
					label = kAWidget3.GetLabel();
				}
				if (label != null)
				{
					label.color = ((item.Value < 0f) ? _StatNegativeColor : _StatPositiveColor);
				}
				Texture statusTexture = GetStatusTexture(text2 + item.Key);
				if (statusTexture != null)
				{
					kAWidget3.SetTexture(statusTexture);
				}
				kAWidget3.SetVisibility(inVisible: true);
			}
		}
	}

	public void SetIcons(KAWidget templateItem, ItemData itemData)
	{
		KAWidget kAWidget = templateItem.FindChildItem("BattleReadyIcon");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(itemData.IsStatAvailable());
			if (itemData.pBundledItems != null && !kAWidget.GetVisibility())
			{
				foreach (ItemData pBundledItem in itemData.pBundledItems)
				{
					if (pBundledItem.IsStatAvailable())
					{
						kAWidget.SetVisibility(pBundledItem.IsStatAvailable());
						break;
					}
				}
			}
		}
		kAWidget = templateItem.FindChildItem("FlightReadyIcon");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(itemData.HasAttribute("FlightSuit"));
		}
	}

	public void ResetMenu()
	{
		mPage = (mCurrentPage = 1);
		mCurrentItemIndex = 0;
	}

	public void OnMultipleStoreLoaded(List<StoreData> sdList)
	{
		mCachedItemData.Clear();
		List<int> list = new List<int>();
		foreach (StoreData sd in sdList)
		{
			if (sd != null && !list.Contains(sd._ID))
			{
				list.Add(sd._ID);
			}
		}
		if (!UtUtilities.IsSameList(list.ToArray(), _UIStore.pStoreIDs))
		{
			return;
		}
		InitStoreFilterList();
		ClearItems();
		mLimitedItems?.Clear();
		foreach (StoreData sd2 in sdList)
		{
			StoreCategoryData storeCategoryData = sd2.FindCategoryData(mFilter);
			if (storeCategoryData == null || storeCategoryData._Items == null)
			{
				continue;
			}
			foreach (ItemData item in storeCategoryData._Items)
			{
				if (mFilter != null && mFilter.CanFilter(item) && CanAddInMenu(item))
				{
					AddInStoreFilterList(item, sd2._ID);
				}
			}
		}
		if (mFilter != null)
		{
			ItemDataStoreFilterInfo itemDataStoreFilterInfo = null;
			if (_StoreFilterOrder.Count > 0)
			{
				itemDataStoreFilterInfo = _StoreFilterOrder.Find((ItemDataStoreFilterInfo x) => x._Filter == mFilter._SortType);
			}
			if (itemDataStoreFilterInfo == null)
			{
				itemDataStoreFilterInfo = _StoreFilterOrder.Find((ItemDataStoreFilterInfo x) => x._Filter == StoreFilter.SortType.None);
			}
			List<ItemDataStoreIDPair> itemDataStoreIDPair = itemDataStoreFilterInfo._ItemDataStoreIDPair;
			if (itemDataStoreFilterInfo != null && itemDataStoreIDPair.Count > 0)
			{
				switch (mFilter._SortType)
				{
				case StoreFilter.SortType.Popular:
					itemDataStoreIDPair.Sort((ItemDataStoreIDPair a, ItemDataStoreIDPair b) => b._ItemData.PopularRank - a._ItemData.PopularRank);
					break;
				case StoreFilter.SortType.Sale:
					itemDataStoreIDPair.Sort((ItemDataStoreIDPair a, ItemDataStoreIDPair b) => b._ItemData.SaleFactor - a._ItemData.SaleFactor);
					break;
				}
			}
		}
		int num = 0;
		int pMaxItemslimit = _UIStore.pCategoryMenu.pMaxItemslimit;
		foreach (ItemDataStoreFilterInfo item2 in _StoreFilterOrder)
		{
			foreach (ItemDataStoreIDPair item3 in item2._ItemDataStoreIDPair)
			{
				if (num < pMaxItemslimit || pMaxItemslimit == -1)
				{
					if (KAUIStore._EnterItemID == item3._ItemData.ItemID)
					{
						mCurrentItemIndex = GetNumItemsPerPage() * (num / GetNumItemsPerPage());
						mCurrentPage = mCurrentItemIndex / GetNumItemsPerPage() + 1;
						mPage = mCurrentPage;
					}
					if (_SinglePageStore)
					{
						mCachedItemData.Add(item3._ItemData, item3._StoreID);
					}
					else
					{
						AddToStoreMenu(item3._ItemData, item3._StoreID);
					}
					num++;
					continue;
				}
				break;
			}
		}
		if (_SinglePageStore)
		{
			if (mCachedItemData.Count > 0)
			{
				int num2 = Mathf.CeilToInt((float)mCachedItemData.Count / (float)GetNumItemsPerPage());
				if (mPage > num2)
				{
					mCurrentItemIndex -= (mPage - num2) * GetNumItemsPerPage();
					mPage = (mCurrentPage = num2);
				}
			}
			ResetMenu();
			UpdateSinglePageStore();
		}
		int numItemsPerPage = GetNumItemsPerPage();
		if (GetNumItems() <= numItemsPerPage)
		{
			mCurrentTopIdx = 0;
		}
		else if (mCurrentTopIdx >= GetNumItems())
		{
			mCurrentTopIdx -= GetNumItemsPerPage();
		}
		SetTopItemIdx(mCurrentTopIdx);
		GoToPage(GetCurrentPage(), instant: true);
		ClearStoreFilterList();
	}

	public override void LoadItem(KAWidget inWidget)
	{
		base.LoadItem(inWidget);
		if (!(inWidget == null))
		{
			KAWidget kAWidget = inWidget.FindChildItem("Icon");
			if (!(kAWidget == null))
			{
				((KAStoreItemData)kAWidget.GetUserData())?.LoadResource();
			}
		}
	}

	private void LoadIAPStore(int inCategoryType)
	{
		if (IAPManager.pIAPStoreData == null || _Template == null)
		{
			return;
		}
		ClearItems();
		if (inCategoryType == 2 && SubscriptionInfo.pIsMember && !IAPManager.IsMembershipUpgradeable())
		{
			if (!_SinglePageStore)
			{
				return;
			}
			UpdateSinglePageStore();
			if (SubscriptionInfo.GetBillFrequency() < 12)
			{
				KAWidget kAWidget = _ParentUi.FindItem("TxtNotAvailable", recursive: false);
				if (kAWidget != null)
				{
					kAWidget.SetText(IAPManager.GetMembershipUpgradeText());
				}
			}
			return;
		}
		IAPItemData[] dataListInCategory = IAPManager.pIAPStoreData.GetDataListInCategory(inCategoryType);
		if (dataListInCategory != null && dataListInCategory.Length != 0)
		{
			IAPItemData[] array = dataListInCategory;
			foreach (IAPItemData iAPItemData in array)
			{
				if (iAPItemData == null || !GameUtilities.CheckItemValidity(iAPItemData.PurchaseDateRange, iAPItemData.Event))
				{
					continue;
				}
				if (iAPItemData.PurchaseType == 1)
				{
					if (!SubscriptionInfo.pIsMember || inCategoryType != 2)
					{
						AddToStoreMenu(ItemStoreDataLoader.GetItem(iAPItemData.ItemID), iAPItemData.ISMStoreID);
					}
				}
				else if (iAPItemData.ItemAvailable && (inCategoryType != 2 || !IAPManager.IsMembershipRecurring() || iAPItemData.BillFrequency > SubscriptionInfo.GetBillFrequency()))
				{
					KAWidget kAWidget2 = AddWidget(_Template.name);
					kAWidget2.SetVisibility(inVisible: true);
					kAWidget2.name = iAPItemData.ItemName.GetLocalizedString();
					KAWidget kAWidget3 = kAWidget2.FindChildItem("AniCreditsInfo");
					if (kAWidget3 != null)
					{
						kAWidget3.SetText(iAPItemData.ItemName.GetLocalizedString());
					}
					KAWidget kAWidget4 = kAWidget3.FindChildItem("AniCreditCard");
					if (kAWidget4 != null)
					{
						kAWidget4.SetVisibility(inVisible: false);
					}
					KAWidget kAWidget5 = kAWidget3.FindChildItem("TxtCurrency");
					if (kAWidget5 != null)
					{
						kAWidget5.SetText(iAPItemData.FormattedPrice);
					}
					KAWidget kAWidget6 = kAWidget2.FindChildItem("AniInfoBehind");
					if (kAWidget6 != null)
					{
						kAWidget6.SetText(iAPItemData.Description.GetLocalizedString());
					}
					KAWidget kAWidget7 = kAWidget2.FindChildItem("BattleReadyIcon");
					if (kAWidget7 != null)
					{
						kAWidget7.SetVisibility(inVisible: false);
					}
					KAWidget kAWidget8 = kAWidget2.FindChildItem("FlightReadyIcon");
					if (kAWidget8 != null)
					{
						kAWidget8.SetVisibility(inVisible: false);
					}
					KAWidget kAWidget9 = kAWidget2.FindChildItem(_ItemColorWidget);
					if (kAWidget9 != null)
					{
						UiItemRarityColorSet.SetItemBackgroundColor(mItemDefaultColor, kAWidget9);
					}
					IAPItemWidgetUserData iAPItemWidgetUserData = new IAPItemWidgetUserData(iAPItemData.IconName, null, iAPItemData.PreviewAsset);
					iAPItemWidgetUserData._AppStoreID = iAPItemData.AppStoreID;
					if (inCategoryType == 1 || (inCategoryType == 2 && !SubscriptionInfo.pIsMember))
					{
						iAPItemWidgetUserData._NoofCoins = iAPItemData.NumberOfCoins;
					}
					else
					{
						iAPItemWidgetUserData._NoofCoins = 0;
					}
					iAPItemWidgetUserData._IAPItemData = iAPItemData;
					KAWidget kAWidget10 = kAWidget2.FindChildItem("Icon");
					if (kAWidget10 != null)
					{
						kAWidget10.SetUserData(iAPItemWidgetUserData);
					}
					KAWidget kAWidget11 = kAWidget2.FindChildItem("AniIconLock");
					if (kAWidget11 != null)
					{
						kAWidget11.SetVisibility(inVisible: false);
					}
					UpdateWidgetState(null, kAWidget2);
					iAPItemWidgetUserData.LoadResource();
				}
			}
			if (mCurrentGrid != null)
			{
				mCurrentGrid.repositionNow = true;
			}
			int numItemsPerPage = GetNumItemsPerPage();
			if (GetNumItems() <= numItemsPerPage)
			{
				mCurrentTopIdx = 0;
			}
			else if (mCurrentTopIdx >= GetNumItems())
			{
				mCurrentTopIdx -= GetNumItemsPerPage();
			}
			SetTopItemIdx(mCurrentTopIdx);
		}
		if (_SinglePageStore)
		{
			UpdateSinglePageStore();
		}
		if (UtPlatform.IsiOS() && GetNumItems() > 0)
		{
			_UIStore.ShowSubscriptionInfoBtn(inCategoryType == 2);
			_UIStore.ShowTermsAndPolicyBtn(inCategoryType == 2);
		}
	}

	public override void ChangeCategory(StoreFilter inFilter, bool forceChange)
	{
		if (mFilter != null)
		{
			if (!mFilter.IsSame(inFilter))
			{
				mCurrentTopIdx = 0;
			}
			if (!forceChange && mFilter.IsSame(inFilter))
			{
				return;
			}
		}
		mFilter = inFilter;
		Input.ResetInputAxes();
		ClearItems();
		if (_UIStore.pCategoryMenu.pType == KAStoreMenuItemData.StoreType.IAPStore)
		{
			if (mFilter._CategoryIDs != null && mFilter._CategoryIDs.Length != 0)
			{
				LoadIAPStore(mFilter._CategoryIDs[0]);
			}
		}
		else
		{
			_UIStore.LoadStores();
		}
	}

	protected override void Update()
	{
		base.Update();
		HandleLimitedStateItems();
		if (mReInitInventory && CommonInventoryData.pIsReady)
		{
			mReInitInventory = false;
			KAUICursorManager.SetDefaultCursor("Arrow");
			ShowSyncSuccessPopUp();
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		_ParentUi.SetVisibility(inVisible);
	}

	private void ShowInfo(bool inShowInfo, KAWidget inItem)
	{
		if (!(inItem == null))
		{
			Vector3 localScale = inItem.transform.localScale;
			TweenScale.Begin(inItem.gameObject, 0.5f, new Vector3(0f, localScale.y, localScale.z));
			UITweener component = inItem.gameObject.GetComponent<UITweener>();
			component.eventReceiver = inItem.gameObject;
			component.callWhenFinished = "RotatedToFrontHalf";
			component.callWhenFinished = (inShowInfo ? "RotatedToBackHalf" : "RotatedToFrontHalf");
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (KAUIStore.pLocked)
		{
			return;
		}
		_UIStore.EnableStoreMenu(inEnable: false);
		if (item == null)
		{
			return;
		}
		if (item.name == "BtnInfo" || item.name == "AniInfoBehind")
		{
			ShowInfo(item.name != "AniInfoBehind", item.GetRootItem());
		}
		if (item.name != "AniInfoBehind" && item.name != "AniIconFront" && item.name != "BtnInfo")
		{
			return;
		}
		KAWidget kAWidget = item.GetRootItem().FindChildItem("AniIconImage").FindChildItemAt(0);
		if (kAWidget == null)
		{
			return;
		}
		if (_UIStore.pChooseUI.pChosenItemData != null)
		{
			StoreWidgetUserData storeWidgetUserData = (StoreWidgetUserData)kAWidget.GetUserData();
			if (storeWidgetUserData != null && storeWidgetUserData.PurchaseStoreType == KAStoreMenuItemData.StoreType.GameStore && storeWidgetUserData is KAStoreItemData kAStoreItemData && kAStoreItemData._ItemData.ItemID == _UIStore.pChooseUI.pChosenItemData._ItemData.ItemID)
			{
				return;
			}
		}
		_UIStore.ProcessChooseSelection(kAWidget);
		if (item.name != "BtnInfo" && item.name != "AniInfoBehind")
		{
			LogStoreItemViewedEvent(kAWidget);
		}
		if (_UIStore.pCategoryMenu.pType == KAStoreMenuItemData.StoreType.GameStore)
		{
			KAStoreItemData kAStoreItemData2 = (KAStoreItemData)kAWidget.GetUserData();
			if (kAStoreItemData2?._ItemData != null && !(InteractiveTutManager._CurrentActiveTutorialObject == null) && kAWidget.GetParentItem() != null)
			{
				InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "Select_" + kAStoreItemData2._ItemData.ItemID);
			}
		}
	}

	protected override void ButtonClicked(KAScrollBar scrollBar, KAScrollButton scrollButton, bool isRepeated)
	{
		_UIStore.EnableStoreMenu(inEnable: false);
		if (_SinglePageStore && !isRepeated)
		{
			UpdateSinglePageStoreIndex(scrollButton);
		}
		else
		{
			base.ButtonClicked(scrollBar, scrollButton, isRepeated);
		}
	}

	public override void ClearItems()
	{
		if (mItemInfo.Count > 0)
		{
			foreach (KAWidget item in mItemInfo)
			{
				item.ResetWidget();
				item.FindChildItem("Icon").SetTexture(null);
			}
		}
		base.ClearItems();
	}

	private void SetupSinglePageScrollBar(KAScrollBar scrollBar)
	{
		mPageRightArrow = scrollBar._DownArrow;
		mPageLeftArrow = scrollBar._UpArrow;
		mPageRightArrow.SetVisibility(inVisible: true);
		mPageLeftArrow.SetVisibility(inVisible: true);
		scrollBar.SetVisibility(inVisible: true);
		scrollBar._UpArrow = null;
		scrollBar._DownArrow = null;
		mScrollBar = scrollBar;
		scrollBar = null;
	}

	private void UpdateSinglePageStoreIndex(KAScrollButton button)
	{
		if (button != null)
		{
			if (button == mPageRightArrow)
			{
				mCurrentItemIndex += GetNumItemsPerPage();
				mPage++;
			}
			else if (button == mPageLeftArrow)
			{
				mCurrentItemIndex = Mathf.Max(0, mCurrentItemIndex - GetNumItemsPerPage());
				mPage--;
			}
		}
		UpdateSinglePageStore();
	}

	private void UpdateSinglePageStore()
	{
		if (mHorizontalScrollbar != null || mVerticalScrollbar != null)
		{
			mHorizontalScrollbar = null;
			mVerticalScrollbar = null;
		}
		int num = GetNumItems();
		int numItemsPerPage = GetNumItemsPerPage();
		if (_UIStore.pCategoryMenu.pType == KAStoreMenuItemData.StoreType.GameStore)
		{
			ClearItems();
			List<ItemData> list = new List<ItemData>(mCachedItemData.Keys);
			num = mCachedItemData.Count;
			for (int i = 0; i < numItemsPerPage && mCurrentItemIndex + i < num; i++)
			{
				AddToStoreMenu(list[mCurrentItemIndex + i], mCachedItemData[list[mCurrentItemIndex + i]]);
			}
		}
		KAWidget kAWidget = _ParentUi.FindItem("TxtNotAvailable", recursive: false);
		KAWidget kAWidget2 = _ParentUi.FindItem("TxtPageCount");
		if (num == 0)
		{
			mPageRightArrow.SetVisibility(inVisible: false);
			mPageLeftArrow.SetVisibility(inVisible: false);
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: true);
				kAWidget.SetText(_ItemsUnavailableText.GetLocalizedString());
			}
			if (kAWidget2 != null)
			{
				kAWidget2.SetText(string.Empty);
			}
		}
		else
		{
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			mPageRightArrow.SetVisibility(inVisible: true);
			mPageLeftArrow.SetVisibility(inVisible: true);
			mCanPageRight = ((mCurrentItemIndex + numItemsPerPage < num) ? true : false);
			mCanPageLeft = ((mCurrentItemIndex >= numItemsPerPage) ? true : false);
			KAUIState state = ((!mCanPageRight) ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
			mPageRightArrow.SetState(state);
			state = ((!mCanPageLeft) ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
			mPageLeftArrow.SetState(state);
			if (kAWidget2 != null)
			{
				if (GetItemCount() == 0)
				{
					kAWidget2.SetText(string.Empty);
				}
				else
				{
					int b = (num + numItemsPerPage - 1) / numItemsPerPage;
					kAWidget2.SetText(mPage + "/" + Mathf.Max(1, b));
				}
			}
		}
		if (!mScrollBar.GetVisibility())
		{
			mScrollBar.SetVisibility(inVisible: true);
		}
	}

	private void OnFlick(int inFingerID, Vector2 inVecPosition, Vector2 inDeltaPos, int inDirection, int inMagnitude)
	{
		Vector3 point = new Vector3(inVecPosition.x - (float)Screen.width / 2f, (float)Screen.height / 2f - inVecPosition.y, base.pDragPanel.bounds.max.z);
		if (!base.pDragPanel.bounds.Contains(point) || !_SinglePageStore || !IsActive() || KAUI._GlobalExclusiveUI != null)
		{
			return;
		}
		switch ((FlickDir)inDirection)
		{
		case FlickDir.NW:
		case FlickDir.W:
		case FlickDir.SW:
			if (mCanPageRight)
			{
				UpdateSinglePageStoreIndex(mPageRightArrow);
			}
			break;
		case FlickDir.SE:
		case FlickDir.E:
		case FlickDir.NE:
			if (mCanPageLeft)
			{
				UpdateSinglePageStoreIndex(mPageLeftArrow);
			}
			break;
		}
	}

	private void LogStoreItemViewedEvent(KAWidget iconItem)
	{
		string itemName = null;
		switch (_UIStore.pCategoryMenu.pType)
		{
		case KAStoreMenuItemData.StoreType.GameStore:
		{
			KAStoreItemData kAStoreItemData2 = (KAStoreItemData)iconItem.GetUserData();
			if (kAStoreItemData2 != null && kAStoreItemData2._ItemData != null)
			{
				itemName = kAStoreItemData2._ItemData.ItemName;
			}
			break;
		}
		case KAStoreMenuItemData.StoreType.IAPStore:
			if (((StoreWidgetUserData)iconItem.GetUserData()).PurchaseStoreType == KAStoreMenuItemData.StoreType.GameStore)
			{
				KAStoreItemData kAStoreItemData = (KAStoreItemData)iconItem.GetUserData();
				if (kAStoreItemData?._ItemData != null)
				{
					itemName = kAStoreItemData._ItemData.ItemName;
				}
			}
			else
			{
				IAPItemWidgetUserData iAPItemWidgetUserData = (IAPItemWidgetUserData)iconItem.GetUserData();
				if (iAPItemWidgetUserData != null)
				{
					itemName = iAPItemWidgetUserData._IAPItemData.AppStoreID;
				}
			}
			break;
		}
		AnalyticStoreItemViewedEvent._ItemName = itemName;
		AnalyticStoreItemViewedEvent._CurrentLocation = RsResourceManager.pLastLevel;
		AnalyticStoreItemViewedEvent._IsMember = SubscriptionInfo.pIsMember.ToString();
		AnalyticStoreItemViewedEvent._IsMultiPlayerModeEnabled = MainStreetMMOClient.pIsMMOEnabled.ToString();
		AnalyticStoreItemViewedEvent._ItemPage = (_SinglePageStore ? mPage.ToString() : mCurrentPage.ToString());
		AnalyticStoreItemViewedEvent.LogEvent();
	}

	public override void GoToPage(int inPageNumber, bool instant)
	{
		if (_SinglePageStore)
		{
			return;
		}
		base.GoToPage(inPageNumber, instant);
		if (_ParentUi == null)
		{
			return;
		}
		KAWidget kAWidget = _ParentUi.FindItem("TxtPageCount");
		if (!(kAWidget == null))
		{
			if (GetItemCount() == 0)
			{
				kAWidget.SetText(string.Empty);
				return;
			}
			int a = Mathf.Max(1, mCurrentPage);
			kAWidget.SetText(a + "/" + Mathf.Max(a, mPageCount));
		}
	}

	public void BuyCurrentItem()
	{
		if (_UIStore.pChooseUI.pChosenItemData == null && _UIStore.pChooseUI.pChosenIAPItemData == null)
		{
			UtDebug.Log("Chosen item cannot be null here.");
			return;
		}
		if (_UIStore.pCategoryMenu.pType == KAStoreMenuItemData.StoreType.IAPStore && _UIStore.pChooseUI.pChosenIAPItemData != null)
		{
			if (GameUtilities.CheckItemValidity(_UIStore.pChooseUI.pChosenIAPItemData._IAPItemData.PurchaseDateRange, _UIStore.pChooseUI.pChosenIAPItemData._IAPItemData.Event))
			{
				mCurrentIAPItemData = _UIStore.pChooseUI.pChosenIAPItemData;
				ShowSyncPopUp();
				IAPManager.pInstance.PurchaseProduct(mCurrentIAPItemData._IAPItemData, _UIStore.GetSelectedIAPStoreCategoryType(), _UIStore.gameObject);
			}
			else
			{
				_UIStore.ShowDialog(_UIStore._ItemExpiredText.GetLocalizedString());
			}
			return;
		}
		mLastPurchasedItem = _UIStore.pChooseUI.pChosenItemData;
		KAStoreInfo pStoreInfo = _UIStore.pStoreInfo;
		if (pStoreInfo == null)
		{
			UtDebug.LogError("Current store data cannot be null!");
			return;
		}
		KAStoreItemData pChosenItemData = _UIStore.pChooseUI.pChosenItemData;
		if (pChosenItemData?._ItemData == null)
		{
			return;
		}
		if (pChosenItemData.IsLocked())
		{
			_UIStore.ShowDialog(_UIStore._NonMemberText._ID, _UIStore._NonMemberText._Text);
			return;
		}
		if (!SubscriptionInfo.pIsMember && pChosenItemData.GetPrereqItemIfNotInInventory() != -1)
		{
			_UIStore.ShowUpsell(pChosenItemData);
			return;
		}
		if (!_UIStore.pCategoryMenu.pDisableRankCheck && pChosenItemData.IsRankLocked(out var rid, pStoreInfo._RankTypeID))
		{
			string localizedString = pStoreInfo._RankLockedText.GetLocalizedString();
			if (pChosenItemData._ItemData.Points.HasValue && pChosenItemData._ItemData.Points.Value > 0)
			{
				int pointType = ((pChosenItemData._ItemData.RewardTypeID > 0) ? pChosenItemData._ItemData.RewardTypeID : pStoreInfo._RankTypeID);
				localizedString = _UIStore.pMainMenu.GetTextFromPointType(pointType);
				localizedString = localizedString.Replace("{{Points}}", rid.ToString());
				localizedString = localizedString.Replace("{{Item}}", pChosenItemData._ItemData.ItemName);
			}
			else
			{
				localizedString = localizedString.Replace("{{RANK}}", rid.ToString());
			}
			_UIStore.ShowDialog(localizedString);
			return;
		}
		_UIStore.SetState(KAUIState.NOT_INTERACTIVE);
		_UIStore.pMainMenu.SetState(KAUIState.NOT_INTERACTIVE);
		_UIStore.pCategoryMenu.SetState(KAUIState.NOT_INTERACTIVE);
		SetState(KAUIState.NOT_INTERACTIVE);
		if (pChosenItemData._ItemData.pBundledItems != null && pChosenItemData._ItemData.pBundledItems.Count > 0)
		{
			UiBundleConfirmation.Init(OnBundleConfirmed, pChosenItemData._ItemData);
			return;
		}
		KAUIStore.pInstance.ShowBuyPopup(t: true, pChosenItemData);
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "Buy_" + pChosenItemData._ItemData.ItemID);
		}
	}

	private void OnBundleConfirmed(UiBundleConfirmation.Status status)
	{
		switch (status)
		{
		case UiBundleConfirmation.Status.Accepted:
		case UiBundleConfirmation.Status.Confirmed:
			KAUIStore.pInstance.ShowBuyPopup(t: true, _UIStore.pChooseUI.pChosenItemData);
			break;
		case UiBundleConfirmation.Status.Closed:
			_UIStore.SetState(KAUIState.INTERACTIVE);
			_UIStore.pMainMenu.SetState(KAUIState.INTERACTIVE);
			_UIStore.pCategoryMenu.SetState(KAUIState.INTERACTIVE);
			_IAPGemsStorePreview.SetState(KAUIState.INTERACTIVE);
			SetState(KAUIState.INTERACTIVE);
			break;
		}
	}

	public void BuyIAPOfferItem(KAWidget currentItem)
	{
		if (currentItem == null)
		{
			UtDebug.Log("iap offer item cannot be null");
			return;
		}
		mCurrentIAPItemData = (IAPItemWidgetUserData)currentItem.GetUserData();
		if (_UIStore.pCategoryMenu.pType == KAStoreMenuItemData.StoreType.IAPStore && mCurrentIAPItemData.PurchaseStoreType == KAStoreMenuItemData.StoreType.IAPStore)
		{
			ShowSyncPopUp();
			IAPManager.pInstance.PurchaseProduct(mCurrentIAPItemData._IAPItemData, _UIStore.GetSelectedIAPStoreCategoryType(), _UIStore.gameObject);
		}
	}

	public void OnSyncUIClosed(bool purchaseSuccess, CommonInventoryResponseItem[] receivedItems)
	{
		mLastReceivedItems = receivedItems;
		if (purchaseSuccess && mLastPurchasedItem._ItemData.IsPrizeItem())
		{
			OnMysteryBoxPurchased(receivedItems);
			return;
		}
		ChangeCategory(_UIStore.pFilter, forceChange: true);
		if (purchaseSuccess && mLastPurchasedItem != null)
		{
			if (mLastPurchasedItem._ItemData.HasAttribute("TrialMembershipRedemptionValue") || mLastPurchasedItem._ItemData.HasCategory(454))
			{
				mCheckOffersPending = true;
				CheckTickets();
			}
			else
			{
				CheckOffers(mLastPurchasedItem._ItemData);
			}
		}
	}

	private void CheckTickets()
	{
		if (!(UserNotifyDragonTicket.pInstance == null))
		{
			KAUIStore.EnableStoreUIs(inEnable: false);
			UserNotifyDragonTicket.pInstance.CheckTickets(null, OnRewardShown);
		}
	}

	private void OnRewardShown(bool success)
	{
		KAUIStore.EnableStoreUIs(inEnable: true);
		if (mCheckOffersPending && mLastPurchasedItem != null)
		{
			CheckOffers(mLastPurchasedItem._ItemData);
		}
		mCheckOffersPending = false;
	}

	private void CheckOffers(ItemData itemData)
	{
		_UIStore.CheckItemOffersAvailableOnPurchase(itemData.ItemID);
	}

	public void OnMysteryBoxPurchased(CommonInventoryResponseItem[] finalPrizes)
	{
		KAUIStore.EnableStoreUIs(inEnable: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = _UIStore._MysteryBoxBundlePath.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], MysteryBoxUiHandler, typeof(GameObject));
	}

	private void MysteryBoxUiHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiMysteryBox";
			UiMysteryChestClaim component = obj.GetComponent<UiMysteryChestClaim>();
			if ((bool)component)
			{
				component.pSelectedUserItemData = CommonInventoryData.pInstance.FindItem(mLastPurchasedItem._ItemData.ItemID);
				component.pMsgObject = base.gameObject;
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			KAUIStore.EnableStoreUIs(inEnable: true);
			break;
		}
	}

	public void OnMysteryBoxClosed(bool closeAll)
	{
		KAUIStore.EnableStoreUIs(inEnable: true);
		ChangeCategory(_UIStore.pFilter, forceChange: true);
	}

	private void SyncPopupClosed()
	{
		_UIStore.SetState(KAUIState.INTERACTIVE);
		_UIStore.pMainMenu.SetState(KAUIState.INTERACTIVE);
		_UIStore.pCategoryMenu.SetState(KAUIState.INTERACTIVE);
		_IAPGemsStorePreview.SetState(KAUIState.INTERACTIVE);
		SetState(KAUIState.INTERACTIVE);
		CheckTickets();
	}

	private void ShowSyncPopUp()
	{
		_UIStore.SetState(KAUIState.NOT_INTERACTIVE);
		_UIStore.pMainMenu.SetState(KAUIState.NOT_INTERACTIVE);
		_UIStore.pCategoryMenu.SetState(KAUIState.NOT_INTERACTIVE);
		_IAPGemsStorePreview.SetState(KAUIState.NOT_INTERACTIVE);
		SetState(KAUIState.NOT_INTERACTIVE);
		_IAPStoreSyncPopUp._MessageObject = base.gameObject;
		_IAPStoreSyncPopUp.SetVisibility(t: true);
		_IAPStoreSyncPopUp.ShowPopup(IAPPurchaseStatus.INITIATED);
	}

	private void OnSyncFailed()
	{
		OnSyncFailed(null);
	}

	private void OnSyncFailed(ReceiptRedemptionResult result)
	{
		_IAPStoreSyncPopUp.ShowPopup(IAPPurchaseStatus.SYNC_FAILED);
	}

	private void OnSyncSuccess()
	{
		if (UserInfo.pInstance != null && mCurrentIAPItemData != null)
		{
			Money.AddToCashCurrency(mCurrentIAPItemData._NoofCoins);
			if (IAPManager.pIAPStoreData != null && (IAPManager.pIAPStoreData.GetIAPCategoryType(mCurrentIAPItemData._AppStoreID) == IAPCategoryType.Item || IAPManager.pIAPStoreData.GetIAPCategoryType(mCurrentIAPItemData._AppStoreID) == IAPCategoryType.Membership))
			{
				mReInitInventory = true;
				KAUICursorManager.SetDefaultCursor("Loading");
				CommonInventoryData.ReInit();
			}
			mCurrentIAPItemData = null;
		}
		if (!mReInitInventory)
		{
			ShowSyncSuccessPopUp();
		}
	}

	private void ShowSyncSuccessPopUp()
	{
		_IAPStoreSyncPopUp.ShowPopup(IAPPurchaseStatus.SYNC_SUCCEDED);
		ResetHighlightWidgets(null);
		_UIStore.ProcessChooseSelection(null);
	}

	private void OnPurchaseSuccessful(string purchase)
	{
		_IAPStoreSyncPopUp.ShowPopup(IAPPurchaseStatus.SUCCEDED);
	}

	private void OnPurchaseFailed(string error)
	{
		_IAPStoreSyncPopUp.ShowPopup(IAPPurchaseStatus.FAILED);
	}

	private void OnPurchaseCancelled(string error)
	{
		if (UtPlatform.IsAndroid())
		{
			_IAPStoreSyncPopUp.ExitPopUp();
		}
		else
		{
			_IAPStoreSyncPopUp.ShowPopup(IAPPurchaseStatus.FAILED);
		}
	}

	private void OnPurchaseDoneStatusEvent(PurchaseStatus status)
	{
	}

	private bool UpdateWidgetState(ItemData inItemData, KAWidget inItemWidget)
	{
		if (inItemWidget == null)
		{
			return false;
		}
		KAWidget kAWidget = inItemWidget.FindChildItem("AniIconHeader");
		if (kAWidget == null)
		{
			return false;
		}
		if (kAWidget.GetVisibility())
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		if (inItemData == null)
		{
			return false;
		}
		UpdateCredits(inItemData, inItemWidget);
		ItemAvailability availability = inItemData.GetAvailability();
		if (mFilter._IconOption == StoreFilter.StoreFilterIcon.Priority && inItemData.IsFree())
		{
			SetHeaderProps(kAWidget, "AniIconFree");
			return availability?.EndDate.HasValue ?? false;
		}
		ItemData.ItemSale currentSale = inItemData.GetCurrentSale();
		if (currentSale != null && (mFilter._IconOption == StoreFilter.StoreFilterIcon.Priority || mFilter._IconOption == StoreFilter.StoreFilterIcon.Sale))
		{
			SetHeaderProps(kAWidget, "AniIconSale");
			DateTime mEndDate = currentSale.mEndDate;
			TimeSpan inTime = mEndDate.Subtract(ServerTime.pCurrentTime);
			if (inTime.TotalSeconds > 0.0 && inTime.TotalHours < 24.0)
			{
				string customizedTimerString = GetCustomizedTimerString(inTime);
				if (!string.IsNullOrEmpty(customizedTimerString))
				{
					kAWidget.SetText(kAWidget.GetText() + "\n" + customizedTimerString);
				}
			}
			return true;
		}
		if (availability != null && availability.EndDate.HasValue && (mFilter._IconOption == StoreFilter.StoreFilterIcon.Priority || mFilter._IconOption == StoreFilter.StoreFilterIcon.LimitedAvailability))
		{
			SetHeaderProps(kAWidget, "AniIconTime");
			DateTime value = availability.EndDate.Value;
			DateTime dateTime = availability.EndDate.Value.ToLocalTime();
			string text = _UIStore._AvailableTilText.GetLocalizedString().Replace("XXXX", dateTime.ToString("MM/dd"));
			TimeSpan inTime2 = value.Subtract(ServerTime.pCurrentTime);
			if (inTime2.TotalSeconds > 0.0 && inTime2.TotalHours < 24.0)
			{
				text = GetCustomizedTimerString(inTime2);
			}
			kAWidget.SetText(text);
			return true;
		}
		if ((mFilter._IconOption == StoreFilter.StoreFilterIcon.Priority || mFilter._IconOption == StoreFilter.StoreFilterIcon.New) && inItemData.IsNew)
		{
			SetHeaderProps(kAWidget, "AniIconNew");
			return false;
		}
		if (mFilter._IconOption != 0 && mFilter._IconOption != StoreFilter.StoreFilterIcon.Popular)
		{
			return false;
		}
		if (!inItemData.pIsPopular)
		{
			return false;
		}
		SetHeaderProps(kAWidget, "AniIconTop");
		return false;
	}

	private void SetHeaderProps(KAWidget inWidget, string key)
	{
		PriorityHeaderInfo priorityHeaderInfo = null;
		for (int i = 0; i < _PriorityHeaderInfo.Length; i++)
		{
			if (_PriorityHeaderInfo[i]._Key.Contains(key))
			{
				priorityHeaderInfo = _PriorityHeaderInfo[i];
				break;
			}
		}
		if (priorityHeaderInfo != null)
		{
			UISlicedSprite componentInChildren = inWidget.GetComponentInChildren<UISlicedSprite>();
			if (!string.IsNullOrEmpty(priorityHeaderInfo._SpriteName) && componentInChildren != null)
			{
				componentInChildren.UpdateSprite(priorityHeaderInfo._SpriteName);
			}
			inWidget.SetText(priorityHeaderInfo._HeaderText.GetLocalizedString());
			inWidget.SetVisibility(inVisible: true);
		}
	}

	private void AddToStoreMenu(ItemData item, int storeDataID)
	{
		string rVo = "";
		if (item.Rollover != null)
		{
			rVo = item.Rollover.Bundle + "/" + item.Rollover.DialogName;
		}
		KAWidget kAWidget = AddWidget(_Template.name, null);
		KAWidget kAWidget2 = kAWidget.FindChildItem(_ItemColorWidget);
		if (kAWidget2 != null)
		{
			if (item.IsStatAvailable())
			{
				UiItemRarityColorSet.SetItemBackgroundColor(item.ItemRarity ?? ItemRarity.Common, kAWidget2);
			}
			else
			{
				UiItemRarityColorSet.SetItemBackgroundColor(mItemDefaultColor, kAWidget2);
			}
		}
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.name = item.ItemName;
		if (CommonInventoryData.pShowItemID)
		{
			KAWidget kAWidget3 = kAWidget.FindChildItem("AniIconFront");
			if (kAWidget3 != null && storeDataID != 0)
			{
				kAWidget3.SetToolTipText(storeDataID + " - " + item.ItemID);
			}
		}
		kAWidget.FindChildItem("AniCreditsInfo").SetText(item.ItemName);
		KAWidget kAWidget4 = kAWidget.FindChildItem("Icon");
		KAWidget kAWidget5 = kAWidget.FindChildItem("AniInfoBehind");
		kAWidget5.SetText(item.Description);
		KAWidget kAWidget6 = kAWidget.FindChildItem("TxtCreativePoints");
		if (item.CreativePoints > 0 && !item.HasCategory(541))
		{
			kAWidget6.SetVisibility(inVisible: true);
			kAWidget6.SetText(item.CreativePoints.ToString());
		}
		else
		{
			kAWidget6.SetVisibility(inVisible: false);
		}
		if (kAWidget5.GetVisibility())
		{
			ShowInfo(inShowInfo: false, kAWidget);
		}
		SetupStatsIcons(kAWidget, item);
		SetIcons(kAWidget, item);
		KAStoreItemData kAStoreItemData = new KAStoreItemData(item.IconName, rVo, item, storeDataID, _WHSize);
		kAStoreItemData.PurchaseStoreType = KAStoreMenuItemData.StoreType.GameStore;
		int num = 0;
		int rid;
		bool flag = kAStoreItemData.IsRankLocked(out rid, _UIStore.pStoreInfo._RankTypeID);
		bool flag2 = flag && kAStoreItemData._ItemData.RewardTypeID == 12;
		KAWidget kAWidget7 = kAWidget.FindChildItem("AniIconLock");
		kAWidget7.SetVisibility(inVisible: false);
		if (!SubscriptionInfo.pIsMember)
		{
			num = kAStoreItemData.GetPrereqItemIfNotInInventory();
			if (num != -1)
			{
				kAWidget7 = kAWidget.FindChildItem("AniIconPrereqItemLock");
			}
			else if (kAStoreItemData.IsLocked())
			{
				kAWidget7 = kAWidget.FindChildItem("AniIconMembershipLock");
			}
		}
		if (flag2)
		{
			kAWidget7 = kAWidget.FindChildItem("AniIconUDTLock");
		}
		else if (flag)
		{
			kAWidget7 = kAWidget.FindChildItem("AniIconLevelLock");
		}
		if (kAWidget7 != null)
		{
			if (!SubscriptionInfo.pIsMember)
			{
				if (num != -1)
				{
					PrereqItemUIMapping prereqItemUIMappingData = _UIStore.GetPrereqItemUIMappingData(kAStoreItemData.GetPrereqItemIfNotInInventory());
					if (prereqItemUIMappingData != null && prereqItemUIMappingData._LockTexture != null)
					{
						kAWidget7.SetTexture(prereqItemUIMappingData._LockTexture);
						kAWidget7.SetVisibility(inVisible: true);
					}
				}
				else if (kAStoreItemData.IsLocked())
				{
					kAWidget7.SetVisibility(inVisible: true);
				}
			}
			if (flag2)
			{
				kAWidget7.SetText(string.Format(_UDTPointsRequiredText.GetLocalizedString(), rid));
				kAWidget7.SetVisibility(inVisible: true);
			}
			else if (!_UIStore.pCategoryMenu.pDisableRankCheck && flag)
			{
				kAWidget7.SetText(string.Format(_LevelRequiredText.GetLocalizedString(), rid));
				kAWidget7.SetVisibility(inVisible: true);
			}
		}
		if (kAWidget4 != null)
		{
			kAWidget4.SetUserData(kAStoreItemData);
		}
		if (UpdateWidgetState(kAStoreItemData._ItemData, kAWidget))
		{
			if (mLimitedItems == null)
			{
				mLimitedItems = new List<LimitedStateItemData>();
			}
			LimitedStateItemData limitedStateItemData = new LimitedStateItemData(kAStoreItemData);
			limitedStateItemData.Initialize();
			limitedStateItemData.UpdateNextUpdateTime();
			mLimitedItems.Add(limitedStateItemData);
		}
		kAStoreItemData.ShowLoadingItem(inShow: true);
		kAStoreItemData._DefaultTexture = _DefaultItemTexture;
		if (KAUIStore._EnterItemID > 0 && KAUIStore._EnterItemID == item.ItemID)
		{
			SetSelectedItem(kAWidget);
			_UIStore.ProcessChooseSelection(kAWidget4);
		}
	}

	private void HandleLimitedStateItems()
	{
		if (mLimitedItems == null || mLimitedItems.Count == 0)
		{
			return;
		}
		List<LimitedStateItemData> list = null;
		foreach (LimitedStateItemData mLimitedItem in mLimitedItems)
		{
			if (mLimitedItem == null || mLimitedItem.mData == null || DateTime.Compare(mLimitedItem.mNextUpdateTime, ServerTime.pCurrentTime) > 0)
			{
				continue;
			}
			mLimitedItem.UpdateNextUpdateTime();
			KAStoreItemData mData = mLimitedItem.mData;
			if (mData._ItemData.IsOutdated() || !mFilter.CanFilter(mData._ItemData))
			{
				if (mLimitedItem.mIconInfo != null && mSelectedItem == mLimitedItem.mIconInfo && _UIStore.pChooseUI != null)
				{
					_UIStore.pChooseUI.SetVisibility(t: false);
				}
				RemoveWidget(mData.GetItem().GetRootItem());
				if (list == null)
				{
					list = new List<LimitedStateItemData>();
				}
				list.Add(mLimitedItem);
			}
			else
			{
				UpdateWidgetState(mData._ItemData, mData.GetItem().GetRootItem());
			}
		}
		if (list == null || list.Count <= 0)
		{
			return;
		}
		foreach (LimitedStateItemData item in list)
		{
			mLimitedItems.Remove(item);
		}
	}

	private string GetCustomizedTimerString(TimeSpan inTime)
	{
		if (inTime.TotalMinutes < 1.0)
		{
			return _UIStore._TimeLeftMinText.GetLocalizedString().Replace("XXXX", Mathf.CeilToInt((float)inTime.TotalMinutes).ToString());
		}
		if (inTime.TotalMinutes < 60.0)
		{
			return _UIStore._TimeLeftMinsText.GetLocalizedString().Replace("XXXX", Mathf.CeilToInt((float)inTime.TotalMinutes).ToString());
		}
		if (inTime.TotalHours < 24.0)
		{
			return _UIStore._TimeLeftHoursText.GetLocalizedString().Replace("XXXX", Mathf.CeilToInt((float)inTime.TotalHours).ToString());
		}
		return string.Empty;
	}

	public void UpdateCredits(ItemData inItemData, KAWidget inWidget)
	{
		if (inItemData == null || inWidget == null)
		{
			return;
		}
		KAWidget kAWidget = inWidget.FindChildItem("AniCreditCard");
		KAWidget kAWidget2 = inWidget.FindChildItem("TxtCurrency");
		Color color = Color.black;
		KAWidget kAWidget3 = inWidget.FindChildItem("TxtStriked");
		if (kAWidget3 == null)
		{
			kAWidget3 = inWidget.FindChildItem("TxtStrikedCurrency");
		}
		KAWidget kAWidget4 = inWidget.FindChildItem("TxtSaleCost");
		UILabel label = kAWidget3.GetLabel();
		float num = -1f;
		Texture texture = null;
		int num2 = 0;
		int num3 = 0;
		if (inItemData.GetPurchaseType() == 1)
		{
			num2 = inItemData.FinalCost;
			texture = _IconTextureCoin;
			num3 = ((!SubscriptionInfo.pIsMember) ? inItemData.pNonMemberCost : inItemData.pMemberCost);
		}
		else
		{
			num2 = inItemData.FinalCashCost;
			texture = _IconTextureGems;
			num3 = (SubscriptionInfo.pIsMember ? inItemData.pMemberCashCost : inItemData.pNonMemberCashCost);
		}
		if (kAWidget != null)
		{
			if (!kAWidget.GetVisibility())
			{
				kAWidget.SetVisibility(inVisible: true);
			}
			kAWidget.SetTexture(texture);
		}
		if (kAWidget2 != null)
		{
			color = kAWidget2.GetLabel().color;
			if (!kAWidget2.GetVisibility())
			{
				kAWidget2.SetVisibility(inVisible: true);
			}
			kAWidget2.SetText(num2.ToString());
		}
		if (kAWidget3 != null)
		{
			if (num3 > num2)
			{
				kAWidget3.SetVisibility(inVisible: true);
				kAWidget3.SetText(num3.ToString());
				if (kAWidget4 != null)
				{
					kAWidget4.SetVisibility(inVisible: true);
					kAWidget4.SetText(num2.ToString());
					if (kAWidget2 != null)
					{
						kAWidget2.SetVisibility(inVisible: false);
					}
				}
				else if (kAWidget2 != null && kAWidget2.GetLabel() != null)
				{
					kAWidget2.GetLabel().color = _OfferPriceColor;
				}
				if (label != null)
				{
					num = label.transform.localScale.x * (float)label.width + 5f;
				}
			}
			else
			{
				if (kAWidget2 != null && kAWidget2.GetLabel() != null)
				{
					kAWidget2.GetLabel().color = color;
				}
				kAWidget3.SetVisibility(inVisible: false);
				kAWidget3.SetText(string.Empty);
			}
		}
		if (num > 0f && !(kAWidget3.pBackground == null))
		{
			kAWidget3.pBackground.width = (int)num;
			Vector3 localPosition = kAWidget3.pBackground.transform.localPosition;
			localPosition.x = 0f - num / 2f;
			kAWidget3.pBackground.transform.localPosition = localPosition;
		}
	}

	public void DisablePageScroll(bool inDisable)
	{
		if (mScrollBar != null)
		{
			mScrollBar.SetDisabled(inDisable);
		}
		if (inDisable)
		{
			mCanPageRight = false;
			mCanPageLeft = false;
			return;
		}
		int numItems = GetNumItems();
		int numItemsPerPage = GetNumItemsPerPage();
		mCanPageRight = mCurrentItemIndex + numItemsPerPage < numItems;
		mCanPageLeft = mCurrentItemIndex >= numItemsPerPage;
	}
}
