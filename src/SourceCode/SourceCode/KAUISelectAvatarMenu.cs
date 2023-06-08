using System;

public class KAUISelectAvatarMenu : KAUISelectMenu
{
	public int[] _BDayHats;

	public int _LegSelectMinRank;

	public int _TopSelectMinRank;

	public AvatarPartTab _CurrentTab;

	[NonSerialized]
	public bool mModified;

	private string mGenderFilter = "U";

	protected string mPartType = "";

	protected override void Awake()
	{
		base.Awake();
		base.pCategoryID = 0;
	}

	public override void Initialize(KAUI parentInt)
	{
		base.Initialize(parentInt);
	}

	public virtual void ChangeCategory(string pType, bool forceChange)
	{
		if (pType == "Hat" && UserInfo.IsBirthdayWeek() && _BDayHats.Length != 0)
		{
			_LoadItemIdList = _BDayHats;
		}
		else
		{
			_LoadItemIdList = null;
		}
		mPartType = pType;
		int categoryID = AvatarData.GetCategoryID(mPartType);
		ChangeCategory(categoryID, forceChange);
	}

	public override void ChangeCategory(int c, bool forceChange)
	{
		mGenderFilter = "U";
		if (AvatarData.GetGender() == Gender.Male)
		{
			mGenderFilter = "M";
		}
		else if (AvatarData.GetGender() == Gender.Female)
		{
			mGenderFilter = "F";
		}
		base.ChangeCategory(c, forceChange);
	}

	public override void ChangeCategory(int[] categories, bool forceChange)
	{
		mGenderFilter = "U";
		if (AvatarData.GetGender() == Gender.Male)
		{
			mGenderFilter = "M";
		}
		else if (AvatarData.GetGender() == Gender.Female)
		{
			mGenderFilter = "F";
		}
		base.ChangeCategory(categories, forceChange);
	}

	public override void FinishMenuItems(bool addParentItems = false)
	{
		base.FinishMenuItems();
		_CurrentTab.OnItemReady(mDBItemList.Count, mMainUI);
	}

	public override void AddInvMenuItem(ItemData item, int quantity)
	{
		if (CanAddItemToMenu(item))
		{
			base.AddInvMenuItem(item, quantity);
		}
	}

	public override void AddInvMenuItem(UserItemData item, int quantity)
	{
		if (CanAddItemToMenu(item.Item))
		{
			base.AddInvMenuItem(item, quantity);
		}
	}

	private bool CanAddItemToMenu(ItemData item)
	{
		if (mPartType == "Legs" && item.RankId < _LegSelectMinRank)
		{
			return false;
		}
		if (mPartType == "Top" && item.RankId < _TopSelectMinRank)
		{
			return false;
		}
		string attribute = item.GetAttribute("Gender", "U");
		if (attribute == mGenderFilter || attribute == "U" || mGenderFilter == "U")
		{
			return true;
		}
		return false;
	}

	protected virtual void EquipItem(KAWidget inWidget)
	{
		SelectItem(inWidget);
		mModified = true;
		_CurrentTab.ApplySelection(inWidget);
	}

	public override void SaveSelection()
	{
		base.SaveSelection();
		if (mModified || !AvatarData.pInitializedFromPreviousSave)
		{
			AvatarData.Save();
		}
	}

	public override bool AllowDuplicateItems()
	{
		if (_CurrentTab != null)
		{
			return _CurrentTab._DisplayDuplicateItems;
		}
		return false;
	}
}
