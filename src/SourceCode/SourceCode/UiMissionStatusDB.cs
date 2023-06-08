using System;
using UnityEngine;

public class UiMissionStatusDB : KAUI
{
	private KAWidget mTitleTxt;

	private KAWidget mStatusTxt;

	private float mTimeToShow;

	private static UiMissionStatusDB mInstance;

	private static Action mAction;

	protected override void Start()
	{
		base.Start();
		mTitleTxt = FindItem("TxtTitle");
		mStatusTxt = FindItem("TxtStatus");
	}

	protected override void Update()
	{
		base.Update();
		if (mAction != null)
		{
			mAction();
			mAction = null;
		}
		if (mTimeToShow > 0f)
		{
			mTimeToShow -= Time.deltaTime;
			if (mTimeToShow <= 0f || AvAvatar.pToolbar == null || !AvAvatar.pToolbar.activeSelf)
			{
				mTimeToShow = 0f;
				RemoveDB();
			}
		}
	}

	public static void RemoveDB()
	{
		if (mInstance != null)
		{
			UnityEngine.Object.Destroy(mInstance.gameObject);
			mInstance = null;
		}
	}

	public void ShowDB(string titleText, string statusText, float timeToShow)
	{
		mTimeToShow = timeToShow;
		mTitleTxt.SetText(titleText);
		mStatusTxt.SetText(statusText);
		SetVisibility(inVisible: true);
	}

	public static void Show(string titleText, string statusText, float timeToShow = 3f)
	{
		if (mInstance == null)
		{
			mInstance = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiMissionStatusDB")).GetComponent<UiMissionStatusDB>();
		}
		mAction = (Action)Delegate.Combine(mAction, (Action)delegate
		{
			mInstance.ShowDB(titleText, statusText, timeToShow);
		});
	}
}
