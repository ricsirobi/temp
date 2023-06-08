using System;
using UnityEngine;

public class UiHighScoresMenu : KAUIMenu
{
	public GUISkin _TextSkin;

	public float _ScrollUpdateTime = 0.2f;

	private UiHighScores mParInterface;

	private GameData mRecentEntry;

	private float mPrevScrolledTime = 0.5f;

	private float mHoldStartTime;

	private bool mDisplayPlayerRankAsOne;

	public void SetMenuData(GameDataSummary dataSummary, string UserName)
	{
		mParInterface = (UiHighScores)_ParentUi;
		RemoveHighScores();
		if (dataSummary != null && dataSummary.GameDataList != null)
		{
			mDisplayPlayerRankAsOne = false;
			if (mParInterface.GetCurrentDisplayPage() == HighScoreDisplayPage.MYBUDDYSCORES && dataSummary.GameDataList.Length == 1)
			{
				mDisplayPlayerRankAsOne = true;
			}
			for (int i = 0; i < dataSummary.GameDataList.Length; i++)
			{
				GameData data = dataSummary.GameDataList[i];
				AddData(dataSummary.Key, data);
			}
			DisplayMyScoreBox(dataSummary);
		}
	}

	private void DisplayMyScoreBox(GameDataSummary dataSummary)
	{
		if (mParInterface.GetCurrentDisplayPage() == HighScoreDisplayPage.MYSCORES)
		{
			if (dataSummary.GameDataList.Length < 1)
			{
				return;
			}
			mRecentEntry = dataSummary.GameDataList[0];
			for (int i = 0; i < dataSummary.GameDataList.Length; i++)
			{
				if (DateTime.Compare(mRecentEntry.DatePlayed.Value, dataSummary.GameDataList[i].DatePlayed.Value) < 0)
				{
					mRecentEntry = dataSummary.GameDataList[i];
				}
			}
			if (mRecentEntry != null && mRecentEntry.RankID.HasValue)
			{
				int index = mRecentEntry.RankID.Value - 1;
				KAWidget kAWidget = mItemInfo[index].FindChildItem("MyScoreBox");
				if (kAWidget != null)
				{
					kAWidget.SetVisibility(inVisible: true);
				}
			}
		}
		else if ((mParInterface.GetCurrentDisplayPage() == HighScoreDisplayPage.ALLSCORES || mParInterface.GetCurrentDisplayPage() == HighScoreDisplayPage.MYBUDDYSCORES) && dataSummary.GameDataList.Length != 0 && dataSummary.UserPosition.HasValue && mItemInfo.Count > dataSummary.UserPosition.Value && UserInfo.pInstance != null)
		{
			KAWidget kAWidget2 = mItemInfo[dataSummary.UserPosition.Value].FindChildItem("MyScoreBox");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: true);
			}
		}
	}

	public void RemoveHighScores()
	{
		ClearItems();
		if (mParInterface != null)
		{
			KAWidget kAWidget = mParInterface.FindItem("TxtTopRank");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
	}

	public void AddData(string key, GameData data)
	{
		if (mParInterface == null)
		{
			Debug.LogError("Parent Interface in null");
			return;
		}
		KAWidget kAWidget = DuplicateWidget(_Template);
		kAWidget.SetVisibility(inVisible: true);
		if (data.RankID.HasValue)
		{
			kAWidget.SetText(data.RankID.ToString());
		}
		else if (mDisplayPlayerRankAsOne)
		{
			kAWidget.SetText("1");
		}
		else
		{
			kAWidget.SetText("---");
		}
		kAWidget.FindChildItem("TxtPlayerName").SetText(data.UserName);
		if (key == "time")
		{
			float time = (float)data.Value / 100f;
			kAWidget.FindChildItem("TxtPlayerScore").SetText(time.ToString());
			kAWidget.FindChildItem("TxtPlayerScore").SetText(GameUtilities.FormatTime(time));
		}
		else
		{
			kAWidget.FindChildItem("TxtPlayerScore").SetText(data.Value.ToString());
		}
		DateTime value = data.DatePlayed.Value;
		kAWidget.FindChildItem("TxtPlayerDate").SetText(value.Month + "/" + value.Day + "/" + value.Year);
		kAWidget.FindChildItem("MemberIcon").SetVisibility(data.IsMember);
		AddWidget(kAWidget);
	}

	public void OnScroll(int direction)
	{
		if (mHoldStartTime > 0.3f)
		{
			if (Time.realtimeSinceStartup - mPrevScrolledTime > _ScrollUpdateTime)
			{
				if (mDragPanel != null)
				{
					mDragPanel.Scroll(direction);
				}
				mPrevScrolledTime = Time.realtimeSinceStartup;
			}
		}
		else
		{
			mHoldStartTime += Time.fixedDeltaTime;
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if ((bool)mParInterface && item.name == "GotoTopBtn")
		{
			mCurrentGrid.Reposition();
		}
		if (item.name.Contains("Scroll"))
		{
			mHoldStartTime = 0f;
		}
	}

	public void AddBlankEntries(int numEntries)
	{
		for (int i = 0; i < numEntries; i++)
		{
			KAWidget kAWidget = DuplicateWidget(_Template);
			kAWidget.SetText("");
			kAWidget.FindChildItem("TxtPlayerName").SetText("");
			kAWidget.FindChildItem("TxtPlayerScore").SetText("");
			kAWidget.FindChildItem("TxtPlayerDate").SetText("");
			AddWidget(kAWidget);
		}
	}

	private string GetTimeString(int time)
	{
		int num = time / 60;
		int num2 = time % 60;
		return $"{num}:{num2:00}";
	}
}
