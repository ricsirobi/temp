using System.Collections.Generic;
using UnityEngine;

public class UiClansSearch : KAUI
{
	public LocaleString _SearchingResultsStatusText;

	public LocaleString _DisplayingResultsStatusText;

	public LocaleString _NoResultsFoundText;

	public int _MinSearchTextLength = 3;

	public KAUIMenu _Menu;

	public string _SearchBoxName = "TxtEditClanName";

	public bool _UseDynamicSearch;

	public bool _DisplayAllClansByDefault;

	public GameObject _MessageObject;

	public bool _IncludeOwnClan = true;

	public int _ClanSearchCount = 100;

	public bool _CaseSensitive;

	private bool mSearchInProcess;

	private bool mSearchPending;

	private string mSearchPendingString;

	private KAWidget mBtnSearch;

	private KAWidget mTxtSearchStatus;

	private KAEditBox mTxtEditClanName;

	private string mCachedLabelText = "";

	private bool mInitialized;

	private string mCachedSearchText;

	protected override void Start()
	{
		base.Start();
		mBtnSearch = FindItem("BtnSearch");
		mTxtSearchStatus = FindItem("TxtSearchStatus");
		mTxtEditClanName = (KAEditBox)FindItem(_SearchBoxName);
		mTxtEditClanName.pInput.isSelected = true;
		if (mTxtSearchStatus != null)
		{
			mTxtSearchStatus.SetVisibility(inVisible: true);
			mCachedLabelText = StringTable.GetStringData(mTxtSearchStatus.GetLabel().textID, mTxtSearchStatus.GetText());
		}
		if (mBtnSearch != null)
		{
			mBtnSearch.SetDisabled(isDisabled: true);
		}
	}

	public override void OnInput(KAWidget inWidget, string inText)
	{
		base.OnInput(inWidget, inText);
		if (!(inWidget == mTxtEditClanName))
		{
			return;
		}
		string text = mTxtEditClanName.GetText();
		string text2 = text.Trim();
		bool flag = !string.Equals(mCachedSearchText, text);
		mCachedSearchText = text;
		if (_UseDynamicSearch)
		{
			if (flag && text.Length >= _MinSearchTextLength)
			{
				SearchInClans(mTxtEditClanName.GetText());
			}
			return;
		}
		bool flag2 = text2.Length < 1 || text.Length < _MinSearchTextLength || text == mTxtEditClanName._DefaultText.GetLocalizedString();
		if (_DisplayAllClansByDefault)
		{
			flag2 = false;
		}
		if (mBtnSearch != null)
		{
			mBtnSearch.SetDisabled(flag2);
		}
		if (!flag2)
		{
			foreach (char c in inText)
			{
				if (c == '\r' || c == '\n')
				{
					SearchInClans(mTxtEditClanName.GetText());
					break;
				}
			}
		}
		else if (!string.IsNullOrEmpty(mCachedLabelText) && mTxtSearchStatus != null)
		{
			mTxtSearchStatus.SetText(mCachedLabelText);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnSearch)
		{
			SearchInClans(mTxtEditClanName.GetText());
			return;
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

	private void SearchInClans(string searchString)
	{
		if (!mSearchInProcess)
		{
			mSearchInProcess = true;
			mSearchPending = false;
			Group.Search(searchString, FilterSearchInClans, _ClanSearchCount, _CaseSensitive);
		}
		else
		{
			mSearchPendingString = searchString;
			mSearchPending = true;
		}
	}

	private void FilterSearchInClans(List<Group> inGroups, string searchString)
	{
		mSearchInProcess = true;
		_Menu.ClearItems();
		_Menu.UpdateScrollbars(inVisible: false);
		if (inGroups != null)
		{
			List<Group> list = null;
			list = ((!_DisplayAllClansByDefault || (!string.IsNullOrEmpty(searchString) && !(searchString == mTxtEditClanName._DefaultText.GetLocalizedString()))) ? inGroups : new List<Group>(inGroups));
			foreach (Group item in list)
			{
				AddSearchResult(item);
			}
		}
		if (_Menu.GetItemCount() > 0)
		{
			_Menu.pViewChanged = true;
			if (mTxtSearchStatus != null)
			{
				mTxtSearchStatus.SetText(_DisplayingResultsStatusText.GetLocalizedString().Replace("[Fred]", searchString));
			}
		}
		else if (mTxtSearchStatus != null)
		{
			mTxtSearchStatus.SetText(_NoResultsFoundText.GetLocalizedString().Replace("[Fred]", searchString));
		}
		mSearchInProcess = false;
	}

	public void AddSearchResult(Group inGroup)
	{
		if (inGroup.TotalMemberCount.HasValue && inGroup.TotalMemberCount.Value != 0 && (_IncludeOwnClan || !UserProfile.pProfileData.InGroup(inGroup.GroupID)))
		{
			ClanData userData = new ClanData(inGroup);
			_Menu.AddWidget(inGroup.Name, userData);
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
		_Menu.SetVisibility(!show);
	}

	private void OnBackBtnClicked()
	{
		ShowClanDetails(show: false);
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		if (!GetVisibility())
		{
			SetVisibility(inVisible: true);
			_Menu.SetVisibility(inVisible: true);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!mInitialized && Group.pIsReady)
		{
			mInitialized = true;
			if (_DisplayAllClansByDefault)
			{
				FilterSearchInClans(Group.pTopGroups, null);
			}
		}
		if (mSearchPending && !mSearchInProcess)
		{
			SearchInClans(mSearchPendingString);
		}
	}
}
