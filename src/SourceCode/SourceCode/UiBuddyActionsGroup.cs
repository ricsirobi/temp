public class UiBuddyActionsGroup : KAUI
{
	private KAWidget mFarmingBtn;

	private KAWidget mVisitBtn;

	private KAWidget mClanBtn;

	private KAWidget mInviteBtn;

	private KAWidget mHomeBtn;

	private KAWidget mMessagesBtn;

	private KAWidget mDeleteBtn;

	private KAWidget mAddBtn;

	private KAWidget mIgnoreBtn;

	public KAWidget CurrentSelectedBuddy { get; set; }

	public void InitItems()
	{
		mFarmingBtn = FindItem("FarmingBtn");
		mVisitBtn = FindItem("VisitBtn");
		mClanBtn = FindItem("ClanBtn");
		mInviteBtn = FindItem("InviteBtn");
		mMessagesBtn = FindItem("MessagesBtn");
		mDeleteBtn = FindItem("DeleteBtn");
		mAddBtn = FindItem("AddBtn");
		mIgnoreBtn = FindItem("IgnoreBtn");
		mHomeBtn = FindItem("HomeBtn");
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (!(item == null))
		{
			UiBuddyList.pInstance.BuddyListActions(item.name);
		}
	}

	public void SetActionAvailable(KAWidget selectedItem, UiBuddyList.BuddyListType type)
	{
		CurrentSelectedBuddy = selectedItem;
		bool flag = CurrentSelectedBuddy == null;
		bool flag2 = type == UiBuddyList.BuddyListType.ONLINE || type == UiBuddyList.BuddyListType.OFFLINE || type == UiBuddyList.BuddyListType.IGNORED;
		mFarmingBtn.SetVisibility(flag2);
		mFarmingBtn.SetDisabled(flag || type == UiBuddyList.BuddyListType.REQUEST || type == UiBuddyList.BuddyListType.IGNORED);
		mVisitBtn.SetVisibility(flag2);
		mVisitBtn.SetDisabled(flag || type != UiBuddyList.BuddyListType.ONLINE);
		mClanBtn.SetVisibility(flag2);
		mClanBtn.SetDisabled(isDisabled: true);
		mMessagesBtn.SetVisibility(flag2);
		mMessagesBtn.SetDisabled(flag || type == UiBuddyList.BuddyListType.REQUEST || type == UiBuddyList.BuddyListType.IGNORED);
		mInviteBtn.SetVisibility(flag2);
		mInviteBtn.SetDisabled(flag || type != UiBuddyList.BuddyListType.ONLINE);
		mHomeBtn.SetVisibility(flag2);
		mHomeBtn.SetDisabled(flag || type == UiBuddyList.BuddyListType.REQUEST || type == UiBuddyList.BuddyListType.IGNORED);
		mAddBtn.SetVisibility(type == UiBuddyList.BuddyListType.REQUEST);
		mAddBtn.SetDisabled(flag || type != UiBuddyList.BuddyListType.REQUEST);
		mIgnoreBtn.SetVisibility(flag2 || type == UiBuddyList.BuddyListType.REQUEST);
		mIgnoreBtn.SetDisabled(flag || type == UiBuddyList.BuddyListType.IGNORED);
		mDeleteBtn.SetVisibility(inVisible: true);
		mDeleteBtn.SetDisabled(flag);
	}

	public void SetClanActionAvailable(bool clan)
	{
		mClanBtn.SetDisabled(!clan);
	}
}
