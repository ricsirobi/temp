using System;
using UnityEngine;

public class UiProfileBase : KAUIDropDown
{
	public LocaleString _DefaultAnswerText = new LocaleString("?");

	public string _DefaultImageURL = "";

	public int _DefaultAnswerID;

	public bool _NoEdit;

	[NonSerialized]
	public UserProfile pProfileData;

	[NonSerialized]
	public ProfileQuestion pQuestion;

	protected int mAnswerID;

	public virtual void OnSetVisibility(bool t)
	{
		SetVisibility(t);
	}

	public override void ProcessMenuSelection(KAWidget item, int idx)
	{
		base.ProcessMenuSelection(item, idx);
		ProfileQuestionUserData profileQuestionUserData = (ProfileQuestionUserData)item.GetUserData();
		if (profileQuestionUserData == null)
		{
			Debug.LogError("Bad answer data");
			return;
		}
		mAnswerID = profileQuestionUserData._AData.ID;
		pProfileData.AnswerQuestion(new ProfileUserAnswer(profileQuestionUserData._QData.ID, mAnswerID));
	}

	public virtual void EnableEdit(bool t)
	{
		if (_NoEdit)
		{
			SetInteractive(interactive: false);
		}
		else
		{
			SetInteractive(t);
		}
	}

	public virtual void ProfileDataReady(UserProfile p)
	{
		pProfileData = p;
		UiProfileMenuBase componentInChildren = base.gameObject.GetComponentInChildren<UiProfileMenuBase>();
		ProfileQuestionList questionList = UserProfile.GetQuestionList(componentInChildren._GroupID);
		if (questionList == null)
		{
			UtDebug.LogWarning("Missing QuestionList data for " + componentInChildren._GroupID + " interface = " + GetName());
			return;
		}
		pQuestion = questionList.GetQuestion(componentInChildren._CategoryID);
		if (pQuestion == null)
		{
			UtDebug.LogWarning("Missing Question data for " + base.name);
			return;
		}
		ProfileUserAnswer profileUserAnswer = p.FindQuestionAnswer(pQuestion.ID);
		if (profileUserAnswer != null)
		{
			mAnswerID = profileUserAnswer.AnswerID;
			ProfileAnswer answer = pQuestion.GetAnswer(profileUserAnswer.AnswerID);
			if (answer != null && mDisplayItem != null)
			{
				UiProfile.SetAnswerItem(mDisplayItem, answer);
			}
			return;
		}
		if (_DefaultAnswerID != 0)
		{
			mAnswerID = _DefaultAnswerID;
			ProfileAnswer answer2 = pQuestion.GetAnswer(_DefaultAnswerID);
			if (answer2 == null)
			{
				Debug.LogError("Default answer not exist for id = " + _DefaultAnswerID);
			}
			UiProfile.SetAnswerItem(mDisplayItem, answer2);
			return;
		}
		if (_DefaultImageURL == "")
		{
			if (mSelectionItem != null)
			{
				mSelectionItem.SetTextByID(_DefaultAnswerText._ID, _DefaultAnswerText._Text);
			}
			if (mDisplayItem != null)
			{
				mDisplayItem.SetTexture(null);
				mDisplayItem.SetTextByID(_DefaultAnswerText._ID, _DefaultAnswerText._Text);
			}
		}
		UiProfile.SetAnswerItem(mDisplayItem, null, _DefaultImageURL, _DefaultAnswerText.GetLocalizedString());
	}
}
