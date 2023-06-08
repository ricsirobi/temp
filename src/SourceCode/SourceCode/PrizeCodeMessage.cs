using UnityEngine;

internal class PrizeCodeMessage : GenericMessage
{
	private GameObject mUIPrizeDB;

	public PrizeCodeMessage(MessageInfo messageInfo, AudioClip audioClip)
		: base(messageInfo, audioClip)
	{
		Start();
	}

	public PrizeCodeMessage(MessageInfo messageInfo, AudioClip memberAudioClip, AudioClip nonMemberAudioclip)
		: base(messageInfo, memberAudioClip, nonMemberAudioclip)
	{
		Start();
	}

	private void Start()
	{
		if (CommonInventoryData.pIsReady)
		{
			CommonInventoryData.ReInit();
		}
	}

	public override void Show()
	{
		mUIPrizeDB = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUiPrizeCodeMessageDB"));
		if (mUIPrizeDB == null)
		{
			WsUserMessage.pInstance.OnClose();
			return;
		}
		UiPrizeCodeDB component = mUIPrizeDB.GetComponent<UiPrizeCodeDB>();
		string empty = string.Empty;
		empty = string.Empty;
		if (_Tagged)
		{
			TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(mMessageInfo);
			if (taggedMessageHelper.MemberMessage.ContainsKey("Line1"))
			{
				empty = taggedMessageHelper.MemberMessage["Line1"];
			}
		}
		if (string.IsNullOrEmpty(empty))
		{
			empty = mMessageInfo.MemberMessage;
		}
		component.Show(empty, mMessageInfo.MemberImageUrl, this);
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
	}

	public override void Close()
	{
		Object.Destroy(mUIPrizeDB);
		SnChannel.StopPool("VO_Pool");
	}
}
