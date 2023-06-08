using System;

public class UiItemCustomizationMenu : KAUISelectMenu
{
	private UserItemData[] mUserItemData;

	private KAUISelectItemData mSelectedItemData;

	private UiAvatarItemCustomization mUIAvatarItemCustomization;

	public void PopulateMenu(UserItemData[] inItem, KAUISelectItemData selectedItemData)
	{
		mUIAvatarItemCustomization = (UiAvatarItemCustomization)_ParentUi;
		ClearItems();
		mUserItemData = inItem;
		if (selectedItemData != null)
		{
			if (Array.Find(inItem, (UserItemData x) => x.ItemID == selectedItemData._UserItemData.ItemID) != null)
			{
				mSelectedItemData = selectedItemData;
			}
			else
			{
				mSelectedItemData = null;
			}
		}
		FinishMenuItems();
	}

	protected override void Update()
	{
	}

	public override void FinishMenuItems(bool addParentItems = false)
	{
		UserItemData[] array = mUserItemData;
		foreach (UserItemData userItemData in array)
		{
			if (userItemData.Item.HasCategory(657))
			{
				AddInvMenuItem(userItemData);
			}
		}
		if (mSelectedItemData != null && mSelectedItemData._ItemData != null && mSelectedItemData._ItemData.HasCategory(657))
		{
			OnClick(mSelectedItemData.GetItem());
		}
		else if (mItemInfo.Count > 0)
		{
			OnClick(mItemInfo[0]);
		}
	}

	public override void SetSelectedItem(KAWidget inWidget)
	{
		mSelectedItemData = (KAUISelectItemData)inWidget.GetUserData();
		base.SetSelectedItem(inWidget);
	}

	protected override void OnGridReposition()
	{
		base.OnGridReposition();
		if (mSelectedItemData != null)
		{
			KAWidget item = mSelectedItemData.GetItem();
			SetSelectedItem(item);
			FocusWidget(item);
		}
	}

	public override void UpdateWidget(KAUISelectItemData id)
	{
		if (id != null && id._ItemData.HasCategory(491) && id._ItemData.AssetName != "NULL" && !string.IsNullOrEmpty(id._ItemData.AssetName))
		{
			UiAvatarItemCustomization.SetThumbnail(id.GetItem(), id._UserInventoryID);
		}
		else
		{
			base.UpdateWidget(id);
		}
		if (mSelectedItemData != null && id._UserItemData.UserInventoryID == mSelectedItemData._UserItemData.UserInventoryID)
		{
			mSelectedItemData = id;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		mSelectedItemData = (KAUISelectItemData)inWidget.GetUserData();
		mUIAvatarItemCustomization.OnSelectItem(inWidget);
	}
}
