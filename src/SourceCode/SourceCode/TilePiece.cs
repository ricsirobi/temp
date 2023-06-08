using System;
using System.Collections;
using UnityEngine;

public class TilePiece : MonoBehaviour
{
	[Flags]
	public enum State
	{
		NONE = 0,
		IDLE = 1,
		SLEEP = 2,
		SELECTED = 4,
		MOVE_FALL = 8,
		FROZEN = 0x10,
		DEAD = 0x20
	}

	private int mRow;

	private int mColumn;

	private int mID;

	[SerializeField]
	protected TileType _Type = TileType.NORMAL;

	[SerializeField]
	protected int _Weight = 10;

	protected Vector3 mFromScale = Vector3.one;

	protected Vector3 mToScale = Vector3.one;

	protected Vector3 mOrgScale = Vector3.one;

	protected float mFactor;

	protected TilePiece mMaskTile;

	protected State mState;

	protected Vector3 mMoveDir = Vector3.zero;

	protected Vector3 mTargetPos = Vector3.zero;

	protected float mVisiblityLimit;

	protected float mCurSpeed;

	protected float mMoveSpeed = 10f;

	protected float mMinSpeed = 10f;

	protected float mSpeedDamp = 10f;

	protected float mAnimateSpeed = 10f;

	protected Plane mPlane = new Plane(Vector3.one, Vector3.zero);

	protected OnTileBehaviourCompletedDeleagte mOnBehaviourCompletedEvent;

	public int pRow
	{
		get
		{
			return mRow;
		}
		set
		{
			mRow = value;
		}
	}

	public int pColumn
	{
		get
		{
			return mColumn;
		}
		set
		{
			mColumn = value;
		}
	}

	public int pID
	{
		get
		{
			return mID;
		}
		set
		{
			mID = value;
		}
	}

	public TileType pType => _Type;

	public virtual int pWeight
	{
		get
		{
			return _Weight;
		}
		set
		{
		}
	}

	public virtual int[] pExplodeRange { get; set; }

	public virtual GameObject pEffect { get; set; }

	public TilePiece pMaskTile => mMaskTile;

	public State pState => mState;

	public void SetGridParams(int inRow, int inCol, TileType inType, int inID)
	{
		if (inRow == -1 || inCol == -1)
		{
			Debug.LogError("@@@@@ Clear Grid " + inRow + " " + inCol);
		}
		mRow = inRow;
		mColumn = inCol;
		mID = inID;
		_Type = inType;
	}

	public virtual void SetRowCol(int inRow, int inCol)
	{
		if (inRow == -1 || inCol == -1)
		{
			Debug.LogError("@@@@@ Clear Grid " + inRow + " " + inCol);
		}
		base.name = "r: " + inRow + ", c: " + inCol;
		mRow = inRow;
		mColumn = inCol;
	}

	public void CopyFrom(TilePiece inSource)
	{
		pRow = inSource.pRow;
		pColumn = inSource.pColumn;
	}

	public virtual void SetMaskTile(TilePiece inMaskTile)
	{
		mMaskTile = inMaskTile;
	}

	public void SetVisibilityMovement(float inVisibleLimit, float inMoveSpeed, float inMinSpeed, float inSpeedDamp)
	{
		mVisiblityLimit = inVisibleLimit;
		mMoveSpeed = inMoveSpeed;
		mMinSpeed = inMinSpeed;
		mSpeedDamp = inSpeedDamp;
	}

	public void MoveTo(Vector3 inTargetPos)
	{
		mMoveDir = inTargetPos - base.transform.localPosition;
		mMoveDir.Normalize();
		mPlane.SetNormalAndPosition(mMoveDir, inTargetPos);
		mCurSpeed = mMoveSpeed;
		mTargetPos = inTargetPos;
		SetState(State.MOVE_FALL);
	}

	public void MoveToAfter(Vector3 inTargetPos, float inDelayTime)
	{
		mMoveDir = inTargetPos - base.transform.localPosition;
		mMoveDir.Normalize();
		mCurSpeed = mMoveSpeed;
		mPlane.SetNormalAndPosition(mMoveDir, inTargetPos);
		SetState(State.SLEEP);
		mTargetPos = inTargetPos;
		StartCoroutine(StartMoveIn(inDelayTime));
	}

	public IEnumerator StartMoveIn(float inDelayTime)
	{
		yield return new WaitForSeconds(inDelayTime);
		SetState(State.MOVE_FALL);
	}

	public virtual void SetState(State inState)
	{
		if (mState == State.DEAD && inState != State.DEAD)
		{
			Debug.LogError("=========== Dead Object state change to " + inState.ToString() + " =============");
			Debug.Break();
		}
		OnStateChange(inState, mState);
		mState = inState;
		if (mState == State.DEAD)
		{
			Die();
		}
	}

	protected virtual void Update()
	{
		State state = mState;
		if (state <= State.SELECTED)
		{
			switch (state)
			{
			case State.IDLE:
				CheckAdjacentTileStatus();
				break;
			case State.SELECTED:
				Animate();
				break;
			}
		}
		else if (state != State.MOVE_FALL)
		{
			_ = 32;
		}
		else
		{
			UpdateMovement();
		}
	}

	private void CheckAdjacentTileStatus()
	{
	}

	public virtual void Animate()
	{
		base.transform.localScale = mFromScale * (1f - mFactor) + mToScale * mFactor;
		mFactor += mAnimateSpeed * Time.deltaTime;
		if (mFactor > 1f || mFactor < 0f)
		{
			mAnimateSpeed *= -1f;
		}
	}

	protected void UpdateMovement()
	{
		mCurSpeed -= mSpeedDamp * Time.deltaTime;
		if (mCurSpeed < mMinSpeed)
		{
			mCurSpeed = mMinSpeed;
		}
		base.transform.localPosition += mMoveDir * mCurSpeed * Time.deltaTime;
		if (IsDestinationReached())
		{
			base.transform.localPosition = mTargetPos;
			SetState(State.IDLE);
		}
	}

	protected bool IsDestinationReached()
	{
		return mPlane.GetDistanceToPoint(base.transform.localPosition) >= 0f;
	}

	public virtual void OnStateChange(State inNewState, State inPrvState)
	{
		if (inPrvState == State.SELECTED)
		{
			base.transform.rotation = Quaternion.identity;
		}
	}

	protected virtual bool IsVisible()
	{
		return base.transform.position.y > mVisiblityLimit;
	}

	protected virtual void Die()
	{
		base.enabled = false;
		base.transform.position = Vector3.one * -10000f;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void OnHint(bool inAnimate)
	{
	}

	public void OnSelected(float inSpeed, float inScalePercent)
	{
		mOrgScale = base.transform.localScale;
		mFromScale = base.transform.localScale;
		mToScale = mFromScale * inScalePercent;
		mAnimateSpeed = inSpeed;
		SetState(State.SELECTED);
	}

	public void OnUnSlect()
	{
		if (mState != State.DEAD && mState == State.SELECTED)
		{
			SetState(State.IDLE);
		}
		base.transform.localScale = mOrgScale;
	}

	public virtual void OnBehaviourCompleted()
	{
		if (mOnBehaviourCompletedEvent != null)
		{
			mOnBehaviourCompletedEvent(-1, this, null);
		}
	}

	public void RegisterBehaviourCallback(OnTileBehaviourCompletedDeleagte inCallback)
	{
		if (mOnBehaviourCompletedEvent == null)
		{
			mOnBehaviourCompletedEvent = inCallback;
		}
		else
		{
			mOnBehaviourCompletedEvent = (OnTileBehaviourCompletedDeleagte)Delegate.Combine(mOnBehaviourCompletedEvent, inCallback);
		}
	}

	public virtual bool IsInState(State inState)
	{
		return (mState & inState) != 0;
	}

	public virtual bool IsInLimbo()
	{
		if (mState != State.SLEEP)
		{
			return mState == State.FROZEN;
		}
		return true;
	}
}
