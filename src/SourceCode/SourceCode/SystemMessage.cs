using UnityEngine;

internal class SystemMessage : GenericMessage
{
	public SystemMessage(MessageInfo inMessage, AudioClip inAudio)
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
