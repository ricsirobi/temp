using System.Collections.Generic;

namespace SquadTactics;

public class UiResultXP : KAUI
{
	public LocaleString _TitleText = new LocaleString("[REVIEW] XP Screen");

	private KAWidget mBtnContinue;

	private KAWidget mTxtTitle;

	private UiResultXPMenu mUIResultXPMenu;

	public UiEndDB pUiEndDB { get; set; }

	protected override void Start()
	{
		base.Start();
		mBtnContinue = FindItem("BtnContinue");
		mTxtTitle = FindItem("Title");
		mUIResultXPMenu = (UiResultXPMenu)_MenuList[0];
		if (mTxtTitle != null)
		{
			mTxtTitle.SetText(_TitleText.GetLocalizedString());
		}
	}

	public void PopulateXPData(List<AchievementReward> inRewards)
	{
		mUIResultXPMenu.PopulateItems(inRewards);
		SetInteractive(interactive: true);
	}

	public void InitUI(UiEndDB endDBUi)
	{
		pUiEndDB = endDBUi;
		base.transform.parent = null;
		mParentVisible = true;
		KAUI.SetExclusive(this);
		SetVisibility(inVisible: true);
		SetInteractive(interactive: false);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnContinue)
		{
			pUiEndDB.ShowLevelRewards();
			KAUI.RemoveExclusive(this);
			SetVisibility(inVisible: false);
		}
	}
}
