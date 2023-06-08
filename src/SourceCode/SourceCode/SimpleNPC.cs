using System;
using UnityEngine;

public class SimpleNPC : ChCharacter
{
	public int _PlayAllCurIndex = -1;

	public string _IdleAnimName = "IdleNorm";

	public SimpleEngage _Camera;

	public string _MouthAnimName = "";

	public string[] _FidgetAnimNames = new string[2] { "IdleFidget01", "IdleFidget02" };

	public string[] _DanceAnimNames = new string[1] { "Dance01" };

	[NonSerialized]
	public bool _WaitForDisplayMessage;

	private GameObject mMessageObject;

	private string mDanceAnim = "";

	private AudioClip mOverrideClip;

	private ObProximityAnimate mProximityAnimate;

	public void OnDance(bool t)
	{
		if (t)
		{
			mDanceAnim = _DanceAnimNames[UnityEngine.Random.Range(0, _DanceAnimNames.Length)];
			PlayAnim(mDanceAnim, -1, 1f, 1);
		}
		else
		{
			StopAnimAtLoopEnd(mDanceAnim);
		}
	}

	public override string GetIdleAnimationName()
	{
		return _IdleAnimName;
	}

	public override void DoFidget()
	{
		if (_FidgetAnimNames.Length != 0)
		{
			PlayAnim(_FidgetAnimNames[UnityEngine.Random.Range(0, _FidgetAnimNames.Length)], 0, 1f, 1);
			if (mProximityAnimate != null)
			{
				mProximityAnimate.enabled = false;
			}
		}
	}

	public override void ReturnToIdle(string aname)
	{
		base.ReturnToIdle(aname);
		if (mProximityAnimate != null)
		{
			mProximityAnimate.enabled = true;
		}
	}

	public virtual string GetMouthAnimationName()
	{
		return _MouthAnimName;
	}

	public void PlayMouthAnim()
	{
		if (GetMouthAnimationName().Length > 0)
		{
			PlayAnim(GetMouthAnimationName(), -1, 1f, 1);
		}
	}

	public void PlayIdleAnim()
	{
		if (GetIdleAnimationName().Length > 0)
		{
			PlayAnim(GetIdleAnimationName(), -1, 1f, 1);
		}
	}

	public override bool IsAnimIdle(string aname, out bool lookatcam)
	{
		lookatcam = false;
		if (aname == _IdleAnimName)
		{
			return true;
		}
		return false;
	}

	public override void Start()
	{
		mProximityAnimate = GetComponent<ObProximityAnimate>();
		Initialize(null);
	}

	public void PlayVO(AudioClip clip)
	{
		if (mOverrideClip != null)
		{
			PrivatePlayVO(mOverrideClip);
			mOverrideClip = null;
		}
		else
		{
			PrivatePlayVO(clip);
		}
	}

	private void PrivatePlayVO(AudioClip clip)
	{
		SnChannel.Play(clip, "VO_Pool", inForce: true, base.gameObject);
		DoAction(base.transform, Character_Action.userAction1);
		PlayMouthAnim();
	}

	public override void SetState(Character_State newstate)
	{
		base.SetState(newstate);
		switch (newstate)
		{
		case Character_State.action:
			if (mProximityAnimate != null)
			{
				mProximityAnimate.enabled = false;
			}
			break;
		case Character_State.idle:
			if (mProximityAnimate != null)
			{
				mProximityAnimate.enabled = true;
			}
			break;
		}
	}

	public void DoFirstTimeEngageWithOverride(AudioClip inClip, GameObject msgObject)
	{
		mMessageObject = msgObject;
		mOverrideClip = inClip;
		_PlayAllCurIndex = -1;
		GetComponent<ObClickable>().OnActivate();
	}

	public void DoFirstTimeEngage(GameObject msgObject)
	{
		mMessageObject = msgObject;
		_PlayAllCurIndex = 0;
		GetComponent<ObClickable>().OnActivate();
	}

	public void OnSnEvent(SnEvent sndEvent)
	{
		if (sndEvent.mType == SnEventType.STOP)
		{
			if ((bool)mMessageObject)
			{
				mMessageObject.SendMessage("NPCEngageEnded", this);
			}
		}
		else
		{
			if (sndEvent.mType != SnEventType.END)
			{
				return;
			}
			if (_PlayAllCurIndex >= 0)
			{
				_PlayAllCurIndex++;
				if (_PlayAllCurIndex < _Camera._VOs.Length)
				{
					PlayVO(_Camera._VOs[_PlayAllCurIndex]);
					return;
				}
				_Camera._CurIdx = 0;
				_PlayAllCurIndex = -1;
			}
			if (_Camera != null)
			{
				if (!_WaitForDisplayMessage)
				{
					_Camera.EndVO();
				}
				else
				{
					PlayIdleAnim();
					base.gameObject.SendMessage("DisplayMessageText", null, SendMessageOptions.DontRequireReceiver);
				}
			}
			if ((bool)mMessageObject && !_WaitForDisplayMessage)
			{
				mMessageObject.SendMessage("NPCEngageEnded", this);
			}
		}
	}
}
