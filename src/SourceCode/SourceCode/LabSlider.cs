using UnityEngine;

public class LabSlider : KAMonoBase
{
	private float mTime;

	private Vector3 mTargetPos;

	private bool mMoving;

	private Vector3 mStartPos;

	private float mTimeModifier;

	private GameObject mMessageObject;

	public void StartMove(Vector2 inTargetPos, float inTime, GameObject inMessageObject)
	{
		Vector3 inTargetPos2 = new Vector3(inTargetPos.x, inTargetPos.y, base.transform.position.z);
		StartMove(inTargetPos2, inTime, inMessageObject);
	}

	public void StartMove(Vector3 inTargetPos, float inTime, GameObject inMessageObject)
	{
		mStartPos = base.transform.position;
		mMoving = true;
		mTime = inTime;
		mTargetPos = inTargetPos;
		mTimeModifier = 0f;
		mMessageObject = inMessageObject;
	}

	public void Update()
	{
		if (mMoving)
		{
			mTimeModifier += Time.deltaTime / mTime;
			base.transform.position = Vector3.Lerp(mStartPos, mTargetPos, mTimeModifier);
			if (mTimeModifier >= 1f)
			{
				mMoving = false;
				mMessageObject.SendMessage("End3DMoveTo", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
