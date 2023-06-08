using UnityEngine;

public class UiProfileFavMenu : KAUIMenu
{
	private UiProfileFav mProfileUI;

	public string _MenuTemplateEdit = "MenuTemplateEdit";

	public string _MenuTemplateNoEdit = "MenuTemplateNoEdit";

	public LocaleString _DefaultAnswerText = new LocaleString("?");

	protected override void Start()
	{
		base.Start();
		mProfileUI = (UiProfileFav)_ParentUi;
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		mSelectedItem = item;
		mProfileUI.ProcessFavMenuSelection(item, 0);
	}

	public void ProfileDataReady(UserProfile p)
	{
		KAWidget kAWidget = null;
		kAWidget = ((mProfileUI.GetState() != KAUIState.NOT_INTERACTIVE) ? mProfileUI.FindItem(_MenuTemplateEdit) : mProfileUI.FindItem(_MenuTemplateNoEdit));
		ProfileQuestionList questionList = UserProfile.GetQuestionList(1);
		if (questionList == null)
		{
			Debug.LogError("Question list doesn't exist for ID = " + 1);
			return;
		}
		ClearItems();
		ProfileQuestion[] questions = questionList.Questions;
		foreach (ProfileQuestion profileQuestion in questions)
		{
			KAWidget kAWidget2 = DuplicateWidget(kAWidget);
			AddWidget(kAWidget2);
			kAWidget2.name = "Q=" + profileQuestion.ID;
			kAWidget2.SetVisibility(inVisible: true);
			ProfileFavQuestionUserData profileFavQuestionUserData = new ProfileFavQuestionUserData();
			profileFavQuestionUserData._QData = profileQuestion;
			kAWidget2.SetUserData(profileFavQuestionUserData);
			ProfileUserAnswer profileUserAnswer = p.FindQuestionAnswer(profileQuestion.ID);
			if (profileUserAnswer != null)
			{
				ProfileAnswer answer = profileQuestion.GetAnswer(profileUserAnswer.AnswerID);
				if (answer == null)
				{
					Debug.LogWarning("Answer is null for qid = " + profileUserAnswer.AnswerID + " answer id = " + profileQuestion.ID);
				}
				else
				{
					UiProfile.SetAnswerItem(kAWidget2, answer);
				}
			}
			else
			{
				kAWidget2.SetTextByID(_DefaultAnswerText._ID, _DefaultAnswerText._Text);
			}
			KAWidget kAWidget3 = kAWidget2.FindChildItem("TxtFavName");
			kAWidget3.name = "Q=" + profileQuestion.ID;
			kAWidget3 = kAWidget2.FindChildItem("FavIcon");
			if (profileQuestion.ImageURL != null && profileQuestion.ImageURL.Length > 0)
			{
				kAWidget3.SetTextureFromURL(profileQuestion.ImageURL);
			}
			else
			{
				kAWidget3.SetVisibility(inVisible: false);
			}
		}
		if (!GetVisibility())
		{
			SetVisibility(inVisible: false);
		}
	}
}
