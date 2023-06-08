using System;
using UnityEngine;

public class ObTimedAnim : KAMonoBase
{
	[Serializable]
	public class TimedAnim
	{
		public string _Name = "";

		public float _Time;

		public float _Blend;

		public WrapMode _WrapMode = WrapMode.Loop;

		public SnRandomSound _Sounds;
	}

	public TimedAnim[] _Anims;

	public bool _PlayOnAwake = true;

	private int mPlayCount;

	private SnChannel mCurrentChannel;

	public void Awake()
	{
		if (_PlayOnAwake)
		{
			PlayNextAnim();
		}
	}

	public void PlayNextAnim()
	{
		if (_Anims != null && _Anims.Length != 0)
		{
			if (mCurrentChannel != null)
			{
				mCurrentChannel.Stop();
			}
			mCurrentChannel = null;
			int num = mPlayCount % _Anims.Length;
			mPlayCount++;
			TimedAnim timedAnim = _Anims[num];
			if (timedAnim != null && base.animation != null && base.animation[timedAnim._Name] != null)
			{
				base.animation.CrossFade(timedAnim._Name, timedAnim._Blend);
				base.animation[timedAnim._Name].wrapMode = timedAnim._WrapMode;
			}
			if (timedAnim._Sounds != null)
			{
				mCurrentChannel = timedAnim._Sounds.Play();
			}
			Invoke("PlayNextAnim", timedAnim._Time);
		}
	}
}
