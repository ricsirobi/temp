using UnityEngine;

public class FarmItemBase : MyRoomItem
{
	public int _StatusInterval = 10;

	public FarmItemContextData _BuildModeObjectCreationContextData;

	public Vector2 _GridCellCount = Vector2.one;

	public bool _StampOnGrids = true;

	public Vector3 _3DCSM_UI_ItemScale_Factor = new Vector3(3f, 3f, 3f);

	protected FarmItem mParent;

	protected bool mIsDeserialized;

	private FarmManager mFarmManager;

	private bool mShowStatus;

	protected static bool pIsBundleLoading;

	public FarmItem pParent => mParent;

	public FarmManager pFarmManager
	{
		get
		{
			return mFarmManager;
		}
		set
		{
			mFarmManager = value;
		}
	}

	public bool pShowStatus
	{
		get
		{
			return mShowStatus;
		}
		set
		{
			mShowStatus = value;
		}
	}

	protected virtual void HideStatus()
	{
		pShowStatus = false;
	}

	protected void AddChildContextDataToParent(ContextData parentContextData, ItemData childItemData, bool inShowInventoryCount = false, string inUseTexturePath = null)
	{
		ContextData contextData = GetContextData(childItemData.ItemName);
		if (contextData == null)
		{
			contextData = new ContextData();
		}
		contextData._Name = childItemData.ItemName;
		contextData._DisplayName = new LocaleString("");
		contextData._ToolTipText = new LocaleString(childItemData.ItemName);
		if (inShowInventoryCount && CommonInventoryData.pInstance != null)
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(childItemData.ItemID);
			if (userItemData != null)
			{
				contextData._DisplayName = new LocaleString(userItemData.Quantity.ToString());
			}
		}
		contextData._DeactivateOnClick = true;
		if (string.IsNullOrEmpty(inUseTexturePath))
		{
			contextData._IconSpriteName = childItemData.IconName.Split('/')[^1];
		}
		else
		{
			contextData._IconSpriteName = inUseTexturePath;
		}
		contextData._2DScaleInPixels = parentContextData._2DScaleInPixels;
		contextData._BackgroundColor = parentContextData._BackgroundColor;
		if (!_DataList.Contains(contextData))
		{
			AddIntoDataList(contextData);
		}
	}

	protected override bool CanActivate()
	{
		if (!MyRoomsIntMain.pDisableBuildMode)
		{
			return !pIsBundleLoading;
		}
		return false;
	}
}
