using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UiClansJoin : KAUI
{
	public KAUIMenu _Menu;

	public int _MaxClanCount = -1;

	public GameObject _MessageObject;

	public bool _IncludeOwnClan = true;

	private KAWidget mTxtClanName;

	private KAWidget mTxtMembers;

	private KAWidget mTxtSelect;

	private KAWidget mTxtTrophies;

	private KAWidget mTxtNoClansAvailable;

	private KAWidget mBtnNextPage;

	private KAWidget mBtnPrevPage;

	private bool mInitialized;

	private ClanOrderBY mOrderBy = ClanOrderBY.POINTS;

	private int mCurrentPageNo = 1;

	private int mPageSize = 100;

	private int mLastObtainedPageSize = 100;

	protected override void Start()
	{
		base.Start();
		mTxtClanName = FindItem("TxtClanName");
		mTxtMembers = FindItem("TxtMembers");
		mTxtSelect = FindItem("TxtSelect");
		mTxtTrophies = FindItem("TxtTrophies");
		mTxtNoClansAvailable = FindItem("TxtNoClansAvailable");
		mBtnNextPage = FindItem("BtnNextPage");
		mBtnPrevPage = FindItem("BtnPrevPage");
		if (mBtnNextPage != null)
		{
			mBtnNextPage.SetVisibility(inVisible: false);
		}
		if (mBtnPrevPage != null)
		{
			mBtnPrevPage.SetVisibility(inVisible: false);
		}
	}

	public void SetMaxClanCount(int maxClanCount)
	{
		if (maxClanCount != _MaxClanCount)
		{
			_MaxClanCount = maxClanCount;
			mInitialized = false;
		}
	}

	public void SetOrderBy(ClanOrderBY inOrderBy)
	{
		if (mOrderBy != inOrderBy)
		{
			mOrderBy = inOrderBy;
			mInitialized = false;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!mInitialized && Group.pIsReady)
		{
			mInitialized = true;
			StopAllCoroutines();
			PopulateMenu(Group.pTopGroups);
			if (mBtnPrevPage != null)
			{
				mBtnPrevPage.SetVisibility(inVisible: false);
			}
			if (mBtnNextPage != null && Group.pTopGroups != null)
			{
				mBtnNextPage.SetVisibility(Group.pTopGroups.Count >= mPageSize);
			}
		}
	}

	private void PopulateMenu(List<Group> inGroups)
	{
		if (inGroups == null || inGroups.Count == 0)
		{
			mTxtNoClansAvailable.SetVisibility(inVisible: true);
		}
		inGroups = ((mOrderBy == ClanOrderBY.MEMBER_COUNT) ? inGroups.OrderByDescending((Group o) => o.TotalMemberCount).ToList() : ((mOrderBy != ClanOrderBY.POINTS) ? inGroups.OrderBy((Group o) => o.Name).ToList() : inGroups.OrderByDescending((Group o) => o.Points).ToList()));
		mTxtClanName.SetVisibility(inVisible: true);
		mTxtMembers.SetVisibility(inVisible: true);
		mTxtSelect.SetVisibility(inVisible: true);
		mTxtTrophies.SetVisibility(inVisible: true);
		mTxtNoClansAvailable.SetVisibility(inVisible: false);
		if (_MaxClanCount != -1)
		{
			inGroups.RemoveRange(_MaxClanCount, inGroups.Count - _MaxClanCount);
		}
		_Menu.ClearItems();
		foreach (Group inGroup in inGroups)
		{
			if (inGroup.TotalMemberCount.HasValue && inGroup.TotalMemberCount.Value != 0 && (_IncludeOwnClan || !UserProfile.pProfileData.InGroup(inGroup.GroupID)))
			{
				ClanData clanData = new ClanData(inGroup);
				clanData._Group = inGroup;
				_Menu.AddWidget(inGroup.Name, clanData);
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mTxtClanName)
		{
			SetOrderBy(ClanOrderBY.NAME);
		}
		else if (inWidget == mTxtMembers)
		{
			SetOrderBy(ClanOrderBY.MEMBER_COUNT);
		}
		else if (inWidget == mTxtTrophies)
		{
			SetOrderBy(ClanOrderBY.POINTS);
		}
		else if (inWidget == mBtnPrevPage)
		{
			if (mCurrentPageNo > 1)
			{
				mCurrentPageNo--;
				Group.GetTopList(mCurrentPageNo, mPageSize, null, includeMemberCount: true, OnGroupTopListEventHandler);
				SetInteractive(interactive: false);
				UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
				mBtnPrevPage.SetVisibility(mCurrentPageNo > 1);
			}
		}
		else if (inWidget == mBtnNextPage && mLastObtainedPageSize == mPageSize)
		{
			mCurrentPageNo++;
			Group.GetTopList(mCurrentPageNo, mPageSize, null, includeMemberCount: true, OnGroupTopListEventHandler);
			SetInteractive(interactive: false);
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			mBtnPrevPage.SetVisibility(inVisible: true);
		}
		ClanData clanData = (ClanData)inWidget.GetUserData();
		if (clanData != null)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage("OnClanSelected", clanData, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				ShowClanDetails(show: true, clanData._Group);
			}
		}
	}

	public void ShowClanDetails(bool show, Group inGroup = null)
	{
		if (UiClans.pInstance != null)
		{
			UiClans.pInstance.ShowBackBtn(show, base.gameObject);
			UiClans.pInstance._UiClansDetails.Show(show, inGroup);
		}
		SetVisibility(!show);
		_Menu.UpdateScrollbars(!show);
		_Menu.gameObject.SetActive(!show);
	}

	private void OnBackBtnClicked()
	{
		ShowClanDetails(show: false);
	}

	public void Show(bool show)
	{
		base.gameObject.SetActive(show);
		if (show)
		{
			SetVisibility(inVisible: true);
			_Menu.gameObject.SetActive(value: true);
			_Menu.UpdateScrollbars(inVisible: true);
		}
	}

	public void OnGroupTopListEventHandler(List<Group> groups)
	{
		if (mBtnNextPage != null)
		{
			mBtnNextPage.SetVisibility(groups != null && groups.Count >= mPageSize);
		}
		if (groups != null)
		{
			mLastObtainedPageSize = groups.Count;
			if (groups.Count > 0)
			{
				PopulateMenu(groups);
			}
		}
		SetInteractive(interactive: true);
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: false);
	}
}
