using UnityEngine;

internal class GenericMessage
{
	protected MessageInfo mMessageInfo;

	protected GameObject mUiGenericDB;

	protected AudioClip mAudioClip;

	protected AudioClip mMemberAudioClip;

	protected AudioClip mNonMemberAudioClip;

	public bool _Tagged;

	public bool _IsSystemMessage;

	public bool _YesBtnVisible;

	public bool _NoBtnVisible;

	public bool _OKBtnVisible = true;

	public bool _CloseBtnVisible;

	public string _OKMessage = "OnClose";

	public string _CloseMessage = "OnClose";

	public string _YesMessage = "OnYes";

	public string _NoMessage = "OnNo";

	public MessageInfo pMessageInfo => mMessageInfo;

	public GenericMessage(MessageInfo messageInfo)
	{
		mMessageInfo = messageInfo;
	}

	public GenericMessage(MessageInfo messageInfo, AudioClip audioClip)
	{
		mMessageInfo = messageInfo;
		mAudioClip = audioClip;
	}

	public GenericMessage(MessageInfo messageInfo, AudioClip memberAudioClip, AudioClip nonMemberAudioClip)
	{
		mMessageInfo = messageInfo;
		mMemberAudioClip = memberAudioClip;
		mNonMemberAudioClip = nonMemberAudioClip;
	}

	public void Save(bool delete = true)
	{
		WsUserMessage.pInstance.SaveMessage(mMessageInfo, delete);
	}

	public virtual void Show()
	{
		if (!_IsSystemMessage)
		{
			if (mUiGenericDB == null)
			{
				mUiGenericDB = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
			}
			KAUIGenericDB component = mUiGenericDB.GetComponent<KAUIGenericDB>();
			component._MessageObject = WsUserMessage.pInstance.gameObject;
			component._CloseMessage = _CloseMessage;
			component._OKMessage = _OKMessage;
			component._YesMessage = _YesMessage;
			component._NoMessage = _NoMessage;
			component.SetButtonVisibility(_YesBtnVisible, _NoBtnVisible, _OKBtnVisible, _CloseBtnVisible);
			if (_Tagged)
			{
				TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(mMessageInfo);
				component.SetText(taggedMessageHelper.MemberMessage["Line1"], interactive: false);
			}
			else
			{
				component.SetText(mMessageInfo.MemberMessage, interactive: false);
			}
			KAUI.SetExclusive(component, WsUserMessage.pInstance._MaskColor);
			if (mMemberAudioClip != null && mNonMemberAudioClip != null)
			{
				if (SubscriptionInfo.pIsMember)
				{
					SnChannel.Play(mMemberAudioClip, "VO_Pool", inForce: true, null);
				}
				else
				{
					SnChannel.Play(mNonMemberAudioClip, "VO_Pool", inForce: true, null);
				}
			}
			else if ((bool)mAudioClip)
			{
				SnChannel.Play(mAudioClip, "VO_Pool", inForce: true, null);
			}
			return;
		}
		WsUserMessage.pInstance.ShowSystemMessage(mMessageInfo, _Tagged);
		if (mAudioClip == null || mMemberAudioClip == null || mMemberAudioClip == null)
		{
			WsUserMessage.pInstance.OnClose();
		}
		else if (mMemberAudioClip != null && mNonMemberAudioClip != null)
		{
			if (SubscriptionInfo.pIsMember)
			{
				SnChannel.Play(mMemberAudioClip, "VO_Pool", inForce: true, WsUserMessage.pInstance.gameObject);
			}
			else
			{
				SnChannel.Play(mNonMemberAudioClip, "VO_Pool", inForce: true, WsUserMessage.pInstance.gameObject);
			}
		}
		else if ((bool)mAudioClip)
		{
			SnChannel.Play(mAudioClip, "VO_Pool", inForce: true, WsUserMessage.pInstance.gameObject);
		}
	}

	public virtual void Update()
	{
	}

	public virtual void Close()
	{
		if (mUiGenericDB != null)
		{
			KAUIGenericDB component = mUiGenericDB.GetComponent<KAUIGenericDB>();
			if (component != null)
			{
				KAUI.RemoveExclusive(component);
			}
			Object.Destroy(mUiGenericDB);
			mUiGenericDB = null;
		}
		SnChannel.StopPool("VO_Pool");
	}

	public virtual void Yes()
	{
	}

	public virtual void No()
	{
	}
}
