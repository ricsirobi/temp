using System;
using System.Collections.Generic;
using JSGames.UI.Util;
using UnityEngine;
using UnityEngine.UI;

namespace JSGames.UI.TerrorMail;

public class UIMessagePopulator : UIToggleButton
{
	protected static Dictionary<string, string> mCachedDisplayNames = new Dictionary<string, string>();

	public Text _TxtSubject;

	public Text _TxtReceivedDate;

	public Text _TxtExpirationTimer;

	public string _DateTimeFormat = "MMM dd, yyyy hh:mm tt";

	public TagAndDefaultText[] _TagAndDefaultText;

	public LocaleString _ChallengeSubjectText = new LocaleString(" has sent you a challenge!");

	public ChallengeInviteMessageData[] _ChallengeMessage;

	[HideInInspector]
	public string[] mKeys = new string[0];

	[HideInInspector]
	public int mUsernamesToRetrieve;

	public string pSubject { get; set; }

	public string pBody { get; set; }

	public string pReceivedDate { get; set; }

	public string pExpirationDate { get; set; }

	public string pChallengeText { get; set; }

	public virtual void Populate(object inData)
	{
		try
		{
			base.gameObject.SetActive(value: true);
			base.pData = inData;
			if (inData is MessageInfo)
			{
				MessageInfo inMessageInfo = inData as MessageInfo;
				Dictionary<string, string> dictionary = TaggedMessageHelper.Match(inMessageInfo.Data);
				TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(inMessageInfo);
				_ = string.Empty;
				if (taggedMessageHelper.MemberMessage.ContainsKey("Line1"))
				{
					mKeys = new string[1] { "Line1" };
				}
				if (dictionary.ContainsKey("name"))
				{
					_ = dictionary["name"];
				}
				if (inMessageInfo.MemberMessage.Contains("{{PetName}}"))
				{
					UIUtil.ReplaceTagWithPetData(ref inMessageInfo.MemberMessage, UtUtilities.DeserializeFromXml(inMessageInfo.Data, typeof(RewardData)) as RewardData);
				}
				if (inMessageInfo.MemberMessage.Contains("{{OwnerUserName}}"))
				{
					mUsernamesToRetrieve++;
					UIUtil.ReplaceTagWithUserNameByID(ref inMessageInfo.MemberMessage, dictionary["OwnerID"], "{{OwnerUserName}}", OnUsernameRetrieved);
					if (inMessageInfo.MemberMessage.Contains("{{OwnerUserName}}"))
					{
						KAUICursorManager.SetDefaultCursor("Loading");
						mParentState = WidgetState.NOT_INTERACTIVE;
					}
				}
				if (inMessageInfo.MemberMessage.Contains("{{BuddyUserName}}"))
				{
					mUsernamesToRetrieve++;
					UIUtil.ReplaceTagWithUserNameByID(ref inMessageInfo.MemberMessage, inMessageInfo.FromUserID.ToString(), "{{BuddyUserName}}", OnUsernameRetrieved);
					if (inMessageInfo.MemberMessage.Contains("{{BuddyUserName}}"))
					{
						KAUICursorManager.SetDefaultCursor("Loading");
						mParentState = WidgetState.NOT_INTERACTIVE;
					}
				}
				if (mUsernamesToRetrieve > 0)
				{
					KAUICursorManager.SetDefaultCursor("Loading");
					mParentState = WidgetState.NOT_INTERACTIVE;
					return;
				}
				UIUtil.FormatTaggedMessage(ref inMessageInfo.MemberMessage, inMessageInfo, mKeys, _TagAndDefaultText);
				if (inMessageInfo.MemberMessage.Contains("{{PetName}}"))
				{
					UIUtil.ReplaceTagWithPetData(ref inMessageInfo.MemberMessage, UtUtilities.DeserializeFromXml(inMessageInfo.Data, typeof(RewardData)) as RewardData);
				}
				pBody = inMessageInfo.MemberMessage;
				pReceivedDate = string.Format(_TxtReceivedDate.text, inMessageInfo.CreateDate.ToShortDateString());
				GiftData giftData = GiftManager.pInstance._Gifts.Find((GiftData t) => t.MessageID == inMessageInfo.MessageID);
				if (giftData != null)
				{
					string value = giftData.Prerequisites.Find((GiftPrerequisite t) => t.Type == GiftPrerequisiteType.DateRange).Value;
					if (!string.IsNullOrEmpty(value))
					{
						pReceivedDate = inMessageInfo.CreateDate.ToString(_DateTimeFormat);
						TimeSpan timeSpan = Convert.ToDateTime(value.Split(',')[1], UtUtilities.GetCultureInfo("en-US").DateTimeFormat) - ServerTime.pCurrentTime;
						pExpirationDate = string.Format(_TxtExpirationTimer.text, timeSpan.Days, timeSpan.Hours, timeSpan.Minutes);
					}
				}
			}
			if (inData is ChallengeInfo)
			{
				ChallengeInfo challengeInfo = inData as ChallengeInfo;
				Buddy buddy = BuddyList.pInstance.GetBuddy(challengeInfo.UserID.ToString());
				pSubject = buddy.DisplayName + _ChallengeSubjectText.GetLocalizedString();
				pChallengeText = buddy.DisplayName + GetChallengeMessage(challengeInfo.ChallengeGameInfo.GameID, challengeInfo.Points);
				pExpirationDate = challengeInfo.ExpirationDate.ToString(_DateTimeFormat);
				pBody = pChallengeText;
			}
			if (inData is Announcement)
			{
				Announcement announcement = inData as Announcement;
				TaggedAnnouncementHelper taggedAnnouncementHelper = new TaggedAnnouncementHelper(announcement.AnnouncementText);
				pSubject = announcement.Description;
				pBody = taggedAnnouncementHelper.Announcement["Message"];
				pReceivedDate = announcement.StartDate.ToLocalTime().ToString(_DateTimeFormat);
			}
			SetFields();
			pVisible = true;
		}
		catch (Exception message)
		{
			Debug.LogError(message);
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	public virtual void OnUsernameRetrieved(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE)
		{
			mUsernamesToRetrieve--;
			MessageInfo messageInfo = base.pData as MessageInfo;
			messageInfo.MemberMessage = messageInfo.MemberMessage.Replace((string)inUserData, (string)inObject);
			if (mUsernamesToRetrieve == 0)
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				pReceivedDate = string.Format(_TxtReceivedDate.text, messageInfo.CreateDate.ToShortDateString());
				UIUtil.FormatTaggedMessage(ref messageInfo.MemberMessage, messageInfo, mKeys, _TagAndDefaultText);
				pBody = messageInfo.MemberMessage;
				SetFields();
				pVisible = true;
				mParentState = WidgetState.INTERACTIVE;
			}
		}
	}

	public void SetFields()
	{
		if ((bool)_TxtSubject)
		{
			_TxtSubject.text = pSubject;
		}
		if ((bool)_Text)
		{
			pText = pBody;
		}
		if ((bool)_TxtReceivedDate)
		{
			_TxtReceivedDate.text = pReceivedDate;
		}
		if ((bool)_TxtExpirationTimer)
		{
			_TxtExpirationTimer.text = pExpirationDate;
		}
	}

	public string GetChallengeMessage(int gameID, int points)
	{
		string result = null;
		ChallengeInviteMessageData challengeInviteMessageData = null;
		ChallengeInviteMessageData[] challengeMessage = _ChallengeMessage;
		foreach (ChallengeInviteMessageData challengeInviteMessageData2 in challengeMessage)
		{
			if (challengeInviteMessageData2._GameID == gameID || (challengeInviteMessageData == null && challengeInviteMessageData2._GameID == -1))
			{
				challengeInviteMessageData = challengeInviteMessageData2;
			}
		}
		if (challengeInviteMessageData != null)
		{
			result = challengeInviteMessageData._MessageText.GetLocalizedString();
			result = result.Replace("[Time]", UtUtilities.GetTimerString(points));
			result = result.Replace("[Points]", points.ToString());
			result = result.Replace("[GameName]", ChallengeInfo.GetGameTitle(gameID));
		}
		return result;
	}
}
