using UnityEngine;

public class UiProfileMenuBase : KAUIDropDownMenu
{
	public int _GroupID = 10;

	public int _CategoryID = 1;

	public string _TemplateName = "MenuTemplate";

	private KAUI mParent;

	protected UserProfile mUserProfile;

	protected bool mPopulated;

	protected override void Start()
	{
		base.Start();
		mDropDownUI = (KAUIDropDown)_ParentUi;
	}

	public override void SetVisibility(bool t)
	{
		base.SetVisibility(t);
		base.pViewChanged = true;
	}

	public void ChangeCategory(int newC)
	{
		if (newC != _CategoryID)
		{
			_CategoryID = newC;
			mPopulated = false;
		}
	}

	public virtual void ProfileDataReady(UserProfile p)
	{
		mUserProfile = p;
		mPopulated = false;
	}

	public override void OnClick(KAWidget inWidget)
	{
		mDropDownUI.ProcessMenuSelection(inWidget, 0);
		mDropDownUI.CloseDropDown();
	}

	public virtual void PopulateItems(ProfileQuestion qd)
	{
		mPopulated = true;
		KAWidget inItem = _ParentUi.FindItem(_TemplateName);
		ClearItems();
		if (qd.Answers == null || qd.Answers.Length == 0)
		{
			Debug.LogError("No answer for " + _CategoryID);
			return;
		}
		for (int i = 1; i <= qd.Answers.Length; i++)
		{
			ProfileAnswer[] answers = qd.Answers;
			foreach (ProfileAnswer profileAnswer in answers)
			{
				if (profileAnswer.Ordinal == i)
				{
					KAWidget kAWidget = DuplicateWidget(inItem);
					AddWidget(kAWidget);
					kAWidget.name = _TemplateName;
					kAWidget.SetVisibility(inVisible: true);
					kAWidget._TooltipInfo._Text = new LocaleString(profileAnswer.DisplayText);
					ProfileQuestionUserData profileQuestionUserData = new ProfileQuestionUserData();
					profileQuestionUserData._QData = qd;
					profileQuestionUserData._AData = profileAnswer;
					profileQuestionUserData.pLoaded = false;
					kAWidget.SetUserData(profileQuestionUserData);
				}
			}
		}
		mParent = _ParentUi._MenuList[0];
		if (mParent != null && _ParentUi.name != "PfUiProfileFav")
		{
			mParent.SetVisibility(inVisible: false);
		}
	}

	public override void LoadItem(KAWidget item)
	{
		ProfileQuestionUserData profileQuestionUserData = (ProfileQuestionUserData)item.GetUserData();
		if (!profileQuestionUserData.pLoaded)
		{
			profileQuestionUserData.pLoaded = true;
			UiProfile.SetAnswerItem(item, profileQuestionUserData._AData);
		}
	}

	public virtual void PopulateItems()
	{
		if (_GroupID <= 0 || _CategoryID <= 0)
		{
			return;
		}
		ProfileQuestionList questionList = UserProfile.GetQuestionList(_GroupID);
		if (questionList != null)
		{
			ProfileQuestion question = questionList.GetQuestion(_CategoryID);
			if (question == null)
			{
				Debug.LogError("Question not found for = " + _CategoryID);
			}
			else
			{
				PopulateItems(question);
			}
		}
		else
		{
			Debug.LogError("Question list doesn't exist for ID = " + _GroupID);
		}
	}

	protected override void Update()
	{
		if (mUserProfile != null && !mPopulated)
		{
			PopulateItems();
			base.pViewChanged = true;
		}
		base.Update();
	}
}
