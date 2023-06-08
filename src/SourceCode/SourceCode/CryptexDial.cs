using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CryptexDial : MonoBehaviour
{
	public delegate void OnDialMove(CryptexDial cryptexDial);

	public GameObject _CryptexDial;

	public AudioClip _DialTurnSound;

	public AudioClip _LockInSound;

	public AudioClip _LockInCorrectSound;

	public bool _StartWithRandomRotation = true;

	public float _RandomStartMinSpin = 2f;

	public float _RandomStartMaxSpin = 5f;

	public float _DragRotationSpeed = 1f;

	public float _FlickRotationSpeed = 0.1f;

	public float _FlickRotationModifier = 0.5f;

	[Tooltip("How fast should the dial move when locking in after being stopped within the tolerance range.")]
	public float _LockInRotationSpeed = 0.1f;

	public float _FloatDurationWhenLockedIn = 1.5f;

	public float _DurationBetweenFloatMovement = 1f;

	private ObFloat mFloat;

	private Vector3 mStartPosition = Vector3.zero;

	private IEnumerator mFloatCoroutine;

	private float mWrappedRotation;

	private float mLockInTolerance;

	private float mLockInPosition;

	private float mCurrentPositionToLock = -1f;

	private List<float> mLockInPositions = new List<float>();

	private bool mIsLockedIn;

	public static OnDialMove OnDialMoveEvent;

	private IEnumerator mLockInCoroutine;

	private IEnumerator mFlickRotateCoroutine;

	private KAWidget mKAWidget;

	private bool mIsActive = true;

	public float pLockInTolerance
	{
		set
		{
			mLockInTolerance = value;
		}
	}

	public float pLockInPosition
	{
		get
		{
			return mLockInPosition;
		}
		set
		{
			mLockInPosition = value;
		}
	}

	public List<float> pLockInPositions
	{
		get
		{
			return mLockInPositions;
		}
		set
		{
			mLockInPositions = value;
		}
	}

	public bool pIsLockedIn => mIsLockedIn;

	public KAWidget pKAWidget
	{
		get
		{
			return mKAWidget;
		}
		set
		{
			mKAWidget = value;
		}
	}

	public bool pIsActive
	{
		get
		{
			return mIsActive;
		}
		set
		{
			mIsActive = value;
		}
	}

	private void Start()
	{
		mFloat = _CryptexDial.gameObject.GetComponent<ObFloat>();
		mStartPosition = base.transform.position;
		if (_StartWithRandomRotation)
		{
			mFlickRotateCoroutine = Rotate(UnityEngine.Random.Range(_RandomStartMinSpin, _RandomStartMaxSpin), UnityEngine.Random.Range(0, 2));
			StartCoroutine(mFlickRotateCoroutine);
		}
	}

	private void OnMouseDown()
	{
		if (mIsActive)
		{
			TouchManager.OnDragEvent = (OnDrag)Delegate.Combine(TouchManager.OnDragEvent, new OnDrag(OnDrag));
			TouchManager.OnFlickEvent = (OnFlick)Delegate.Combine(TouchManager.OnFlickEvent, new OnFlick(OnFlick));
		}
	}

	private void OnMouseUp()
	{
		if (mIsActive)
		{
			TouchManager.OnDragEvent = (OnDrag)Delegate.Remove(TouchManager.OnDragEvent, new OnDrag(OnDrag));
			mCurrentPositionToLock = IsPositionWithinAnyLockRange();
			if (mCurrentPositionToLock >= 0f)
			{
				StartDialLockIn();
			}
			if (OnDialMoveEvent != null)
			{
				OnDialMoveEvent(this);
			}
			StartCoroutine(UnregisterFlick());
		}
	}

	private void OnMouseEnter()
	{
		if (mIsActive)
		{
			UICursorManager.SetCursor("Activate", showHideSystemCursor: true);
		}
	}

	private void OnMouseExit()
	{
		if (mIsActive)
		{
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		}
	}

	private float IsPositionWithinAnyLockRange()
	{
		for (int i = 0; i < mLockInPositions.Count; i++)
		{
			if (mLockInPositions[i] == 0f && ((mWrappedRotation >= UtUtilities.WrapNumber(mLockInPositions[i] - mLockInTolerance, 360f) && UtUtilities.WrapNumber(mWrappedRotation + mLockInTolerance, 360f) <= UtUtilities.WrapNumber(mLockInPositions[i] + mLockInTolerance, 360f)) || (mWrappedRotation <= UtUtilities.WrapNumber(mLockInPositions[i] + mLockInTolerance, 360f) && UtUtilities.WrapNumber(mWrappedRotation - mLockInTolerance, 360f) >= UtUtilities.WrapNumber(mLockInPositions[i] - mLockInTolerance, 360f))))
			{
				return mLockInPositions[i];
			}
			if (mWrappedRotation >= UtUtilities.WrapNumber(mLockInPositions[i] - mLockInTolerance, 360f) && mWrappedRotation <= UtUtilities.WrapNumber(mLockInPositions[i] + mLockInTolerance, 360f))
			{
				return mLockInPositions[i];
			}
		}
		return -1f;
	}

	private bool IsDialPositionCorrectAnswer()
	{
		if (mLockInPosition == 0f && ((mWrappedRotation >= UtUtilities.WrapNumber(mLockInPosition - mLockInTolerance, 360f) && UtUtilities.WrapNumber(mWrappedRotation + mLockInTolerance, 360f) <= UtUtilities.WrapNumber(mLockInPosition + mLockInTolerance, 360f)) || (mWrappedRotation <= UtUtilities.WrapNumber(mLockInPosition + mLockInTolerance, 360f) && UtUtilities.WrapNumber(mWrappedRotation - mLockInTolerance, 360f) >= UtUtilities.WrapNumber(mLockInPosition - mLockInTolerance, 360f))))
		{
			return true;
		}
		if (mWrappedRotation >= UtUtilities.WrapNumber(mLockInPosition - mLockInTolerance, 360f) && mWrappedRotation <= UtUtilities.WrapNumber(mLockInPosition + mLockInTolerance, 360f))
		{
			return true;
		}
		return false;
	}

	private bool OnDrag(Vector2 inNewPosition, Vector2 inOldPosition, int inFingerI)
	{
		mIsLockedIn = false;
		if (mFloat.enabled)
		{
			ActivateFloat(activate: false);
		}
		if (mFlickRotateCoroutine != null)
		{
			StopCoroutine(mFlickRotateCoroutine);
			mFlickRotateCoroutine = null;
		}
		if (mLockInCoroutine != null)
		{
			StopCoroutine(mLockInCoroutine);
			mLockInCoroutine = null;
		}
		float num = inNewPosition.y - inOldPosition.y;
		if (num != 0f)
		{
			float amountToAdd = _DragRotationSpeed * num;
			WrapRotation(amountToAdd);
			_CryptexDial.transform.localEulerAngles = new Vector3(mWrappedRotation, 0f, 0f);
		}
		if (OnDialMoveEvent != null)
		{
			OnDialMoveEvent(this);
		}
		return true;
	}

	private void OnFlick(int inFingerID, Vector2 inVecPosition, Vector2 inDeltaPos, int inDirection, int inMagnitude)
	{
		if (inMagnitude == 0)
		{
			return;
		}
		mIsLockedIn = false;
		if (OnDialMoveEvent != null)
		{
			OnDialMoveEvent(this);
		}
		if (mFloat.enabled)
		{
			ActivateFloat(activate: false);
		}
		if (inDirection == 1 || inDirection == 2 || inDirection == 8 || inDirection == 4 || inDirection == 5 || inDirection == 6)
		{
			if (mFlickRotateCoroutine != null)
			{
				StopCoroutine(mFlickRotateCoroutine);
			}
			mFlickRotateCoroutine = Rotate(inMagnitude, inDirection);
			StartCoroutine(mFlickRotateCoroutine);
		}
		TouchManager.OnFlickEvent = (OnFlick)Delegate.Remove(TouchManager.OnFlickEvent, new OnFlick(OnFlick));
	}

	private IEnumerator Rotate(float magnitude, int direction)
	{
		if (mLockInCoroutine != null)
		{
			StopCoroutine(mLockInCoroutine);
			mLockInCoroutine = null;
		}
		float flickDuration = _FlickRotationModifier * magnitude;
		float lerpPercentage = 0f;
		float currentTimer = 0f;
		while (lerpPercentage < 1f)
		{
			lerpPercentage = currentTimer / flickDuration;
			float num = Mathf.Lerp(_FlickRotationSpeed, 0f, lerpPercentage);
			if (direction == 1 || direction == 2 || direction == 8)
			{
				num *= -1f;
			}
			WrapRotation(num);
			_CryptexDial.transform.localEulerAngles = new Vector3(mWrappedRotation, 0f, 0f);
			currentTimer += Time.deltaTime;
			yield return null;
		}
		mFlickRotateCoroutine = null;
		mCurrentPositionToLock = IsPositionWithinAnyLockRange();
		if (mCurrentPositionToLock >= 0f)
		{
			StartDialLockIn();
		}
		if (OnDialMoveEvent != null)
		{
			OnDialMoveEvent(this);
		}
	}

	private IEnumerator RunDialLockIn()
	{
		if (UtUtilities.WrapNumber(mWrappedRotation - mLockInTolerance, 360f) > UtUtilities.WrapNumber(mCurrentPositionToLock - mLockInTolerance, 360f))
		{
			while (UtUtilities.WrapNumber(mWrappedRotation - mLockInTolerance, 360f) >= UtUtilities.WrapNumber(mCurrentPositionToLock - mLockInTolerance, 360f) + 0.1f)
			{
				float lockInRotationSpeed = _LockInRotationSpeed;
				lockInRotationSpeed *= Time.deltaTime * -1f;
				WrapRotation(lockInRotationSpeed);
				_CryptexDial.transform.localEulerAngles = new Vector3(mWrappedRotation, 0f, 0f);
				yield return null;
			}
		}
		else
		{
			while (UtUtilities.WrapNumber(mWrappedRotation - mLockInTolerance, 360f) <= UtUtilities.WrapNumber(mCurrentPositionToLock - mLockInTolerance, 360f) + 0.1f)
			{
				float amountToAdd = _LockInRotationSpeed * Time.deltaTime;
				WrapRotation(amountToAdd);
				_CryptexDial.transform.localEulerAngles = new Vector3(mWrappedRotation, 0f, 0f);
				yield return null;
			}
		}
		mIsLockedIn = IsDialPositionCorrectAnswer();
		if (mIsLockedIn)
		{
			ActivateFloat(activate: true);
			if (_LockInCorrectSound != null)
			{
				SnChannel.Play(_LockInCorrectSound, "DialPool", inForce: true);
			}
		}
		else if (_LockInSound != null)
		{
			SnChannel.Play(_LockInSound, "DialPool", inForce: true);
		}
		if (OnDialMoveEvent != null)
		{
			OnDialMoveEvent(this);
		}
		mLockInCoroutine = null;
	}

	private void StartDialLockIn()
	{
		if (mLockInCoroutine != null)
		{
			mLockInCoroutine = null;
		}
		mLockInCoroutine = RunDialLockIn();
		StartCoroutine(mLockInCoroutine);
	}

	public void ActivateFloat(bool activate)
	{
		if (activate)
		{
			if (mFloatCoroutine == null)
			{
				mFloatCoroutine = RunFloat();
				StartCoroutine(mFloatCoroutine);
			}
			return;
		}
		mFloat.enabled = false;
		base.transform.position = mStartPosition;
		if (mFloatCoroutine != null)
		{
			StopCoroutine(mFloatCoroutine);
			mFloatCoroutine = null;
		}
	}

	private IEnumerator RunFloat()
	{
		mFloat.enabled = true;
		yield return new WaitForSeconds(_FloatDurationWhenLockedIn);
		mFloat.enabled = false;
		yield return new WaitForSeconds(_DurationBetweenFloatMovement);
		mFloatCoroutine = null;
		ActivateFloat(mIsLockedIn);
	}

	private IEnumerator UnregisterFlick()
	{
		yield return new WaitForEndOfFrame();
		TouchManager.OnFlickEvent = (OnFlick)Delegate.Remove(TouchManager.OnFlickEvent, new OnFlick(OnFlick));
	}

	private void WrapRotation(float amountToAdd)
	{
		mWrappedRotation += amountToAdd;
		mWrappedRotation = UtUtilities.WrapNumber(mWrappedRotation, 360f);
		if ((mWrappedRotation % 45f < 1f || mWrappedRotation % 45f > 44f) && _DialTurnSound != null)
		{
			SnChannel.Play(_DialTurnSound, "DialPool", inForce: true);
		}
	}

	public void OnQuit()
	{
		TouchManager.OnDragEvent = (OnDrag)Delegate.Remove(TouchManager.OnDragEvent, new OnDrag(OnDrag));
		TouchManager.OnFlickEvent = (OnFlick)Delegate.Remove(TouchManager.OnFlickEvent, new OnFlick(OnFlick));
	}
}
