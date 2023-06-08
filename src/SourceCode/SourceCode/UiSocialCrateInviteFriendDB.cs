using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class UiSocialCrateInviteFriendDB : KAUI
{
	public UiSignatureList _SignatureList;

	public string _SelectFriendDBUrl = "RS_DATA/PfUiSelectFriendDO.unity3d/PfUiSelectFriendDO";

	public UiSocialCrateInviteFriendEmailDB _EmailInviteDB;

	public ClansInvitePlayerResultInfo[] _InvitePlayerResultInfo;

	public EmailInviteFriendResultInfo[] _EmailInviteFriendResultInfo;

	public LocaleString _BuddyInviteTitleText = new LocaleString("Invite from your Group");

	public LocaleString _NoBuddyAvailableText = new LocaleString("No buddies available");

	public LocaleString _InviteAlreadySentText = new LocaleString("Invite to [Name] is already sent!");

	public LocaleString _NotValidEmailID = new LocaleString("[Email] is not a valid email id");

	public LocaleString _GenericErrorText = new LocaleString("Failed processing input, please try again!");

	public LocaleString _InviteSuccessText = new LocaleString("Invite sent");

	public int _SocialBoxInviteMessageID = 1276;

	private KAWidget mBtnFriendList;

	private KAWidget mBtnInviteEmail;

	private KAWidget mBtnClose;

	private List<string> mClanInviteResponseList = new List<string>();

	protected string mBuddyCode = "";

	protected GameObject mUiGenericDB;

	protected KAUIGenericDB mKAUIGenericDB;

	private int mPendingGetNameForInvite;

	private static List<string> mInvitedFriendList = new List<string>();

	private static Regex mRegexIsGuid = new Regex("^(\\{){0,1}[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}(\\}){0,1}$");

	protected override void Start()
	{
		base.Start();
		mBtnFriendList = FindItem("BtnFriendsList");
		mBtnInviteEmail = FindItem("BtnInviteEmail");
		mBtnClose = FindItem("CloseBtn");
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnFriendList)
		{
			SetInteractive(interactive: false);
			string[] array = _SelectFriendDBUrl.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], AssetEventHandler, typeof(GameObject));
		}
		else if (inWidget == mBtnInviteEmail)
		{
			_EmailInviteDB.SetVisibility(inVisible: true);
			_EmailInviteDB.Init();
			KAUI.SetExclusive(_EmailInviteDB);
			SetVisibility(inVisible: false);
		}
		else if (inWidget == mBtnClose)
		{
			SetVisibility(inVisible: false);
			KAUI.RemoveExclusive(this);
			if (_SignatureList != null)
			{
				_SignatureList.SetVisibility(inVisible: true);
				KAUI.SetExclusive(_SignatureList);
			}
		}
	}

	private void AssetEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
				SetInteractive(interactive: true);
				SetVisibility(inVisible: false);
				if (RsResourceManager.Compare(inURL, _SelectFriendDBUrl))
				{
					UiSelectFriend component = gameObject.GetComponent<UiSelectFriend>();
					if (component != null)
					{
						component.SetMessage(base.gameObject, "SendMessageToFriends", "SendMessageToFriendsNo", null);
						component.ShowFriendSelection(_BuddyInviteTitleText.GetLocalizedString(), _NoBuddyAvailableText.GetLocalizedString(), allowMultipleSelection: true);
						component._UseMask = true;
						component._AllowMultipleSelection = true;
					}
				}
			}
			else
			{
				SetInteractive(interactive: true);
			}
			break;
		case RsResourceLoadEvent.ERROR:
			SetInteractive(interactive: true);
			break;
		}
	}

	public void ProcessMessageFriend(string inText, string inUserName)
	{
		string[] array = inText.Split(',');
		List<string> list = new List<string>();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string text = array2[i].Trim();
			if (!string.IsNullOrEmpty(text))
			{
				if (!UtUtilities.IsValidEmail(text))
				{
					AddMessageToList(text, _NotValidEmailID);
				}
				else if (!list.Contains(text))
				{
					list.Add(text);
				}
				else if (mInvitedFriendList.Contains(text))
				{
					AddMessageToList(text, _InviteAlreadySentText);
				}
			}
		}
		if (list.Count > 0)
		{
			SendMessageToFriends(list.ToArray(), inUserName, isEmailInvite: true);
		}
		else
		{
			ShowResponse();
		}
	}

	private void AddMessageToList(string userName, LocaleString result)
	{
		string text = result.GetLocalizedString();
		if (text.Contains("[Name]"))
		{
			if (mRegexIsGuid.IsMatch(userName))
			{
				Buddy buddy = BuddyList.pInstance.GetBuddy(userName);
				if (buddy == null)
				{
					mPendingGetNameForInvite++;
					WsWebService.GetDisplayNameByUserID(userName, ServiceEventHandler, text);
					return;
				}
				text = text.Replace("[Name]", buddy.DisplayName);
			}
			else
			{
				text = text.Replace("[Name]", userName);
			}
		}
		else if (text.Contains("[Email]"))
		{
			text = text.Replace("[Email]", userName);
		}
		if (!mClanInviteResponseList.Contains(text))
		{
			mClanInviteResponseList.Add(text);
		}
	}

	private void ShowResponse()
	{
		KillGenericDB();
		if (mClanInviteResponseList.Count > 0)
		{
			ShowGenericDB("PfKAUIGenericDBSm", mClanInviteResponseList[0], null, null, null, "ShowResponse", null);
			mClanInviteResponseList.RemoveAt(0);
		}
		else if (mPendingGetNameForInvite > 0)
		{
			SetInteractive(interactive: false);
		}
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
	}

	private void SendMessageToFriends(string[] inInvitees)
	{
		SetVisibility(inVisible: true);
		SendMessageToFriends(inInvitees, "", isEmailInvite: false);
	}

	public void SendMessageToFriendsNo()
	{
		SetVisibility(inVisible: true);
	}

	private void SendMessageToFriends(string[] inInvitees, string inUserName, bool isEmailInvite)
	{
		SetInteractive(interactive: false);
		if (isEmailInvite)
		{
			WsWebService.SendFriendInviteRequest(new FriendInviteRequest
			{
				InviterName = inUserName,
				FriendEmailIDs = inInvitees,
				EmailType = EmailType.SocialBox
			}, ServiceEventHandler, null);
			return;
		}
		List<Guid> list = new List<Guid>();
		foreach (string g in inInvitees)
		{
			list.Add(new Guid(g));
		}
		WsWebService.SendMessageBulk(list.ToArray(), _SocialBoxInviteMessageID, null, ServiceEventHandler, null);
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.SEND_MESSAGE_BULK:
			switch (inEvent)
			{
			default:
				return;
			case WsServiceEvent.COMPLETE:
				SetInteractive(interactive: true);
				if ((bool)inObject)
				{
					AddMessageToList(null, _InviteSuccessText);
					ShowResponse();
					return;
				}
				break;
			case WsServiceEvent.ERROR:
				break;
			}
			SetInteractive(interactive: true);
			AddMessageToList(null, _GenericErrorText);
			ShowResponse();
			break;
		case WsServiceType.SEND_FRIEND_INVITE_REQUEST:
			switch (inEvent)
			{
			default:
				return;
			case WsServiceEvent.COMPLETE:
			{
				if (inObject == null)
				{
					break;
				}
				SetInteractive(interactive: true);
				InviteData inviteData = (InviteData)inObject;
				if (inviteData.FilterNameResponse)
				{
					InviteFriendResult[] inviteFriendResult = inviteData.InviteFriendResult;
					foreach (InviteFriendResult inviteFriendResult2 in inviteFriendResult)
					{
						EmailInviteFriendResultInfo[] emailInviteFriendResultInfo = _EmailInviteFriendResultInfo;
						foreach (EmailInviteFriendResultInfo emailInviteFriendResultInfo2 in emailInviteFriendResultInfo)
						{
							if (emailInviteFriendResultInfo2._Status == inviteFriendResult2.MailingResult)
							{
								AddMessageToList(inviteFriendResult2.Email, emailInviteFriendResultInfo2._StatusText);
								break;
							}
						}
						if (inviteFriendResult2.MailingResult == MailingResult.Sent)
						{
							mInvitedFriendList.Add(inviteFriendResult2.Email);
						}
					}
				}
				else
				{
					GameUtilities.DisplayOKMessage("PfKAUIGenericDBSm", _EmailInviteDB._BadPhraseUse.GetLocalizedString(), null, "");
				}
				ShowResponse();
				return;
			}
			case WsServiceEvent.ERROR:
				break;
			}
			SetInteractive(interactive: true);
			AddMessageToList(null, _GenericErrorText);
			ShowResponse();
			break;
		}
	}
}
