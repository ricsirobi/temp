using System.Collections;
using UnityEngine;

public class UiUWSwimAirMeter : KAUI
{
	public float _MeterFlashThreshold = 20f;

	public CompactUI _HealthMeter;

	public AudioClip _LowAirAlarmSFX;

	private KAWidget mMeterBarProgress;

	private KAWidget mMeterBarBkg;

	private AvAvatarController mAvController;

	private Transform mHealthMeterParent;

	private bool mIsAlarmPlaying;

	protected override void Start()
	{
		base.Start();
		mMeterBarProgress = FindItem("MeterBarAir");
		mMeterBarBkg = FindItem("MeterBarAirBkg");
		if (AvAvatar.pObject != null)
		{
			mAvController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		}
	}

	public void SetMeter(float val)
	{
		val = Mathf.Clamp(val, 0f, mAvController._Stats._MaxAir);
		float num = mAvController._Stats._MaxAir;
		float num2 = val;
		if (num == 0f)
		{
			num = 1f;
		}
		float progressLevel = num2 / num;
		mMeterBarProgress.SetProgressLevel(progressLevel);
		mAvController._Stats._CurrentAir = val;
		if (mAvController._Stats._CurrentAir >= _MeterFlashThreshold)
		{
			if (mMeterBarBkg.GetCurrentAnim() != "Normal")
			{
				mMeterBarBkg.PlayAnim("Normal");
			}
			PlayAlarm(inPlay: false);
		}
		else if (mMeterBarBkg.GetCurrentAnim() != "FlashRed")
		{
			mMeterBarBkg.PlayAnim("FlashRed");
			PlayAlarm(inPlay: true);
		}
	}

	public void UpdateMeter(float val)
	{
		SetMeter(mAvController._Stats._CurrentAir + val);
	}

	public void AttachToToolbar(bool attach)
	{
		if (attach)
		{
			if (AvAvatar.pToolbar != null)
			{
				base.transform.parent = AvAvatar.pToolbar.transform;
			}
			if (_HealthMeter != null && mHealthMeterParent != null)
			{
				_HealthMeter.transform.parent = mHealthMeterParent;
			}
		}
		else
		{
			base.transform.parent = null;
			if (_HealthMeter != null && _HealthMeter.transform.parent != base.transform)
			{
				StartCoroutine("EnableHealthMeter");
			}
		}
		SetHealthMeterInteractive(attach);
	}

	private IEnumerator EnableHealthMeter()
	{
		yield return new WaitForEndOfFrame();
		mHealthMeterParent = _HealthMeter.transform.parent;
		_HealthMeter.transform.parent = base.transform;
	}

	private void SetHealthMeterInteractive(bool interactive)
	{
		CompactUI component = _HealthMeter.GetComponent<CompactUI>();
		if (component != null && component._ChildList != null && component._ChildList.Length != 0)
		{
			for (int i = 0; i < component._ChildList.Length; i++)
			{
				component._ChildList[i].SetInteractive(interactive);
			}
		}
	}

	public void PlayAlarm(bool inPlay)
	{
		if (mIsAlarmPlaying != inPlay)
		{
			SnChannel snChannel = null;
			if (inPlay)
			{
				snChannel = SnChannel.Play(_LowAirAlarmSFX, "SFX_Pool", inForce: true);
			}
			else
			{
				snChannel = SnChannel.AcquireChannel("SFX_Pool", inForce: true);
				SnChannel.StopPool("SFX_Pool");
			}
			if (snChannel != null)
			{
				snChannel.pLoop = inPlay;
			}
			mIsAlarmPlaying = inPlay;
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (!inVisible)
		{
			PlayAlarm(inPlay: false);
		}
	}
}
