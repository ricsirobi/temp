using UnityEngine;

public class UiSocialCrateInviteFriendEmailDB : KAUI
{
	public UiSocialCrateInviteFriendDB _UiMessageBuddy;

	public LocaleString _FriendEmailText = new LocaleString("Friend's Email");

	public LocaleString _BadPhraseUse = new LocaleString("Bad phrase used");

	public LocaleString _EmptyFriendEmailErrorText = new LocaleString("Email can not be empty.");

	public LocaleString _EmptyUserNameErrorText = new LocaleString("User name can not be empty.");

	public LocaleString _ErrorText = new LocaleString("Error");

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	private KAWidget mBtnOk;

	private KAWidget mBtnClose;

	private KAEditBox mTxtEditEmailID;

	private KAEditBox mTxtEditUserName;

	private KAUIGenericDB mKAUIGenericDB;

	protected override void Start()
	{
		base.Start();
		mBtnOk = FindItem("BtnInviteFriend");
		mBtnClose = FindItem("BtnClosePopUp");
		mTxtEditEmailID = (KAEditBox)FindItem("TxtEditEmailID");
		mTxtEditUserName = (KAEditBox)FindItem("TxtEditUserName");
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnOk)
		{
			string text = mTxtEditUserName.GetText().Trim();
			string text2 = mTxtEditEmailID.GetText();
			if (string.IsNullOrEmpty(text))
			{
				ShowEmptyFieldErrorDB(isEmptyName: true);
				return;
			}
			if (string.IsNullOrEmpty(text2.Trim()))
			{
				ShowEmptyFieldErrorDB(isEmptyName: false);
				return;
			}
			_UiMessageBuddy.ProcessMessageFriend(text2, text);
			_UiMessageBuddy.SetVisibility(inVisible: true);
			SetVisibility(inVisible: false);
			KAUI.RemoveExclusive(this);
		}
		else if (inWidget == mBtnClose)
		{
			_UiMessageBuddy.SetVisibility(inVisible: true);
			SetVisibility(inVisible: false);
			KAUI.RemoveExclusive(this);
		}
	}

	protected override void Update()
	{
		base.Update();
	}

	public void Init()
	{
		FindItem("TxtTitleFriends").SetText(_FriendEmailText.GetLocalizedString());
		FindItem("TxtEditEmailID").SetText("");
		FindItem("TxtEditUserName").SetText("");
		KAUI.SetExclusive(this);
	}

	private void ShowEmptyFieldErrorDB(bool isEmptyName)
	{
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "EmptyFieldErrorDB");
		if (isEmptyName)
		{
			mKAUIGenericDB.SetText(_EmptyUserNameErrorText.GetLocalizedString(), interactive: false);
		}
		else
		{
			mKAUIGenericDB.SetText(_EmptyFriendEmailErrorText.GetLocalizedString(), interactive: false);
		}
		mKAUIGenericDB.SetTitle(_ErrorText.GetLocalizedString());
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "KillGenericDB";
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(mKAUIGenericDB, _MaskColor);
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
