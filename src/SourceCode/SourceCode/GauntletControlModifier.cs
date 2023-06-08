using UnityEngine;

public class GauntletControlModifier : MonoBehaviour
{
	public float _MovementSpeed;

	public GauntletControlData _Slowdown;

	public GauntletControlData _Halt;

	public GauntletControlData _Accelerate;

	public bool _PauseStates;

	private GauntletController mHitGauntletController;

	private GauntletControlStates mState;

	private float mSlowdownTimer;

	private float mPrevSpeed;

	private float mHaltTimer;

	private float mAcclerateTimer;

	private float mRotationTweenTimer;

	private float mRotationTweenDuration;

	private float mAvoidReCollisionTimer;

	private Quaternion mPrevRotation = Quaternion.identity;

	private Quaternion mDesiredRotation = Quaternion.identity;

	public GauntletControlStates pState => mState;

	public void OnTriggerEnter(Collider collider)
	{
		GauntletController component = collider.gameObject.GetComponent<GauntletController>();
		if (component != null)
		{
			if (component == mHitGauntletController)
			{
				UtDebug.LogError("Error !!! Same trigger being invoked again");
				return;
			}
			mHitGauntletController = component;
			Slowdown();
		}
	}

	private void Slowdown()
	{
		if (!(mHitGauntletController != null))
		{
			return;
		}
		if (_Slowdown._Duration > 0f)
		{
			mState = GauntletControlStates.SLOW;
			mPrevRotation = mHitGauntletController.transform.rotation;
			if (_Slowdown._LookAtMarker != null)
			{
				mDesiredRotation = _Slowdown._LookAtMarker.transform.rotation;
			}
			mPrevSpeed = mHitGauntletController.Speed;
			mSlowdownTimer = _Slowdown._Duration;
			mRotationTweenDuration = (mRotationTweenTimer = Mathf.Min(_Slowdown._LookAtTweenDuration, _Slowdown._Duration));
			PlayAudioClip(_Slowdown._AudioClip, _Slowdown._AudioPoolName);
		}
		else if (!_Slowdown._PauseStep)
		{
			Halt();
		}
		if (_Slowdown._MessageObject != null)
		{
			_Slowdown._MessageObject.SendMessage(_Slowdown._MessageFunctionName, this, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void Halt()
	{
		if (!(mHitGauntletController != null))
		{
			return;
		}
		if (_Halt._Duration > 0f)
		{
			mState = GauntletControlStates.HALT;
			mPrevRotation = mHitGauntletController.transform.rotation;
			if (_Halt._LookAtMarker != null)
			{
				mDesiredRotation = _Halt._LookAtMarker.transform.rotation;
			}
			mHitGauntletController.Speed = 0f;
			mHaltTimer = _Halt._Duration;
			mRotationTweenDuration = (mRotationTweenTimer = Mathf.Min(_Halt._LookAtTweenDuration, _Halt._Duration));
			PlayAudioClip(_Halt._AudioClip, _Halt._AudioPoolName);
		}
		else if (!_Halt._PauseStep)
		{
			Accelerate();
		}
		if (_Halt._MessageObject != null)
		{
			_Halt._MessageObject.SendMessage(_Halt._MessageFunctionName, this, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void Accelerate()
	{
		if (!(mHitGauntletController != null))
		{
			return;
		}
		if (_Accelerate._Duration > 0f)
		{
			mState = GauntletControlStates.ACCELERATE;
			mAcclerateTimer = _Accelerate._Duration;
			mRotationTweenDuration = (mRotationTweenTimer = Mathf.Min(_Accelerate._LookAtTweenDuration, _Accelerate._Duration));
			mPrevRotation = mHitGauntletController.transform.rotation;
			if (_Accelerate._LookAtMarker != null)
			{
				mDesiredRotation = _Accelerate._LookAtMarker.transform.rotation;
			}
		}
		else if (!_Accelerate._PauseStep)
		{
			OnAccelerateDone();
		}
		PlayAudioClip(_Accelerate._AudioClip, _Accelerate._AudioPoolName);
		if (_Accelerate._MessageObject != null)
		{
			_Accelerate._MessageObject.SendMessage(_Accelerate._MessageFunctionName, this, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnAccelerateDone()
	{
		mAvoidReCollisionTimer = 2f;
		mState = GauntletControlStates.NONE;
		mHitGauntletController.Speed = _MovementSpeed;
	}

	public void ResumeStep(bool isForceResume)
	{
		switch (mState)
		{
		case GauntletControlStates.SLOW:
			if (!isForceResume && _Slowdown._PauseStep && mSlowdownTimer > 0f)
			{
				_Slowdown._PauseStep = false;
			}
			else
			{
				Halt();
			}
			break;
		case GauntletControlStates.HALT:
			if (!isForceResume && _Halt._PauseStep && mHaltTimer > 0f)
			{
				_Halt._PauseStep = false;
			}
			else
			{
				Accelerate();
			}
			break;
		case GauntletControlStates.ACCELERATE:
			if (!isForceResume && _Accelerate._PauseStep && mAcclerateTimer > 0f)
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

	private void PlayAudioClip(AudioClip inClip, string inPool)
	{
		if (inClip != null && inPool.Length > 0)
		{
			SnChannel.Play(inClip, inPool, inForce: true);
		}
	}

	public void Update()
	{
		if (!(mHitGauntletController != null) || _PauseStates)
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
			mHitGauntletController.transform.rotation = Quaternion.Slerp(mPrevRotation, mDesiredRotation, (mRotationTweenDuration - mRotationTweenTimer) / mRotationTweenDuration);
		}
		switch (mState)
		{
		case GauntletControlStates.SLOW:
			if (mSlowdownTimer > 0f)
			{
				mSlowdownTimer -= Time.deltaTime;
				if (mSlowdownTimer < 0f)
				{
					mSlowdownTimer = 0f;
				}
				mHitGauntletController.Speed = Mathf.Lerp(mPrevSpeed, 0f, (_Slowdown._Duration - mSlowdownTimer) / _Slowdown._Duration);
				if (mSlowdownTimer <= 0f && !_Slowdown._PauseStep)
				{
					Halt();
				}
			}
			break;
		case GauntletControlStates.HALT:
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
		case GauntletControlStates.ACCELERATE:
			if (!(mAcclerateTimer > 0f))
			{
				break;
			}
			mAcclerateTimer -= Time.deltaTime;
			mRotationTweenTimer -= Time.deltaTime;
			if (mAcclerateTimer < 0f)
			{
				mAcclerateTimer = 0f;
			}
			if (!_Accelerate._PauseStep)
			{
				mHitGauntletController.Speed = Mathf.Lerp(0f, _MovementSpeed, (_Accelerate._Duration - mAcclerateTimer) / _Accelerate._Duration);
				if (mAcclerateTimer <= 0f)
				{
					OnAccelerateDone();
				}
			}
			break;
		case GauntletControlStates.NONE:
			if (mAvoidReCollisionTimer > 0f)
			{
				mAvoidReCollisionTimer -= Time.deltaTime;
				if (mAvoidReCollisionTimer <= 0f)
				{
					mHitGauntletController = null;
				}
			}
			break;
		}
	}
}
