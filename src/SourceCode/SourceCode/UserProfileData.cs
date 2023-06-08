using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "UserProfileDisplayData", IsNullable = true, Namespace = "")]
public class UserProfileData
{
	public string ID;

	[XmlElement(ElementName = "Avatar")]
	public AvatarDisplayData AvatarInfo;

	[XmlElement(ElementName = "Ach")]
	public int AchievementCount;

	[XmlElement(ElementName = "Mth")]
	public int MythieCount;

	[XmlElement(ElementName = "Answer", IsNullable = true)]
	public UserAnswerData AnswerData;

	[XmlElement(ElementName = "Game", IsNullable = true)]
	public GameDataSummary GameInfo;

	[XmlElement(ElementName = "gc", IsNullable = true)]
	public int? GameCurrency;

	[XmlElement(ElementName = "cc", IsNullable = true)]
	public int? CashCurrency;

	[XmlElement(ElementName = "BuddyCount", IsNullable = true)]
	public int? BuddyCount;

	[XmlElement(ElementName = "ActivityCount", IsNullable = true)]
	public int? ActivityCount;

	[XmlElement(ElementName = "Groups")]
	public UserProfileGroupData[] Groups;

	[XmlElement(ElementName = "UPT")]
	public UserProfileTag UserProfileTag;

	[XmlElement(ElementName = "UG", IsNullable = true)]
	public UserGrade UserGradeData;

	public ProfileUserAnswer FindQuestionAnswer(int qid)
	{
		if (AnswerData == null || AnswerData.Answers == null)
		{
			return null;
		}
		ProfileUserAnswer[] answers = AnswerData.Answers;
		foreach (ProfileUserAnswer profileUserAnswer in answers)
		{
			if (profileUserAnswer != null && profileUserAnswer.QuestionID == qid)
			{
				return profileUserAnswer;
			}
		}
		return null;
	}

	public void SetQuestionAnswer(ProfileUserAnswer sel, WsServiceEventHandler inCallback, object inUserData)
	{
		int[] answers = new int[1] { sel.AnswerID };
		ProfileUserAnswer profileUserAnswer = FindQuestionAnswer(sel.QuestionID);
		if (profileUserAnswer != null)
		{
			if (profileUserAnswer.AnswerID != sel.AnswerID)
			{
				profileUserAnswer.AnswerID = sel.AnswerID;
				WsWebService.SetUserProfileAnswers(answers, inCallback, inUserData);
			}
			return;
		}
		if (AnswerData == null)
		{
			AnswerData = new UserAnswerData();
			AnswerData.Answers = new ProfileUserAnswer[1];
		}
		else if (AnswerData.Answers == null)
		{
			AnswerData.Answers = new ProfileUserAnswer[1];
		}
		else
		{
			Array.Resize(ref AnswerData.Answers, AnswerData.Answers.Length + 1);
		}
		AnswerData.Answers[AnswerData.Answers.Length - 1] = new ProfileUserAnswer(sel);
		WsWebService.SetUserProfileAnswers(answers, inCallback, inUserData);
	}

	public string GetGenderURL(GameObject inAvatar)
	{
		string result = ((inAvatar != null) ? inAvatar.GetComponent<AvAvatarProperties>()._DefaultGenderURL : "");
		ProfileQuestionList questionList = UserProfile.GetQuestionList(2);
		if (questionList != null)
		{
			ProfileQuestion question = questionList.GetQuestion(32);
			if (question != null)
			{
				ProfileUserAnswer profileUserAnswer = FindQuestionAnswer(question.ID);
				if (profileUserAnswer != null)
				{
					ProfileAnswer answer = question.GetAnswer(profileUserAnswer.AnswerID);
					if (answer != null)
					{
						result = answer.ImageURL;
					}
				}
			}
		}
		return result;
	}

	public int GetCountryAnswerID()
	{
		ProfileQuestionList questionList = UserProfile.GetQuestionList(2);
		if (questionList != null)
		{
			ProfileQuestion question = questionList.GetQuestion(33);
			if (question != null)
			{
				ProfileUserAnswer profileUserAnswer = FindQuestionAnswer(question.ID);
				if (profileUserAnswer != null)
				{
					return profileUserAnswer.AnswerID;
				}
			}
		}
		return -1;
	}

	public string GetCountryURL(GameObject inAvatar)
	{
		string result = ((inAvatar != null) ? inAvatar.GetComponent<AvAvatarProperties>()._DefaultCountryURL : "");
		ProfileQuestionList questionList = UserProfile.GetQuestionList(2);
		if (questionList != null)
		{
			ProfileQuestion question = questionList.GetQuestion(33);
			if (question != null)
			{
				ProfileUserAnswer profileUserAnswer = FindQuestionAnswer(question.ID);
				if (profileUserAnswer != null)
				{
					ProfileAnswer answer = question.GetAnswer(profileUserAnswer.AnswerID);
					if (answer != null)
					{
						result = answer.ImageURL;
					}
				}
			}
		}
		return result;
	}

	public int GetMoodAnswerID()
	{
		int result = 288;
		ProfileQuestionList questionList = UserProfile.GetQuestionList(2);
		if (questionList != null)
		{
			ProfileQuestion question = questionList.GetQuestion(34);
			if (question != null)
			{
				ProfileUserAnswer profileUserAnswer = FindQuestionAnswer(question.ID);
				if (profileUserAnswer != null)
				{
					result = profileUserAnswer.AnswerID;
				}
			}
		}
		return result;
	}

	public string GetMoodURL()
	{
		string result = "";
		ProfileQuestionList questionList = UserProfile.GetQuestionList(2);
		if (questionList != null)
		{
			ProfileQuestion question = questionList.GetQuestion(34);
			if (question != null)
			{
				ProfileAnswer profileAnswer = null;
				ProfileUserAnswer profileUserAnswer = FindQuestionAnswer(question.ID);
				profileAnswer = ((profileUserAnswer == null) ? question.GetAnswer(288) : question.GetAnswer(profileUserAnswer.AnswerID));
				if (profileAnswer != null)
				{
					result = profileAnswer.ImageURL;
				}
			}
		}
		return result;
	}

	public void AddGroup(string inGroupID, UserRole inRole)
	{
		if (Groups == null)
		{
			Groups = new UserProfileGroupData[1];
		}
		else
		{
			Array.Resize(ref Groups, Groups.Length + 1);
		}
		UserProfileGroupData userProfileGroupData = new UserProfileGroupData();
		userProfileGroupData.GroupID = inGroupID;
		userProfileGroupData.RoleID = (int)inRole;
		Groups[Groups.Length - 1] = userProfileGroupData;
	}

	public void RemoveGroup(string inGroupID)
	{
		if (Groups != null)
		{
			List<UserProfileGroupData> list = new List<UserProfileGroupData>(Groups);
			UserProfileGroupData userProfileGroupData = list.Find((UserProfileGroupData g) => g.GroupID == inGroupID);
			if (userProfileGroupData != null)
			{
				list.Remove(userProfileGroupData);
				Groups = list.ToArray();
			}
		}
	}

	public void ReplaceGroup(int inIndex, string inGroupID, UserRole inRole)
	{
		if (Groups != null && inIndex < Groups.Length)
		{
			RemoveGroup(Groups[inIndex].GroupID);
		}
		AddGroup(inGroupID, inRole);
	}

	public bool HasGroup()
	{
		if (Groups != null)
		{
			return Groups.Length != 0;
		}
		return false;
	}

	public bool InGroup(string inGroupID)
	{
		if (Groups == null)
		{
			return false;
		}
		UserProfileGroupData[] groups = Groups;
		for (int i = 0; i < groups.Length; i++)
		{
			if (groups[i].GroupID == inGroupID)
			{
				return true;
			}
		}
		return false;
	}

	public string GetGroupID(int index = 0)
	{
		if (Groups == null || Groups.Length <= index)
		{
			return "";
		}
		return UserProfile.pProfileData.Groups[index].GroupID;
	}

	public AchievementTask GetGroupAchievement(int achievementID, string relatedID = "", int achInfoID = 0, int points = 0)
	{
		if (HasGroup())
		{
			return new AchievementTask(Groups[0].GroupID, achievementID, relatedID, achInfoID, points, 2);
		}
		return null;
	}
}
