using UnityEngine;

public class AnnouncementUserData : KAWidgetUserData
{
	public Announcement _Announcement;

	public string _Message;

	public string _MessageVOURL = "";

	public TaggedAnnouncementHelper _Helper;

	private AudioClip mVOClip;

	public AnnouncementUserData(Announcement announcement, string tag)
	{
		_Announcement = announcement;
		_Helper = new TaggedAnnouncementHelper(announcement.AnnouncementText);
		if (_Helper.Announcement.ContainsKey(tag))
		{
			_Message = _Helper.Announcement[tag];
		}
		else
		{
			_Message = announcement.AnnouncementText;
		}
		if (_Helper.Announcement.ContainsKey("VO"))
		{
			string text = _Helper.Announcement["VO"];
			if (!string.IsNullOrEmpty(text))
			{
				_MessageVOURL = text;
			}
		}
	}

	public void OnVOLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			mVOClip = (AudioClip)inObject;
			if (mVOClip != null && GetItem() != null && GetItem().GetVisibility())
			{
				PlayVO();
			}
			else
			{
				SnChannel.StopPool("VO_Pool");
			}
		}
	}

	public void PlayVO()
	{
		if (mVOClip == null)
		{
			if (_MessageVOURL.Length > 0)
			{
				RsResourceManager.Load(_MessageVOURL, OnVOLoaded, RsResourceType.NONSTREAMING_AUDIO);
			}
			else
			{
				SnChannel.StopPool("VO_Pool");
			}
		}
		else
		{
			SnChannel.Play(mVOClip, "VO_Pool", inForce: true);
		}
	}
}
