public class UiDragonsReward : UiReward
{
	public LocaleString _FailedText = new LocaleString("Failed");

	private string mScoreItemNameStr;

	private bool mIsInitalised;

	protected override void Start()
	{
		if (!mIsInitalised)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		KAWidget kAWidget = FindItem("Reward").FindChildItem("TxtScoreItemName");
		mScoreItemNameStr = kAWidget.GetText();
		mLoading = FindItem("Loading");
		mLoading.SetVisibility(inVisible: true);
		mScrollRtBtn = FindItem("ScrollRtBtn");
		mIsInitalised = true;
	}

	public void SetGameData(int score, string grade, string gradeBgColor, int gold, int bonusXP)
	{
		if (!mIsInitalised)
		{
			Initialize();
		}
		KAWidget kAWidget = FindItem("Reward");
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.FindChildItem("TxtItemName").SetVisibility(inVisible: false);
		kAWidget.FindChildItem("TxtAmount").SetVisibility(inVisible: false);
		kAWidget.FindChildItem("TxtMyName").SetVisibility(inVisible: false);
		mLoading.SetVisibility(inVisible: false);
		kAWidget.FindChildItem("TxtScoreAmount").SetText(score.ToString());
		kAWidget.FindChildItem("TxtGoldAmount").SetText(gold.ToString());
		if (bonusXP <= 0)
		{
			kAWidget.FindChildItem("TxtBonusXPAmount").SetVisibility(inVisible: false);
			kAWidget.FindChildItem("TxtBonusXPItemName").SetVisibility(inVisible: false);
		}
		kAWidget.FindChildItem("TxtBonusXPAmount").SetText(bonusXP.ToString());
		mScrollRtBtn.SetVisibility(inVisible: true);
		string inWidgetName = "AniGrade" + gradeBgColor;
		KAWidget kAWidget2 = FindItem(inWidgetName);
		if (grade != null && kAWidget2 != null)
		{
			kAWidget2.SetVisibility(inVisible: true);
			kAWidget2.SetText(grade);
		}
	}

	public void SetGameData(float time, int gold, int bonusXP)
	{
		KAWidget kAWidget = FindItem("Reward");
		kAWidget.FindChildItem("TxtItemName").SetVisibility(inVisible: false);
		kAWidget.FindChildItem("TxtAmount").SetVisibility(inVisible: false);
		KAWidget kAWidget2 = kAWidget.FindChildItem("TxtScoreItemName");
		KAWidget kAWidget3 = kAWidget.FindChildItem("TxtScoreAmount");
		if (time > 0f)
		{
			kAWidget2.SetText(mScoreItemNameStr);
			kAWidget3.SetText(time.ToString());
		}
		else
		{
			kAWidget2.SetText("");
			kAWidget3.SetText("");
		}
		kAWidget.FindChildItem("TxtGoldAmount").SetText(gold.ToString());
		kAWidget.FindChildItem("TxtBonusXPAmount").SetText(bonusXP.ToString());
		if (time <= 0f)
		{
			kAWidget2.SetText(_FailedText.GetLocalizedString());
		}
	}
}
