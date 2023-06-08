using System;
using UnityEngine;

public class KAUIStoreChoose : KAUI
{
	public static GameObject _ExitToObject;

	public string _LinkBtnName = "";

	public string _LinkLevelName = "";

	public string _LinkActivateObjectName = "";

	public bool _ShowDescription;

	public StoreItemProperty _Description;

	public float _RotationSpeed = 40f;

	[NonSerialized]
	public KAUIStore _StoreUI;

	protected StoreWidgetUserData mChosenItemData;

	protected StoreFilter mFilter;

	protected KAWidget mChosenIcon;

	protected KAWidget mAd;

	protected KAWidget mLockedItem;

	protected KAWidget mRankLockedItem;

	protected KAWidget mRankLockedTextItem;

	protected KAWidget mNewItem;

	protected KAWidget mSaleItem;

	protected KAWidget mDescriptionTextItem;

	private string mStoreName = "";

	public IAPItemWidgetUserData pChosenIAPItemData => mChosenItemData as IAPItemWidgetUserData;

	public KAStoreItemData pChosenItemData => mChosenItemData as KAStoreItemData;

	protected override void Start()
	{
		mChosenIcon = FindItem("Selected");
		mAd = FindItem("aniAd");
		if (_LinkBtnName != "")
		{
			FindItem(_LinkBtnName).SetVisibility(inVisible: true);
		}
		base.Start();
	}

	public virtual void ShowAD(bool t)
	{
		mAd.SetVisibility(t);
		if (!UtPlatform.IsMobile())
		{
			mChosenIcon.SetVisibility(!t);
		}
	}

	public override void SetVisibility(bool t)
	{
		base.SetVisibility(t);
	}

	public virtual void ProcessChooseSelection(KAWidget item)
	{
		ProcessSelection(item);
	}

	protected void ProcessSelection(KAWidget item)
	{
		if (item != null)
		{
			mChosenItemData = (StoreWidgetUserData)item.GetUserData();
			if (_StoreUI.pCategoryMenu.pType == KAStoreMenuItemData.StoreType.GameStore || mChosenItemData.PurchaseStoreType == KAStoreMenuItemData.StoreType.GameStore)
			{
				if (pChosenItemData == null)
				{
					UtDebug.LogError("Item user data missing.");
					return;
				}
				KAUIStore._EnterItemID = pChosenItemData._ItemData.ItemID;
			}
			else if (_StoreUI.pCategoryMenu.pType == KAStoreMenuItemData.StoreType.IAPStore && _StoreUI != null)
			{
				_StoreUI.ShowPreviewBuyButton(visible: true);
			}
			Texture texture = item.GetTexture();
			if (texture != null)
			{
				mChosenIcon.SetTexture(texture);
			}
			return;
		}
		if (mLockedItem != null)
		{
			mLockedItem.SetVisibility(inVisible: false);
		}
		if (mRankLockedItem != null)
		{
			mRankLockedItem.SetVisibility(inVisible: false);
		}
		if (mNewItem != null)
		{
			mNewItem.SetVisibility(inVisible: false);
		}
		if (mSaleItem != null)
		{
			mSaleItem.SetVisibility(inVisible: false);
		}
		if (mDescriptionTextItem != null)
		{
			mDescriptionTextItem.SetVisibility(inVisible: false);
		}
		mChosenItemData = null;
		if (_StoreUI != null)
		{
			_StoreUI.ShowAdsButton(visible: false);
			_StoreUI.ShowPreviewBuyButton(visible: false);
			_StoreUI.ShowPreviewRotateButtons(visible: false);
			_StoreUI.ShowFullPreviewButton(visible: false);
		}
	}

	public void ResetSelectedItemData()
	{
		mChosenItemData = null;
	}

	public virtual void PurchaseSuccessful(CommonInventoryResponseItem[] ret)
	{
	}

	public virtual void BuyCurrentItem(int count = 1)
	{
		_StoreUI._CurrentPurchaseItem = new PurchaseItemData(pChosenItemData._ItemData, pChosenItemData._StoreID, count);
	}

	public virtual void ChangeCategory(StoreFilter inFilter)
	{
		mFilter = inFilter;
	}

	public virtual void OnStoreSelected(KAStoreInfo inStoreInfo)
	{
	}

	public virtual void SetStore(string sname, StoreFilter inFilter, KAStoreMenuItemData inMenuItemData)
	{
		if (!(sname == mStoreName) || !mFilter.IsSame(inFilter))
		{
			ProcessChooseSelection(null);
			mStoreName = sname;
			mFilter = inFilter;
			_StoreUI.pChooseMenu.ChangeCategory(inFilter);
		}
	}

	public virtual void RotatePreview(float rotationDelta)
	{
	}

	public void RotatePreview(bool rotateLeft = true)
	{
		if (rotateLeft)
		{
			RotatePreview(_RotationSpeed);
		}
		else
		{
			RotatePreview(0f - _RotationSpeed);
		}
	}

	public virtual void MovePreviewPageLeft()
	{
	}

	public virtual void MovePreviewPageRight()
	{
	}

	public virtual void OnSyncUIClosed(bool isPurchaseSuccess)
	{
	}

	public virtual void ResetPreview()
	{
		if (StorePreview.pInstance != null)
		{
			StorePreview.pInstance.Reset();
		}
	}
}
