using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(Namespace = "http://api.jumpstart.com/", ElementName = "ArrayOfBuddy", IsNullable = true)]
public class BuddyList
{
	[XmlElement(ElementName = "Buddy")]
	public Buddy[] Buddy;

	private static BuddyList mInstance = null;

	private static bool mInitialized = false;

	private static bool mReInitialize = false;

	private static BuddyComparer mBC = new BuddyComparer();

	private static BuddyStatusComparer mBSC = new BuddyStatusComparer();

	private const string mRefusalMessage = "[[Line1]]=[[{{BuddyUserName}} is not accepting friend requests.]]";

	private const int mRefusalMessageID = 6016;

	private static List<Buddy> mList = null;

	private static List<MessageInfo> mMessageList = new List<MessageInfo>();

	private static List<MessageInfo> mInviteMessageList = new List<MessageInfo>();

	public static ArrayOfMessage mStatusMessages = null;

	public static List<BuddyStatusMessage> pStatusMessages = new List<BuddyStatusMessage>();

	public static bool pIsStatusReady = false;

	private static SyncBuddyListEventHandler mSyncBuddyListEventHandlers = null;

	private static List<GameObject> mInviteMessageListeners = new List<GameObject>();

	private BuddyListEventHandler mEventDelegate;

	public static BuddyList pInstance
	{
		get
		{
			if (!pIsReady)
			{
				Init();
			}
			return mInstance;
		}
	}

	public static bool pIsReady => mInstance != null;

	public static Buddy[] pList
	{
		get
		{
			if (mList != null)
			{
				return mList.ToArray();
			}
			return null;
		}
	}

	public static int pCount
	{
		get
		{
			if (mList == null)
			{
				return 0;
			}
			return mList.Count;
		}
	}

	public static List<MessageInfo> pMessageList => mMessageList;

	public static List<MessageInfo> pInviteMessageList => mInviteMessageList;

	[XmlIgnore]
	public BuddyListEventHandler pEventDelegate
	{
		get
		{
			return mEventDelegate;
		}
		set
		{
			mEventDelegate = value;
		}
	}

	public static void AddSyncBuddyListEventHandler(SyncBuddyListEventHandler inHandler)
	{
		if (inHandler != null)
		{
			mSyncBuddyListEventHandlers = (SyncBuddyListEventHandler)Delegate.Combine(mSyncBuddyListEventHandlers, inHandler);
		}
	}

	public static void RemoveSyncBuddyListEventHandler(SyncBuddyListEventHandler inHandler)
	{
		mSyncBuddyListEventHandlers = (SyncBuddyListEventHandler)Delegate.Remove(mSyncBuddyListEventHandlers, inHandler);
	}

	public static void AddInviteMessageListener(GameObject inListener)
	{
		if (inListener != null && !mInviteMessageListeners.Contains(inListener))
		{
			mInviteMessageListeners.Add(inListener);
		}
	}

	public static void RemoveInviteMessageListener(GameObject inListener)
	{
		mInviteMessageListeners.Remove(inListener);
	}

	public static void Init()
	{
		if (!mInitialized)
		{
			mInitialized = true;
			WsWebService.GetBuddyList(ServiceEventHandler, null);
		}
	}

	public static void ReInit(SyncBuddyListEventHandler inCallback = null)
	{
		if (!mReInitialize)
		{
			mInitialized = true;
			mReInitialize = true;
			if (inCallback != null)
			{
				AddSyncBuddyListEventHandler(inCallback);
			}
			WsWebService.GetBuddyList(ServiceEventHandler, null);
		}
	}

	public static void Reset()
	{
		mInitialized = false;
		mList = null;
		mInstance = null;
		SetVisible(visible: false);
	}

	public static void InitDefault()
	{
		mInstance = new BuddyList();
		mList = new List<Buddy>();
	}

	private static void SetVisible(bool visible)
	{
		if (AvAvatar.pToolbar != null && !visible)
		{
			Transform transform = AvAvatar.pToolbar.transform.Find("PfUiBuddyList");
			if (transform != null)
			{
				transform.gameObject.SendMessage("OnDisconnect", null, SendMessageOptions.DontRequireReceiver);
				AvAvatar.pState = AvAvatarState.IDLE;
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			GameObject gameObject = GameObject.Find("PfUiBuddyMessage");
			if (gameObject != null)
			{
				gameObject.SendMessage("OnClose", null, SendMessageOptions.DontRequireReceiver);
				UnityEngine.Object.Destroy(gameObject);
			}
		}
	}

	public void AddBuddy(string userID, string displayName, BuddyListEventHandler inCallback)
	{
		mEventDelegate = inCallback;
		WsWebService.AddBuddy(userID, ServiceEventHandler, displayName);
	}

	public void AddBuddyByFriendCode(string code, BuddyListEventHandler inCallback)
	{
		mEventDelegate = inCallback;
		WsWebService.AddBuddyByFriendCode(code, ServiceEventHandler, code);
	}

	public void AddOneWayBuddy(Guid[] userIDs, BuddyListEventHandler inCallback)
	{
		mEventDelegate = inCallback;
		WsWebService.AddOneWayBuddy(userIDs, ServiceEventHandler, userIDs);
	}

	public void ApproveBuddy(string userID, BuddyListEventHandler inCallback, bool addFromFriendCode)
	{
		mEventDelegate = inCallback;
		BuddyData buddyData = new BuddyData();
		buddyData._UserID = userID;
		buddyData._AddFromBFFCode = addFromFriendCode;
		WsWebService.ApproveBuddy(userID, ServiceEventHandler, buddyData);
	}

	public void RemoveBuddy(string userID, BuddyListEventHandler inCallback)
	{
		mEventDelegate = inCallback;
		WsWebService.RemoveBuddy(userID, ServiceEventHandler, userID);
	}

	public void BlockBuddy(string userID, BuddyListEventHandler inCallback)
	{
		mEventDelegate = inCallback;
		WsWebService.BlockBuddy(userID, ServiceEventHandler, userID);
	}

	public void InviteBuddy(string userID, BuddyListEventHandler inCallback)
	{
		mEventDelegate = inCallback;
		WsWebService.InviteBuddy(userID, null, null);
	}

	public void JoinBuddy(string userID, BuddyListEventHandler inCallback)
	{
		mEventDelegate = inCallback;
		WsWebService.GetBuddyLocation(userID, ServiceEventHandler, userID);
	}

	public Buddy GetBuddy(string userID)
	{
		foreach (Buddy m in mList)
		{
			if (m.UserID == userID)
			{
				return m;
			}
		}
		return null;
	}

	public BuddyStatus GetBuddyStatus(string userID)
	{
		foreach (Buddy m in mList)
		{
			if (m.UserID == userID)
			{
				return m.Status;
			}
		}
		return BuddyStatus.Unknown;
	}

	public static void SyncUiStatus()
	{
		if (mSyncBuddyListEventHandlers != null)
		{
			mSyncBuddyListEventHandlers();
		}
	}

	public static bool SetBuddyStatus(string userID, BuddyStatus status, bool bestBuddy)
	{
		bool flag = false;
		foreach (Buddy m in mList)
		{
			if (m == null || !(m.UserID == userID))
			{
				continue;
			}
			switch (status)
			{
			case BuddyStatus.BlockedByOther:
				if (m.Status == BuddyStatus.BlockedBySelf)
				{
					status = BuddyStatus.BlockedByBoth;
				}
				else if (m.Status == BuddyStatus.PendingApprovalFromOther)
				{
					AddRefusal(userID);
				}
				break;
			case BuddyStatus.Approved:
				if (m.Status != BuddyStatus.BlockedByOther)
				{
					break;
				}
				goto end_IL_0071;
			}
			m.Status = status;
			m.BestBuddy = bestBuddy;
			flag = true;
			SyncUiStatus();
			break;
			continue;
			end_IL_0071:
			break;
		}
		if (!flag && (status == BuddyStatus.BlockedByOther || status == BuddyStatus.Approved))
		{
			InsertNewBuddy(userID, "", status, onlineStatus: false);
		}
		return flag;
	}

	public static void InsertNewBuddy(string userID, string displayName, BuddyStatus status, bool onlineStatus)
	{
		Buddy buddy = new Buddy();
		buddy.UserID = userID;
		buddy.DisplayName = displayName;
		buddy.Status = status;
		buddy.Online = onlineStatus;
		int num = mList.BinarySearch(buddy, mBC);
		if (num < 0)
		{
			mList.Insert(~num, buddy);
		}
		else
		{
			mList.Insert(num, buddy);
		}
		if (string.IsNullOrEmpty(displayName))
		{
			WsWebService.GetDisplayNameByUserID(userID, ServiceEventHandler, userID);
		}
	}

	public static void RemoveBuddy(string userID)
	{
		Buddy buddy = pInstance.GetBuddy(userID);
		if (buddy != null)
		{
			if (buddy.Status == BuddyStatus.PendingApprovalFromOther)
			{
				AddRefusal(userID);
			}
			if (buddy.Status == BuddyStatus.BlockedByBoth)
			{
				buddy.Status = BuddyStatus.BlockedBySelf;
			}
			else if (buddy.Status != BuddyStatus.BlockedBySelf)
			{
				mList.Remove(buddy);
			}
			SyncUiStatus();
		}
	}

	public static void ClearMessages()
	{
		int num = 0;
		foreach (MessageInfo mMessage in mMessageList)
		{
			if (mMessage.MessageTypeID != 5 && mMessage.MessageTypeID != 27)
			{
				num++;
				continue;
			}
			break;
		}
		mMessageList.RemoveRange(num, mMessageList.Count - num);
		mInviteMessageList.Clear();
	}

	public static void AddMessage(MessageInfo messageInfo)
	{
		int num = 0;
		using (List<MessageInfo>.Enumerator enumerator = mMessageList.GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current.MessageTypeID != 5)
			{
				num++;
			}
		}
		if (messageInfo.MessageTypeID == 27)
		{
			foreach (MessageInfo mMessage in mMessageList)
			{
				if (mMessage.MessageTypeID == 27 && mMessage.FromUserID == messageInfo.FromUserID && mMessage.Data == messageInfo.Data)
				{
					if (messageInfo.UserMessageQueueID.HasValue)
					{
						WsWebService.SaveMessage(messageInfo.UserMessageQueueID.Value, isNew: false, isDeleted: true, null, null);
					}
					return;
				}
			}
		}
		mMessageList.Insert(num, messageInfo);
		if (messageInfo.MessageTypeID == 5)
		{
			UpdateBuddyBtnCount();
		}
	}

	public static void MessagesReceived()
	{
		if (mSyncBuddyListEventHandlers != null)
		{
			mSyncBuddyListEventHandlers();
		}
	}

	public static void RemoveMessage(int msgIndex)
	{
		bool num = mMessageList[msgIndex].MessageTypeID == 5;
		mMessageList.RemoveAt(msgIndex);
		if (num)
		{
			UpdateBuddyBtnCount();
		}
	}

	public static void UpdateBuddyBtnCount()
	{
		int num = mMessageList.FindAll((MessageInfo item) => item.MessageTypeID == 5)?.Count ?? 0;
		if (!(AvAvatar.pToolbar != null))
		{
			return;
		}
		Transform transform = AvAvatar.pToolbar.transform.Find("PfUiAvatarCSM");
		if (!(transform != null))
		{
			return;
		}
		KAUI component = transform.gameObject.GetComponent<KAUI>();
		if (!(component != null))
		{
			return;
		}
		KAWidget kAWidget = component.FindItem("BtnCSMBuddies");
		if (kAWidget != null)
		{
			KAWidget kAWidget2 = kAWidget.FindChildItem("AniAlert");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(num != 0);
				kAWidget2.SetText(num.ToString());
			}
		}
	}

	public static void AddInvite(string fromUserID, string fromZone, BuddyMessageType inType)
	{
		for (int i = 0; i < mInviteMessageList.Count; i++)
		{
			MessageInfo messageInfo = mInviteMessageList[i];
			if (messageInfo.FromUserID == fromUserID)
			{
				if (!(messageInfo.Data != fromZone) && messageInfo.MessageTypeID == (int?)inType)
				{
					return;
				}
				if (i > 0)
				{
					mInviteMessageList.Remove(messageInfo);
					break;
				}
			}
		}
		MessageInfo messageInfo2 = new MessageInfo();
		messageInfo2.FromUserID = fromUserID;
		messageInfo2.Data = fromZone;
		messageInfo2.MessageTypeID = (int)inType;
		mInviteMessageList.Add(messageInfo2);
		GameObject gameObject = GameObject.Find("PfCheckBuddyInviteMessages");
		if (gameObject != null)
		{
			gameObject.SendMessage("ForceInviteMessageUpdate", null, SendMessageOptions.DontRequireReceiver);
		}
		foreach (GameObject mInviteMessageListener in mInviteMessageListeners)
		{
			if (mInviteMessageListener != null)
			{
				mInviteMessageListener.SendMessage("ForceInviteMessageUpdate", null, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public static void AddRefusal(string fromUserID)
	{
		AddMessage(new MessageInfo
		{
			FromUserID = fromUserID,
			MemberMessage = StringTable.GetStringData(6016, "[[Line1]]=[[{{BuddyUserName}} is not accepting friend requests.]]"),
			MessageTypeID = -2
		});
		MessagesReceived();
	}

	public static void RemoveApproval(string fromUserID)
	{
		if (GameObject.Find("PfUiBuddyMessage") != null)
		{
			return;
		}
		MessageInfo[] array = mMessageList.ToArray();
		foreach (MessageInfo messageInfo in array)
		{
			if (messageInfo.FromUserID == fromUserID)
			{
				WsWebService.SaveMessage(messageInfo.UserMessageQueueID.Value, isNew: false, isDeleted: true, null, null);
				mMessageList.Remove(messageInfo);
				MessagesReceived();
			}
		}
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.PROGRESS)
		{
			return;
		}
		switch (inType)
		{
		case WsServiceType.GET_BUDDY_STATUS_LIST:
			if (inEvent == WsServiceEvent.COMPLETE)
			{
				mStatusMessages = (ArrayOfMessage)inObject;
				SyncBuddyStatus(isNew: true);
			}
			break;
		case WsServiceType.GET_BUDDY_LIST:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				mInstance = (BuddyList)inObject;
				if (mInstance != null)
				{
					if (mInstance.Buddy != null)
					{
						mList = new List<Buddy>(mInstance.Buddy);
						mList.Sort(mBC);
					}
					else
					{
						mList = new List<Buddy>();
					}
				}
				else
				{
					UtDebug.Log("WEB SERVICE CALL GetBuddyList RETURNED NO DATA!!!");
					InitDefault();
				}
				if (mReInitialize)
				{
					mReInitialize = false;
					SyncBuddyStatus(isNew: false);
					SyncUiStatus();
				}
				SetVisible(visible: true);
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL GetBuddyList FAILED!!!");
				InitDefault();
				if (mReInitialize)
				{
					mReInitialize = false;
					SyncUiStatus();
				}
				else
				{
					SetVisible(visible: true);
				}
				break;
			}
			break;
		case WsServiceType.ADD_BUDDY:
			if (inEvent == WsServiceEvent.COMPLETE)
			{
				BuddyActionResult buddyActionResult3 = (BuddyActionResult)inObject;
				if (buddyActionResult3 != null && buddyActionResult3.Result == BuddyActionResultType.Success)
				{
					InsertNewBuddy(buddyActionResult3.BuddyUserID, (string)inUserData, buddyActionResult3.Status, onlineStatus: false);
				}
			}
			if (mInstance.mEventDelegate != null)
			{
				mInstance.mEventDelegate(inType, inObject);
			}
			mInstance.mEventDelegate = null;
			break;
		case WsServiceType.ADD_BUDDY_BY_FRIEND_CODE:
			if (inEvent == WsServiceEvent.COMPLETE)
			{
				BuddyActionResult buddyActionResult = (BuddyActionResult)inObject;
				if (buddyActionResult != null && buddyActionResult.Result == BuddyActionResultType.Success)
				{
					InsertNewBuddy(buddyActionResult.BuddyUserID, "", buddyActionResult.Status, onlineStatus: false);
				}
			}
			if (mInstance.mEventDelegate != null)
			{
				mInstance.mEventDelegate(inType, inObject);
			}
			mInstance.mEventDelegate = null;
			break;
		case WsServiceType.ADD_ONE_WAY_BUDDY:
			if (inEvent == WsServiceEvent.COMPLETE)
			{
				BuddyActionResult buddyActionResult2 = (BuddyActionResult)inObject;
				if (buddyActionResult2 != null && buddyActionResult2.Result == BuddyActionResultType.Success)
				{
					InsertNewBuddy(buddyActionResult2.BuddyUserID, "", buddyActionResult2.Status, onlineStatus: false);
				}
			}
			if (mInstance.mEventDelegate != null)
			{
				mInstance.mEventDelegate(inType, inObject);
			}
			mInstance.mEventDelegate = null;
			break;
		case WsServiceType.APPROVE_BUDDY:
			if (inEvent == WsServiceEvent.COMPLETE)
			{
				if ((bool)inObject)
				{
					BuddyData buddyData = (BuddyData)inUserData;
					SetBuddyStatus(buddyData._UserID, BuddyStatus.Approved, buddyData._AddFromBFFCode);
					if (buddyData._AddFromBFFCode)
					{
						Sort();
					}
				}
				else
				{
					inObject = null;
				}
			}
			if (mInstance.mEventDelegate != null)
			{
				mInstance.mEventDelegate(inType, inObject);
			}
			mInstance.mEventDelegate = null;
			break;
		case WsServiceType.REMOVE_BUDDY:
			if (inEvent == WsServiceEvent.COMPLETE)
			{
				if ((bool)inObject)
				{
					string userID3 = (string)inUserData;
					Buddy buddy3 = mInstance.GetBuddy(userID3);
					if (buddy3 != null)
					{
						if (buddy3.Status == BuddyStatus.BlockedByBoth)
						{
							buddy3.Status = BuddyStatus.BlockedByOther;
						}
						else if (buddy3.Status != BuddyStatus.BlockedByOther)
						{
							mList.Remove(buddy3);
						}
					}
				}
				else
				{
					inObject = null;
				}
			}
			if (mInstance.mEventDelegate != null)
			{
				mInstance.mEventDelegate(inType, inObject);
			}
			mInstance.mEventDelegate = null;
			break;
		case WsServiceType.BLOCK_BUDDY:
			if (inEvent == WsServiceEvent.COMPLETE)
			{
				if ((bool)inObject)
				{
					string userID2 = (string)inUserData;
					Buddy buddy2 = mInstance.GetBuddy(userID2);
					if (buddy2 != null)
					{
						if (buddy2.Status == BuddyStatus.BlockedByOther)
						{
							buddy2.Status = BuddyStatus.BlockedByBoth;
						}
						else
						{
							if (buddy2.Status == BuddyStatus.PendingApprovalFromSelf)
							{
								RemoveApproval(buddy2.UserID);
							}
							buddy2.Status = BuddyStatus.BlockedBySelf;
						}
					}
					else
					{
						InsertNewBuddy(userID2, "", BuddyStatus.BlockedBySelf, onlineStatus: false);
					}
				}
				else
				{
					inObject = null;
				}
			}
			if (mInstance.mEventDelegate != null)
			{
				mInstance.mEventDelegate(inType, inObject);
			}
			mInstance.mEventDelegate = null;
			break;
		case WsServiceType.GET_BUDDY_LOCATION:
			if (inEvent == WsServiceEvent.COMPLETE)
			{
				BuddyLocation buddyLocation = (BuddyLocation)inObject;
				if (buddyLocation != null)
				{
					MainStreetMMOPlugin.JoinBuddy(buddyLocation);
					break;
				}
				inObject = null;
				if (mInstance.mEventDelegate != null)
				{
					mInstance.mEventDelegate(inType, JoinBuddyResultType.JoinFailedCommon);
				}
				mInstance.mEventDelegate = null;
			}
			else if (mInstance.mEventDelegate != null)
			{
				mInstance.mEventDelegate(inType, JoinBuddyResultType.JoinFailedCommon);
				mInstance.mEventDelegate = null;
			}
			break;
		case WsServiceType.GET_DISPLAYNAME_BY_USER_ID:
			if (inEvent == WsServiceEvent.COMPLETE)
			{
				string userID = (string)inUserData;
				Buddy buddy = pInstance.GetBuddy(userID);
				if (buddy != null)
				{
					buddy.DisplayName = (string)inObject;
				}
			}
			break;
		}
	}

	public static void GetBuddyStatusList()
	{
		WsWebService.GetBuddyStatusList(ServiceEventHandler, null);
	}

	public static void SyncBuddyStatus(bool isNew)
	{
		pIsStatusReady = true;
		pStatusMessages.Clear();
		if (mStatusMessages == null || mStatusMessages.Messages == null || mInstance == null)
		{
			return;
		}
		int count = mList.Count;
		Message[] messages = mStatusMessages.Messages;
		foreach (Message message in messages)
		{
			for (int j = 0; j < count; j++)
			{
				if (message.Creator == mList[j].UserID)
				{
					BuddyStatusMessage buddyStatusMessage = new BuddyStatusMessage();
					buddyStatusMessage.Buddy = mList[j];
					buddyStatusMessage.StatusMessage = message;
					pStatusMessages.Add(buddyStatusMessage);
					break;
				}
			}
		}
		pStatusMessages.Sort(mBSC);
	}

	public static void Sort()
	{
		if (mList != null && mList.Count > 0)
		{
			mList.Sort(mBC);
		}
	}
}
