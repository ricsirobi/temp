using System;
using System.Collections.Generic;
using JSGames.UI.Util;

namespace JSGames.UI.TerrorMail;

public class UIMessageItem : UIMessagePopulator
{
	public UIWidget _SenderWidget;

	public UIWidget _AnnouncementMessageWidget;

	public UIWidget _ChallengeMessageWidget;

	public UIWidget _MessageReplyCountWidget;

	public UIWidget _GiftUnopenedWidget;

	public UIWidget _GiftOpenedWidget;

	public UIWidget _BuddyIconWidget;

	public UIWidget _IgnoredIconWidget;

	public UIWidget _IgnoreBtn;

	public UIWidget _ReportBtn;

	public UIWidget _DeleteBtn;

	public int _MaxTextLength = 20;

	public string _TruncatedSymbolText = "...";

	public LocaleString _BuddyNameFallback = new LocaleString("A friend ");

	public GiftData pGiftData { get; set; }

	public override void Populate(object inData)
	{
		if (inData == null)
		{
			return;
		}
		base.pData = inData;
		Buddy buddy = null;
		if (base.pData is MessageInfo)
		{
			MessageInfo messageInfo = base.pData as MessageInfo;
			base.pSubject = messageInfo.MemberMessage;
			switch (messageInfo.MessageTypeID.Value)
			{
			case 14:
				UIUtil.ReplaceTagWithUserNameByID(ref messageInfo.MemberMessage, messageInfo.FromUserID.ToString(), "{{BuddyUserName}}", OnUsernameRetrieved);
				if ((bool)_ChallengeMessageWidget)
				{
					_ChallengeMessageWidget.pVisible = true;
				}
				break;
			case 19:
				pGiftData = GiftManager.pInstance.GetGiftDataByMessageID(messageInfo.MessageID.Value);
				if (pGiftData != null && (bool)_GiftOpenedWidget && (bool)_GiftUnopenedWidget)
				{
					bool messageTag = GiftManager.pInstance.GetMessageTag(pGiftData, "Claim");
					_GiftOpenedWidget.pVisible = messageTag;
					_GiftUnopenedWidget.pVisible = !messageTag;
				}
				break;
			case 21:
			{
				if (messageInfo.MemberMessage.Contains("{{PetName}}"))
				{
					UIUtil.ReplaceTagWithPetData(ref messageInfo.MemberMessage, UtUtilities.DeserializeFromXml(messageInfo.Data, typeof(RewardData)) as RewardData);
				}
				Dictionary<string, string> dictionary = TaggedMessageHelper.Match(messageInfo.Data);
				if (messageInfo.MemberMessage.Contains("{{OwnerUserName}}"))
				{
					UIUtil.ReplaceTagWithUserNameByID(ref messageInfo.MemberMessage, dictionary["OwnerID"], "{{OwnerUserName}}", OnUsernameRetrieved);
					if (messageInfo.MemberMessage.Contains("{{OwnerUserName}}"))
					{
						mUsernamesToRetrieve++;
					}
				}
				if (messageInfo.MemberMessage.Contains("{{BuddyUserName}}"))
				{
					UIUtil.ReplaceTagWithUserNameByID(ref messageInfo.MemberMessage, messageInfo.FromUserID.ToString(), "{{BuddyUserName}}", OnUsernameRetrieved);
					if (messageInfo.MemberMessage.Contains("{{BuddyUserName}}"))
					{
						mUsernamesToRetrieve++;
					}
				}
				if (mUsernamesToRetrieve > 0)
				{
					base.gameObject.SetActive(value: false);
					KAUICursorManager.SetDefaultCursor("Loading");
					mParentState = WidgetState.NOT_INTERACTIVE;
					return;
				}
				break;
			}
			}
			base.pSubject = ((!string.IsNullOrEmpty(messageInfo.MemberMessage)) ? messageInfo.MemberMessage : messageInfo.NonMemberMessage);
			string inMessage = base.pSubject;
			UIUtil.FormatTaggedMessage(ref inMessage, messageInfo, new string[1] { "Line1" }, _TagAndDefaultText);
			base.pSubject = inMessage;
			base.pReceivedDate = messageInfo.CreateDate.ToString(_DateTimeFormat);
		}
		if (inData is Announcement)
		{
			Announcement announcement = inData as Announcement;
			base.pSubject = announcement.Description;
			base.pReceivedDate = announcement.StartDate.ToLocalTime().ToString(_DateTimeFormat);
			if ((bool)_AnnouncementMessageWidget)
			{
				_AnnouncementMessageWidget.pVisible = true;
			}
		}
		if (base.pData is ChallengeInfo)
		{
			ChallengeInfo challengeInfo = base.pData as ChallengeInfo;
			if ((bool)_ChallengeMessageWidget)
			{
				_ChallengeMessageWidget.pVisible = true;
			}
			if (BuddyList.pInstance != null)
			{
				buddy = BuddyList.pInstance.GetBuddy(challengeInfo.UserID.ToString());
			}
			base.pReceivedDate = (challengeInfo.ExpirationDate - new TimeSpan(7, 0, 0, 0)).ToLocalTime().ToString(_DateTimeFormat);
			base.pSubject = ((buddy == null) ? _BuddyNameFallback.GetLocalizedString() : (buddy.DisplayName + _ChallengeSubjectText.GetLocalizedString()));
		}
		if (base.pData is Message)
		{
			if (!UITerrorMail.pIsMyBoard && _DeleteBtn != null)
			{
				_DeleteBtn.pVisible = false;
			}
			Message message = base.pData as Message;
			_BuddyIconWidget.pVisible = true;
			if (BuddyList.pIsReady && message.Creator != UserInfo.pInstance.UserID)
			{
				buddy = BuddyList.pInstance.GetBuddy(message.Creator);
				if (buddy != null && buddy.Status == BuddyStatus.BlockedBySelf && message.Creator != UserInfo.pInstance.UserID && (bool)_IgnoredIconWidget)
				{
					_IgnoredIconWidget.pVisible = true;
					_BuddyIconWidget.pVisible = false;
				}
			}
			base.pReceivedDate = message.CreateTime.ToLocalTime().ToString(_DateTimeFormat);
			if (_MaxTextLength <= 0)
			{
				base.pBody = message.Content;
				string value;
				if (message.Creator == UserProfile.pProfileData.ID)
				{
					string playerNameByUserID = GetPlayerNameByUserID(message.Creator);
					if (string.IsNullOrEmpty(playerNameByUserID))
					{
						WsWebService.GetDisplayNameByUserID(message.Creator, ServiceEventHandler, message);
					}
					else
					{
						base.pSubject = playerNameByUserID;
					}
					if ((bool)_IgnoreBtn)
					{
						_IgnoreBtn.pState = WidgetState.DISABLED;
					}
					if ((bool)_ReportBtn)
					{
						_ReportBtn.pState = WidgetState.DISABLED;
					}
					OnPopulateComplete();
				}
				else if (UIMessagePopulator.mCachedDisplayNames.TryGetValue(message.Creator, out value))
				{
					base.pSubject = value;
					OnPopulateComplete();
				}
				else
				{
					string playerNameByUserID2 = GetPlayerNameByUserID(message.Creator);
					if (string.IsNullOrEmpty(playerNameByUserID2))
					{
						WsWebService.GetDisplayNameByUserID(message.Creator, ServiceEventHandler, message);
						return;
					}
					base.pSubject = playerNameByUserID2;
					OnPopulateComplete();
				}
				return;
			}
			base.pSubject = message.Content;
		}
		OnPopulateComplete();
	}

	public string GetPlayerNameByUserID(string userID)
	{
		if (UserProfile.pProfileData != null && userID == UserProfile.pProfileData.ID && UserProfile.pProfileData.AvatarInfo != null && UserProfile.pProfileData.AvatarInfo.AvatarData != null)
		{
			return UserProfile.pProfileData.AvatarInfo.AvatarData.DisplayName;
		}
		if (UiProfile.pUserProfile != null && UiProfile.pUserProfile.UserID == userID)
		{
			return UiProfile.pUserProfile.GetDisplayName();
		}
		if (BuddyList.pIsReady)
		{
			Buddy buddy = BuddyList.pInstance.GetBuddy(userID);
			if (buddy != null && !string.IsNullOrEmpty(buddy.DisplayName))
			{
				return buddy.DisplayName;
			}
		}
		return null;
	}

	public override void OnUsernameRetrieved(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		base.OnUsernameRetrieved(inType, inEvent, inProgress, inObject, inUserData);
		if (mUsernamesToRetrieve == 0 && inEvent == WsServiceEvent.COMPLETE)
		{
			base.gameObject.SetActive(value: true);
			MessageInfo messageInfo = base.pData as MessageInfo;
			base.pSubject = ((!string.IsNullOrEmpty(messageInfo.MemberMessage)) ? messageInfo.MemberMessage : messageInfo.NonMemberMessage);
			string inMessage = base.pSubject;
			UIUtil.FormatTaggedMessage(ref inMessage, messageInfo, new string[1] { "Line1" }, _TagAndDefaultText);
			base.pSubject = inMessage;
			base.pReceivedDate = messageInfo.CreateDate.ToString(_DateTimeFormat);
			OnPopulateComplete();
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && inType == WsServiceType.GET_DISPLAYNAME_BY_USER_ID)
		{
			if (!UIMessagePopulator.mCachedDisplayNames.ContainsKey(((Message)inUserData).Creator))
			{
				UIMessagePopulator.mCachedDisplayNames.Add(((Message)inUserData).Creator, (string)inObject);
			}
			base.pSubject = (string)inObject;
			OnPopulateComplete();
		}
	}

	private void FormatSubject()
	{
		if (!string.IsNullOrEmpty(base.pSubject))
		{
			base.pSubject = base.pSubject.Replace("\n", " ");
			if (_MaxTextLength > 0 && base.pSubject.Length > _MaxTextLength)
			{
				base.pSubject = base.pSubject.Substring(0, _MaxTextLength) + _TruncatedSymbolText;
			}
		}
	}

	private void OnPopulateComplete()
	{
		FormatSubject();
		SetFields();
		pVisible = true;
	}

	public void SetReplyCountFields(int replyCount)
	{
		if (_MessageReplyCountWidget != null)
		{
			if (replyCount <= 0)
			{
				_MessageReplyCountWidget.pVisible = false;
				return;
			}
			_MessageReplyCountWidget.pVisible = true;
			_MessageReplyCountWidget.pText = replyCount.ToString();
		}
	}
}
