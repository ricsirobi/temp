public class UiClansInviteFriendEmailDB : KAUI
{
	public bool _UseBuddyCodeInvite;

	public UiClansInviteFriendDB _UiInviteFriendDB;

	public LocaleString _FriendEmailText = new LocaleString("Friend's Email");

	public LocaleString _FriendBuddyCodeText = new LocaleString("Friend's Buddy Code");

	public LocaleString _InvalidBuddyCode = new LocaleString("Invalid buddy code");

	public LocaleString _BadPhraseUse = new LocaleString("Bad phrase used");

	private KAWidget mBtnOk;

	private KAWidget mBtnClose;

	private KAEditBox mTxtEditEmailID;

	private KAEditBox mTxtEditUserName;

	private bool mIsEmailInvite = true;

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
			string inUserName = mTxtEditUserName.GetText().Trim();
			_UiInviteFriendDB.ProcessFriendInvite(mTxtEditEmailID.GetText(), inUserName, mIsEmailInvite);
			_UiInviteFriendDB.SetVisibility(inVisible: true);
			base.gameObject.SetActive(value: false);
		}
		else if (inWidget == mBtnClose)
		{
			_UiInviteFriendDB.SetVisibility(inVisible: true);
			base.gameObject.SetActive(value: false);
		}
	}

	public void Init(bool isEmailInvite, string inClanName)
	{
		mIsEmailInvite = isEmailInvite;
		FindItem("TxtTitleFriends").SetText(mIsEmailInvite ? _FriendEmailText.GetLocalizedString() : _FriendBuddyCodeText.GetLocalizedString());
		KAWidget kAWidget = FindItem("TxtClanName");
		if (kAWidget != null)
		{
			kAWidget.SetText(inClanName);
		}
		FindItem("TxtEditEmailID").SetText("");
		FindItem("TxtEditUserName").SetText("");
	}
}
