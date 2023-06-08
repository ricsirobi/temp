public class UiProfileGender : UiProfileBase
{
	private KAWidget mCloseBtn;

	protected override void Start()
	{
		base.Start();
		mDropDownMenu = (KAUIDropDownMenu)_MenuList[0];
		mCloseBtn = FindItem("MenuCloseBtn");
		mCloseBtn.SetVisibility(inVisible: false);
	}

	private void SetGender(Gender gn)
	{
		pProfileData.SetGender(gn, save: true);
		mSelectionItem.SetInteractive(isInteractive: true);
		mDisplayItem.SetInteractive(isInteractive: true);
		ProfileUserAnswer profileUserAnswer = pProfileData.FindQuestionAnswer(pQuestion.ID);
		ProfileAnswer profileAnswer = null;
		if (profileUserAnswer != null)
		{
			profileAnswer = pQuestion.GetAnswer(profileUserAnswer.AnswerID);
		}
		if (profileAnswer != null)
		{
			UiProfile.SetAnswerItem(mDisplayItem, profileAnswer);
		}
	}

	public override void ProcessMenuSelection(KAWidget item, int idx)
	{
		base.ProcessMenuSelection(item, idx);
		if (mAnswerID == ProfileQuestionData.ConvertGenderAnswerID(Gender.Male))
		{
			SetGender(Gender.Male);
		}
		else if (mAnswerID == ProfileQuestionData.ConvertGenderAnswerID(Gender.Female))
		{
			SetGender(Gender.Female);
		}
	}

	public override void ProfileDataReady(UserProfile p)
	{
		if (p.GetGender() != 0)
		{
			if (mSelectionItem != null)
			{
				mSelectionItem.SetInteractive(isInteractive: false);
			}
			if (mDisplayItem != null)
			{
				mDisplayItem.SetInteractive(isInteractive: true);
			}
		}
		base.ProfileDataReady(p);
		if (pQuestion == null)
		{
			return;
		}
		int num = ProfileQuestionData.ConvertGenderAnswerID(p.GetGender());
		ProfileUserAnswer profileUserAnswer = p.FindQuestionAnswer(pQuestion.ID);
		if (p.GetGender() == Gender.Unknown && profileUserAnswer != null && pQuestion.GetAnswer(profileUserAnswer.AnswerID) != null && mDisplayItem != null)
		{
			if (mSelectionItem != null)
			{
				mSelectionItem.SetInteractive(isInteractive: false);
			}
			if (mDisplayItem != null)
			{
				mDisplayItem.SetInteractive(isInteractive: true);
			}
		}
		if (p.GetGender() != 0 && p.UserID == UserInfo.pInstance.UserID && (profileUserAnswer == null || num != profileUserAnswer.AnswerID))
		{
			p.AnswerQuestion(new ProfileUserAnswer(pQuestion.ID, num));
			ProfileAnswer answer = pQuestion.GetAnswer(num);
			if (answer != null)
			{
				UiProfile.SetAnswerItem(mDisplayItem, answer);
			}
		}
	}

	public override void CloseDropDown()
	{
		base.CloseDropDown();
		mCloseBtn.SetVisibility(inVisible: false);
	}

	public override void EnableEdit(bool t)
	{
		if (mSelectionItem != null)
		{
			mSelectionItem.SetInteractive(t);
		}
		if (!t && mDisplayItem != null)
		{
			mDisplayItem.SetInteractive(isInteractive: true);
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mSelectionItem)
		{
			mCloseBtn.SetVisibility(inVisible: true);
		}
		if (item.name == "MenuCloseBtn")
		{
			CloseDropDown();
			if (GetBackgroundItem() != null)
			{
				GetBackgroundItem().SetVisibility(inVisible: false);
			}
		}
	}
}
