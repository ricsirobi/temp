using System.Collections.Generic;

public class UiJAEquipment : KAUISelectAvatar
{
	public List<EquipmentPartTab> _EquipmentPartTab;

	public List<EquipmentBeltTab> _EquipmentBeltTab;

	private UiAvatarCustomizationCategoryMenu mEquipmentCategoryMenu;

	private UiJAEquipmentMenu mEquipmentMenu;

	private UiJAEquipmentBeltMenu mEquipmentBeltMenu;

	private EquipmentPartTab mCurrentTab;

	private int mItemsToLoadCnt = -1;

	private List<ItemData> mItemData = new List<ItemData>();

	public EquipmentPartTab pCurrentTab => mCurrentTab;

	public override void Initialize()
	{
		base.Initialize();
		mEquipmentCategoryMenu = (UiAvatarCustomizationCategoryMenu)GetMenu("UiAvatarCustomizationCategoryMenu");
		mEquipmentMenu = (UiJAEquipmentMenu)GetMenu("UiJAEquipmentMenu");
		mEquipmentBeltMenu = (UiJAEquipmentBeltMenu)GetMenu("UiJAEquipmentBeltMenu");
		foreach (EquipmentPartTab item2 in _EquipmentPartTab)
		{
			KAWidget kAWidget = DuplicateWidget("CatTemplateBtn");
			kAWidget.SetVisibility(inVisible: true);
			kAWidget.gameObject.name = item2._BtnName;
			kAWidget.SetTexture(item2._Icon, inPixelPerfect: true);
			kAWidget._TooltipInfo = item2._ToolTipInfo;
			mEquipmentCategoryMenu.AddWidget(kAWidget);
		}
		foreach (EquipmentBeltTab item3 in _EquipmentBeltTab)
		{
			KAWidget kAWidget2 = DuplicateWidget("BeltTemplateBtn");
			kAWidget2.SetVisibility(inVisible: true);
			EquipmentBeltData equipmentBeltData = new EquipmentBeltData();
			equipmentBeltData._Item = kAWidget2;
			equipmentBeltData._CategoryID = item3._SlotCategoryId;
			kAWidget2.SetUserData(equipmentBeltData);
			kAWidget2.gameObject.name = item3._CatName;
			mEquipmentBeltMenu.AddWidget(kAWidget2);
			UserItemData item = AvatarEquipment.pInstance.GetItem(item3._CatName);
			if (item != null)
			{
				equipmentBeltData.LoadIcon(item.Item);
			}
		}
		if (mEquipmentCategoryMenu.GetNumItems() > 0)
		{
			OnClick(mEquipmentCategoryMenu.GetItemAt(0));
		}
	}

	public void ItemDataEventHandler(int itemID, ItemData dataItem, object inUserData)
	{
		if (dataItem != null)
		{
			mItemsToLoadCnt--;
			mItemData.Add(dataItem);
			if (mItemsToLoadCnt == 0)
			{
				PopulateBeltItems();
			}
		}
	}

	private void PopulateBeltItems()
	{
		foreach (ItemData mItemDatum in mItemData)
		{
			for (int i = 0; i < mItemDatum.Category.Length; i++)
			{
				bool flag = false;
				foreach (EquipmentBeltTab item in _EquipmentBeltTab)
				{
					if (mItemDatum.Category[i].CategoryId == item._SlotCategoryId)
					{
						KAWidget kAWidget = mEquipmentBeltMenu.FindItem(item._CatName);
						if (kAWidget != null)
						{
							((EquipmentBeltData)kAWidget.GetUserData()).LoadIcon(mItemDatum);
						}
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
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		foreach (EquipmentPartTab item in _EquipmentPartTab)
		{
			if (inWidget.name == item._BtnName)
			{
				mCurrentTab = item;
				SelectTab(mCurrentTab._CategoryID);
				LocaleString text = inWidget._TooltipInfo._Text;
				if (mTxtCategory != null && !string.IsNullOrEmpty(text._Text))
				{
					mTxtCategory.SetTextByID(text._ID, text._Text);
				}
				break;
			}
		}
	}

	public string GetPartType(int cID)
	{
		return cID switch
		{
			387 => AvatarData.pPartSettings.AVATAR_PART_HAND_PROP_RIGHT, 
			409 => "Sickle", 
			_ => "", 
		};
	}

	public void SelectTab(int inTabCat)
	{
		if (!(mEquipmentMenu == null))
		{
			SetButtonDisabled(inTabCat);
			if (mEquipmentMenu._CurrentTab != null)
			{
				mEquipmentMenu._CurrentTab.OnSelected(t: false, this);
			}
			mEquipmentMenu.ChangeCategory(inTabCat, forceChange: false);
			KAUISelectAvatar._ResetAnim = true;
		}
	}

	private void SetButtonDisabled(int inCat)
	{
		string partType = GetPartType(inCat);
		if (string.IsNullOrEmpty(partType))
		{
			return;
		}
		for (int i = 0; i < mEquipmentCategoryMenu.GetNumItems(); i++)
		{
			if (mEquipmentCategoryMenu.FindItemAt(i).name.Contains(partType))
			{
				mEquipmentCategoryMenu.FindItemAt(i).SetDisabled(isDisabled: true);
			}
			else
			{
				mEquipmentCategoryMenu.FindItemAt(i).SetDisabled(isDisabled: false);
			}
		}
		for (int j = 0; j < mEquipmentBeltMenu.GetNumItems(); j++)
		{
			if (mEquipmentBeltMenu.FindItemAt(j).name.Contains(partType))
			{
				mEquipmentBeltMenu.FindItemAt(j).SetDisabled(isDisabled: true);
			}
			else
			{
				mEquipmentBeltMenu.FindItemAt(j).SetDisabled(isDisabled: false);
			}
		}
	}

	private void OnDisable()
	{
		if (mEquipmentMenu != null)
		{
			mEquipmentMenu.SaveSelection();
			AvatarEquipment.pInstance.Save();
		}
	}

	public void SetBeltItem(KAWidget inItem, string inPartName)
	{
		KAWidget kAWidget = mEquipmentBeltMenu.FindItem(inPartName);
		if (kAWidget != null)
		{
			KAWidget kAWidget2 = kAWidget.FindChildItem("Data");
			kAWidget2.SetTexture(inItem.GetTexture(), inPixelPerfect: true);
			kAWidget2.SetState(KAUIState.NOT_INTERACTIVE);
			kAWidget2.SetVisibility(inVisible: true);
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inItem.GetUserData();
			if (kAUISelectItemData != null)
			{
				((EquipmentBeltData)kAWidget.GetUserData())._ItemData = kAUISelectItemData._ItemData;
				AvatarEquipment.pInstance.EquipItem(inPartName, kAUISelectItemData._ItemID);
			}
		}
	}
}
