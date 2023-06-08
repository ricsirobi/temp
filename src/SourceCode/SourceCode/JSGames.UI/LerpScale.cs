using UnityEngine;

namespace JSGames.UI;

public class LerpScale : MonoBehaviour
{
	[SerializeField]
	private Vector3 m_TargetScale;

	[SerializeField]
	private float m_Duration;

	[SerializeField]
	private bool m_PlayOnAwake;

	private Vector3 mOriginalScale;

	private float mTime;

	private float mLerpTime;

	private bool mEffectApplied;

	private bool mAllowEffect;

	private void Start()
	{
		mOriginalScale = base.transform.localScale;
		mTime = 0f;
		mAllowEffect = m_PlayOnAwake;
	}

	private void Update()
	{
		if (mAllowEffect)
		{
			PlayLerp();
		}
	}

	private void PlayLerp()
	{
		Vector3 a = (mEffectApplied ? m_TargetScale : mOriginalScale);
		Vector3 b = (mEffectApplied ? mOriginalScale : m_TargetScale);
		mLerpTime += Time.deltaTime / m_Duration;
		Vector3 localScale = Vector3.Lerp(a, b, mLerpTime);
		base.transform.localScale = localScale;
		mTime += Time.deltaTime;
		if (mTime > m_Duration)
		{
			mTime = 0f;
			mLerpTime = 0f;
			mEffectApplied = !mEffectApplied;
		}
	}

	public void ResetEffect()
	{
		mTime = 0f;
		mEffectApplied = false;
		mAllowEffect = false;
		base.transform.localScale = mOriginalScale;
	}
}
