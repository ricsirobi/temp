using System.Collections.Generic;
using UnityEngine;

public class KAUIStoreMainMenu : KAUIMenu
{
	public KAStoreInfo[] _StoreData;

	public LocaleString _NoPetText = new LocaleString("You must have a dragon to access this store.");

	public LocaleString _PetNotAllowedText = new LocaleString("You cannot access the pet store at this time.");

	public PointTypeLockText[] _PointTypeLockTextData;

	private KAUIStore mUI;

	private KAToggleButton mTemplateItem;

	private KAWidget mSelectedWidget;

	protected override void Start()
	{
		base.Start();
		mUI = base.transform.root.GetComponentInChildren<KAUIStore>();
		mTemplateItem = (KAToggleButton)_Template;
		KAStoreInfo[] storeData = _StoreData;
		foreach (KAStoreInfo kAStoreInfo in storeData)
		{
			if (true)
			{
				KAWidget kAWidget = DuplicateWidget(mTemplateItem);
				kAWidget.name = kAStoreInfo._Name;
				AddWidget(kAWidget);
				KAButton component = kAWidget.gameObject.GetComponent<KAButton>();
				if (component != null)
				{
					component.SetToolTipText(kAStoreInfo._ToolTipText.GetLocalizedString());
				}
				kAStoreInfo._UserData = kAWidget;
				kAWidget.SetVisibility(inVisible: true);
				kAWidget.SetUserData(kAStoreInfo);
				KAWidget kAWidget2 = kAWidget.FindChildItem("AniIcon");
				kAWidget2.SetState(KAUIState.NOT_INTERACTIVE);
				kAWidget2.SetTexture(kAStoreInfo._Icon);
			}
		}
		UpdateSale();
	}

	private void ResetToggles()
	{
		foreach (KAToggleButton item in GetItems())
		{
			item.SetChecked(isChecked: false);
		}
	}

	public void SetSelectedWidget(KAWidget item)
	{
		mSelectedWidget = item;
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (KAUIStore.pLocked)
		{
			return;
		}
		Input.ResetInputAxes();
		KAStoreInfo kAStoreInfo = (KAStoreInfo)item.GetUserData();
		if (kAStoreInfo._OnlyForPet && (SanctuaryManager.pCurPetData == null || SanctuaryManager.pCurPetInstance == null))
		{
			KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
			if (SanctuaryManager.pCurPetData == null)
			{
				kAUIGenericDB.SetText(_NoPetText.GetLocalizedString(), interactive: false);
			}
			else if (SanctuaryManager.pCurPetInstance == null)
			{
				kAUIGenericDB.SetText(_PetNotAllowedText.GetLocalizedString(), interactive: false);
			}
			kAUIGenericDB._MessageObject = base.gameObject;
			kAUIGenericDB._OKMessage = "DestroyMessageDB";
			kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
			kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			KAUI.SetExclusive(kAUIGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		}
		else if (!(mSelectedWidget != null) || !(mSelectedWidget == item))
		{
			mSelectedWidget = item;
			AnalyticStoreItemViewedEvent._ItemCategory = kAStoreInfo._Name;
			mUI.OnStoreSelected(kAStoreInfo);
			mUI.SetSelectedStoreIcon(item.FindChildItem("AniIcon").GetTexture());
			mUI.pStatCompareMenu.RemoveStatPreview();
			mUI.pDragonStatMenu.RemoveStatPreview();
		}
	}

	private void DestroyMessageDB()
	{
		OnClick(FindItem(mUI.pStoreInfo._Name));
	}

	public void OnStoreSelected(string StoreName)
	{
		ResetToggles();
		KAStoreInfo[] storeData = _StoreData;
		foreach (KAStoreInfo kAStoreInfo in storeData)
		{
			if (kAStoreInfo._Name == StoreName)
			{
				AnalyticStoreItemViewedEvent._ItemCategory = kAStoreInfo._Name;
				((KAToggleButton)kAStoreInfo._UserData).SetChecked(isChecked: true);
			}
		}
	}

	public KAStoreInfo FindStore(string n)
	{
		KAStoreInfo[] storeData = _StoreData;
		foreach (KAStoreInfo kAStoreInfo in storeData)
		{
			if (kAStoreInfo._Name == n)
			{
				return kAStoreInfo;
			}
		}
		return null;
	}

	public List<KAStoreInfo> FindStores(int n)
	{
		List<KAStoreInfo> list = new List<KAStoreInfo>();
		KAStoreInfo[] storeData = _StoreData;
		foreach (KAStoreInfo kAStoreInfo in storeData)
		{
			int[] iDs = kAStoreInfo._IDs;
			for (int j = 0; j < iDs.Length; j++)
			{
				if (iDs[j] == n)
				{
					list.Add(kAStoreInfo);
				}
			}
		}
		return list;
	}

	public KAStoreInfo FindStore(int n)
	{
		KAStoreInfo[] storeData = _StoreData;
		foreach (KAStoreInfo kAStoreInfo in storeData)
		{
			int[] iDs = kAStoreInfo._IDs;
			for (int j = 0; j < iDs.Length; j++)
			{
				if (iDs[j] == n)
				{
					return kAStoreInfo;
				}
			}
		}
		return null;
	}

	private void UpdateSale()
	{
		if (_StoreData == null || _StoreData.Length == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		KAStoreInfo[] storeData = _StoreData;
		foreach (KAStoreInfo kAStoreInfo in storeData)
		{
			if (kAStoreInfo._ShowSale)
			{
				for (int j = 0; j < kAStoreInfo._IDs.Length; j++)
				{
					list.Add(kAStoreInfo._IDs[j]);
				}
			}
		}
		ItemStoreDataLoader.Load(list.ToArray(), OnStoreListedLoaded, null, (mUI == null) ? (-1f) : mUI._TimeToForceStoreLoad);
	}

	private void UpdateSale(List<KAStoreInfo> inStoreInfoList)
	{
		if (inStoreInfoList == null || inStoreInfoList.Count == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (KAStoreInfo inStoreInfo in inStoreInfoList)
		{
			if (inStoreInfo._ShowSale)
			{
				for (int i = 0; i < inStoreInfo._IDs.Length; i++)
				{
					list.Add(inStoreInfo._IDs[i]);
				}
			}
		}
		ItemStoreDataLoader.Load(list.ToArray(), OnStoreListedLoaded, null, (mUI == null) ? (-1f) : mUI._TimeToForceStoreLoad);
	}

	private void OnStoreListedLoaded(List<StoreData> inSDList, object inUserData)
	{
		if (inSDList == null)
		{
			return;
		}
		foreach (StoreData inSD in inSDList)
		{
			if (inSD == null)
			{
				continue;
			}
			List<KAStoreInfo> list = FindStores(inSD._ID);
			if (list == null)
			{
				continue;
			}
			foreach (KAStoreInfo item in list)
			{
				KAStoreMenuItemData[] menuData = item._MenuData;
				foreach (KAStoreMenuItemData kAStoreMenuItemData in menuData)
				{
					if (kAStoreMenuItemData == null || kAStoreMenuItemData._Filter == null || kAStoreMenuItemData._Filter._CategoryIDs == null || kAStoreMenuItemData._Filter._CategoryIDs.Length == 0 || inSD._Data.SalesAtStore == null)
					{
						continue;
					}
					ItemsInStoreDataSale[] salesAtStore = inSD._Data.SalesAtStore;
					foreach (ItemsInStoreDataSale itemsInStoreDataSale in salesAtStore)
					{
						if (itemsInStoreDataSale == null || itemsInStoreDataSale.IsOutdated() || (itemsInStoreDataSale.ForMembers.HasValue && itemsInStoreDataSale.ForMembers.Value))
						{
							continue;
						}
						bool flag = false;
						int[] categoryIDs = kAStoreMenuItemData._Filter._CategoryIDs;
						foreach (int inCategoryID in categoryIDs)
						{
							if (itemsInStoreDataSale.HasCategory(inCategoryID))
							{
								kAStoreMenuItemData._Filter._Discount = Mathf.Max(kAStoreMenuItemData._Filter._Discount, itemsInStoreDataSale.Modifier);
								flag = true;
								break;
							}
						}
						if (flag)
						{
							break;
						}
					}
				}
				UpdateDiscount(item);
			}
		}
	}

	private void UpdateDiscount(KAStoreInfo inStoreInfo)
	{
		float num = 0f;
		KAStoreMenuItemData[] menuData = inStoreInfo._MenuData;
		foreach (KAStoreMenuItemData kAStoreMenuItemData in menuData)
		{
			if (kAStoreMenuItemData == null || kAStoreMenuItemData._Filter == null)
			{
				continue;
			}
			num = Mathf.Max(num, kAStoreMenuItemData._Filter._Discount);
			if (!(kAStoreMenuItemData._UserData != null))
			{
				continue;
			}
			KAWidget kAWidget = kAStoreMenuItemData._UserData as KAWidget;
			if (!(kAWidget != null))
			{
				continue;
			}
			KAWidget kAWidget2 = kAWidget.FindChildItem("AniSaleBurst");
			if (!(kAWidget2 != null))
			{
				continue;
			}
			KAWidget kAWidget3 = kAWidget2.FindChildItem("TxtSaleBurst");
			if (kAWidget3 != null)
			{
				if (kAStoreMenuItemData._Filter._Discount > 0f)
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
		if (!(inStoreInfo._UserData != null))
		{
			return;
		}
		KAWidget kAWidget4 = inStoreInfo._UserData as KAWidget;
		if (!(kAWidget4 != null))
		{
			return;
		}
		KAWidget kAWidget5 = kAWidget4.FindChildItem("AniSaleBurst");
		KAWidget kAWidget6 = kAWidget5.FindChildItem("TxtSaleBurst");
		if (kAWidget5 != null && kAWidget6 != null)
		{
			if (num > 0f)
			{
				kAWidget6.SetText(num * 100f + "%");
				kAWidget5.SetVisibility(inVisible: true);
			}
			else
			{
				kAWidget6.SetText(string.Empty);
				kAWidget5.SetVisibility(inVisible: false);
			}
		}
	}

	public string GetTextFromPointType(int pointType)
	{
		if (_PointTypeLockTextData == null || _PointTypeLockTextData.Length == 0)
		{
			return string.Empty;
		}
		for (int i = 0; i < _PointTypeLockTextData.Length; i++)
		{
			if (_PointTypeLockTextData[i]._PointTypeID == pointType)
			{
				return _PointTypeLockTextData[i]._LockText.GetLocalizedString();
			}
		}
		return string.Empty;
	}
}
