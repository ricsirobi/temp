using UnityEngine;

public class KAUIStoreCategoryMenu : KAUIMenu
{
	public KAStoreMenuItemData[] _MenuItemData;

	private int mMaxItemsLimit = -1;

	private KAStoreMenuItemData[] mCategories;

	private KAWidget mLastSelectedItem;

	private KAWidget mItemTemplate;

	private KAStoreMenuItemData.StoreType mType = KAStoreMenuItemData.StoreType.GameStore;

	private bool mDisableRankCheck;

	public int pMaxItemslimit => mMaxItemsLimit;

	public KAStoreMenuItemData.StoreType pType => mType;

	public KAWidget pLastSelectedItem
	{
		get
		{
			return mLastSelectedItem;
		}
		set
		{
			mLastSelectedItem = value;
		}
	}

	public bool pDisableRankCheck => mDisableRankCheck;

	protected override void Start()
	{
		base.Start();
		mItemTemplate = _Template;
		SetVisibility(inVisible: true);
	}

	protected override void ButtonClicked(KAScrollBar scrollBar, KAScrollButton scrollButton, bool isRepeated)
	{
		base.ButtonClicked(scrollBar, scrollButton, isRepeated);
		if (KAUIStoreCategory.pInstance._StoreUI != null)
		{
			((KAUIStore)KAUIStoreCategory.pInstance._StoreUI).EnableStoreMenu(inEnable: false);
		}
	}

	public void SetCategories(KAStoreMenuItemData[] categories)
	{
		if (categories == mCategories)
		{
			return;
		}
		mCategories = categories;
		ClearItems();
		int num = 0;
		foreach (KAStoreMenuItemData kAStoreMenuItemData in categories)
		{
			if (!kAStoreMenuItemData._IsEnabled)
			{
				continue;
			}
			KAWidget kAWidget = DuplicateWidget(mItemTemplate);
			AddWidget(kAWidget);
			kAStoreMenuItemData._UserData = kAWidget;
			KAWidget kAWidget2 = kAWidget.FindChildItem("AniSaleBurst");
			if (kAWidget2 != null)
			{
				KAWidget kAWidget3 = kAWidget2.FindChildItem("TxtSaleBurst");
				if (kAWidget3 != null)
				{
					if (kAStoreMenuItemData._Filter != null && kAStoreMenuItemData._Filter._Discount > 0f)
					{
						kAWidget3.SetText(kAStoreMenuItemData._Filter._Discount * 100f + "%");
						kAWidget2.SetVisibility(inVisible: true);
					}
					else
					{
						kAWidget3.SetText(string.Empty);
						kAWidget2.SetVisibility(inVisible: false);
					}
				}
				else
				{
					kAWidget2.SetVisibility(inVisible: false);
				}
			}
			kAWidget.SetVisibility(inVisible: true);
			kAWidget.name = num + kAStoreMenuItemData._Name;
			kAWidget.SetTextByID(kAStoreMenuItemData._DisplayText._ID, kAStoreMenuItemData._DisplayText._Text);
			kAStoreMenuItemData._MenuIdx = num;
			num++;
		}
		GoToPage(1, instant: true);
		ResetPosition();
	}

	private void ResetToggles()
	{
		foreach (KAToggleButton item in GetItems())
		{
			item.SetChecked(isChecked: false);
		}
	}

	public override void OnClick(KAWidget item)
	{
		if (item == mLastSelectedItem)
		{
			return;
		}
		base.OnClick(item);
		mLastSelectedItem = item;
		Input.ResetInputAxes();
		KAStoreMenuItemData[] array = mCategories;
		foreach (KAStoreMenuItemData kAStoreMenuItemData in array)
		{
			if (kAStoreMenuItemData._UserData == item)
			{
				AnalyticStoreItemViewedEvent._ItemSubCategory = kAStoreMenuItemData._Name;
				if (KAUIStoreCategory.pInstance._StoreUI != null)
				{
					KAUIStoreCategory.pInstance._StoreUI.SelectCategory(kAStoreMenuItemData);
				}
				else
				{
					Debug.LogError("the store manager ui should be created before this");
				}
				break;
			}
		}
		KAUIStore.pInstance.pStatCompareMenu.RemoveStatPreview();
		KAUIStore.pInstance.pDragonStatMenu.RemoveStatPreview();
	}

	public void OnCategorySelected(StoreFilter filter)
	{
		ResetToggles();
		mLastSelectedItem = null;
		if (mCategories == null || mCategories.Length == 0)
		{
			return;
		}
		KAStoreMenuItemData[] array = mCategories;
		foreach (KAStoreMenuItemData kAStoreMenuItemData in array)
		{
			if (kAStoreMenuItemData._Filter.IsSame(filter))
			{
				mMaxItemsLimit = kAStoreMenuItemData._MaxItemsLimit;
				mType = kAStoreMenuItemData._StoreType;
				mDisableRankCheck = filter._DisableRankCheck;
				AnalyticStoreItemViewedEvent._ItemSubCategory = kAStoreMenuItemData._Name;
				if (kAStoreMenuItemData._UserData != null)
				{
					mLastSelectedItem = (KAToggleButton)kAStoreMenuItemData._UserData;
					((KAToggleButton)kAStoreMenuItemData._UserData).SetChecked(isChecked: true);
				}
				if (InteractiveTutManager._CurrentActiveTutorialObject != null && filter._CategoryIDs != null && filter._CategoryIDs.Length != 0)
				{
					string value = "KAUIStoreCategoryMenu_SelectedCategory_" + filter._CategoryIDs[0];
					InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", value);
				}
			}
		}
	}
}
