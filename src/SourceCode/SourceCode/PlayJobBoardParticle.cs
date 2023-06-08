using UnityEngine;

public class PlayJobBoardParticle : MonoBehaviour
{
	public ParticleSystem _Particle;

	public float _Duration;

	private bool mPlaying;

	private float mTimer;

	private bool mTrashed;

	private GameObject mMsgObject;

	public bool pTrashed => mTrashed;

	public void Init(GameObject inObject, bool isTrashed)
	{
		mMsgObject = inObject;
		mTrashed = isTrashed;
	}

	public void PlayParticle(bool isPlay)
	{
		if (_Particle == null)
		{
			if (mMsgObject != null)
			{
				mMsgObject.SendMessage("OnParticleEffectDone", base.gameObject.GetComponent<KAWidget>(), SendMessageOptions.RequireReceiver);
			}
			return;
		}
		mPlaying = isPlay;
		if (isPlay)
		{
			_Particle.Play();
		}
		else
		{
			_Particle.Stop();
		}
	}

	private void Update()
	{
		if (!mPlaying || !(_Duration > 0f))
		{
			return;
		}
		if (mTimer >= _Duration)
		{
			mTimer = 0f;
			PlayParticle(isPlay: false);
			if (mMsgObject != null)
			{
				mMsgObject.SendMessage("OnParticleEffectDone", base.gameObject, SendMessageOptions.RequireReceiver);
			}
		}
		else
		{
			mTimer += Time.deltaTime;
		}
	}
}
