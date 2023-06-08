using UnityEngine;

public class UiIncredibleMachinesEndDB : UiDragonsEndDB
{
	public LocaleString _TimeBonusText = new LocaleString("Time Bonus");

	public LocaleString _ItemEfficiency = new LocaleString("Item Efficiency");

	public LocaleString _GameScoreText = new LocaleString("Score");

	public void SetResultData(int moves, float time, int levelStars, int totalScore)
	{
		bool flag = ((CTLevelManager)IMGLevelManager.pInstance).GetNextLevel() == -1;
		KAWidget kAWidget = FindItem("NextBtn");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(!flag);
		}
		if (!(mResultUI != null))
		{
			return;
		}
		SetResultData("Time Bonus", StringTable.GetStringData(_TimeBonusText._ID, _TimeBonusText._Text) + ": ", time.ToString());
		SetResultData("Item Efficiency", StringTable.GetStringData(_ItemEfficiency._ID, _ItemEfficiency._Text) + ": ", moves.ToString());
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
			KAUI.RemoveExclusive(this);
			base.pMessageObject.SendMessage("PlayNextLevel", base.gameObject, SendMessageOptions.RequireReceiver);
			SetVisibility(Visibility: false);
		}
		if (inWidget.name == "DeclineBtn")
		{
			KAUI.RemoveExclusive(this);
			base.pMessageObject.SendMessage("OnMainMenu", base.gameObject, SendMessageOptions.RequireReceiver);
			SetVisibility(Visibility: false);
		}
	}
}
