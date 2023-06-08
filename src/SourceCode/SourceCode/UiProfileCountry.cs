public class UiProfileCountry : UiProfileBase
{
	private KAWidget mCloseBtn;

	protected override void Start()
	{
		base.Start();
		mDropDownMenu = (KAUIDropDownMenu)_MenuList[0];
		mSelectionItem = FindItem("CountryBtn");
		mDisplayItem = FindItem("CountryIcon");
		mCloseBtn = FindItem("MenuCloseBtn");
		mCloseBtn.SetVisibility(inVisible: false);
	}

	public override void ProcessMenuSelection(KAWidget item, int idx)
	{
		base.ProcessMenuSelection(item, idx);
		SetToolTip();
		if (AvatarData.pInstanceInfo != null)
		{
			AvatarData.pInstanceInfo.LoadCountry(item.pTextureURL);
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			ProfileUserAnswer profileUserAnswer = pProfileData.FindQuestionAnswer(pQuestion.ID);
			MainStreetMMOClient.pInstance.SetCountry(profileUserAnswer.AnswerID);
		}
	}

	public override void ProfileDataReady(UserProfile p)
	{
		base.ProfileDataReady(p);
		SetToolTip();
	}

	public void SetToolTip()
	{
		if (pQuestion != null)
		{
			ProfileUserAnswer profileUserAnswer = pProfileData.FindQuestionAnswer(pQuestion.ID);
			ProfileAnswer profileAnswer = null;
			if (profileUserAnswer != null)
			{
				profileAnswer = pQuestion.GetAnswer(profileUserAnswer.AnswerID);
			}
			if (profileAnswer != null)
			{
				mSelectionItem._TooltipInfo._Text = new LocaleString(profileAnswer.DisplayText);
			}
		}
	}

	public override void EnableEdit(bool t)
	{
		if (mSelectionItem != null)
		{
			mSelectionItem.SetInteractive(t);
		}
		if (mDisplayItem != null && !t)
		{
			mDisplayItem.SetInteractive(isInteractive: true);
		}
	}

	public override void CloseDropDown()
	{
		base.CloseDropDown();
		mCloseBtn.SetVisibility(inVisible: false);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mSelectionItem)
		{
			mCloseBtn.SetVisibility(inVisible: true);
		}
		if (inWidget.name == "MenuCloseBtn")
		{
			CloseDropDown();
		}
	}
}
