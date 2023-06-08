using UnityEngine;

internal class PromoMessage : GenericMessage
{
	public PromoMessage(MessageInfo inMessage, AudioClip inAudio)
		: base(inMessage, inAudio)
	{
		mMessageInfo = inMessage;
		mAudioClip = inAudio;
	}

	public override void Show()
	{
		_OKBtnVisible = true;
		base.Show();
	}
}
