using UnityEngine;

[RequireComponent(typeof(KAWidget))]
public class KAPulsateWidget : KAMonoBase
{
	[SerializeField]
	private KASkinInfo m_FlashEffectInfo = new KASkinInfo();

	[SerializeField]
	private bool m_PlayOnStart;

	[SerializeField]
	private bool m_PlayOnVisible;

	[SerializeField]
	private bool m_StopOnClick;

	[SerializeField]
	private int m_PulseCount = -1;

	private float mDeltaTime;

	private float mWaitTime;

	private int mLoopCount;

	private bool mIsPlaying;

	private KAWidget mWidget;

	private void Awake()
	{
		if (mWidget == null)
		{
			mWidget = GetComponent<KAWidget>();
		}
		mWidget.SetPulsateReference(this);
	}

	private void Start()
	{
		if (m_PlayOnStart)
		{
			Play();
		}
	}

	public void OnVisibilityChange(bool isVisible)
	{
		if (m_PlayOnVisible && isVisible)
		{
			Play();
		}
		else if (mIsPlaying && !isVisible)
		{
			Stop();
		}
	}

	public void OnWidgetClicked(KAWidget widget)
	{
		if (m_StopOnClick && mIsPlaying)
		{
			Stop();
		}
	}

	public void DoUpdate()
	{
		m_FlashEffectInfo.Update();
		if (!mIsPlaying || !(m_FlashEffectInfo._Duration > 0f) || !(mDeltaTime < mWaitTime))
		{
			return;
		}
		mDeltaTime += Time.deltaTime;
		if (!(mDeltaTime >= mWaitTime))
		{
			return;
		}
		if (m_PulseCount != -1)
		{
			mLoopCount++;
			if (mLoopCount == m_PulseCount)
			{
				mIsPlaying = false;
				return;
			}
		}
		mDeltaTime -= mWaitTime;
		m_FlashEffectInfo.DoEffect(inShowEffect: true);
	}

	public void Play()
	{
		mDeltaTime = 0f;
		mWaitTime = 2f * m_FlashEffectInfo._Duration;
		mLoopCount = 0;
		mIsPlaying = true;
		m_FlashEffectInfo.DoEffect(inShowEffect: true);
	}

	public void Stop()
	{
		mIsPlaying = false;
		m_FlashEffectInfo.DoEffect(inShowEffect: false);
	}
}
