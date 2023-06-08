using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class WsUserMessage : MonoBehaviour
{
	public class MessageInfoData
	{
		public MessageInfo _MessageInfo;

		public int _NumPendingWSCalls;

		public MessageInfoData(MessageInfo messageInfo)
		{
			_MessageInfo = messageInfo;
		}
	}

	public int _AlertCount = 1;

	public int _AlertMessageLength = 15;

	public bool _CheckOnAwake = true;

	public string _HouseLevel = "FarmingDO";

	public string _ProfileSelectionScene = "ProfileSelectionDO";

	public string _AchievementDBAssetName;

	public string _SocialRankDBAssetName;

	public string _MissionDBAssetName = "";

	public AudioClip _AchievementRibbonClip;

	public AudioClip _Clip;

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public GameObject _AchievementMsgBox;

	public TagAndDefaultText[] _TagAndDefaultText;

	public Vector2 _AlertOffset;

	public LocaleString _JoinBuddyErrorText = new LocaleString("You cannot visit your Friend at this time.");

	public LocaleString _TurnOnMMOText = new LocaleString("[REVIEW] Please turn on MMO from settings to accept friend request.");

	public LocaleString _ViewGiftActionText = new LocaleString("[REVIEW] view your gifts");

	private static WsUserMessage mInstance = null;

	private static bool mIsReady = true;

	private static GameObject mNotifyObject = null;

	private GenericMessage mMessage;

	private bool mReinitUserRank;

	private bool mReinitUserMoney;

	private bool mIsForceUpdate;

	private Dictionary<int, object> mMessages = new Dictionary<int, object>();

	private Dictionary<int, object> mStoredMessages = new Dictionary<int, object>();

	private List<MessageInfo> mAlerts = new List<MessageInfo>();

	private int mMessageTypeWanted = -1;

	private float mForceUpdateTime;

	private bool mAddParentMessages;

	private static bool mBlockMessages = false;

	private static bool mForceShowMessages = false;

	private static bool mPauseMessages = false;

	public static WsUserMessage pInstance => mInstance;

	public static bool pIsReady => mIsReady;

	private bool pCanShowMessage
	{
		get
		{
			if (!mForceShowMessages)
			{
				if (!mBlockMessages && !RsResourceManager.pLevelLoadingScreen && AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pInputEnabled && AvAvatar.pToolbar != null && AvAvatar.pToolbar.activeInHierarchy)
				{
					return InteractiveTutManager._CurrentActiveTutorialObject == null;
				}
				return false;
			}
			return true;
		}
	}

	public static bool pBlockMessages
	{
		get
		{
			return mBlockMessages;
		}
		set
		{
			mBlockMessages = value;
		}
	}

	public static bool pForceShowMessages
	{
		get
		{
			return mForceShowMessages;
		}
		set
		{
			mForceShowMessages = value;
		}
	}

	public static bool pPauseMessages
	{
		get
		{
			return mPauseMessages;
		}
		set
		{
			mPauseMessages = value;
		}
	}

	public static void ShowMessage(GameObject go)
	{
		mNotifyObject = go;
		if (mInstance != null && mInstance.pCanShowMessage)
		{
			mInstance.ShowNextMessage(firstMessage: true);
		}
		else if (mNotifyObject != null)
		{
			mNotifyObject.SendMessage("OnNotifyUIClosed", SendMessageOptions.DontRequireReceiver);
			mNotifyObject = null;
		}
	}

	private void Awake()
	{
		mInstance = this;
		mIsReady = !_CheckOnAwake;
		if (_CheckOnAwake)
		{
			ReCheckUserMessage();
		}
		else
		{
			mAddParentMessages = true;
		}
	}

	public void RemoveMessageWithoutShowing(int inMessageType, bool inDelete)
	{
		if (mMessages.ContainsKey(inMessageType))
		{
			MessageInfo inMessageInfo = (MessageInfo)mMessages[inMessageType];
			SaveMessage(inMessageInfo, inDelete);
			mMessages.Remove(inMessageType);
		}
	}

	public void ShowMessage(int inMessageType, bool delete)
	{
		if (!mMessages.ContainsKey(inMessageType))
		{
			return;
		}
		mMessage = null;
		switch (inMessageType)
		{
		case 25:
			mMessage = new GenericMessage((MessageInfo)mMessages[inMessageType], null);
			mMessage._IsSystemMessage = true;
			mMessages.Remove(inMessageType);
			break;
		case 24:
		{
			MessageInfo messageInfo = (MessageInfo)mMessages[inMessageType];
			if (new TaggedMessageHelper(messageInfo).SubType == "Subscription")
			{
				mMessage = new RenewSubscriptionMessage(messageInfo);
				mMessages.Remove(inMessageType);
			}
			break;
		}
		case 35:
		{
			List<MessageInfo> list2 = (List<MessageInfo>)mMessages[inMessageType];
			mMessage = new SystemMessage(list2[0], null);
			RemoveFirstMessage(35);
			break;
		}
		case 34:
		{
			List<MessageInfo> list = (List<MessageInfo>)mMessages[inMessageType];
			mMessage = new PromoMessage(list[0], null);
			RemoveFirstMessage(34);
			break;
		}
		}
		if (mMessage == null)
		{
			mMessage = new GenericMessage((MessageInfo)mMessages[inMessageType], null);
			mMessage._IsSystemMessage = true;
			mMessages.Remove(inMessageType);
		}
		mMessage.Save(delete);
		mMessage.Show();
	}

	private void RemoveFirstMessage(int messageTypeID)
	{
		List<MessageInfo> list = (List<MessageInfo>)mMessages[messageTypeID];
		list.RemoveAt(0);
		if (list.Count == 0)
		{
			mMessages.Remove(messageTypeID);
		}
		else
		{
			mMessages[messageTypeID] = list;
		}
	}

	public void SaveMessage(MessageInfo inMessageInfo, bool inDelete)
	{
		if (inMessageInfo.ParentMessage)
		{
			string pUserToken = WsWebService.pUserToken;
			WsWebService.SetToken(ProductConfig.pToken);
			WsWebService.SaveMessage(inMessageInfo.UserMessageQueueID.Value, isNew: false, inDelete, null, null);
			ParentData.pInstance.RemoveMessage(inMessageInfo.UserMessageQueueID);
			WsWebService.SetToken(pUserToken);
		}
		else
		{
			WsWebService.SaveMessage(inMessageInfo.UserMessageQueueID.Value, isNew: false, inDelete, null, null);
		}
	}

	public void ShowNextMessage(bool firstMessage)
	{
		if (mMessages.Count == 0)
		{
			if (!firstMessage && !RsResourceManager.pCurrentLevel.Equals(_ProfileSelectionScene))
			{
				AvAvatar.pState = AvAvatarState.IDLE;
				AvAvatar.SetUIActive(inActive: true);
			}
			if (mNotifyObject != null)
			{
				mNotifyObject.SendMessage("OnNotifyUIClosed", SendMessageOptions.DontRequireReceiver);
				mNotifyObject = null;
			}
			ShowAlerts();
			return;
		}
		if (mMessages.ContainsKey(13))
		{
			MessageInfo messageInfo = ((List<MessageInfo>)mMessages[13])[0];
			mMessage = new MissionMessage(messageInfo, _MissionDBAssetName);
			mMessage._CloseBtnVisible = true;
			mMessage.Save();
			ReInitUserRankData();
			RemoveFirstMessage(13);
		}
		else if (mMessages.ContainsKey(4))
		{
			MessageInfo messageInfo2 = ((List<MessageInfo>)mMessages[4])[0];
			mMessage = new RankMessage(messageInfo2);
			mMessage._IsSystemMessage = true;
			RemoveFirstMessage(4);
		}
		else if (mMessages.ContainsKey(11))
		{
			mMessage = new SocialRankMessage((MessageInfo)mMessages[11], _SocialRankDBAssetName);
			mMessages.Remove(11);
		}
		else if (mMessages.ContainsKey(25))
		{
			mMessage = new GenericMessage((MessageInfo)mMessages[25], null);
			mMessage._IsSystemMessage = true;
			mMessage.Save();
			mMessages.Remove(25);
		}
		else if (mMessages.ContainsKey(8))
		{
			MessageInfo messageInfo3 = ((List<MessageInfo>)mMessages[8])[0];
			mMessage = new PrizeCodeMessage(messageInfo3, null);
			mMessage.Save();
			mMessage._Tagged = true;
			mMessage._YesBtnVisible = false;
			mMessage._NoBtnVisible = false;
			mMessage._OKBtnVisible = true;
			RemoveFirstMessage(8);
		}
		else if (mMessages.ContainsKey(9))
		{
			MessageInfo messageInfo4 = ((List<MessageInfo>)mMessages[9])[0];
			mMessage = new AchievementMessage(messageInfo4, _AchievementDBAssetName);
			mMessage.Save(delete: false);
			RemoveFirstMessage(9);
		}
		else if (mMessages.ContainsKey(30))
		{
			MessageInfo messageInfo5 = ((List<MessageInfo>)mMessages[30])[0];
			string inAssetPath = "";
			TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(messageInfo5);
			if (taggedMessageHelper.MemberMessage.ContainsKey("Prefab"))
			{
				inAssetPath = taggedMessageHelper.MemberMessage["Prefab"];
			}
			mMessage = new AchievementMessage(messageInfo5, inAssetPath);
			mMessage.Save(delete: false);
			RemoveFirstMessage(30);
		}
		else if (mMessages.ContainsKey(15))
		{
			MessageInfo messageInfo6 = ((List<MessageInfo>)mMessages[15])[0];
			mMessage = new InviteFriendMessage(messageInfo6);
			mMessage._Tagged = true;
			mMessage.Save();
			RemoveFirstMessage(15);
		}
		else if (mMessages.ContainsKey(12))
		{
			MessageInfo messageInfo7 = ((List<MessageInfo>)mMessages[12])[0];
			TaggedMessageHelper taggedMessageHelper2 = new TaggedMessageHelper(messageInfo7);
			if (!string.IsNullOrEmpty(taggedMessageHelper2.SubType) && taggedMessageHelper2.SubType == "SocialBox")
			{
				mMessage = new GenericMessage(messageInfo7);
				mMessage._IsSystemMessage = true;
				mMessage._Tagged = true;
				mMessage.Save();
				RemoveFirstMessage(12);
			}
		}
		else if (mMessages.ContainsKey(34))
		{
			List<MessageInfo> list = (List<MessageInfo>)mMessages[34];
			mMessage = new PromoMessage(list[0], null);
			mMessage.Save();
			RemoveFirstMessage(34);
		}
		else if (mMessages.ContainsKey(35))
		{
			List<MessageInfo> list2 = (List<MessageInfo>)mMessages[35];
			mMessage = new SystemMessage(list2[0], null);
			mMessage.Save();
			RemoveFirstMessage(35);
		}
		else
		{
			int num = 3;
			using (Dictionary<int, object>.KeyCollection.Enumerator enumerator = mMessages.Keys.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					num = enumerator.Current;
				}
			}
			mMessage = new GenericMessage((MessageInfo)mMessages[num], _Clip);
			mMessage._IsSystemMessage = true;
			if (num == 6)
			{
				mMessage._Tagged = true;
			}
			mMessages.Remove(num);
			if (mMessage.pMessageInfo.ParentMessage)
			{
				ParentData.pInstance.RemoveMessage(mMessage.pMessageInfo.UserMessageQueueID);
			}
		}
		if (firstMessage && !mMessage._IsSystemMessage && !RsResourceManager.pCurrentLevel.Equals(_ProfileSelectionScene))
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
		}
		mMessage.Show();
	}

	public void ReplaceTagWithUserNameByID(string userID, string tagName, MessageInfoData messageInfoData)
	{
		MessageInfo messageInfo = messageInfoData._MessageInfo;
		if (!messageInfo.MemberMessage.Contains(tagName))
		{
			return;
		}
		bool flag = false;
		if (BuddyList.pIsReady)
		{
			Buddy buddy = BuddyList.pInstance.GetBuddy(userID);
			if (buddy != null && !string.IsNullOrEmpty(buddy.DisplayName))
			{
				flag = true;
				messageInfo.MemberMessage = messageInfo.MemberMessage.Replace(tagName, buddy.DisplayName);
			}
			else if (userID.Equals(UserProfile.pProfileData.GetGroupID()))
			{
				flag = true;
				messageInfo.MemberMessage = messageInfo.MemberMessage.Replace(tagName, "{{GroupName}}");
			}
		}
		if (!flag)
		{
			List<object> list = new List<object>();
			messageInfoData._NumPendingWSCalls++;
			list.Add(messageInfoData);
			list.Add(tagName);
			WsWebService.GetDisplayNameByUserID(userID, ServiceEventHandler, list);
		}
	}

	public string ReplaceTagsWithDefault(string message)
	{
		TagAndDefaultText[] tagAndDefaultText = _TagAndDefaultText;
		foreach (TagAndDefaultText tagAndDefaultText2 in tagAndDefaultText)
		{
			message = message.Replace(tagAndDefaultText2._Tag, tagAndDefaultText2._DefaultText.GetLocalizedString());
		}
		return message;
	}

	public void ShowAlerts()
	{
		if (mAlerts.Count > 0)
		{
			ReInitUserRankData();
		}
		for (int i = 0; i < mAlerts.Count; i++)
		{
			MessageInfoData messageInfoData = new MessageInfoData(mAlerts[0]);
			mAlerts.RemoveAt(0);
			int num = -1;
			if (messageInfoData._MessageInfo.MessageTypeID.HasValue)
			{
				num = messageInfoData._MessageInfo.MessageTypeID.Value;
			}
			bool inDelete = CanDeleteMessage(messageInfoData._MessageInfo);
			SaveMessage(messageInfoData._MessageInfo, inDelete);
			MessageInfoData messageInfoData2 = new MessageInfoData(messageInfoData._MessageInfo);
			ReplaceTagWithUserNameByID(messageInfoData2._MessageInfo.FromUserID, "{{BuddyUserName}}", messageInfoData2);
			if (messageInfoData._MessageInfo.MemberMessage.Contains("{{OwnerUserName}}"))
			{
				Dictionary<string, string> dictionary = TaggedMessageHelper.Match(messageInfoData2._MessageInfo.Data);
				if (dictionary.ContainsKey("OwnerID"))
				{
					ReplaceTagWithUserNameByID(dictionary["OwnerID"], "{{OwnerUserName}}", messageInfoData2);
				}
			}
			messageInfoData._NumPendingWSCalls = messageInfoData2._NumPendingWSCalls;
			if (messageInfoData._MessageInfo.MemberMessage.Contains("{{Message}}"))
			{
				Dictionary<string, string> dictionary2 = TaggedMessageHelper.Match(messageInfoData2._MessageInfo.Data);
				if (dictionary2.ContainsKey("Message"))
				{
					if (dictionary2["Message"].Length <= _AlertMessageLength)
					{
						messageInfoData._MessageInfo.MemberMessage = messageInfoData2._MessageInfo.MemberMessage.Replace("{{Message}}", dictionary2["Message"]);
					}
					else
					{
						messageInfoData._MessageInfo.MemberMessage = messageInfoData2._MessageInfo.MemberMessage.Replace("{{Message}}", dictionary2["Message"].Substring(0, _AlertMessageLength) + "...");
					}
				}
			}
			TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(messageInfoData._MessageInfo);
			string line = ReplaceTagsWithDefault(taggedMessageHelper.MemberMessage["Line1"]);
			switch (num)
			{
			case 9:
			case 30:
				if ((taggedMessageHelper.SubType == "Medal" || taggedMessageHelper.SubType == "Ribbon") && (bool)_AchievementRibbonClip && !SnChannel.FindChannel("VO_Pool", "VOChannel").pIsPlaying)
				{
					SnChannel.Play(_AchievementRibbonClip, "VO_Pool", inForce: true, base.gameObject);
				}
				if (messageInfoData._MessageInfo.Data != null && num == 9)
				{
					bool pDisabled = RewardManager.pDisabled;
					RewardManager.pDisabled = true;
					RewardManager.SetReward(messageInfoData._MessageInfo.Data, inImmediateShow: false, Vector2.zero);
					RewardManager.pDisabled = pDisabled;
				}
				break;
			case 19:
				if (messageInfoData._NumPendingWSCalls == 0)
				{
					UiChatHistory.AddSystemNotification(line, messageInfoData._MessageInfo, OnSystemMessageClicked, ignoreDuplicateMessage: false, _ViewGiftActionText.GetLocalizedString());
					continue;
				}
				break;
			case 28:
			{
				string groupID = UserProfile.pProfileData.GetGroupID();
				string text = null;
				Dictionary<string, string> dictionary4 = TaggedMessageHelper.Match(messageInfoData._MessageInfo.Data);
				if (dictionary4.ContainsKey("GroupID"))
				{
					text = dictionary4["GroupID"];
				}
				if (messageInfoData._MessageInfo.MemberMessage.Contains("{{yourclan}}"))
				{
					Group group = Group.GetGroup(groupID);
					if (group != null)
					{
						messageInfoData._MessageInfo.MemberMessage = messageInfoData._MessageInfo.MemberMessage.Replace("{{yourclan}}", group.Name);
					}
					else
					{
						GetGroup(messageInfoData._MessageInfo.FromUserID, messageInfoData2, "{{yourclan}}");
					}
				}
				if (messageInfoData._MessageInfo.MemberMessage.Contains("{{clanname}}"))
				{
					Group group2 = Group.GetGroup(text);
					if (group2 != null)
					{
						messageInfoData._MessageInfo.MemberMessage = messageInfoData._MessageInfo.MemberMessage.Replace("{{clanname}}", group2.Name);
					}
					else
					{
						GetGroup(messageInfoData._MessageInfo.FromUserID, messageInfoData2, "{{clanname}}");
					}
				}
				if (groupID != null && groupID == text)
				{
					if (taggedMessageHelper.SubType == "Kicked")
					{
						UserProfile.pProfileData.RemoveGroup(groupID);
					}
					else if (taggedMessageHelper.SubType == "Demoted")
					{
						UserProfile.pProfileData.Groups[0].RoleID = 1;
					}
					else if (taggedMessageHelper.SubType == "PromotedToElder")
					{
						UserProfile.pProfileData.Groups[0].RoleID = 2;
					}
					else if (taggedMessageHelper.SubType == "PromotedToLeader")
					{
						UserProfile.pProfileData.Groups[0].RoleID = 3;
					}
				}
				if (text != null && taggedMessageHelper.SubType == "Approved")
				{
					UserProfile.pProfileData.ReplaceGroup(0, text, UserRole.Member);
				}
				taggedMessageHelper = new TaggedMessageHelper(messageInfoData._MessageInfo);
				line = ReplaceTagsWithDefault(taggedMessageHelper.MemberMessage["Line1"]);
				break;
			}
			case 29:
			{
				Dictionary<string, string> dictionary3 = TaggedMessageHelper.Match(messageInfoData._MessageInfo.Data);
				if (dictionary3.ContainsKey("GemsCount"))
				{
					string s = dictionary3["GemsCount"];
					int result = 0;
					if (int.TryParse(s, out result))
					{
						Money.AddToCashCurrency(result);
					}
				}
				break;
			}
			}
			if (messageInfoData._NumPendingWSCalls == 0)
			{
				UiChatHistory.AddSystemNotification(line, messageInfoData._MessageInfo, OnSystemMessageClicked);
			}
		}
	}

	public void OnSystemMessageClicked(object message)
	{
		MessageInfo messageInfo = (MessageInfo)message;
		if (messageInfo == null)
		{
			return;
		}
		TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(messageInfo);
		switch (messageInfo.MessageTypeID.Value)
		{
		case 9:
		case 30:
			if (taggedMessageHelper.SubType == "Medal")
			{
				using (StringReader textReader = new StringReader(messageInfo.Data))
				{
					if (new XmlSerializer(typeof(RewardData)).Deserialize(textReader) is RewardData rewardData && rewardData.TaskGroupID.HasValue)
					{
						UiAchievements.SelectedAchievementGroupID = rewardData.TaskGroupID.Value;
					}
				}
				if (messageInfo.MessageTypeID.Value == 9)
				{
					JournalLoader.Load("BtnAchievements", "", setDefaultMenuItem: true, null);
				}
				else
				{
					UiClans.ShowClan(UserInfo.pInstance.UserID, null, ClanTabs.ACHIEVEMENTS);
				}
			}
			else if (taggedMessageHelper.SubType == "Social")
			{
				LaunchPDA("Star Status");
			}
			else if (taggedMessageHelper.SubType == "UDTAchievement")
			{
				JournalLoader.Load("UDTLeaderBoardBtn", "", setDefaultMenuItem: true, null, resetLastSceneRef: true, UILoadOptions.AUTO, "OpenUDTPage");
			}
			break;
		case 3:
			if (taggedMessageHelper.SubType == "Challenge")
			{
				MessageBoardLoader.Load(UserInfo.pInstance.UserID);
			}
			break;
		case 21:
		{
			Dictionary<string, string> dictionary = TaggedMessageHelper.Match(messageInfo.Data);
			if (!dictionary.ContainsKey("OwnerID"))
			{
				break;
			}
			int result = -1;
			if (dictionary.ContainsKey("MessageID"))
			{
				int.TryParse(dictionary["MessageID"], out result);
			}
			string text = dictionary["OwnerID"];
			if (result != -1)
			{
				if (text == UserProfile.pProfileData.GetGroupID())
				{
					UiClans.ShowClan(UserInfo.pInstance.UserID);
				}
				else
				{
					MessageBoardLoader.Load(text, result);
				}
			}
			else
			{
				MessageBoardLoader.Load(text);
			}
			break;
		}
		case 19:
			UiChatHistory.SystemMessageAccepted(message);
			MessageBoardLoader.Load(UserInfo.pInstance.UserID, -1, CombinedMessageType.USER_MESSAGE_QUEUE, 19, UILoadOptions.AUTO, isMessageBoardUI: true);
			break;
		case 31:
			UiClans.ShowClan(UserInfo.pInstance.UserID);
			break;
		case 5:
		case 27:
			if (BuddyList.pInstance != null)
			{
				UiBuddyMessage.LoadBuddyMessageDB(messageInfo);
			}
			else
			{
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _TurnOnMMOText.GetLocalizedString(), null, "");
			}
			break;
		case 25:
			if (AvAvatar.pToolbar != null)
			{
				UiJournalCustomization._OpenedByExternal = true;
				AvAvatar.pState = AvAvatarState.IDLE;
				JournalLoader.Load("EquipBtn", "", setDefaultMenuItem: true, null, resetLastSceneRef: false);
			}
			UiAvatarCustomization._SelectTab = "ClothesBtn";
			break;
		case 12:
			if (!MainStreetMMOClient.pIsMMOEnabled || !MainStreetMMOClient.pInstance.JoinOwnerSpace(_HouseLevel, messageInfo.FromUserID))
			{
				AvAvatar.pState = AvAvatarState.PAUSED;
				AvAvatar.SetUIActive(inActive: false);
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _JoinBuddyErrorText.GetLocalizedString(), "", pInstance.gameObject, "", "", "OnClose", "OnClose", inDestroyOnClick: true);
			}
			break;
		case 15:
			UiChatHistory.SystemMessageAccepted(message);
			mMessage = new InviteFriendMessage(messageInfo);
			((InviteFriendMessage)mMessage).LoadInviteFriendDB(disableUI: true);
			break;
		}
	}

	private void LaunchPDA(string tabName)
	{
		UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiPDA")).name = "PfUiPDA";
	}

	private void GetGroup(string inUserID, MessageInfoData messageInfoData, string inPattern)
	{
		Dictionary<string, MessageInfoData> dictionary = new Dictionary<string, MessageInfoData>();
		dictionary.Add(inPattern, messageInfoData);
		Group.Get(inUserID, OnGetGroup, dictionary);
	}

	private void OnGetGroup(GetGroupsResult result, object inUserData)
	{
		if (!result.Success || result.Groups == null || result.Groups.Length == 0)
		{
			return;
		}
		Dictionary<string, MessageInfoData> dictionary = (Dictionary<string, MessageInfoData>)inUserData;
		if (dictionary == null)
		{
			return;
		}
		Group group = result.Groups[0];
		foreach (KeyValuePair<string, MessageInfoData> item in dictionary)
		{
			string memberMessage = item.Value._MessageInfo.MemberMessage;
			if (!memberMessage.Contains(item.Key))
			{
				continue;
			}
			Dictionary<string, string> dictionary2 = TaggedMessageHelper.Match(item.Value._MessageInfo.Data);
			if (dictionary2.ContainsKey("GroupID"))
			{
				string text = dictionary2["GroupID"];
				Group[] groups = result.Groups;
				foreach (Group group2 in groups)
				{
					if (group2.GroupID == text)
					{
						group = group2;
						break;
					}
				}
			}
			item.Value._MessageInfo.MemberMessage = memberMessage.Replace(item.Key, group.Name);
			item.Value._NumPendingWSCalls--;
			if (item.Value._NumPendingWSCalls <= 0)
			{
				ShowSystemMessage(item.Value._MessageInfo);
			}
		}
	}

	private bool CanDeleteMessage(MessageInfo info)
	{
		int num = (info.MessageTypeID.HasValue ? info.MessageTypeID.Value : (-1));
		if (num == 9 || num == 19 || num == 12 || num == 11 || num == 4 || num == 21 || num == 30)
		{
			return false;
		}
		return true;
	}

	public void ReCheckUserMessage()
	{
		mMessages.Clear();
		mAlerts.Clear();
		BuddyList.ClearMessages();
		mStoredMessages.Clear();
		mReinitUserRank = true;
		mReinitUserMoney = true;
		WsWebService.GetUserMessageQueue(showOld: false, showDeleted: false, ServiceEventHandler, null);
	}

	public void ForceUserMessageUpdate()
	{
		if (mIsReady)
		{
			mIsForceUpdate = true;
			mIsReady = false;
			ReCheckUserMessage();
		}
	}

	public void ForceUserMessageUpdate(int messageTypeWanted, GameObject go)
	{
		mNotifyObject = go;
		mMessageTypeWanted = messageTypeWanted;
		ForceUserMessageUpdate();
	}

	public void ReInitUserRankData()
	{
		if (!mReinitUserRank)
		{
			return;
		}
		mReinitUserRank = false;
		UserRankData.ReInit();
		if (AvAvatar.pToolbar != null)
		{
			UiToolbar component = AvAvatar.pToolbar.GetComponent<UiToolbar>();
			if (component != null)
			{
				component.UpdateRankFromServer();
			}
		}
	}

	public void ReInitUserMoney()
	{
		if (mReinitUserMoney)
		{
			mReinitUserMoney = false;
			Money.UpdateMoneyFromServer();
		}
	}

	public void OnClose()
	{
		if (mMessage != null)
		{
			mMessage.Close();
		}
		mMessage = null;
		ShowNextMessage(firstMessage: false);
	}

	public void OnSnEvent()
	{
		OnCloseMessage();
	}

	public void OnYes()
	{
		mMessage.Yes();
	}

	public void OnNo()
	{
		mMessage.No();
	}

	public void OnCloseMessage()
	{
		if (mMessage != null)
		{
			mMessage.Close();
		}
		mMessage = null;
	}

	private void SocialBoxInviteHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && inType == WsServiceType.GET_DISPLAYNAME_BY_USER_ID && !string.IsNullOrEmpty((string)inObject) && inUserData != null)
		{
			MessageInfo info = (MessageInfo)inUserData;
			AddSocialBoxMessage(info, (string)inObject);
			mIsForceUpdate = true;
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType == WsServiceType.GET_USER_MESSAGE_QUEUE)
		{
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				mIsReady = true;
				mForceUpdateTime = 2f;
				mAddParentMessages = true;
				ArrayOfMessageInfo arrayOfMessageInfo = (ArrayOfMessageInfo)inObject;
				if (arrayOfMessageInfo != null && arrayOfMessageInfo.MessageInfo != null)
				{
					AddMessages(arrayOfMessageInfo);
				}
				BuddyList.MessagesReceived();
				break;
			}
			case WsServiceEvent.ERROR:
				mIsReady = true;
				BuddyList.MessagesReceived();
				UtDebug.LogError("WEB SERVICE CALL GET_USER_MESSAGE_QUEUE FAILED!!!");
				break;
			}
		}
		if (inType != WsServiceType.GET_DISPLAYNAME_BY_USER_ID || inEvent != WsServiceEvent.COMPLETE || inUserData == null || string.IsNullOrEmpty((string)inObject))
		{
			return;
		}
		List<object> list = (List<object>)inUserData;
		if (list.Count <= 0)
		{
			return;
		}
		string oldValue = "";
		if (list.Count > 1)
		{
			oldValue = (string)list[1];
		}
		MessageInfoData messageInfoData = (MessageInfoData)list[0];
		if (messageInfoData == null)
		{
			return;
		}
		MessageInfo messageInfo = messageInfoData._MessageInfo;
		if (BuddyList.pIsReady)
		{
			Buddy buddy = BuddyList.pInstance.GetBuddy(messageInfo.FromUserID);
			if (buddy != null)
			{
				buddy.DisplayName = (string)inObject;
			}
		}
		messageInfo.MemberMessage = messageInfo.MemberMessage.Replace(oldValue, (string)inObject);
		messageInfoData._NumPendingWSCalls--;
		if (messageInfoData._NumPendingWSCalls <= 0)
		{
			ShowSystemMessage(messageInfo);
		}
	}

	private void AddMessages(ArrayOfMessageInfo inMessages, bool inParentMessages = false)
	{
		for (int i = 0; i < inMessages.MessageInfo.Length; i++)
		{
			MessageInfo messageInfo = inMessages.MessageInfo[i];
			messageInfo.ParentMessage = inParentMessages;
			int num = -1;
			if (messageInfo.MessageTypeID.HasValue)
			{
				num = messageInfo.MessageTypeID.Value;
			}
			if (mMessageTypeWanted >= 0 && mMessageTypeWanted == num)
			{
				mMessageTypeWanted = -1;
				if (mNotifyObject != null)
				{
					mNotifyObject.SendMessage("OnUserMessageReceived", SendMessageOptions.DontRequireReceiver);
				}
			}
			switch (num)
			{
			case 5:
			case 27:
				if (!UiChatHistory.IsSystemMessageShown(messageInfo))
				{
					UiChatHistory.AddAsFirstSessionSystemMessage(messageInfo);
					MessageInfoData messageInfoData = new MessageInfoData(messageInfo);
					if (messageInfo.MessageTypeID == 27)
					{
						string inGroupID = null;
						string inGroupID2 = null;
						if (UserProfile.pProfileData.HasGroup())
						{
							inGroupID = UserProfile.pProfileData.Groups[0].GroupID;
						}
						Dictionary<string, string> dictionary2 = TaggedMessageHelper.Match(messageInfo.Data);
						if (dictionary2.ContainsKey("GroupID"))
						{
							inGroupID2 = dictionary2["GroupID"];
						}
						if (messageInfo.MemberMessage.Contains("{{yourclan}}"))
						{
							Group group = Group.GetGroup(inGroupID);
							if (group != null)
							{
								messageInfo.MemberMessage = messageInfo.MemberMessage.Replace("{{yourclan}}", group.Name);
							}
						}
						if (messageInfo.MemberMessage.Contains("{{clanname}}"))
						{
							Group group2 = Group.GetGroup(inGroupID2);
							if (group2 != null)
							{
								messageInfo.MemberMessage = messageInfo.MemberMessage.Replace("{{clanname}}", group2.Name);
							}
							else
							{
								messageInfoData._NumPendingWSCalls++;
								GetGroup(messageInfo.FromUserID, messageInfoData, "{{clanname}}");
							}
						}
					}
					Buddy buddy = null;
					if (BuddyList.pIsReady)
					{
						buddy = BuddyList.pInstance.GetBuddy(messageInfo.FromUserID);
					}
					if (buddy != null && !string.IsNullOrEmpty(buddy.DisplayName))
					{
						messageInfo.MemberMessage = messageInfo.MemberMessage.Replace("{{BuddyUserName}}", buddy.DisplayName);
					}
					else
					{
						messageInfoData._NumPendingWSCalls++;
						List<object> list = new List<object>();
						list.Add(messageInfoData);
						list.Add("{{BuddyUserName}}");
						WsWebService.GetDisplayNameByUserID(messageInfo.FromUserID, ServiceEventHandler, list);
					}
					if (messageInfoData._NumPendingWSCalls <= 0)
					{
						ShowSystemMessage(messageInfo);
					}
				}
				BuddyList.AddMessage(messageInfo);
				continue;
			case 9:
			case 30:
			{
				TaggedMessageHelper taggedMessageHelper2 = new TaggedMessageHelper(messageInfo);
				if (!string.IsNullOrEmpty(taggedMessageHelper2.SubType))
				{
					string subType = taggedMessageHelper2.SubType;
					if (subType != "Trophy" && subType != "TrophyItem" && subType != "MedalItem" && subType != "Cash")
					{
						mAlerts.Add(messageInfo);
						continue;
					}
				}
				goto case 8;
			}
			case 8:
			case 13:
			case 15:
			{
				List<MessageInfo> list3 = ((!mMessages.ContainsKey(num)) ? new List<MessageInfo>() : ((List<MessageInfo>)mMessages[num]));
				list3.Add(messageInfo);
				mMessages[num] = list3;
				continue;
			}
			case 4:
			{
				List<MessageInfo> list2 = ((!mMessages.ContainsKey(num)) ? new List<MessageInfo>() : ((List<MessageInfo>)mMessages[num]));
				int result = -1;
				TaggedMessageHelper taggedMessageHelper3 = new TaggedMessageHelper(messageInfo);
				if (taggedMessageHelper3.MemberMessage.ContainsKey("Type"))
				{
					string text3 = taggedMessageHelper3.MemberMessage["Type"];
					if (!string.IsNullOrEmpty(text3))
					{
						int.TryParse(text3, out result);
					}
				}
				bool flag = false;
				foreach (MessageInfo item in list2)
				{
					int result2 = -1;
					taggedMessageHelper3 = new TaggedMessageHelper(item);
					if (taggedMessageHelper3.MemberMessage.ContainsKey("Type"))
					{
						string text4 = taggedMessageHelper3.MemberMessage["Type"];
						if (!string.IsNullOrEmpty(text4))
						{
							int.TryParse(text4, out result2);
						}
					}
					if (result == result2)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list2.Add(messageInfo);
					mMessages[num] = list2;
				}
				else
				{
					SaveMessage(messageInfo, inDelete: true);
				}
				RewardData rewardData = null;
				int num2 = -1;
				if (!string.IsNullOrEmpty(messageInfo.Data))
				{
					rewardData = UtUtilities.DeserializeFromXml(messageInfo.Data, typeof(RewardData)) as RewardData;
				}
				if (rewardData != null && rewardData.Rewards != null)
				{
					Reward[] rewards = rewardData.Rewards;
					foreach (Reward reward in rewards)
					{
						if (reward.Type == "Inventory Item")
						{
							num2 = reward.ItemID.Value;
						}
					}
				}
				if (num2 != -1)
				{
					CommonInventoryData.ReInit();
				}
				continue;
			}
			case 10:
			{
				Dictionary<string, string> dictionary4 = TaggedMessageHelper.Match(messageInfo.Data);
				if (dictionary4.ContainsKey("UnreadMessageCount"))
				{
					UiToolbar.pMessageCount = Convert.ToInt32(dictionary4["UnreadMessageCount"]);
				}
				continue;
			}
			case 12:
			{
				TaggedMessageHelper taggedMessageHelper4 = new TaggedMessageHelper(messageInfo);
				if (!string.IsNullOrEmpty(taggedMessageHelper4.SubType) && taggedMessageHelper4.SubType == "SocialBox")
				{
					Buddy buddy2 = null;
					if (BuddyList.pIsReady)
					{
						buddy2 = BuddyList.pInstance.GetBuddy(messageInfo.FromUserID);
					}
					if (buddy2 != null && !string.IsNullOrEmpty(buddy2.DisplayName))
					{
						AddSocialBoxMessage(messageInfo, buddy2.DisplayName);
					}
					else
					{
						WsWebService.GetDisplayNameByUserID(messageInfo.FromUserID, SocialBoxInviteHandler, messageInfo);
					}
				}
				else
				{
					mAlerts.Add(messageInfo);
				}
				continue;
			}
			case 19:
			case 21:
			case 28:
			case 31:
				mAlerts.Add(messageInfo);
				continue;
			case 20:
			{
				string text5 = messageInfo.MemberMessage;
				Dictionary<string, string> dictionary3 = TaggedMessageHelper.Match(messageInfo.Data);
				if (dictionary3.ContainsKey("BuddyCount"))
				{
					text5 = text5.Replace("{{BuddyCount}}", dictionary3["BuddyCount"]);
				}
				messageInfo.MemberMessage = text5;
				mAlerts.Add(messageInfo);
				continue;
			}
			case 29:
			{
				string text = messageInfo.MemberMessage;
				Dictionary<string, string> dictionary = TaggedMessageHelper.Match(messageInfo.Data);
				if (dictionary.ContainsKey("GemsCount"))
				{
					string text2 = dictionary["GemsCount"];
					text = text.Replace("{{GemsCount}}", text2);
					if (int.Parse(text2) == 1)
					{
						text = text.Replace("Gems", "Gem");
					}
				}
				messageInfo.MemberMessage = text;
				mAlerts.Add(messageInfo);
				continue;
			}
			case 34:
			case 35:
			{
				List<MessageInfo> list4 = ((!mMessages.ContainsKey(num)) ? new List<MessageInfo>() : ((List<MessageInfo>)mMessages[num]));
				list4.Add(messageInfo);
				mMessages[num] = list4;
				continue;
			}
			case 3:
			{
				TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(messageInfo);
				if (taggedMessageHelper.SubType == "Challenge")
				{
					mAlerts.Add(messageInfo);
					continue;
				}
				if (!(taggedMessageHelper.SubType == "Mission"))
				{
					break;
				}
				if (messageInfo.Data != null)
				{
					if (!mReinitUserRank)
					{
						mReinitUserRank = true;
					}
					ReInitUserRankData();
					RewardManager.SetReward(messageInfo.Data, inImmediateShow: true);
				}
				continue;
			}
			case 6:
			case 7:
			case 11:
			case 24:
			case 25:
				break;
			default:
				continue;
			}
			if (num == 11 && mMessages.ContainsKey(num))
			{
				SaveMessage((MessageInfo)mMessages[num], inDelete: false);
			}
			mMessages[num] = messageInfo;
		}
	}

	public void ShowSystemMessage(MessageInfo messageInfo, bool isTagged = true)
	{
		string text = messageInfo.MemberMessage;
		TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(messageInfo);
		if (isTagged)
		{
			string key = "Line1";
			if (messageInfo.MessageTypeID == 27)
			{
				key = "None";
				if (UserProfile.pProfileData.HasGroup())
				{
					key = ((UserRole)UserProfile.pProfileData.Groups[0].RoleID.Value).ToString();
				}
			}
			else if (messageInfo.MessageTypeID == 15)
			{
				Dictionary<string, string> dictionary = TaggedMessageHelper.Match(messageInfo.Data);
				if (taggedMessageHelper.SubType == "Inviter")
				{
					string key2 = "RUN";
					string newValue = "";
					if (dictionary.ContainsKey(key2))
					{
						newValue = dictionary[key2];
					}
					taggedMessageHelper.MemberMessage[key] = taggedMessageHelper.MemberMessage[key].Replace("{{RUN}}", newValue);
				}
			}
			text = ReplaceTagsWithDefault(taggedMessageHelper.MemberMessage[key]);
		}
		if (text.Contains("{{PetName}}"))
		{
			RewardData rewardData = null;
			if (!string.IsNullOrEmpty(messageInfo.Data))
			{
				rewardData = UtUtilities.DeserializeFromXml(messageInfo.Data, typeof(RewardData)) as RewardData;
			}
			if (rewardData != null && !string.IsNullOrEmpty(rewardData.EntityID))
			{
				RaisedPetData byEntityID = RaisedPetData.GetByEntityID(new Guid(rewardData.EntityID));
				if (byEntityID != null)
				{
					string newValue2 = byEntityID.Name;
					SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(byEntityID.PetTypeID);
					text = text.Replace("{{PetName}}", newValue2);
					text = text.Replace("{{PetType}}", sanctuaryPetTypeInfo._NameText.GetLocalizedString());
				}
			}
		}
		UiChatHistory.AddSystemNotification(text, messageInfo, OnSystemMessageClicked);
	}

	private void AddSocialBoxMessage(MessageInfo info, string buddyName)
	{
		int value = info.MessageTypeID.Value;
		List<MessageInfo> list = ((!mMessages.ContainsKey(value)) ? new List<MessageInfo>() : ((List<MessageInfo>)mMessages[value]));
		list.Add(info);
		mMessages[value] = list;
		info.MemberMessage = info.MemberMessage.Replace("{friend}", buddyName);
	}

	private void Update()
	{
		if (mAddParentMessages)
		{
			AddParentMessages();
		}
		if (mIsForceUpdate && mIsReady && pCanShowMessage)
		{
			ShowNextMessage(firstMessage: true);
			mIsForceUpdate = false;
		}
		if (mMessage != null)
		{
			mMessage.Update();
		}
		if (mMessageTypeWanted >= 0 && mIsReady && mForceUpdateTime > 0f)
		{
			mForceUpdateTime -= Time.deltaTime;
			if (mForceUpdateTime <= 0f)
			{
				ForceUserMessageUpdate();
			}
		}
	}

	public object GetStoredMessages(int messageType)
	{
		if (mStoredMessages.ContainsKey(messageType))
		{
			return mStoredMessages[messageType];
		}
		return null;
	}

	public void SetStoredMessage(int messageType, object inObject)
	{
		mStoredMessages[messageType] = inObject;
	}

	public void AddParentMessages()
	{
		if (ParentData.pInstance != null && ParentData.pInstance.pUserMessageReady)
		{
			ArrayOfMessageInfo messages = ParentData.pInstance.GetMessages();
			if (messages != null && messages.MessageInfo != null)
			{
				AddMessages(messages, inParentMessages: true);
				UtDebug.Log("Parent messages count = " + messages.MessageInfo.Length);
			}
			mAddParentMessages = false;
		}
	}

	private void OnDestroy()
	{
		mInstance = null;
	}

	private void OnApplicationPause(bool isPaused)
	{
		if (!isPaused)
		{
			ForceUserMessageUpdate();
		}
	}
}
