using UnityEngine;

public class UiCountDown : KAUI
{
	public delegate void CountdownDone();

	public AudioClip _CountdownSound;

	public AudioClip _GoSound;

	public string[] _CountDownWidgetOrder;

	private KAWidget mCountdown;

	private bool mStartCountdown;

	private int mCountdownIndex;

	private float mTimeDuration;

	private bool mIsCountDownPaused;

	public event CountdownDone OnCountdownDone;

	protected override void Start()
	{
		base.Start();
		mCountdown = FindItem("CountDownGroup");
		base.enabled = false;
	}

	protected override void Update()
	{
		base.Update();
		if (!mIsCountDownPaused && mStartCountdown)
		{
			mTimeDuration += Time.deltaTime;
			if (mTimeDuration > 1f)
			{
				mTimeDuration = 0f;
				ShowCountdown();
			}
		}
	}

	private void ShowCountdown()
	{
		if (_CountDownWidgetOrder == null || _CountDownWidgetOrder.Length == 0)
		{
			StartCountDown(inStart: false);
			Debug.LogError("COUNT DOWN ITEM ORDER NOT DEFIND!!!");
			return;
		}
		for (int i = 0; i < _CountDownWidgetOrder.Length; i++)
		{
			KAWidget kAWidget = mCountdown.FindChildItem(_CountDownWidgetOrder[i]);
			if (i == mCountdownIndex)
			{
				kAWidget.SetVisibility(inVisible: true);
				if (i != _CountDownWidgetOrder.Length - 1)
				{
					if ((bool)_CountdownSound)
					{
						SnChannel.Play(_CountdownSound);
					}
				}
				else if ((bool)_GoSound)
				{
					SnChannel.Play(_GoSound);
				}
			}
			else
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
		mCountdownIndex++;
		_ = mCountdownIndex;
		_ = 1;
		if (this.OnCountdownDone != null && mCountdownIndex > _CountDownWidgetOrder.Length)
		{
			this.OnCountdownDone();
			StartCountDown(inStart: false);
		}
	}

	public bool IsCountDownOver()
	{
		if (mCountdownIndex > _CountDownWidgetOrder.Length)
		{
			return true;
		}
		return false;
	}

	public void ApplyPause(bool pause)
	{
		mIsCountDownPaused = pause;
	}

	public void StartCountDown(bool inStart)
	{
		mCountdown = FindItem("CountDownGroup");
		mCountdown.SetVisibility(inStart);
		if (_CountDownWidgetOrder != null && _CountDownWidgetOrder.Length != 0)
		{
			for (int i = 0; i < _CountDownWidgetOrder.Length; i++)
			{
				mCountdown.FindChildItem(_CountDownWidgetOrder[i]).SetVisibility(inVisible: false);
			}
		}
		mTimeDuration = 0f;
		mCountdownIndex = 0;
		mStartCountdown = inStart;
		base.enabled = inStart;
		mIsCountDownPaused = false;
	}
}
