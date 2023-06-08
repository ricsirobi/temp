using UnityEngine;
using UnityEngine.UI;

namespace JSGames.UI;

public class TextFlash : MonoBehaviour
{
	[SerializeField]
	private Color32 m_TargetColor;

	[SerializeField]
	private Vector3 m_TargetScale;

	[SerializeField]
	private float m_FlashSpeed;

	[SerializeField]
	private bool m_DisableBlink;

	[SerializeField]
	private bool m_LerpEffects;

	private Color32 mOriginalColor;

	private Vector3 mOriginalScale;

	private Text mText;

	private float mTime;

	private bool mEffectApplied;

	private float mLerpTime;

	private void Start()
	{
		mText = base.gameObject.GetComponent<Text>();
		if (mText == null)
		{
			UIWidget component = base.gameObject.GetComponent<UIWidget>();
			if (component != null)
			{
				mText = component._Text;
			}
		}
		if (mText != null)
		{
			mOriginalColor = mText.color;
			mOriginalScale = mText.transform.localScale;
			mTime = 0f;
		}
	}

	private void Update()
	{
		if (mText == null)
		{
			return;
		}
		if (m_DisableBlink)
		{
			if (!mEffectApplied)
			{
				ApplyEffects();
			}
			return;
		}
		if (m_LerpEffects)
		{
			LerpEffects();
		}
		mTime += Time.deltaTime;
		if (mTime > m_FlashSpeed)
		{
			mTime = 0f;
			mLerpTime = 0f;
			ApplyEffects();
		}
	}

	private void LerpEffects()
	{
		Color32 a = (mEffectApplied ? m_TargetColor : mOriginalColor);
		Color32 b = (mEffectApplied ? mOriginalColor : m_TargetColor);
		mLerpTime += Time.deltaTime / m_FlashSpeed;
		Color32 color = Color32.Lerp(a, b, mLerpTime);
		Vector3 a2 = (mEffectApplied ? m_TargetScale : mOriginalScale);
		Vector3 b2 = (mEffectApplied ? mOriginalScale : m_TargetScale);
		Vector3 scale = Vector3.Lerp(a2, b2, mLerpTime);
		ApplyEffects(color, scale);
	}

	private void ApplyEffects()
	{
		mText.color = (mEffectApplied ? mOriginalColor : m_TargetColor);
		mText.transform.localScale = (mEffectApplied ? mOriginalScale : m_TargetScale);
		mEffectApplied = !mEffectApplied;
	}

	private void ApplyEffects(Color color, Vector3 scale)
	{
		mText.color = color;
		mText.transform.localScale = scale;
	}

	public void ResetEffects()
	{
		mTime = 0f;
		mEffectApplied = false;
		mText.color = mOriginalColor;
		mText.transform.localScale = mOriginalScale;
	}
}
