using UnityEngine;
using UnityEngine.UI;

namespace JSGames.UI;

public class Tooltip : KAMonoBase
{
	public float _Timer = 1f;

	private static Tooltip mInstance;

	private float mEffectTimer;

	private TooltipInfo mTooltipInfo;

	[SerializeField]
	private Image mBackground;

	[SerializeField]
	private Text mLabel;

	private float mTimer;

	private UIWidget mWidget;

	private bool mDisplay;

	private RectTransform mTooltipRect;

	public void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
			base.enabled = false;
			mTooltipRect = mBackground.rectTransform;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public static void Show(UIWidget widget, bool show)
	{
		if (!(mInstance == null) && !(widget == null) && widget._TooltipInfo != null && !string.IsNullOrEmpty(widget._TooltipInfo._Text.GetLocalizedString()))
		{
			if (show)
			{
				mInstance.enabled = show;
				mInstance.SetValues(widget);
			}
			else
			{
				mInstance.DisplayTooltip(widget, show: false);
			}
		}
	}

	private void SetValues(UIWidget widget)
	{
		mWidget = widget;
		mTimer = _Timer;
	}

	public void DisplayTooltip(UIWidget widget, bool show)
	{
		mTooltipInfo = widget._TooltipInfo;
		mDisplay = !mTooltipInfo._HideTooltip && show;
		if (mDisplay)
		{
			SetTooltipInfo();
			if (mTooltipInfo._Sound != null && mTooltipInfo._Sound._AudioClip != null)
			{
				SnChannel.Play(mTooltipInfo._Sound._AudioClip, mTooltipInfo._Sound._Settings, mTooltipInfo._Sound._Triggers, inForce: false);
			}
		}
		SetActive(mDisplay);
	}

	private void SetTooltipInfo()
	{
		mTooltipRect.localScale = Vector3.one;
		mTooltipRect.position = mWidget.pPosition + new Vector3(mTooltipInfo._Offset.x * mTooltipRect.lossyScale.x, mTooltipInfo._Offset.y * mTooltipRect.lossyScale.y, 0f);
		mTooltipRect.localRotation = Quaternion.identity;
		mEffectTimer = 0f;
		if (mTooltipInfo._Font != null)
		{
			mLabel.font = mTooltipInfo._Font;
		}
		mLabel.text = mTooltipInfo._Text.GetLocalizedString();
		mLabel.fontSize = mTooltipInfo._FontSize;
		Color color = mTooltipInfo._Color;
		if (mBackground != null)
		{
			if (mTooltipInfo._BackgroundImage != null)
			{
				mBackground.sprite = mTooltipInfo._BackgroundImage;
			}
			if (mTooltipInfo._Style == TooltipStyle.FADE || mTooltipInfo._Style == TooltipStyle.FADE_AND_SCALE)
			{
				color.a = 0f;
			}
			mBackground.color = color;
		}
		color = mTooltipInfo._TextColor;
		if (mTooltipInfo._Style == TooltipStyle.FADE || mTooltipInfo._Style == TooltipStyle.FADE_AND_SCALE)
		{
			color.a = 0f;
		}
		mLabel.color = color;
		if (mTooltipInfo._Style == TooltipStyle.SCALE || mTooltipInfo._Style == TooltipStyle.FADE_AND_SCALE)
		{
			mTooltipRect.localScale = Vector3.one * mTooltipInfo._InitialScale;
		}
	}

	private void Update()
	{
		if (mTimer > 0f)
		{
			mTimer -= Time.deltaTime;
			if (mTimer <= 0f)
			{
				mInstance.DisplayTooltip(mWidget, show: true);
			}
		}
		if (RsResourceManager.IsLoading())
		{
			SetActive(inEnable: false);
		}
		else if (mDisplay && mTooltipInfo != null && mEffectTimer < mTooltipInfo._Duration)
		{
			mEffectTimer = Mathf.Min(mTooltipInfo._Duration, mEffectTimer + Time.deltaTime);
			if (mTooltipInfo._Style == TooltipStyle.FADE)
			{
				DoAlphaFadeInEffect();
			}
			else if (mTooltipInfo._Style == TooltipStyle.SCALE)
			{
				DoScaleEffect();
			}
			else if (mTooltipInfo._Style == TooltipStyle.FADE_AND_SCALE)
			{
				DoAlphaFadeInEffect();
				DoScaleEffect();
			}
		}
	}

	private void DoAlphaFadeInEffect()
	{
		float t = ((mTooltipInfo._Duration > 0f) ? (mEffectTimer / mTooltipInfo._Duration) : 1f);
		if (mLabel != null)
		{
			float a = Mathf.Lerp(mTooltipInfo._InitialAlpha, mTooltipInfo._TextColor.a, t);
			Color color = mLabel.color;
			color.a = a;
			mLabel.color = color;
		}
		if (mBackground != null)
		{
			float a2 = Mathf.Lerp(mTooltipInfo._InitialAlpha, mTooltipInfo._Color.a, t);
			Color color2 = mBackground.color;
			color2.a = a2;
			mBackground.color = color2;
		}
	}

	private void DoScaleEffect()
	{
		float num = mTooltipInfo._FinalScale;
		if (mTooltipInfo._Duration > 0f)
		{
			num = Mathf.Lerp(mTooltipInfo._InitialScale, mTooltipInfo._FinalScale, mEffectTimer / mTooltipInfo._Duration);
		}
		mTooltipRect.localScale = Vector3.one * num;
	}

	private void SetActive(bool inEnable)
	{
		if (mLabel != null)
		{
			mLabel.enabled = inEnable;
		}
		if (mBackground != null)
		{
			mBackground.enabled = inEnable;
		}
		base.enabled = inEnable;
	}
}
