using UnityEngine;

public class ObMotionBezier : MonoBehaviour
{
	public GameObject _MessageObject;

	public Transform _Target;

	public float _TimetoChange = 5f;

	public bool _MoveOnLocalAxis;

	private Vector3 mStartPos = Vector3.zero;

	private Vector3 mEndPos = Vector3.zero;

	private Vector3 mControlPoint = Vector3.zero;

	private float mRadiusElapsedTime;

	private float mTimeScale;

	private void Update()
	{
		if ((_Target == null && mControlPoint == Vector3.zero) || mStartPos == mEndPos || _TimetoChange <= 0f)
		{
			return;
		}
		if (mTimeScale >= 1f)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage("OnBezierMotionDone", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}
		mRadiusElapsedTime += Time.deltaTime;
		mTimeScale = mRadiusElapsedTime / _TimetoChange;
		if (mControlPoint == Vector3.zero && _Target != null)
		{
			mControlPoint = _Target.transform.position;
		}
		Vector3 vector = mStartPos;
		float num = mTimeScale * mTimeScale;
		float num2 = 1f - mTimeScale;
		float num3 = num2 * num2;
		vector.x = num3 * mStartPos.x + 2f * num2 * mTimeScale * mControlPoint.x + num * mEndPos.x;
		vector.y = num3 * mStartPos.y + 2f * num2 * mTimeScale * mControlPoint.y + num * mEndPos.y;
		vector.z = num3 * mStartPos.z + 2f * num2 * mTimeScale * mControlPoint.z + num * mEndPos.z;
		if (!_MoveOnLocalAxis)
		{
			base.transform.position = vector;
		}
		else
		{
			base.transform.localPosition = vector;
		}
	}

	public void SetEndPoints(Vector3 inStart, Vector3 inEnd, Vector3 inControlPoint)
	{
		mStartPos = inStart;
		mEndPos = inEnd;
		mControlPoint = inControlPoint;
	}
}
