using System;
using System.Collections.Generic;
using UnityEngine;

public class BuddyInviteMessage : KAMonoBase
{
	public BuddyInviteMessageTypeData[] _MessageData;

	public LocaleString _NoDisplayNameText;

	public LocaleString _BuddyLocationErrorText;

	public LocaleString _GenericErrorText;

	public LocaleString _NeedPetText = new LocaleString("You need a dragon to play.");

	public LocaleString _TooTiredText = new LocaleString("Your pet is too angry/tired to shoot");

	public LocaleString _StageBlockedText = new LocaleString("Your dragon needs to be older to enter !!!");

	public string _DefaultMessageDBURL;

	public string _ChallengeMessageDBURL;

	public string _UiGiftMessageURL;

	public int _SocialAcceptBuddyInviteAchievementID = 109;

	public int _AcceptChallengeAchievementID = 133;

	public ChallengeInviteMessageData[] _ChallengeMessage;

	private bool mIsForceUpdate = true;

	private bool mJoinOwnerSpace;

	private KAUIGenericDB mKAUIGenericDB;

	public static string INVITE_OBJ_NAME = "PfCheckBuddyInviteMessages";

	private static bool mBlockInvites = false;

	public static bool pBlockInvites
	{
		get
		{
			return mBlockInvites;
		}
		set
		{
			mBlockInvites = value;
		}
	}

	protected virtual bool pCanShowInvite
	{
		get
		{
			if (!mBlockInvites && !RsResourceManager.pLevelLoadingScreen && AvAvatar.pToolbar != null)
			{
				return AvAvatar.pToolbar.activeInHierarchy;
			}
			return false;
		}
	}

	private void Update()
	{
		if (mIsForceUpdate && BuddyList.pIsReady && pCanShowInvite)
		{
			ShowNextInvite();
			mIsForceUpdate = false;
		}
	}

	public void ForceInviteMessageUpdate()
	{
		mIsForceUpdate = true;
	}

	public void ShowNextInvite()
	{
		if (ChallengeInfo.pInviteMessageList != null && ChallengeInfo.pInviteMessageList.Count > 0)
		{
			ChallengeInfo challengeInfo = ChallengeInfo.pInviteMessageList[0];
			ChallengeInfo.pInviteMessageList.RemoveAt(0);
			Buddy buddy = BuddyList.pInstance.GetBuddy(challengeInfo.UserID.ToString());
			string text = "";
			if (buddy != null)
			{
				string displayName = buddy.DisplayName;
				if (!string.IsNullOrEmpty(displayName))
				{
					text = GetChallengeMessage(challengeInfo.ChallengeGameInfo.GameID, challengeInfo.Points, displayName);
				}
			}
			else
			{
				text = GetChallengeMessage(challengeInfo.ChallengeGameInfo.GameID, challengeInfo.Points, _NoDisplayNameText.GetLocalizedString());
				if (Nicknames.pInstance != null)
				{
					string nickname = Nicknames.pInstance.GetNickname(challengeInfo.UserID.ToString());
					if (!string.IsNullOrEmpty(nickname))
					{
						text = GetChallengeMessage(challengeInfo.ChallengeGameInfo.GameID, challengeInfo.Points, nickname);
					}
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				WsWebService.GetDisplayNameByUserID(challengeInfo.UserID.ToString(), ServiceEventHandler, challengeInfo);
			}
			else
			{
				UiChatHistory.AddSystemNotification(text, challengeInfo, OnInvitationAccept);
			}
		}
		else if (BuddyList.pInviteMessageList != null && BuddyList.pInviteMessageList.Count > 0)
		{
			AddSystemNotification(BuddyList.pInviteMessageList[0]);
		}
	}

	private void AddSystemNotification(MessageInfo activeMessage)
	{
		if (activeMessage.MessageTypeID.Value == 12)
		{
			WsWebService.SaveMessage(activeMessage.UserMessageQueueID.Value, isNew: false, isDeleted: false, null, null);
		}
		else if (activeMessage.MessageTypeID.Value == 19)
		{
			if (CommonInventoryData.pIsReady)
			{
				CommonInventoryData.ReInit();
			}
			WsWebService.SaveMessage(activeMessage.UserMessageQueueID.Value, isNew: false, isDeleted: false, null, null);
		}
		else
		{
			string data = activeMessage.Data;
			LocaleString localeString = new LocaleString("");
			mJoinOwnerSpace = false;
			BuddyInviteMessageTypeData[] messageData = _MessageData;
			foreach (BuddyInviteMessageTypeData buddyInviteMessageTypeData in messageData)
			{
				if (buddyInviteMessageTypeData._MessageType != (BuddyMessageType)activeMessage.MessageTypeID.Value)
				{
					continue;
				}
				localeString = buddyInviteMessageTypeData._DefaultMessageText;
				BuddyInviteMessageZoneData[] zoneSpecificData = buddyInviteMessageTypeData._ZoneSpecificData;
				foreach (BuddyInviteMessageZoneData buddyInviteMessageZoneData in zoneSpecificData)
				{
					if (data == buddyInviteMessageZoneData._ZoneName)
					{
						if (!string.IsNullOrEmpty(buddyInviteMessageZoneData._ZoneMessageText._Text))
						{
							localeString = buddyInviteMessageZoneData._ZoneMessageText;
						}
						mJoinOwnerSpace = buddyInviteMessageZoneData._JoinOwnerSpace;
						break;
					}
				}
			}
			if (!string.IsNullOrEmpty(localeString._Text))
			{
				activeMessage.MemberMessage = StringTable.GetStringData(localeString._ID, localeString._Text);
			}
		}
		TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(activeMessage);
		if (!taggedMessageHelper.MemberMessage.ContainsKey("Line1") || !taggedMessageHelper.MemberMessage["Line1"].Contains("{{BuddyUserName}}"))
		{
			return;
		}
		string text = null;
		Buddy buddy = BuddyList.pInstance.GetBuddy(activeMessage.FromUserID);
		if (buddy != null)
		{
			string displayName = buddy.DisplayName;
			if (!string.IsNullOrEmpty(displayName))
			{
				text = taggedMessageHelper.MemberMessage["Line1"].Replace("{{BuddyUserName}}", displayName);
			}
		}
		else
		{
			text = taggedMessageHelper.MemberMessage["Line1"].Replace("{{BuddyUserName}}", _NoDisplayNameText.GetLocalizedString());
		}
		if (string.IsNullOrEmpty(text))
		{
			WsWebService.GetDisplayNameByUserID(activeMessage.FromUserID, ServiceEventHandler, activeMessage);
		}
		else
		{
			UiChatHistory.AddSystemNotification(text, activeMessage, OnInvitationAccept);
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			string text = (string)inObject;
			if (inUserData == null)
			{
				break;
			}
			if (inUserData.GetType() == typeof(ChallengeInfo))
			{
				ChallengeInfo challengeInfo = (ChallengeInfo)inUserData;
				if (challengeInfo != null && !string.IsNullOrEmpty(text))
				{
					UiChatHistory.AddSystemNotification(GetChallengeMessage(challengeInfo.ChallengeGameInfo.GameID, challengeInfo.Points, text), inUserData, OnInvitationAccept);
				}
			}
			else
			{
				UiChatHistory.AddSystemNotification(new TaggedMessageHelper((MessageInfo)inUserData).MemberMessage["Line1"].Replace("{{BuddyUserName}}", text), inUserData, OnInvitationAccept);
			}
			break;
		}
		case WsServiceEvent.ERROR:
			UtDebug.Log("GetDisplayNameByUserID Service call failed", 0);
			break;
		}
	}

	public string GetChallengeMessage(int gameID, int points, string playerName)
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
			result = result.Replace("[Name]", playerName);
			result = result.Replace("[Time]", UtUtilities.GetTimerString(points));
			result = result.Replace("[Points]", points.ToString());
			result = result.Replace("[GameName]", ChallengeInfo.GetGameTitle(gameID));
		}
		return result;
	}

	public void OnInvitationAccept(object objData)
	{
		if (objData == null)
		{
			return;
		}
		if (objData.GetType() == typeof(MessageInfo))
		{
			MessageInfo messageInfo = (MessageInfo)objData;
			if (messageInfo.MessageTypeID.Value == -1)
			{
				AvAvatar.pState = AvAvatarState.PAUSED;
				AvAvatar.SetUIActive(inActive: false);
				string data = messageInfo.Data;
				RaisedPetStage rs = RaisedPetStage.NONE;
				PetActions petActions = PetActions.UNKNOWN;
				bool isPetRequired = false;
				BuddyInviteMessageTypeData[] messageData = _MessageData;
				for (int i = 0; i < messageData.Length; i++)
				{
					BuddyInviteMessageZoneData[] zoneSpecificData = messageData[i]._ZoneSpecificData;
					foreach (BuddyInviteMessageZoneData buddyInviteMessageZoneData in zoneSpecificData)
					{
						if (data == buddyInviteMessageZoneData._ZoneName)
						{
							isPetRequired = buddyInviteMessageZoneData._IsPetRequired;
							rs = buddyInviteMessageZoneData._PetStage;
							petActions = buddyInviteMessageZoneData._PetAction;
							break;
						}
					}
				}
				int num = 0;
				int num2 = 0;
				if (SanctuaryManager.pCurPetData != null)
				{
					num = RaisedPetData.GetAgeIndex(SanctuaryManager.pCurPetData.pStage);
					num2 = RaisedPetData.GetAgeIndex(rs);
				}
				if (IsVisitAllowed(isPetRequired, num, num2, petActions))
				{
					UiChatHistory.SystemMessageAccepted(objData);
					KAUICursorManager.SetDefaultCursor("Loading");
					if (mJoinOwnerSpace)
					{
						MainStreetMMOClient.pInstance.JoinOwnerSpace(messageInfo.Data, messageInfo.FromUserID);
					}
					else
					{
						BuddyList.pInstance.JoinBuddy(messageInfo.FromUserID, BuddyListEventHandler);
					}
					UserAchievementTask.Set(new AchievementTask(_SocialAcceptBuddyInviteAchievementID, messageInfo.FromUserID), new AchievementTask(messageInfo.FromUserID, _SocialAcceptBuddyInviteAchievementID, UserInfo.pInstance.UserID));
				}
				else
				{
					CheckConstraints(num, num2, petActions);
				}
			}
			else if (messageInfo.MessageTypeID.Value == 6)
			{
				if (MainStreetMMOClient.pInstance != null)
				{
					string ownerIDForLevel = MainStreetMMOClient.pInstance.GetOwnerIDForLevel(RsResourceManager.pCurrentLevel);
					if (messageInfo.Data != RsResourceManager.pCurrentLevel || messageInfo.FromUserID != ownerIDForLevel)
					{
						UiChatHistory.SystemMessageAccepted(objData);
						KAUICursorManager.SetDefaultCursor("Loading");
						AvAvatar.pState = AvAvatarState.PAUSED;
						AvAvatar.SetUIActive(inActive: false);
						MainStreetMMOClient.pInstance.JoinOwnerSpace(messageInfo.Data, messageInfo.FromUserID);
					}
				}
			}
			else if (messageInfo.MessageTypeID.Value == 19)
			{
				UiChatHistory.SystemMessageAccepted(objData);
				AvAvatar.SetUIActive(inActive: false);
				AvAvatar.pState = AvAvatarState.PAUSED;
				KAUICursorManager.SetDefaultCursor("Loading");
				string[] array = _UiGiftMessageURL.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], LoadGiftMessageDB, typeof(GameObject));
			}
			return;
		}
		ChallengeInfo challengeInfo = (ChallengeInfo)objData;
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		ChallengePetData petData = ChallengeInfo.GetPetData(challengeInfo.ChallengeGameInfo.GameID);
		if (petData != null && petData._IsPetRequired)
		{
			if (SanctuaryManager.pCurPetData == null)
			{
				ShowGenericDB("PfKAUIGenericDBSm", _NeedPetText.GetLocalizedString(), null, null, null, "KillGenericDB", null);
				return;
			}
			if (petData._BlockedStage != null)
			{
				ChallengePetBlockedStages[] blockedStage = petData._BlockedStage;
				foreach (ChallengePetBlockedStages challengePetBlockedStages in blockedStage)
				{
					if (SanctuaryManager.pCurPetData.pStage == challengePetBlockedStages._PetStage)
					{
						ShowGenericDB("PfKAUIGenericDBSm", challengePetBlockedStages._StageText.GetLocalizedString(), null, null, null, "KillGenericDB", null);
						return;
					}
				}
			}
			if (petData._PetAction != null)
			{
				ChallengeRequiredPetAction[] petAction = petData._PetAction;
				foreach (ChallengeRequiredPetAction challengeRequiredPetAction in petAction)
				{
					if (!SanctuaryManager.IsActionAllowed(SanctuaryManager.pCurPetData, (PetActions)Enum.Parse(typeof(PetActions), challengeRequiredPetAction._RequiredPetAction)))
					{
						ShowGenericDB("PfKAUIGenericDBSm", challengeRequiredPetAction._Text.GetLocalizedString(), null, null, null, "KillGenericDB", null);
						return;
					}
				}
			}
		}
		if (challengeInfo.GetState() == ChallengeState.Initiated)
		{
			UserAchievementTask.Set(_AcceptChallengeAchievementID);
		}
		UiChatHistory.SystemMessageAccepted(objData);
		WsWebService.AcceptChallenge(challengeInfo.ChallengeID, -1, null, null);
		ChallengeInfo.pActiveChallenge = challengeInfo;
		AvAvatar.SetActive(inActive: false);
		PetPlayAreaLoader._ExitToScene = RsResourceManager.pCurrentLevel;
		string sceneName = ChallengeInfo.GetSceneName(challengeInfo.ChallengeGameInfo.GameID);
		if (!RsResourceManager.pCurrentLevel.Equals(sceneName))
		{
			AvAvatar.SetStartPositionAndRotation();
		}
		RsResourceManager.LoadLevel(sceneName);
	}

	private bool IsVisitAllowed(bool isPetRequired, int currPetAge, int zonePetAge, PetActions zonePetAction)
	{
		if (!isPetRequired)
		{
			return true;
		}
		if (SanctuaryManager.pCurPetData != null && currPetAge >= zonePetAge && SanctuaryManager.IsActionAllowed(SanctuaryManager.pCurPetData, zonePetAction))
		{
			return true;
		}
		return false;
	}

	private void CheckConstraints(int inDragonAge, int allowedAge, PetActions inPetAction)
	{
		if (SanctuaryManager.pCurPetData == null)
		{
			ShowGenericDB("PfKAUIGenericDBSm", _NeedPetText.GetLocalizedString(), null, null, null, "KillGenericDB", null);
		}
		else if (inDragonAge < allowedAge)
		{
			ShowGenericDB("PfKAUIGenericDBSm", _StageBlockedText.GetLocalizedString(), null, null, null, "KillGenericDB", null);
		}
		else if (!SanctuaryManager.IsActionAllowed(SanctuaryManager.pCurPetData, inPetAction))
		{
			ShowGenericDB("PfKAUIGenericDBSm", _TooTiredText.GetLocalizedString(), null, null, null, "KillGenericDB", null);
		}
		else
		{
			OnClose();
		}
	}

	public void LoadGiftMessageDB(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
			}
			else
			{
				CloseGiftMessage(null);
			}
			break;
		case RsResourceLoadEvent.ERROR:
			CloseGiftMessage(null);
			break;
		}
	}

	public void CloseGiftMessage(MessageInfo messageInfo)
	{
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	private void RemoveStoredMessage(MessageInfo mInfo, bool markDeleted)
	{
		if (mInfo != null && mInfo.MessageTypeID.HasValue)
		{
			if (markDeleted)
			{
				WsWebService.SaveMessage(mInfo.UserMessageQueueID.Value, isNew: false, isDeleted: true, null, null);
			}
			List<MessageInfo> list = (List<MessageInfo>)WsUserMessage.pInstance.GetStoredMessages(mInfo.MessageTypeID.Value);
			if (list != null)
			{
				list.Remove(mInfo);
				WsUserMessage.pInstance.SetStoredMessage(mInfo.MessageTypeID.Value, list);
			}
		}
	}

	public virtual void BuddyListEventHandler(WsServiceType inType, object inResult)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (inType == WsServiceType.GET_BUDDY_LOCATION)
		{
			if (inResult == null || (JoinBuddyResultType)inResult == JoinBuddyResultType.JoinFailedCommon)
			{
				ShowGenericDB("PfKAUIGenericDBSm", _BuddyLocationErrorText.GetLocalizedString(), null, null, null, "KillGenericDB", null);
				return;
			}
		}
		else if (inResult == null)
		{
			ShowGenericDB("PfKAUIGenericDBSm", _GenericErrorText.GetLocalizedString(), null, null, null, "KillGenericDB", null);
			return;
		}
		OnClose();
	}

	public virtual void OnClose()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
	}

	public void RestoreUI()
	{
	}

	private void ShowGenericDB(string inGenericDBName, string inString, string inTitle, string inYesMessage, string inNoMessage, string inOkMessage, string inCloseMessage)
	{
		mKAUIGenericDB = GameUtilities.DisplayGenericDB(inGenericDBName, inString, inTitle, base.gameObject, inYesMessage, inNoMessage, inOkMessage, inCloseMessage);
	}

	private void KillGenericDB()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
		OnClose();
	}

	public void OnDestroy()
	{
		if (BuddyList.pIsReady && BuddyList.pInstance.pEventDelegate != null)
		{
			BuddyList pInstance = BuddyList.pInstance;
			pInstance.pEventDelegate = (BuddyListEventHandler)Delegate.Remove(pInstance.pEventDelegate, new BuddyListEventHandler(BuddyListEventHandler));
		}
	}
}
