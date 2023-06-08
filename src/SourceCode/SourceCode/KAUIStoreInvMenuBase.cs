using UnityEngine;

public class KAUIStoreInvMenuBase : KAUIMenu
{
	public int _WHSize = 80;

	protected StoreFilter mFilter;

	protected int mCurrentTopIdx;

	public void AddInvMenuItem(string resName, string rvo, int quanty, int itemID)
	{
		KAWidget loadingItemStatic = CoBundleLoader.GetLoadingItemStatic(new KAStoreInventoryItemData(this, resName, rvo, quanty, _WHSize, itemID));
		AddWidget(loadingItemStatic);
		loadingItemStatic.SetState(KAUIState.NOT_INTERACTIVE);
	}

	public virtual void ChangeCategory(StoreFilter inFilter)
	{
		Input.ResetInputAxes();
		ChangeCategory(inFilter, forceChange: false);
	}

	public virtual void ChangeCategory(StoreFilter inFilter, bool forceChange)
	{
		if (inFilter != null && !inFilter.IsSame(mFilter))
		{
			mCurrentTopIdx = 0;
		}
		if (!forceChange && inFilter != null && inFilter.IsSame(mFilter))
		{
			return;
		}
		mFilter = inFilter;
		ClearItems();
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(mFilter._CategoryIDs);
		if (items != null)
		{
			UserItemData[] array = items;
			foreach (UserItemData userItemData in array)
			{
				if (!userItemData.Item.IsSubPart())
				{
					string rvo = "";
					if (userItemData.Item.Rollover != null)
					{
						rvo = userItemData.Item.Rollover.Bundle + "/" + userItemData.Item.Rollover.DialogName;
					}
					AddInvMenuItem(userItemData.Item.IconName, rvo, userItemData.Quantity, userItemData.Item.ItemID);
				}
			}
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
	}

	public override void SetTopItemIdx(int idx)
	{
		base.SetTopItemIdx(idx);
		mCurrentTopIdx = GetTopItemIdx();
	}

	public virtual void UpdateRange(int s, int e)
	{
		CoBundleLoader.SetVisibleRangeStatic(this, s, e);
	}
}
