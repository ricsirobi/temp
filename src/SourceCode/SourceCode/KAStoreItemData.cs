using UnityEngine;

public class KAStoreItemData : StoreWidgetUserData
{
	public ItemData _ItemData;

	public int _StoreID;

	public Texture _DefaultTexture;

	private int mWH;

	public KAStoreItemData(string iconTex, string rVo, ItemData item, int inStoreID, int wh)
	{
		_ItemTextureData.Init(iconTex);
		_ItemRVOData.Init(rVo);
		_StoreID = inStoreID;
		_ItemData = item;
		mWH = wh;
	}

	public void CopyData(KAStoreItemData s)
	{
		_ItemData = s._ItemData;
		mWH = s.mWH;
	}

	public bool IsLocked()
	{
		if (_ItemData.Locked)
		{
			return !SubscriptionInfo.pIsMember;
		}
		return false;
	}

	public bool IsRankLocked(out int rid, int rankType)
	{
		rid = 0;
		if (_ItemData.RewardTypeID > 0)
		{
			rankType = _ItemData.RewardTypeID;
		}
		if (_ItemData.Points.HasValue && _ItemData.Points.Value > 0)
		{
			rid = _ItemData.Points.Value;
			UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(rankType);
			if (userAchievementInfoByType != null && userAchievementInfoByType.AchievementPointTotal.HasValue)
			{
				return rid > userAchievementInfoByType.AchievementPointTotal.Value;
			}
			return true;
		}
		if (_ItemData.RankId.HasValue && _ItemData.RankId.Value > 0)
		{
			rid = _ItemData.RankId.Value;
			UserRank userRank = ((rankType == 8) ? PetRankData.GetUserRank(SanctuaryManager.pCurPetData) : UserRankData.GetUserRankByType(rankType));
			if (userRank != null)
			{
				return rid > userRank.RankID;
			}
			return true;
		}
		return false;
	}

	public bool IsRankLocked(out int rid)
	{
		rid = 0;
		if (_ItemData.RankId.HasValue)
		{
			rid = _ItemData.RankId.Value;
		}
		return rid > UserRankData.pInstance.RankID;
	}

	public int GetPrereqItemIfNotInInventory()
	{
		if (_ItemData.Relationship == null)
		{
			return -1;
		}
		ItemDataRelationship[] relationship = _ItemData.Relationship;
		foreach (ItemDataRelationship itemDataRelationship in relationship)
		{
			if (!(itemDataRelationship.Type != "Prereq"))
			{
				if ((ParentData.pIsReady && ParentData.pInstance.HasItem(itemDataRelationship.ItemId)) || CommonInventoryData.pInstance.FindItem(itemDataRelationship.ItemId) != null)
				{
					return -1;
				}
				return itemDataRelationship.ItemId;
			}
		}
		return -1;
	}

	public override void OnAllDownloaded()
	{
		KAWidget item = GetItem();
		if (!(item == null) && (!(KAUIStoreCategory.pInstance == null) || !(IAPManager.pInstance == null)))
		{
			ShowLoadingItem(inShow: false);
			base.OnAllDownloaded();
			if (_ItemTextureData._Texture == null)
			{
				item.SetTexture(_DefaultTexture, mMakePixelPerfect);
			}
			item.SetState(KAUIState.NOT_INTERACTIVE);
		}
	}

	public override void ShowLoadingItem(bool inShow)
	{
		base.ShowLoadingItem(inShow);
		if (GetItem() != null)
		{
			KAWidget kAWidget = GetItem().FindChildItem("Loading");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inShow);
			}
		}
	}
}
