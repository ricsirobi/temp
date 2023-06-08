using System.Collections.Generic;

public class UiJournalAnnouncementsMenu : KAUIMenu
{
	private UiJournalAnnouncements mUiJournalAnnouncements;

	private List<AnnouncementUserData> mAnnouncementsUserData = new List<AnnouncementUserData>();

	public UiJournalAnnouncements _UiJournalAnnouncements;

	public void PopulateMenu(List<AnnouncementUserData> announcements)
	{
		mAnnouncementsUserData = announcements;
		ShowAnnouncements();
	}

	private void ShowAnnouncements()
	{
		foreach (AnnouncementUserData mAnnouncementsUserDatum in mAnnouncementsUserData)
		{
			Announcement announcement = mAnnouncementsUserDatum._Announcement;
			if (announcement.Type == AnnouncementType.VoiceOver || !_UiJournalAnnouncements.CheckMessageHasToShow(announcement))
			{
				continue;
			}
			TaggedAnnouncementHelper helper = mAnnouncementsUserDatum._Helper;
			KAWidget kAWidget = AddWidget(_Template.name, null);
			KAWidget kAWidget2 = kAWidget.FindChildItem("AniIcon");
			KAWidget kAWidget3 = kAWidget.FindChildItem("TxtTitle");
			kAWidget3.SetText(announcement.Description);
			kAWidget3.SetVisibility(inVisible: true);
			KAWidget kAWidget4 = kAWidget.FindChildItem("TxtMessage");
			KAWidget kAWidget5 = kAWidget.FindChildItem("BtnGoNow");
			KAWidget kAWidget6 = kAWidget.FindChildItem("IconNew");
			if (kAWidget6 != null)
			{
				if (_UiJournalAnnouncements.CheckMessageNew(announcement))
				{
					kAWidget6.SetVisibility(inVisible: true);
				}
				else
				{
					kAWidget6.SetVisibility(inVisible: false);
				}
			}
			if (kAWidget5 != null && (helper.Announcement.ContainsKey("Scene") || helper.Announcement.ContainsKey("Store")))
			{
				kAWidget5.SetVisibility(inVisible: true);
			}
			if (kAWidget4 != null && helper.Announcement.ContainsKey("Message"))
			{
				kAWidget4.SetText(helper.Announcement["Message"]);
			}
			if (kAWidget2 != null && helper.Announcement.ContainsKey("Image"))
			{
				kAWidget2.SetTextureFromURL(helper.Announcement["Image"]);
				kAWidget2.SetVisibility(inVisible: true);
			}
			kAWidget.SetUserData(mAnnouncementsUserDatum);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnGoNow")
		{
			AnnouncementUserData userData = (AnnouncementUserData)inWidget.GetParentItem().GetUserData();
			_UiJournalAnnouncements.DoMessageAction(userData);
		}
	}
}
