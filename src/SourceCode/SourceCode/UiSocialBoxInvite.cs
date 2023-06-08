using UnityEngine;

public class UiSocialBoxInvite : KAUI
{
	public string _HouseLevel = "FarmingDO";

	public LocaleString _JoinBuddyErrorText = new LocaleString("You cannot visit your Friend at this time.");

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public GameObject _MessageObject;

	public string _CloseMessage = "";

	private KAWidget mOKBtn;

	private KAWidget mCloseBtn;

	private string mFriendUserID = "";

	private KAUIGenericDB mKAUIGenericDB;

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	private void Initialize()
	{
		mOKBtn = FindItem("OKBtn");
		mCloseBtn = FindItem("CloseBtn");
		KAUI.SetExclusive(this, _MaskColor);
	}

	public void SetFriendName(string name)
	{
		KAWidget kAWidget = FindItem("TxtDialog");
		string text = kAWidget.GetText();
		text = text.Replace("{friend}", name);
		kAWidget.SetText(text);
	}

	public void SetFriendUID(string userID)
	{
		mFriendUserID = userID;
		bool flag = false;
		if (BuddyList.pIsReady)
		{
			Buddy buddy = BuddyList.pInstance.GetBuddy(userID);
			if (buddy != null && !string.IsNullOrEmpty(buddy.DisplayName))
			{
				flag = true;
				SetFriendName(buddy.DisplayName);
			}
		}
		if (!flag)
		{
			WsWebService.GetDisplayNameByUserID(userID, InviteHandler, null);
		}
	}

	private void InviteHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && inType == WsServiceType.GET_DISPLAYNAME_BY_USER_ID && !string.IsNullOrEmpty((string)inObject))
		{
			SetFriendName((string)inObject);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mOKBtn)
		{
			if (MainStreetMMOClient.pIsMMOEnabled && MainStreetMMOClient.pInstance.JoinOwnerSpace(_HouseLevel, mFriendUserID))
			{
				AvAvatar.pState = AvAvatarState.IDLE;
				AvAvatar.SetActive(inActive: false);
				Input.ResetInputAxes();
			}
			else
			{
				ShowDialog(_JoinBuddyErrorText);
			}
		}
		else if (inWidget == mCloseBtn && _CloseMessage.Length > 0 && _MessageObject != null)
		{
			_MessageObject.SendMessage(_CloseMessage, null, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void ShowDialog(LocaleString text)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "Message Box");
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "KillGenericDB";
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB.SetTextByID(text._ID, text._Text, interactive: false);
		KAUI.SetExclusive(mKAUIGenericDB);
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
}
