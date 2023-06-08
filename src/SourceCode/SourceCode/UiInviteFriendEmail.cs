using System.Collections.Generic;
using UnityEngine;

public class UiInviteFriendEmail : KAUI
{
	public class InviteResponse
	{
		public string _EmailID;

		public LocaleString _ResultText;

		public InviteResponse(string emailID, LocaleString resultText)
		{
			_EmailID = emailID;
			_ResultText = resultText;
		}
	}

	public enum InviteResult
	{
		Sent = 0,
		InvalidEmailAddress = 1,
		CannotParseAuthorizationKey = 2,
		AlreadySent = 3,
		BadPhrase = 4,
		GenericError = 99
	}

	private KAEditBox mTxtUserName;

	private KAEditBox mTxtEmailID;

	private KAUIGenericDB mGenericDB;

	private List<InviteResponse> mInviteResponseList = new List<InviteResponse>();

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public LocaleString _InviteSent = new LocaleString("Invite sent ");

	public LocaleString _InvalidEmailID = new LocaleString("Invalid EMail ID");

	public LocaleString _CannotParseAuthorizationKey = new LocaleString(" CannotParseAuthorizationKey ");

	public LocaleString _GenericError = new LocaleString("GenericError");

	public LocaleString _AlreadySent = new LocaleString("Already sent You have invited this {{emailID}} already. You will only receive Gems once preregistered email");

	public LocaleString _BadPhraseUse = new LocaleString("Bad phrase used");

	public LocaleString _InvalidNameText = new LocaleString("Name field cannot be empty.");

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	private void Initialize()
	{
		mTxtUserName = (KAEditBox)FindItem("TxtEditUserName");
		mTxtEmailID = (KAEditBox)FindItem("TxtEditEmailID");
		KAUI.SetExclusive(this, _MaskColor);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnInviteFriend")
		{
			string text = mTxtUserName.GetText().Trim();
			string[] array = mTxtEmailID.GetText().Split(',');
			List<string> list = new List<string>();
			if (string.IsNullOrEmpty(text))
			{
				ShowMessage(_InvalidNameText, "OnCloseDB");
				return;
			}
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string text2 = array2[i].Trim();
				if (!UtUtilities.IsValidEmail(text2))
				{
					AddMessageToList(text2, InviteResult.InvalidEmailAddress);
				}
				else if (!string.IsNullOrEmpty(text2) && !list.Contains(text2))
				{
					list.Add(text2);
				}
			}
			if (list.Count > 0)
			{
				InviteFriend.SendInvite(text, list.ToArray(), InviteFriendResponse, null);
				KAUICursorManager.SetDefaultCursor("Loading");
				SetInteractive(interactive: false);
			}
			else
			{
				OnExit();
			}
		}
		else if (inWidget.name == "BtnClosePopUp")
		{
			OnExit();
		}
	}

	public void InviteFriendResponse(WsServiceType inType, WsServiceEvent inEvent, object inResult)
	{
		if (inType != WsServiceType.SEND_FRIEND_INVITE)
		{
			return;
		}
		bool flag = false;
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			if (inResult == null)
			{
				AddMessageToList("", InviteResult.GenericError);
			}
			else
			{
				InviteData inviteData = (InviteData)inResult;
				if (inviteData.FilterNameResponse)
				{
					InviteFriendResult[] inviteFriendResult = inviteData.InviteFriendResult;
					foreach (InviteFriendResult inviteFriendResult2 in inviteFriendResult)
					{
						AddMessageToList(inviteFriendResult2.Email, (InviteResult)inviteFriendResult2.MailingResult);
					}
				}
				else
				{
					mTxtUserName.SetText("");
					InviteResponse resultResponse = new InviteResponse("", GetResultText(InviteResult.BadPhrase));
					PopUpResultText(resultResponse, "BadPhraseDBClosed");
				}
			}
			flag = true;
			break;
		case WsServiceEvent.ERROR:
			AddMessageToList("", InviteResult.GenericError);
			flag = true;
			break;
		}
		if (flag)
		{
			ShowResponse();
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	private void OnExit()
	{
		InviteFriend.OnInviteFriendClosed();
		if (mGenericDB != null && mGenericDB.gameObject != null)
		{
			Object.Destroy(mGenericDB.gameObject);
		}
		if (!ShowResponse())
		{
			Object.Destroy(base.transform.root.gameObject);
			if (InviteFriend.pUpdateUserMessageObj && WsUserMessage.pInstance != null)
			{
				WsUserMessage.pInstance.OnClose();
				InviteFriend.pUpdateUserMessageObj = false;
			}
			KAUI.RemoveExclusive(this);
			RsResourceManager.Unload(GameConfig.GetKeyData("InviteFriendAsset"));
		}
	}

	private void PopUpResultText(InviteResponse resultResponse, string okMsg)
	{
		mGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Invite Friend Result");
		SetVisibility(inVisible: false);
		if (mGenericDB != null)
		{
			mGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			string localizedString = resultResponse._ResultText.GetLocalizedString();
			localizedString = localizedString.Replace("{{email id}}", resultResponse._EmailID);
			mGenericDB.SetText(localizedString, interactive: false);
			mGenericDB._MessageObject = base.gameObject;
			mGenericDB._OKMessage = okMsg;
			KAUI.SetExclusive(mGenericDB, _MaskColor);
		}
	}

	private LocaleString GetResultText(InviteResult result)
	{
		LocaleString result2 = new LocaleString("");
		switch (result)
		{
		case InviteResult.Sent:
			result2 = _InviteSent;
			break;
		case InviteResult.InvalidEmailAddress:
			result2 = _InvalidEmailID;
			break;
		case InviteResult.CannotParseAuthorizationKey:
			result2 = _CannotParseAuthorizationKey;
			break;
		case InviteResult.GenericError:
			result2 = _GenericError;
			break;
		case InviteResult.AlreadySent:
			result2 = _AlreadySent;
			break;
		case InviteResult.BadPhrase:
			result2 = _BadPhraseUse;
			break;
		}
		return result2;
	}

	private bool ShowResponse()
	{
		if (mInviteResponseList.Count > 0)
		{
			InviteResponse resultResponse = mInviteResponseList[0];
			PopUpResultText(resultResponse, "OnExit");
			mInviteResponseList.RemoveAt(0);
			return true;
		}
		return false;
	}

	private void AddMessageToList(string emailID, InviteResult mailingResult)
	{
		InviteResponse item = new InviteResponse(emailID, GetResultText(mailingResult));
		mInviteResponseList.Add(item);
	}

	private void BadPhraseDBClosed()
	{
		if (mGenericDB != null && mGenericDB.gameObject != null)
		{
			Object.Destroy(mGenericDB.gameObject);
		}
		KAUI.SetExclusive(this, _MaskColor);
		SetVisibility(inVisible: true);
		SetInteractive(interactive: true);
	}

	private void ShowMessage(LocaleString inMessage, string okMsg)
	{
		if (!(mGenericDB != null))
		{
			mGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Invite Friend Result");
			SetVisibility(inVisible: false);
			if (mGenericDB != null)
			{
				mGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
				mGenericDB.SetText(inMessage.GetLocalizedString(), interactive: false);
				mGenericDB._MessageObject = base.gameObject;
				mGenericDB._OKMessage = okMsg;
				KAUI.SetExclusive(mGenericDB, _MaskColor);
			}
		}
	}

	private void OnCloseDB()
	{
		if (mGenericDB != null)
		{
			KAUI.RemoveExclusive(mGenericDB);
			Object.Destroy(mGenericDB.gameObject);
			SetVisibility(inVisible: true);
		}
	}
}
