using UnityEngine;

public class UiInviteRegister : KAUI
{
	private KAWidget mTxtRegistering;

	private KAWidget mTxtGift;

	private KAWidget mOKBtn;

	private KAWidget mCloseBtn;

	private bool mInitialized;

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public GameObject _MessageObject;

	public string _OKMessage = "";

	public string _CloseMessage = "";

	protected override void Start()
	{
		base.Start();
		if (!mInitialized)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		mTxtRegistering = FindItem("TxtRegistering");
		mTxtGift = FindItem("TxtGift");
		mOKBtn = FindItem("OKBtn");
		mCloseBtn = FindItem("CloseBtn");
		KAUI.SetExclusive(this, _MaskColor);
		mInitialized = true;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mOKBtn && _OKMessage.Length > 0)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage(_OKMessage, null, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (inWidget == mCloseBtn && _CloseMessage.Length > 0 && _MessageObject != null)
		{
			_MessageObject.SendMessage(_CloseMessage, null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void SetReward(string registeringLine, string rewardLine)
	{
		if (!mInitialized)
		{
			Initialize();
		}
		if (mTxtRegistering != null)
		{
			mTxtRegistering.SetText(registeringLine);
		}
		if (mTxtGift != null)
		{
			mTxtGift.SetText(rewardLine);
		}
	}
}
