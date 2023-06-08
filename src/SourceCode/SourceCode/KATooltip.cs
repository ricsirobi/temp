using UnityEngine;

public class KATooltip : KAMonoBase
{
	private static KATooltip mInstance;

	private float mEffectTimer;

	private KATooltipInfo mTooltipInfo;

	private UISprite mBackground;

	private UILabel mLabel;

	private Vector3 mSize;

	private bool mReferenceAdded;

	public bool pReferenceAdded
	{
		get
		{
			return mReferenceAdded;
		}
		set
		{
			mReferenceAdded = value;
		}
	}

	public void Awake()
	{
		if (!mReferenceAdded)
		{
			KAUI.UpdateReferences(base.gameObject, add: true);
		}
		if (mInstance == null)
		{
			mInstance = this;
			mLabel = GetComponentInChildren(typeof(UILabel)) as UILabel;
			mBackground = GetComponentInChildren(typeof(UISprite)) as UISprite;
			base.enabled = false;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public static void Show(KAButton inWidget, bool inShow, int inLayer, float inZOffset)
	{
		if (!(mInstance == null) && !(inWidget == null) && inWidget._TooltipInfo != null)
		{
			mInstance.DisplayTooltip(inWidget, inLayer, inZOffset, inShow);
		}
	}

	public void DisplayTooltip(KAButton inWidget, int inLayer, float inZOffset, bool inShow)
	{
		mTooltipInfo = inWidget._TooltipInfo;
		if (mTooltipInfo._Sound != null && mTooltipInfo._Sound._AudioClip != null && inShow)
		{
			SnChannel.Play(mTooltipInfo._Sound._AudioClip, mTooltipInfo._Sound._Settings, mTooltipInfo._Sound._Triggers, inForce: false);
		}
		if (!mTooltipInfo._ShowUI)
		{
			if (!inShow)
			{
				mInstance.enabled = false;
				return;
			}
			if (string.IsNullOrEmpty(mTooltipInfo._Text.GetLocalizedString()))
			{
				return;
			}
			base.enabled = true;
			SetTooltipInfo(inWidget);
		}
		else
		{
			base.enabled = true;
			EnableComponent(inEnable: false);
			if (inWidget.pUI != null)
			{
				inWidget.pUI.OnShowUITooltip(mInstance, inWidget, inShow);
			}
		}
		if (mTooltipInfo._UpdatePosition)
		{
			PositionTooltip(inWidget, inLayer, inZOffset);
		}
	}

	private void PositionTooltip(KAButton inWidget, int inLayer, float inZOffset)
	{
		if (inLayer == LayerMask.NameToLayer("2DNGUI"))
		{
			mInstance.transform.parent = null;
			base.transform.position = inWidget.transform.position + new Vector3(mTooltipInfo._Offset.x, mTooltipInfo._Offset.y, inZOffset);
			Bounds bounds = new Bounds(KAUIManager.pInstance.camera.pixelRect.center, KAUIManager.pInstance.camera.pixelRect.size);
			Vector3 vector = KAUIManager.pInstance.camera.ScreenToWorldPoint(bounds.min);
			Vector3 vector2 = KAUIManager.pInstance.camera.ScreenToWorldPoint(bounds.max);
			Bounds bounds2 = NGUIMath.CalculateAbsoluteWidgetBounds(base.transform);
			Vector3 center = bounds2.center;
			if (bounds2.min.x < vector.x)
			{
				center.x = vector.x + bounds2.size.x / 2f;
			}
			else if (bounds2.max.x > vector2.x)
			{
				center.x = vector2.x - bounds2.size.x / 2f;
			}
			if (bounds2.min.y < vector.y)
			{
				center.y = vector.y + bounds2.size.y / 2f;
			}
			else if (bounds2.max.y > vector2.y)
			{
				center.y = vector2.y - bounds2.size.y / 2f;
			}
			bounds2.center = center;
			base.transform.position = bounds2.center;
			base.transform.rotation = Quaternion.identity;
		}
		else if (inLayer == LayerMask.NameToLayer("3DNGUI"))
		{
			mInstance.transform.parent = inWidget.transform;
			base.transform.localPosition = new Vector3(mTooltipInfo._Offset.x, mTooltipInfo._Offset.y, inZOffset);
			base.transform.localRotation = Quaternion.identity;
		}
		base.transform.localScale = Vector3.one;
		if (inLayer > 0)
		{
			mInstance.gameObject.layer = inLayer;
		}
	}

	private void SetTooltipInfo(KAButton inWidget)
	{
		mEffectTimer = 0f;
		mTooltipInfo = inWidget._TooltipInfo;
		if (mTooltipInfo._Font != null)
		{
			if (pReferenceAdded)
			{
				KAUI.UpdateReferences(base.gameObject, add: false);
			}
			mLabel.bitmapFont = mTooltipInfo._Font;
			KAUI.UpdateReferences(base.gameObject, add: true);
		}
		mLabel.text = mTooltipInfo._Text.GetLocalizedString();
		Color color = mTooltipInfo._Color;
		if (mBackground != null)
		{
			if (mTooltipInfo._Atlas != null)
			{
				mBackground.atlas = mTooltipInfo._Atlas;
				mBackground.spriteName = mTooltipInfo._BackgroundSprite;
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
		if (mBackground != null && mLabel != null && mLabel.bitmapFont != null)
		{
			mBackground.width = mLabel.width + (int)mTooltipInfo._Padding.x;
			mBackground.height = mLabel.height + (int)mTooltipInfo._Padding.y;
		}
		if (mTooltipInfo._Style == TooltipStyle.SCALE || mTooltipInfo._Style == TooltipStyle.FADE_AND_SCALE)
		{
			base.transform.localScale = Vector3.one * mTooltipInfo._InitialScale;
		}
	}

	private void Update()
	{
		if (RsResourceManager.pLevelLoading)
		{
			mInstance.enabled = false;
		}
		else if (mEffectTimer < mTooltipInfo._Duration)
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
		base.transform.localScale = Vector3.one * num;
	}

	private void OnEnable()
	{
		EnableComponent(inEnable: true);
	}

	private void OnDisable()
	{
		base.transform.parent = null;
		EnableComponent(inEnable: false);
	}

	private void EnableComponent(bool inEnable)
	{
		if (mLabel != null)
		{
			mLabel.enabled = inEnable;
		}
		if (mBackground != null)
		{
			mBackground.enabled = inEnable;
		}
	}

	protected void OnDestroy()
	{
		if (mReferenceAdded)
		{
			KAUI.UpdateReferences(base.gameObject, add: false);
		}
	}
}
