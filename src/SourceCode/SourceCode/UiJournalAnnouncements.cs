using System.Collections.Generic;
using UnityEngine;

public class UiJournalAnnouncements : KAUI, IJournal
{
	public int _JournalNewsWorldID = 100;

	public string _LoadFromXML = "RS_DATA/AnnouncementsDO.xml";

	public KAWidget _NoAnnouncementTxt;

	private bool mBusy;

	private bool mInitialize;

	private bool mDataDirty;

	private AnnouncementUserData mActionUserData;

	private List<AnnouncementUserData> mAnnouncementsUserData;

	private UiJournalAnnouncementsMenu mAnnouncementMenu;

	protected override void Start()
	{
		base.Start();
		mAnnouncementMenu = base.transform.GetComponentInChildren<UiJournalAnnouncementsMenu>();
		KAUICursorManager.SetDefaultCursor("Loading");
		if (_JournalNewsWorldID > 0)
		{
			Announcement.Init(_JournalNewsWorldID, AchievementListCallback, null, _LoadFromXML);
		}
		else
		{
			Debug.LogError("No WorldID defined!!");
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!mInitialize && mAnnouncementsUserData != null && AnnouncementData.pInstance.pIsReady)
		{
			Initialize();
		}
	}

	public void AchievementListCallback(AnnouncementList aList, object inUserData)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (aList != null && aList.Announcements != null && aList.Announcements.Length != 0)
		{
			mAnnouncementsUserData = new List<AnnouncementUserData>();
			Announcement[] announcements = aList.Announcements;
			for (int i = 0; i < announcements.Length; i++)
			{
				AnnouncementUserData item = new AnnouncementUserData(announcements[i], "Message");
				mAnnouncementsUserData.Add(item);
			}
		}
		else if (_NoAnnouncementTxt != null)
		{
			_NoAnnouncementTxt.SetVisibility(inVisible: true);
		}
	}

	public void Initialize()
	{
		mInitialize = true;
		mAnnouncementMenu.PopulateMenu(mAnnouncementsUserData);
		if (GetVisibility())
		{
			MarkAllAsRead();
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible)
		{
			MarkAllAsRead();
		}
	}

	private void MarkAllAsRead()
	{
		if (mAnnouncementsUserData == null)
		{
			return;
		}
		foreach (AnnouncementUserData mAnnouncementsUserDatum in mAnnouncementsUserData)
		{
			mDataDirty = true;
			AnnouncementData.pInstance.MarkAsRead(mAnnouncementsUserDatum._Announcement.AnnouncementID);
		}
	}

	public void DoMessageAction(AnnouncementUserData userData)
	{
		if (userData != null)
		{
			MarkMessageVisited(userData._Announcement);
			mActionUserData = userData;
			SaveData();
		}
	}

	public void SaveData()
	{
		mDataDirty = false;
		mBusy = true;
		AnnouncementData.pInstance.Save(AnnouncementSaveDone);
	}

	public bool CheckMessageNew(Announcement an)
	{
		return !AnnouncementData.pInstance.IsRead(an.AnnouncementID);
	}

	public void MarkMessageRead(Announcement an)
	{
		mDataDirty = true;
		AnnouncementData.pInstance.MarkAsRead(an.AnnouncementID);
	}

	public void MarkMessageVisited(Announcement an)
	{
		mDataDirty = true;
		AnnouncementData.pInstance.MarkAsVisited(an.AnnouncementID);
	}

	public bool CheckMessageHasToShow(Announcement an)
	{
		return !CheckMessageExpired(an);
	}

	public bool CheckMessageExpired(Announcement an)
	{
		if (an.EndDate.HasValue)
		{
			return ServerTime.pCurrentTime.Subtract(an.EndDate.Value).Ticks > 0;
		}
		return false;
	}

	public void Exit()
	{
		if (mDataDirty)
		{
			SaveData();
		}
	}

	private void AnnouncementSaveDone(bool success)
	{
		mBusy = false;
		if (mActionUserData == null)
		{
			return;
		}
		if (mActionUserData._Helper.Announcement.ContainsKey("Scene"))
		{
			string text = mActionUserData._Helper.Announcement["Scene"];
			if (!string.IsNullOrEmpty(text))
			{
				UiJournal.pInstance.GoToScene(text);
			}
		}
		else if (mActionUserData._Helper.Announcement.ContainsKey("Store"))
		{
			string text2 = mActionUserData._Helper.Announcement["Store"];
			if (!string.IsNullOrEmpty(text2))
			{
				string[] array = text2.Split(',');
				if (array.Length >= 1)
				{
					string store = array[0];
					string category = string.Empty;
					if (array.Length >= 2)
					{
						category = array[1];
					}
					UiJournal.pInstance.PopUpStoreUI(store, category, "NewsBtn");
				}
			}
		}
		mActionUserData = null;
	}

	public void Clear()
	{
	}

	public void ProcessClose()
	{
	}

	public void ActivateUI(int uiIndex, bool isActive = true)
	{
	}

	public bool IsBusy()
	{
		return mBusy;
	}
}
