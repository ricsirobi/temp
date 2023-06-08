using UnityEngine;

public class UiRacingMessage : KAUI
{
	public Vector3 _MaxBlinkScale = new Vector3(60f, 60f, 1f);

	public Vector3 _MinBlinkScale = new Vector3(40f, 40f, 1f);

	public float _BlinkInDuration = 1f;

	private KAWidget mBkgWrongWay;

	private KAWidget mBkgFinalLap;

	private KAWidget mHurryUp;

	private KAWidget mFinished;

	private KAWidget mMessageText;

	private KAWidget mLapTimeBkg;

	private KAWidget mTextWrongWay;

	private bool mStartHidingWidget;

	private float mHideTime;

	private float mHideTimer;

	private KAWidget mAltHidingWidget;

	private KAWidget mTextHidingWidget;

	private float mPlrLeftMsgShowTimer;

	private KAWidget mTxtPlayerLeft;

	private bool mBlinking;

	private float mBlinkTime;

	public KAWidget pBkgWrongWay => mBkgWrongWay;

	public KAWidget pBkgFinalLap => mBkgFinalLap;

	public KAWidget pHurryUp => mHurryUp;

	protected override void Start()
	{
		base.Start();
		GetReferences();
		HideAll();
	}

	protected override void Update()
	{
		base.Update();
		if (mStartHidingWidget)
		{
			if (mHideTime - mHideTimer > 0f)
			{
				mHideTimer += Time.deltaTime;
			}
			else
			{
				mHideTime = 0f;
				mHideTimer = 0f;
				mStartHidingWidget = false;
				if (mTextHidingWidget != null)
				{
					mTextHidingWidget.SetVisibility(inVisible: false);
				}
				if (mAltHidingWidget != null)
				{
					mAltHidingWidget.SetVisibility(inVisible: false);
				}
				mAltHidingWidget = null;
				mTextHidingWidget = null;
			}
		}
		if (mPlrLeftMsgShowTimer > 0f)
		{
			mPlrLeftMsgShowTimer -= Time.deltaTime;
		}
		else if (mTxtPlayerLeft != null && mTxtPlayerLeft.GetVisibility())
		{
			mTxtPlayerLeft.SetText(string.Empty);
			mTxtPlayerLeft.SetVisibility(inVisible: false);
			mPlrLeftMsgShowTimer = 0f;
		}
		if (mBkgWrongWay != null && mBkgWrongWay.GetVisibility())
		{
			UpdateWidgetScaling(mBkgWrongWay.GetLabel().gameObject);
		}
	}

	private void GetReferences()
	{
		mHurryUp = FindItem("BkgHurryUp");
		mFinished = FindItem("BkgFinished");
		mMessageText = FindItem("TxtMessage");
		mLapTimeBkg = FindItem("BkgLapTime");
		mBkgWrongWay = FindItem("BkgWrongWay");
		mBkgFinalLap = FindItem("BkgFinalLap");
		mTextWrongWay = FindItem("TxtWrongWay");
	}

	public void SetText(LocaleString localeString, string normalString, bool isAppend, bool visibility)
	{
		if (!(mMessageText != null))
		{
			return;
		}
		if (localeString != null)
		{
			if (normalString != null && normalString.Length > 0)
			{
				string stringData = StringTable.GetStringData(localeString._ID, localeString._Text);
				stringData = ((!isAppend) ? (normalString + stringData) : (stringData + normalString));
				mMessageText.SetText(stringData);
			}
			else
			{
				mMessageText.SetTextByID(localeString._ID, localeString._Text);
			}
		}
		else if (normalString != null && normalString.Length > 0)
		{
			mMessageText.SetText(normalString);
		}
		if (visibility)
		{
			ResetFading();
		}
		mMessageText.SetVisibility(visibility);
	}

	public void SetPlayerLeftText(string inText, float inShowTime)
	{
		if (mTxtPlayerLeft == null)
		{
			mTxtPlayerLeft = FindItem("TxtPlayerLeft");
		}
		if (mTxtPlayerLeft != null && !string.IsNullOrEmpty(inText))
		{
			mTxtPlayerLeft.SetText(inText);
			mTxtPlayerLeft.SetVisibility(inVisible: true);
			mPlrLeftMsgShowTimer = inShowTime;
		}
	}

	public void SetLapTimeText(string text, bool visibility)
	{
		if (mLapTimeBkg != null && mMessageText != null)
		{
			SetText(null, text, isAppend: false, visibility: true);
			mLapTimeBkg.SetVisibility(visibility);
		}
	}

	public void SetWrongWayText(string text, bool visibility)
	{
		if (mTextWrongWay != null)
		{
			mTextWrongWay.SetText(text);
			mTextWrongWay.SetVisibility(visibility);
		}
	}

	private void ResetFading()
	{
		mHideTime = 0f;
		mHideTimer = 0f;
		mStartHidingWidget = false;
		if (mTextHidingWidget != null)
		{
			mTextHidingWidget.SetVisibility(inVisible: false);
		}
		if (mAltHidingWidget != null)
		{
			mAltHidingWidget.SetVisibility(inVisible: false);
		}
		mAltHidingWidget = null;
		mTextHidingWidget = null;
	}

	public void FadeOutText(float holdTime, float fadeTime)
	{
		FadeOut(holdTime, fadeTime, mMessageText, null);
	}

	public void FadeOutLapTextandTexture(float holdTime, float fadeTime)
	{
		FadeOut(holdTime, fadeTime, mMessageText, mLapTimeBkg);
	}

	private void FadeOut(float holdTime, float fadeTime, KAWidget inTxtWidget, KAWidget inAltWidget)
	{
		mStartHidingWidget = true;
		mHideTime = holdTime + fadeTime;
		mHideTimer = 0f;
		if (mAltHidingWidget != null)
		{
			mAltHidingWidget.SetVisibility(inVisible: false);
		}
		mAltHidingWidget = inAltWidget;
		if (mTextHidingWidget != null)
		{
			mTextHidingWidget.SetVisibility(inVisible: false);
		}
		mTextHidingWidget = inTxtWidget;
	}

	public void SetTextPlayerEliminated(string text, bool isPlayer, bool visibility)
	{
	}

	public void SetTextEliminatedFade(bool isPlayer)
	{
	}

	public void HideAll()
	{
		GetReferences();
		StopAllCoroutines();
		if (mHurryUp != null)
		{
			mHurryUp.SetVisibility(inVisible: false);
		}
		if (mMessageText != null)
		{
			mMessageText.SetVisibility(inVisible: false);
		}
		if (mHurryUp != null)
		{
			mHurryUp.SetVisibility(inVisible: false);
		}
		if (mFinished != null)
		{
			mFinished.SetVisibility(inVisible: false);
		}
		if (mLapTimeBkg != null)
		{
			mLapTimeBkg.SetVisibility(inVisible: false);
		}
		if (mBkgWrongWay != null)
		{
			mBkgWrongWay.SetVisibility(inVisible: false);
		}
		if (mBkgFinalLap != null)
		{
			mBkgFinalLap.SetVisibility(inVisible: false);
		}
		if (mTextWrongWay != null)
		{
			mTextWrongWay.SetVisibility(inVisible: false);
		}
	}

	public void Flicker(KAWidget item, float holdTime, float duration)
	{
	}

	public void UpdateWidgetScaling(GameObject inObj)
	{
		if (Time.realtimeSinceStartup - mBlinkTime > _BlinkInDuration && inObj != null)
		{
			mBlinkTime = Time.realtimeSinceStartup;
			TweenScale tweenScale = (mBlinking ? TweenScale.Begin(inObj, 1f, _MinBlinkScale) : TweenScale.Begin(inObj, 1f, _MaxBlinkScale));
			mBlinking = !mBlinking;
			tweenScale.method = UITweener.Method.Linear;
		}
	}
}
