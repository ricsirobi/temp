using UnityEngine;

public class UiCogsGameEndDB : UiDragonsEndDB
{
	public LocaleString _TimeText = new LocaleString("Time");

	public LocaleString _MovesText = new LocaleString("Moves");

	public LocaleString _GameScoreText = new LocaleString("Score");

	public void SetResultData(int moves, float time, int levelStars, int totalScore)
	{
		bool flag = CogsLevelManager.pInstance.GetNextLevel() == -1;
		KAWidget kAWidget = FindItem("NextBtn");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(!flag);
		}
		if (!(mResultUI != null))
		{
			return;
		}
		int num = (int)(time / 60f);
		int num2 = (int)(time % 60f);
		string txtValue = $"{num:00}" + ":" + $"{num2:00}";
		SetResultData("TimeUsed", StringTable.GetStringData(_TimeText._ID, _TimeText._Text) + ": ", txtValue);
		SetResultData("MovesUsed", StringTable.GetStringData(_MovesText._ID, _MovesText._Text) + ": ", moves.ToString());
		SetResultData("TotalScore", StringTable.GetStringData(_GameScoreText._ID, _GameScoreText._Text) + ": ", totalScore.ToString());
		KAWidget kAWidget2 = FindItem("StarsIcon");
		for (int i = 1; i <= 3; i++)
		{
			KAWidget kAWidget3 = kAWidget2.FindChildItem("Star" + i);
			if (kAWidget3 != null)
			{
				kAWidget3.SetVisibility(i <= levelStars);
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget.name == "NextBtn")
		{
			base.pMessageObject.SendMessage("OnNextLevel", base.gameObject, SendMessageOptions.RequireReceiver);
			SetVisibility(Visibility: false);
		}
		if (inWidget.name == "DeclineBtn")
		{
			base.pMessageObject.SendMessage("OnMainMenu", base.gameObject, SendMessageOptions.RequireReceiver);
			SetVisibility(Visibility: false);
		}
	}
}
