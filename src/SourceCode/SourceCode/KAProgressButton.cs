using UnityEngine;

public class KAProgressButton : KAButton
{
	[SerializeField]
	private UISprite _FillBackground;

	public float _Time = 10f;

	public float _FlashAtTime = 3f;

	public float _FlashSpeed = 0.25f;

	private Color _FlashColor = Color.red;

	private float mElapsedTime;

	private Color mDefaultColor = Color.white;

	private bool mPause;

	private void Start()
	{
		base.enabled = false;
		if (_FillBackground != null)
		{
			mDefaultColor = _FillBackground.color;
		}
		ShowFillBackground(isVisible: false);
	}

	protected override void Update()
	{
		base.Update();
		if (mPause)
		{
			return;
		}
		mElapsedTime += Time.deltaTime;
		float num = 1f - mElapsedTime / _Time;
		if (num >= 0f)
		{
			if (_FillBackground != null)
			{
				_FillBackground.fillAmount = num;
			}
		}
		else
		{
			base.enabled = false;
			ShowFillBackground(isVisible: false);
		}
	}

	public void SetProgress(float inProgress)
	{
		if (_FillBackground != null)
		{
			_FillBackground.fillAmount = inProgress;
		}
	}

	public void AddProgressTime(float inAddTime)
	{
		mElapsedTime -= inAddTime;
	}

	public void StartAnimation(float inTime, float inProgress = 0f)
	{
		_Time = inTime;
		SetProgress(inProgress);
		base.enabled = true;
		mElapsedTime = 0f;
		ShowFillBackground(isVisible: true);
	}

	public void StopAnimation()
	{
		_Time = 0f;
		SetProgress(0f);
		base.enabled = false;
		mElapsedTime = 0f;
		ShowFillBackground(isVisible: false);
	}

	public void PauseAnimation(bool pause)
	{
		mPause = pause;
	}

	public override void SetVisibility(bool isVisible)
	{
		base.SetVisibility(isVisible);
		ShowFillBackground(base.enabled);
	}

	private void ShowFillBackground(bool isVisible)
	{
		if (_FillBackground != null)
		{
			_FillBackground.enabled = isVisible;
			if (_FillBackground.enabled)
			{
				_FillBackground.color = mDefaultColor;
				CancelInvoke("Flash");
				InvokeRepeating("Flash", _Time - _FlashAtTime, _FlashSpeed);
			}
			else
			{
				CancelInvoke("Flash");
			}
		}
	}

	private void Flash()
	{
		if (_FillBackground != null)
		{
			_FillBackground.color = ((_FillBackground.color == mDefaultColor) ? _FlashColor : mDefaultColor);
		}
	}
}
