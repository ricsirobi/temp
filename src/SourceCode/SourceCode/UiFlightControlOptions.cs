using System;
using UnityEngine;

public class UiFlightControlOptions : KAUI
{
	public GameObject _MessageObj;

	public string _CloseMsg;

	[NonSerialized]
	public string _BundlePath = string.Empty;

	private KAToggleButton mBtnTiltSteer;

	private KAToggleButton mBtnTouchSteer;

	private KAWidget mCloseBtn;

	protected override void Start()
	{
		base.Start();
		mBtnTiltSteer = (KAToggleButton)FindItem("BtnFlightControlTiltCheck");
		mBtnTouchSteer = (KAToggleButton)FindItem("BtnFlightControlTouchCheck");
		mCloseBtn = FindItem("BtnBack");
		if (UiOptions.pIsTiltSteer)
		{
			mBtnTiltSteer.SetChecked(isChecked: true);
		}
		else
		{
			mBtnTouchSteer.SetChecked(isChecked: true);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnTiltSteer)
		{
			UiOptions.pIsTiltSteer = true;
		}
		else if (inWidget == mBtnTouchSteer)
		{
			UiOptions.pIsTiltSteer = false;
		}
		else if (inWidget == mCloseBtn)
		{
			_MessageObj.SendMessage(_CloseMsg, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void Destroy()
	{
		UnityEngine.Object.Destroy(base.gameObject);
		RsResourceManager.Unload(_BundlePath);
	}
}
