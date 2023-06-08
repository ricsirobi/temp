using System;
using System.Collections.Generic;

public class UiJAEquipmentMenu : KAUISelectMenu
{
	[NonSerialized]
	public bool mModified;

	public AvatarPartTab _CurrentTab;

	private EquipmentPartTab mCurrentTab;

	public override void SelectItem(KAWidget inWidget)
	{
		base.SelectItem(inWidget);
		mModified = true;
		int num = -1;
		if (((UiJAEquipment)_ParentUi).pCurrentTab != null)
		{
			mCurrentTab = ((UiJAEquipment)_ParentUi).pCurrentTab;
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
			if (kAUISelectItemData == null)
			{
				UtDebug.Log("selected Item data is null");
				return;
			}
			if (kAUISelectItemData._ItemData != null && kAUISelectItemData._ItemData.Category != null && kAUISelectItemData._ItemData.Category.Length != 0)
			{
				for (int i = 0; i < kAUISelectItemData._ItemData.Category.Length; i++)
				{
					if (kAUISelectItemData._ItemData.Category[i].CategoryId == mCurrentTab._CategoryID)
					{
						num = mCurrentTab._CategoryID;
						break;
					}
					if (mCurrentTab._ChildCategoryIDs != null && mCurrentTab._ChildCategoryIDs.Count > 0 && mCurrentTab._ChildCategoryIDs.Contains(kAUISelectItemData._ItemData.Category[i].CategoryId))
					{
						num = kAUISelectItemData._ItemData.Category[i].CategoryId;
						break;
					}
				}
			}
		}
		if (num != -1)
		{
			string partType = ((UiJAEquipment)mMainUI).GetPartType(num);
			foreach (AvatarPartTab item in new List<AvatarPartTab>(((KAUISelectAvatar)_ParentUi)._Tabs))
			{
				if (partType.Equals(item._PrtTypeName))
				{
					_CurrentTab = item;
					break;
				}
			}
		}
		if (_CurrentTab != null)
		{
			((UiJAEquipment)_ParentUi).SetBeltItem(inWidget, _CurrentTab._PrtTypeName);
			_WHSize = _CurrentTab._WHSize;
			_CurrentTab.ApplySelection(inWidget);
		}
	}

	public override void SaveSelection()
	{
		base.SaveSelection();
		if (mModified || !AvatarData.pInitializedFromPreviousSave)
		{
			AvatarData.Save();
		}
	}
}
