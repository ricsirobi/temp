using System;
using UnityEngine;

public class UiAnnouncements : KAUI
{
	public int _WorldID;

	public UserNotifyAnnouncements _NotifyObject;

	private KAWidget mTxtAnnouncement;

	private KAWidget mBtnOK;

	private KAWidget mBtnYes;

	private KAWidget mBtnNo;

	private KAWidget mImage;

	private string mPrefKeyName = "";

	private int mCurrentMessage;

	private Announcement[] mAnnouncements;

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	public bool IsValidFrequency(Announcement inAnnouncement)
	{
		TaggedAnnouncementHelper taggedAnnouncementHelper = new TaggedAnnouncementHelper(inAnnouncement.AnnouncementText);
		if (taggedAnnouncementHelper.Announcement.ContainsKey("Frequency"))
		{
			int result = -1;
			if (int.TryParse(taggedAnnouncementHelper.Announcement["Frequency"], out result))
			{
				if (inAnnouncement._Frequency > result)
				{
					inAnnouncement._Frequency = 1;
					return true;
				}
				inAnnouncement._Frequency++;
				return false;
			}
		}
		return false;
	}

	public bool CheckMessageUnread(Announcement inAnnouncement)
	{
		string @string = PlayerPrefs.GetString(GetKeyName(inAnnouncement), string.Empty);
		if (@string == string.Empty)
		{
			inAnnouncement._Frequency = 1;
			return true;
		}
		DateTime minValue = DateTime.MinValue;
		minValue = DateTime.Parse(@string, UtUtilities.GetCultureInfo("en-US"));
		UtDebug.Log(GetKeyName(inAnnouncement) + " :: " + minValue);
		if (minValue > inAnnouncement.StartDate)
		{
			DateTime value = minValue;
			DateTime? endDate = inAnnouncement.EndDate;
			if (value < endDate)
			{
				return IsValidFrequency(inAnnouncement);
			}
		}
		inAnnouncement._Frequency = 1;
		return true;
	}

	public void AListCallback(AnnouncementList aList, object inUserData)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (aList != null && aList.Announcements != null && aList.Announcements.Length != 0)
		{
			mAnnouncements = aList.Announcements;
			SetAnnouncementFrequency();
			ShowAnnouncements();
		}
		else
		{
			Done();
		}
	}

	private void SetAnnouncementFrequency()
	{
		string @string = PlayerPrefs.GetString(mPrefKeyName + "Frequency", string.Empty);
		if (string.IsNullOrEmpty(@string))
		{
			return;
		}
		string[] array = @string.Split('|');
		if (array == null || array.Length == 0)
		{
			return;
		}
		for (int i = 0; i < array.Length - 1; i += 2)
		{
			int result = -1;
			int result2 = -1;
			if (!int.TryParse(array[i], out result) || !int.TryParse(array[i + 1], out result2))
			{
				continue;
			}
			Announcement[] array2 = mAnnouncements;
			foreach (Announcement announcement in array2)
			{
				if (result == announcement.AnnouncementID)
				{
					announcement._Frequency = result2;
					break;
				}
			}
		}
	}

	public void Initialize()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		mTxtAnnouncement = FindItem("TxtDialog");
		mBtnOK = FindItem("OKBtn");
		mBtnYes = FindItem("YesBtn");
		mBtnNo = FindItem("NoBtn");
		mImage = FindItem("AniImage");
		if (UtPlatform.IsiOS() && mBtnYes != null && mBtnNo != null)
		{
			Vector3 localPosition = mBtnNo.transform.localPosition;
			mBtnNo.transform.localPosition = mBtnYes.transform.localPosition;
			mBtnYes.transform.localPosition = localPosition;
		}
		if (UserInfo.pIsReady)
		{
			mPrefKeyName = UserInfo.pInstance.UserID + "READ_MSGID";
		}
		else
		{
			UtDebug.LogWarning("UserInfo not ready");
			mPrefKeyName = "READ_MSGID";
		}
		if (_WorldID > 0)
		{
			Announcement.Init(_WorldID, AListCallback, null);
		}
		else
		{
			Debug.LogError("No WorldID defined!!");
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnNo || inWidget == mBtnOK)
		{
			UpdateAnnouncement();
		}
		else
		{
			if (!(inWidget == mBtnYes))
			{
				return;
			}
			TaggedAnnouncementHelper taggedAnnouncementHelper = new TaggedAnnouncementHelper(mAnnouncements[mCurrentMessage].AnnouncementText);
			UpdateAnnouncement();
			if (!taggedAnnouncementHelper.Announcement.ContainsKey("Action") || !taggedAnnouncementHelper.Announcement["Action"].Equals("Load") || !taggedAnnouncementHelper.Announcement.ContainsKey("Data"))
			{
				return;
			}
			string[] array = taggedAnnouncementHelper.Announcement["Data"].Split(',');
			if (array != null && array.Length > 1)
			{
				string text = array[0];
				string text2 = array[1];
				if (text.Equals(RsResourceManager.pCurrentLevel))
				{
					AvAvatar.TeleportToObject(text2);
					return;
				}
				AvAvatar.pStartLocation = text2;
				RsResourceManager.LoadLevel(text);
			}
		}
	}

	public void UpdateAnnouncement()
	{
		MarkMessageRead();
		KAInput.ResetInputAxes();
		mCurrentMessage++;
		CheckAnnouncementsDone();
	}

	public void MarkMessageRead()
	{
		string text = ServerTime.pCurrentTime.ToString(UtUtilities.GetCultureInfo("en-US"));
		UtDebug.Log(GetKeyName(mAnnouncements[mCurrentMessage]) + " :: " + text);
		PlayerPrefs.SetString(GetKeyName(mAnnouncements[mCurrentMessage]), text);
	}

	private bool CheckMessageExpired(Announcement inAnnouncement)
	{
		if (inAnnouncement.EndDate.HasValue)
		{
			return ServerTime.pCurrentTime.Subtract(inAnnouncement.EndDate.Value).Ticks > 0;
		}
		return false;
	}

	private void ShowAnnouncements()
	{
		Announcement announcement = mAnnouncements[mCurrentMessage];
		if (announcement.Type == AnnouncementType.VoiceOver)
		{
			UtDebug.LogError("AnnouncementType.VoiceOver is not supported now");
			mCurrentMessage++;
			CheckAnnouncementsDone();
		}
		else if (CheckMessageExpired(announcement))
		{
			mCurrentMessage++;
			CheckAnnouncementsDone();
		}
		else if (CheckMessageUnread(announcement))
		{
			if (!GetVisibility())
			{
				SetVisibility(inVisible: true);
			}
			if (GetState() != 0)
			{
				SetInteractive(interactive: true);
			}
			TaggedAnnouncementHelper taggedAnnouncementHelper = new TaggedAnnouncementHelper(announcement.AnnouncementText);
			if (mTxtAnnouncement != null)
			{
				mTxtAnnouncement.SetText(taggedAnnouncementHelper.Announcement["Message"]);
			}
			if (mImage != null && taggedAnnouncementHelper.Announcement.ContainsKey("Image"))
			{
				mImage.SetTextureFromURL(taggedAnnouncementHelper.Announcement["Image"]);
			}
			if (taggedAnnouncementHelper.Announcement.ContainsKey("Action"))
			{
				if (mBtnYes != null)
				{
					mBtnYes.SetVisibility(inVisible: true);
				}
				if (mBtnNo != null)
				{
					mBtnNo.SetVisibility(inVisible: true);
				}
			}
			else if (mBtnOK != null)
			{
				mBtnOK.SetVisibility(inVisible: true);
			}
		}
		else
		{
			mCurrentMessage++;
			CheckAnnouncementsDone();
		}
	}

	private void Done()
	{
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		if (_NotifyObject != null)
		{
			_NotifyObject.OnAnnouncementDone();
		}
		if (mAnnouncements != null)
		{
			SaveFrequencyData();
		}
	}

	private void SaveFrequencyData()
	{
		string text = "";
		Announcement[] array = mAnnouncements;
		foreach (Announcement announcement in array)
		{
			if (announcement != null)
			{
				text = text + announcement.AnnouncementID + "|" + announcement._Frequency + "|";
			}
		}
		PlayerPrefs.SetString(mPrefKeyName + "Frequency", text);
	}

	private void CheckAnnouncementsDone()
	{
		if (mCurrentMessage >= mAnnouncements.Length)
		{
			Done();
		}
		else
		{
			ShowAnnouncements();
		}
	}

	private string GetKeyName(Announcement an)
	{
		return mPrefKeyName + "|" + an.AnnouncementID;
	}
}
