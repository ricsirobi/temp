using UnityEngine;

public class UiInviteFriend : KAUI
{
	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public UiInviteFriendEmail _EmailInvite;

	protected AvAvatarState mCachedAvatarState = AvAvatarState.IDLE;

	protected KAWidget mCloseBtn;

	protected KAWidget mBtnInviteEmail;

	protected GameObject mUiGenericDB;

	protected string mBuddyCode = "";

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	protected virtual void Initialize()
	{
		KAUI.SetExclusive(this, _MaskColor);
		mBtnInviteEmail = FindItem("BtnInviteEmail");
		mCloseBtn = FindItem("CloseBtn");
		if (_EmailInvite != null)
		{
			_EmailInvite.SetVisibility(inVisible: false);
			_EmailInvite.gameObject.SetActive(value: false);
		}
		if (AvAvatar.pState != AvAvatarState.PAUSED)
		{
			mCachedAvatarState = AvAvatar.pState;
			AvAvatar.pState = AvAvatarState.PAUSED;
		}
		ShowEmailInvite();
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

	protected virtual void ShowEmailInvite()
	{
		KAUI.RemoveExclusive(this);
		SetVisibility(inVisible: false);
		_EmailInvite.gameObject.SetActive(value: true);
		_EmailInvite.SetVisibility(inVisible: true);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnInviteEmail)
		{
			if (null != _EmailInvite)
			{
				ShowEmailInvite();
			}
		}
		else if (inWidget == mCloseBtn)
		{
			OnExit();
		}
	}

	protected virtual void OnExit()
	{
		InviteFriend.OnInviteFriendClosed();
		Object.Destroy(base.gameObject);
		KAUI.RemoveExclusive(this);
		if (InviteFriend.pUpdateUserMessageObj && WsUserMessage.pInstance != null)
		{
			WsUserMessage.pInstance.OnClose();
			InviteFriend.pUpdateUserMessageObj = false;
		}
		RsResourceManager.Unload(GameConfig.GetKeyData("InviteFriendAsset"));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		AvAvatar.pState = mCachedAvatarState;
	}
}
