using UnityEngine;

public class UiInviteFriendRegistered : KAUI
{
	private KAWidget mTxtFriendRegistered;

	private KAWidget mTxtGift;

	private KAWidget mTxtAddedFriend;

	private KAWidget mYesBtn;

	private KAWidget mNoBtn;

	private KAWidget mCloseBtn;

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public GameObject _MessageObject;

	public string _YesMessage = "";

	public string _NoMessage = "";

	public string _CloseMessage = "";

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	private void Initialize()
	{
		UpdateRef();
		KAUI.SetExclusive(this, _MaskColor);
	}

	private void UpdateRef()
	{
		mTxtFriendRegistered = FindItem("TxtFriendRegistered");
		mTxtGift = FindItem("TxtGift");
		mTxtAddedFriend = FindItem("TxtAddedFriend");
		mYesBtn = FindItem("YesBtn");
		mNoBtn = FindItem("NoBtn");
		mCloseBtn = FindItem("CloseBtn");
		if (UtPlatform.IsiOS() && mYesBtn != null && mNoBtn != null)
		{
			Vector3 localPosition = mNoBtn.transform.localPosition;
			mNoBtn.transform.localPosition = mYesBtn.transform.localPosition;
			mYesBtn.transform.localPosition = localPosition;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mYesBtn && _YesMessage.Length > 0)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage(_YesMessage, null, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (inWidget == mNoBtn && _NoMessage.Length > 0)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage(_NoMessage, null, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (inWidget == mCloseBtn && _CloseMessage.Length > 0 && _MessageObject != null)
		{
			_MessageObject.SendMessage(_CloseMessage, null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void SetFriendNameAndRewardText(string friendRegisteredLine, string rewardLine, string buddyLine)
	{
		UpdateRef();
		if (mTxtFriendRegistered != null)
		{
			mTxtFriendRegistered.SetText(friendRegisteredLine);
		}
		if (mTxtGift != null)
		{
			mTxtGift.SetText(rewardLine);
		}
		if (mTxtAddedFriend != null)
		{
			mTxtAddedFriend.SetText(buddyLine);
		}
		Money.UpdateMoneyFromServer();
	}
}
