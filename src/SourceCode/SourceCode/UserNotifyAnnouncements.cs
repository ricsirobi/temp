using UnityEngine;

public class UserNotifyAnnouncements : UserNotify
{
	public string _AnnouncementAssetName;

	public string[] _FirstTimeTutorialName;

	private GameObject mAnnouncementObj;

	private static bool mDoneOnce;

	public override void OnWaitBeginImpl()
	{
		if (RsResourceManager.pLevelLoading)
		{
			return;
		}
		bool flag = false;
		string[] firstTimeTutorialName = _FirstTimeTutorialName;
		for (int i = 0; i < firstTimeTutorialName.Length; i++)
		{
			if (ProductData.TutorialComplete(firstTimeTutorialName[i]))
			{
				flag = true;
				break;
			}
		}
		if (flag && !mDoneOnce)
		{
			mDoneOnce = true;
			mAnnouncementObj = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources(_AnnouncementAssetName));
			UiAnnouncements component = mAnnouncementObj.GetComponent<UiAnnouncements>();
			component.SetVisibility(inVisible: false);
			component.SetInteractive(interactive: false);
			component._NotifyObject = this;
		}
		else
		{
			OnAnnouncementDone();
		}
	}

	public void OnAnnouncementDone()
	{
		if (mAnnouncementObj != null)
		{
			Object.Destroy(mAnnouncementObj);
		}
		OnWaitEnd();
	}

	public static void Reset()
	{
		mDoneOnce = false;
	}
}
