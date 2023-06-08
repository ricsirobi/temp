using System;
using System.Collections;
using UnityEngine;

public class CryptexEndPiece : MonoBehaviour
{
	public GameObject _EndPiece;

	[Tooltip("The maximum rotation the end piece is allowed to spin based off of the number of currently correct dials.")]
	public float[] _RotationsAllowedByPercentageSolved;

	[Tooltip("The amount that the end piece moves when dragged.")]
	public float _DragRotationSpeed = 1f;

	[Tooltip("The speed at which the end piece returns back to it's initial position after rotating.")]
	public float _ResetRotationSpeed = 0.2f;

	private float mCurrentRotation;

	private float mCurrentMaxRotationAllowed;

	private int mLockedInDials;

	private IEnumerator mEndPieceMoveCoroutine;

	private bool mIsActive = true;

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

	private void OnMouseDown()
	{
		if (mIsActive)
		{
			TouchManager.OnDragEvent = (OnDrag)Delegate.Combine(TouchManager.OnDragEvent, new OnDrag(OnDrag));
		}
	}

	private void OnMouseUp()
	{
		TouchManager.OnDragEvent = (OnDrag)Delegate.Remove(TouchManager.OnDragEvent, new OnDrag(OnDrag));
		if (mCurrentRotation > 0f)
		{
			StartEndPieceMove();
		}
	}

	private void OnMouseEnter()
	{
		if (mIsActive && mLockedInDials > 0)
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

	private bool OnDrag(Vector2 inNewPosition, Vector2 inOldPosition, int inFingerI)
	{
		if (mEndPieceMoveCoroutine != null)
		{
			StopCoroutine(mEndPieceMoveCoroutine);
		}
		if (mLockedInDials == 0)
		{
			mCurrentMaxRotationAllowed = 0f;
		}
		else
		{
			mCurrentMaxRotationAllowed = _RotationsAllowedByPercentageSolved[mLockedInDials - 1];
		}
		float num = inNewPosition.y - inOldPosition.y;
		if (inNewPosition.y > inOldPosition.y)
		{
			float num2 = _DragRotationSpeed * num;
			if (mCurrentRotation < mCurrentMaxRotationAllowed)
			{
				mCurrentRotation += num2;
				_EndPiece.transform.localEulerAngles = new Vector3(mCurrentRotation, 0f, 0f);
			}
		}
		return true;
	}

	public void UpdateLockedDials(int lockedDials, bool puzzleSolved = false)
	{
		if (puzzleSolved)
		{
			StartEndPieceMove(puzzleSolved);
		}
		else if (lockedDials < mLockedInDials)
		{
			mLockedInDials = lockedDials;
			StartEndPieceMove();
		}
		else
		{
			mLockedInDials = lockedDials;
		}
	}

	private void StartEndPieceMove(bool puzzleSolved = false)
	{
		if (mEndPieceMoveCoroutine != null)
		{
			StopCoroutine(mEndPieceMoveCoroutine);
		}
		mEndPieceMoveCoroutine = RunEndPieceMove(puzzleSolved);
		StartCoroutine(mEndPieceMoveCoroutine);
	}

	private IEnumerator RunEndPieceMove(bool puzzleSolved = false)
	{
		float targetRotation = 0f;
		if (puzzleSolved)
		{
			targetRotation = _RotationsAllowedByPercentageSolved[_RotationsAllowedByPercentageSolved.Length - 1];
			while (mCurrentRotation <= targetRotation)
			{
				float num = _ResetRotationSpeed * Time.deltaTime;
				mCurrentRotation += num;
				_EndPiece.transform.localEulerAngles = new Vector3(mCurrentRotation, 0f, 0f);
				yield return null;
			}
		}
		else
		{
			while (mCurrentRotation >= targetRotation)
			{
				float num2 = _ResetRotationSpeed * (Time.deltaTime * -1f);
				mCurrentRotation += num2;
				_EndPiece.transform.localEulerAngles = new Vector3(mCurrentRotation, 0f, 0f);
				yield return null;
			}
		}
		mCurrentRotation = targetRotation;
		_EndPiece.transform.localEulerAngles = new Vector3(mCurrentRotation, 0f, 0f);
		mEndPieceMoveCoroutine = null;
	}

	public void OnQuit()
	{
		TouchManager.OnDragEvent = (OnDrag)Delegate.Remove(TouchManager.OnDragEvent, new OnDrag(OnDrag));
	}
}
