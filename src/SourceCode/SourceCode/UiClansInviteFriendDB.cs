using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class UiClansInviteFriendDB : KAUI
{
	public string _SelectFriendDBUrl = "RS_DATA/PfUiSelectFriendDO.unity3d/PfUiSelectFriendDO";

	public UiClansInviteFriendEmailDB _EmailInviteDB;

	public ClansInvitePlayerResultInfo[] _InvitePlayerResultInfo;

	public EmailInviteFriendResultInfo[] _EmailInviteFriendResultInfo;

	public LocaleString _BuddyInviteTitleText = new LocaleString("Invite from your Group");

	public LocaleString _NoBuddyAvailableText = new LocaleString("No buddies available");

	public LocaleString _InviteAlreadySentText = new LocaleString("Invite to [Name] is already sent!");

	public LocaleString _NotValidEmailID = new LocaleString("[Email] is not a valid email id");

	public LocaleString _GenericErrorText = new LocaleString("Failed processing input, please try again!");

	public LocaleString _InviteSelfText = new LocaleString("You cannot send clan invite to yourself");

	private KAWidget mBtnInviteEmail;

	private KAWidget mBtnInviteBuddyCode;

	private KAWidget mBtnInviteFriendList;

	private KAWidget mBtnClose;

	private string mClanID;

	private string mClanName;

	private bool mIsBuddyCodeInvite;

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
		KAUI.SetExclusive(this);
		mBtnInviteEmail = FindItem("BtnInviteEmail");
		mBtnInviteBuddyCode = FindItem("BtnBuddyCode");
		mBtnInviteFriendList = FindItem("BtnFriendsList");
		mBtnClose = FindItem("CloseBtn");
	}

	public virtual void MyBuddyCodeEventHandler(string code)
	{
		mBuddyCode = code;
	}

	public void SetClanInfo(string inClanID, string inClanName)
	{
		mClanID = inClanID;
		mClanName = inClanName;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnInviteEmail)
		{
			mIsBuddyCodeInvite = false;
			_EmailInviteDB.gameObject.SetActive(value: true);
			_EmailInviteDB.Init(isEmailInvite: true, mClanName);
			SetVisibility(inVisible: false);
		}
		if (inWidget == mBtnInviteBuddyCode)
		{
			mIsBuddyCodeInvite = true;
			_EmailInviteDB.gameObject.SetActive(value: true);
			_EmailInviteDB.Init(isEmailInvite: false, mClanName);
			SetVisibility(inVisible: false);
		}
		else if (inWidget == mBtnInviteFriendList)
		{
			mIsBuddyCodeInvite = false;
			SetInteractive(interactive: false);
			string[] array = _SelectFriendDBUrl.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], AssetEventHandler, typeof(GameObject));
		}
		else if (inWidget == mBtnClose)
		{
			OnExit();
		}
	}

	private void AssetEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				GameObject gameObject = Object.Instantiate((GameObject)inObject);
				SetInteractive(interactive: true);
				SetVisibility(inVisible: false);
				if (RsResourceManager.Compare(inURL, _SelectFriendDBUrl))
				{
					UiSelectFriend component = gameObject.GetComponent<UiSelectFriend>();
					if (component != null)
					{
						component.SetMessage(base.gameObject, "SendClanInvite", "SendClanInviteNo", null);
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

	public override void SetInteractive(bool interactive)
	{
		base.SetInteractive(interactive);
		KAUICursorManager.SetDefaultCursor(interactive ? "Arrow" : "Loading");
	}

	public void ProcessFriendInvite(string inText, string inUserName, bool isEmailInvite)
	{
		string[] array = inText.Split(',');
		List<string> list = new List<string>();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string text = array2[i].Trim();
			if (!string.IsNullOrEmpty(text))
			{
				if (isEmailInvite && !UtUtilities.IsValidEmail(text))
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
			SendClanInvite(list.ToArray(), inUserName, isEmailInvite);
		}
		else
		{
			ShowResponse();
		}
	}

	private void SendClanInvite(string[] inInvitees)
	{
		SetVisibility(inVisible: true);
		SendClanInvite(inInvitees, "", isEmailInvite: false);
	}

	public void SendClanInviteNo()
	{
		SetVisibility(inVisible: true);
	}

	private void SendClanInvite(string[] inInvitees, string inUserName, bool isEmailInvite)
	{
		SetInteractive(interactive: false);
		if (isEmailInvite)
		{
			WsWebService.SendFriendInviteRequest(new FriendInviteRequest
			{
				InviterName = inUserName,
				FriendEmailIDs = inInvitees,
				GroupID = mClanID
			}, ServiceEventHandler, null);
		}
		else
		{
			WsWebService.InvitePlayer(new InvitePlayerRequest
			{
				GroupID = mClanID,
				InviteByBuddyCode = mIsBuddyCodeInvite,
				InviteeIDs = inInvitees
			}, ServiceEventHandler, null);
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.INVITE_PLAYER:
			switch (inEvent)
			{
			default:
				return;
			case WsServiceEvent.COMPLETE:
			{
				SetInteractive(interactive: true);
				if (inObject == null)
				{
					break;
				}
				InvitePlayerResult invitePlayerResult = (InvitePlayerResult)inObject;
				if (invitePlayerResult == null || invitePlayerResult.InviteeStatus == null)
				{
					break;
				}
				InvitePlayerPlayerResult[] inviteeStatus = invitePlayerResult.InviteeStatus;
				foreach (InvitePlayerPlayerResult invitePlayerPlayerResult in inviteeStatus)
				{
					if (invitePlayerPlayerResult.Status == InvitePlayerStatus.Success)
					{
						mInvitedFriendList.Add(invitePlayerPlayerResult.InviteeID);
					}
					ClansInvitePlayerResultInfo[] invitePlayerResultInfo = _InvitePlayerResultInfo;
					foreach (ClansInvitePlayerResultInfo clansInvitePlayerResultInfo in invitePlayerResultInfo)
					{
						if (invitePlayerPlayerResult.Status == clansInvitePlayerResultInfo._Status)
						{
							AddMessageToList(invitePlayerPlayerResult.InviteeID, clansInvitePlayerResultInfo._StatusText);
							break;
						}
					}
				}
				SetInteractive(interactive: true);
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
		case WsServiceType.GET_DISPLAYNAME_BY_USER_ID:
			if ((uint)(inEvent - 2) > 1u)
			{
				break;
			}
			if (inObject != null)
			{
				string item = ((string)inUserData).Replace("[Name]", (string)inObject);
				if (!mClanInviteResponseList.Contains(item))
				{
					mClanInviteResponseList.Add(item);
				}
			}
			mPendingGetNameForInvite--;
			if (mKAUIGenericDB == null)
			{
				SetInteractive(interactive: true);
				ShowResponse();
			}
			break;
		}
	}

	private void AddMessageToList(string userName, LocaleString result)
	{
		string text;
		if (userName == UserInfo.pInstance.UserID)
		{
			text = _InviteSelfText.GetLocalizedString();
		}
		else
		{
			text = result.GetLocalizedString();
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
		}
		if (!mClanInviteResponseList.Contains(text))
		{
			mClanInviteResponseList.Add(text);
		}
	}

	protected virtual void OnExit()
	{
		Object.Destroy(base.gameObject);
		KAUI.RemoveExclusive(this);
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
			Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	protected virtual void ShowDialog(int id, string text, string inTitle)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
		mUiGenericDB = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSmSocial"));
		KAUIGenericDB component = mUiGenericDB.GetComponent<KAUIGenericDB>();
		component._MessageObject = base.gameObject;
		component._OKMessage = "OnClose";
		component.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		component.SetTextByID(id, text, interactive: false);
		component.SetTitle(inTitle);
		KAUI.SetExclusive(component);
	}

	public virtual void OnClose()
	{
		KAUI.RemoveExclusive(mUiGenericDB.GetComponent<KAUIGenericDB>());
		Object.Destroy(mUiGenericDB);
	}
}
