using UnityEngine;

public class UiDragonsEndDBArenaFrenzy : UiDragonsEndDB
{
	public KAWidget[] _ScoreWidgets;

	public Texture _NoPlayerIcoTexture;

	public void SetPlayerInfo(int idx, string name, Texture icon)
	{
		KAWidget kAWidget = _ScoreWidgets[idx].FindChildItem("TxtNamePlayer");
		if (kAWidget != null)
		{
			if (!string.IsNullOrEmpty(name))
			{
				kAWidget.SetText(name);
			}
			else
			{
				kAWidget.SetText("");
			}
			kAWidget.SetVisibility(inVisible: true);
		}
		KAWidget kAWidget2 = _ScoreWidgets[idx].FindChildItem("PlayerAvatar");
		if (kAWidget2 != null)
		{
			if (icon != null)
			{
				kAWidget2.SetTexture(icon);
			}
			else if (_NoPlayerIcoTexture != null)
			{
				kAWidget2.SetTexture(_NoPlayerIcoTexture);
			}
			kAWidget2.SetVisibility(inVisible: true);
		}
	}

	public void SetResultData(int idx, string widgetName, string txtName, string txtValue)
	{
		if (idx < _ScoreWidgets.Length)
		{
			KAWidget kAWidget = _ScoreWidgets[idx].FindChildItem("GrpScoreData");
			if (kAWidget != null)
			{
				SetResultData(widgetName, txtName, txtValue, kAWidget, kAWidget.FindChildItem(widgetName));
			}
			else
			{
				Debug.Log("Unable to find Paren widget(GrpScoreData) : " + _ScoreWidgets[idx]);
			}
		}
	}

	public void SetWinLossText(string resultsText)
	{
		if (mResultUI != null)
		{
			KAWidget kAWidget = mResultUI.FindItem("ResultText");
			if (kAWidget != null)
			{
				kAWidget.SetText(resultsText);
			}
		}
	}
}
