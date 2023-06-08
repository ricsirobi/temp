using UnityEngine;

public class UiBackpackItemTrashDB : UiBackpackItemActionDB
{
	public LocaleString _TrashItemText = new LocaleString("Are you sure you want to trash this item?");

	public LocaleString _TrashConfirmDBTitleText = new LocaleString("Confirmation");

	private KAWidget mBtnTrash;

	private KAWidget mTxtItemName;

	private KAWidget mTxtItemDescription;

	private KAWidget mIcoItem;

	private KAWidget mTxtQuantity;

	private KAWidget mQuantityPlus;

	private KAWidget mQuantityMinus;

	private int mItemQuantity = 1;

	private int mInventoryQuantity;

	private KAUIGenericDB mKAUIGenericDB;

	protected override void Start()
	{
		base.Start();
		mTxtItemName = FindItem("TxtTitle");
		mTxtItemName.SetText(mItemData.ItemName);
		mIcoItem = FindItem("IcoItem");
		mIcoItem.SetTextureFromBundle(mItemData.IconName);
		mBtnTrash = FindItem("BtnTrash");
		mBtnTrash.SetVisibility(inVisible: true);
		mTxtItemDescription = FindItem("TxtItemDescription");
		mTxtItemDescription.SetText(mItemData.Description);
		SetupQuantity();
	}

	private void SetupQuantity()
	{
		if (mBtnTrash.GetVisibility())
		{
			KAWidget kAWidget = FindItem("Quantity");
			mTxtQuantity = kAWidget.FindChildItem("TxtQuantityValue");
			mQuantityMinus = kAWidget.FindChildItem("BtnNegative");
			mQuantityPlus = kAWidget.FindChildItem("BtnPositive");
			kAWidget.SetVisibility(inVisible: true);
			mInventoryQuantity = CommonInventoryData.pInstance.GetQuantity(mItemData.ItemID);
			RefreshQuantity();
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnTrash)
		{
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "Trash Item");
			mKAUIGenericDB.SetMessage(base.gameObject, "OnTrashConfirm", "OnTrashNo", null, null);
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			mKAUIGenericDB.SetText(_TrashItemText.GetLocalizedString(), interactive: false);
			mKAUIGenericDB.SetTitle(_TrashConfirmDBTitleText.GetLocalizedString());
			KAUI.SetExclusive(mKAUIGenericDB);
		}
		else if (inWidget == mQuantityPlus)
		{
			mItemQuantity++;
			RefreshQuantity();
		}
		else if (inWidget == mQuantityMinus && mItemQuantity > 1)
		{
			mItemQuantity--;
			RefreshQuantity();
		}
	}

	private void RefreshQuantity()
	{
		mTxtQuantity.SetText(mItemQuantity.ToString());
		mQuantityPlus.SetDisabled(mInventoryQuantity == mItemQuantity);
		mQuantityMinus.SetDisabled(mItemQuantity <= 1);
	}

	private void OnTrashConfirm()
	{
		if (base.pMessageObject != null)
		{
			base.pMessageObject.SendMessage("TrashItem", mItemQuantity, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			UtDebug.LogError("Message object not set for the Use Button in ItemActionDB");
		}
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			mKAUIGenericDB.Destroy();
			mKAUIGenericDB = null;
		}
		Destroy();
	}

	private void OnTrashNo()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			mKAUIGenericDB.Destroy();
			mKAUIGenericDB = null;
		}
	}
}
