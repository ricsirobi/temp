using UnityEngine;

public class SplineControlModifier : MonoBehaviour
{
	public GameObject _Object;

	public SplineControlData _Slowdown;

	public SplineControlData _Halt;

	public SplineControlData _Accelerate;

	private SplineControl mHitSplineControl;

	private SplineControlStates mState;

	private float mSlowdownTimer;

	private float mPrevSpeed;

	private float mHaltTimer;

	private float mAccelerateTimer;

	private float mRotationTweenTimer;

	private float mRotationTweenDuration;

	private float mAvoidReCollisionTimer;

	private Quaternion mPrevRotation = Quaternion.identity;

	private Quaternion mDesiredRotation = Quaternion.identity;

	public void OnTriggerEnter(Collider collider)
	{
		SplineControl component = collider.gameObject.GetComponent<SplineControl>();
		if (component != null && (_Object == null || collider.gameObject == _Object))
		{
			if (component == mHitSplineControl)
			{
				UtDebug.LogError("Error !!! Same trigger being invoked again");
				return;
			}
			mHitSplineControl = component;
			Slowdown();
		}
	}

	private void Slowdown()
	{
		if (!(mHitSplineControl != null))
		{
			return;
		}
		if (_Slowdown._Duration > 0f)
		{
			mState = SplineControlStates.SLOW;
			mPrevRotation = mHitSplineControl.transform.rotation;
			if (_Slowdown._LookAtMarker != null)
			{
				mDesiredRotation = _Slowdown._LookAtMarker.transform.rotation;
			}
			mPrevSpeed = mHitSplineControl.Speed;
			mSlowdownTimer = _Slowdown._Duration;
			mRotationTweenDuration = (mRotationTweenTimer = Mathf.Min(_Slowdown._LookAtTweenDuration, _Slowdown._Duration));
			if (_Slowdown._Sound._AudioClip != null)
			{
				_Slowdown._Sound.Play();
			}
			if (_Slowdown._MessageObject != null)
			{
				_Slowdown._MessageObject.SendMessage(_Slowdown._MessageFunctionName, this, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (!_Slowdown._PauseStep)
		{
			Halt();
		}
	}

	private void Halt()
	{
		if (!(mHitSplineControl != null))
		{
			return;
		}
		if (_Halt._Duration > 0f)
		{
			mState = SplineControlStates.HALT;
			mPrevRotation = mHitSplineControl.transform.rotation;
			if (_Halt._LookAtMarker != null)
			{
				mDesiredRotation = _Halt._LookAtMarker.transform.rotation;
			}
			mHitSplineControl.Speed = 0f;
			mHaltTimer = _Halt._Duration;
			mRotationTweenDuration = (mRotationTweenTimer = Mathf.Min(_Halt._LookAtTweenDuration, _Halt._Duration));
			if (_Halt._Sound._AudioClip != null)
			{
				_Halt._Sound.Play();
			}
			if (_Halt._MessageObject != null)
			{
				_Halt._MessageObject.SendMessage(_Halt._MessageFunctionName, this, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (!_Halt._PauseStep)
		{
			Accelerate();
		}
	}

	private void Accelerate()
	{
		if (!(mHitSplineControl != null))
		{
			return;
		}
		if (_Accelerate._Duration > 0f)
		{
			mState = SplineControlStates.ACCELERATE;
			mPrevRotation = mHitSplineControl.transform.rotation;
			if (_Accelerate._LookAtMarker != null)
			{
				mDesiredRotation = _Accelerate._LookAtMarker.transform.rotation;
			}
			mPrevSpeed = mHitSplineControl.Speed;
			mAccelerateTimer = _Accelerate._Duration;
			mRotationTweenDuration = (mRotationTweenTimer = Mathf.Min(_Accelerate._LookAtTweenDuration, _Accelerate._Duration));
			if (_Accelerate._Sound._AudioClip != null)
			{
				_Accelerate._Sound.Play();
			}
			if (_Accelerate._MessageObject != null)
			{
				_Accelerate._MessageObject.SendMessage(_Accelerate._MessageFunctionName, this, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			mAvoidReCollisionTimer = 2f;
			mState = SplineControlStates.NONE;
		}
	}

	private void OnAccelerateDone()
	{
		mAvoidReCollisionTimer = 2f;
		mState = SplineControlStates.NONE;
		mHitSplineControl.Speed = _Accelerate._Speed;
	}

	public void ResumeStep(bool isForceResume)
	{
		switch (mState)
		{
		case SplineControlStates.SLOW:
			if (!isForceResume && _Slowdown._PauseStep && mSlowdownTimer > 0f)
			{
				_Slowdown._PauseStep = false;
			}
			else
			{
				Halt();
			}
			break;
		case SplineControlStates.HALT:
			if (!isForceResume && _Halt._PauseStep && mHaltTimer > 0f)
			{
				_Halt._PauseStep = false;
			}
			else
			{
				Accelerate();
			}
			break;
		case SplineControlStates.ACCELERATE:
			if (!isForceResume && _Accelerate._PauseStep && mAccelerateTimer > 0f)
			{
				_Accelerate._PauseStep = false;
			}
			else
			{
				OnAccelerateDone();
			}
			break;
		}
	}

	public void Update()
	{
		if (!(mHitSplineControl != null))
		{
			return;
		}
		if (mState != 0 && mRotationTweenTimer > 0f)
		{
			mRotationTweenTimer -= Time.deltaTime;
			if (mRotationTweenTimer < 0f)
			{
				mRotationTweenTimer = 0f;
			}
			mHitSplineControl.transform.rotation = Quaternion.Slerp(mPrevRotation, mDesiredRotation, (mRotationTweenDuration - mRotationTweenTimer) / mRotationTweenDuration);
		}
		switch (mState)
		{
		case SplineControlStates.SLOW:
			if (mSlowdownTimer > 0f)
			{
				mSlowdownTimer -= Time.deltaTime;
				if (mSlowdownTimer < 0f)
				{
					mSlowdownTimer = 0f;
				}
				mHitSplineControl.Speed = Mathf.Lerp(mPrevSpeed, _Slowdown._Speed, (_Slowdown._Duration - mSlowdownTimer) / _Slowdown._Duration);
				if (mSlowdownTimer <= 0f && !_Slowdown._PauseStep)
				{
					Halt();
				}
			}
			break;
		case SplineControlStates.HALT:
			if (mHaltTimer > 0f)
			{
				mHaltTimer -= Time.deltaTime;
				mRotationTweenTimer -= Time.deltaTime;
				if (mHaltTimer < 0f)
				{
					mHaltTimer = 0f;
				}
				if (mHaltTimer <= 0f && !_Halt._PauseStep)
				{
					Accelerate();
				}
			}
			break;
		case SplineControlStates.ACCELERATE:
			if (!(mAccelerateTimer > 0f))
			{
				break;
			}
			mAccelerateTimer -= Time.deltaTime;
			mRotationTweenTimer -= Time.deltaTime;
			if (mAccelerateTimer < 0f)
			{
				mAccelerateTimer = 0f;
			}
			if (!_Accelerate._PauseStep)
			{
				mHitSplineControl.Speed = Mathf.Lerp(mPrevSpeed, _Accelerate._Speed, (_Accelerate._Duration - mAccelerateTimer) / _Accelerate._Duration);
				if (mAccelerateTimer <= 0f)
				{
					OnAccelerateDone();
				}
			}
			break;
		case SplineControlStates.NONE:
			if (mAvoidReCollisionTimer > 0f)
			{
				mAvoidReCollisionTimer -= Time.deltaTime;
				if (mAvoidReCollisionTimer <= 0f)
				{
					mHitSplineControl = null;
				}
			}
			break;
		}
	}
}
