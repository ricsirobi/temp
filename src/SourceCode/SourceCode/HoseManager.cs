using UnityEngine;

public class HoseManager : KAMonoBase
{
	public Vector3 _FollowOffset = new Vector3(-1f, 3f, 0f);

	public float _FollowSpeed = 1f;

	public Transform _TopOfHose;

	public Transform _BottomOfHose;

	public Rigidbody _FinalHoseJoint;

	public float _RangeTolerance = 2.5f;

	public MinMax _HeightTolerance = new MinMax(2f, 3.5f);

	private Vector3 mSafeTargetPosition;

	private float xDistance;

	private float yDistance;

	private float zDistance;

	private void Update()
	{
		UpdateHosePosition();
	}

	private void UpdateHosePosition()
	{
		Vector3 vector = _BottomOfHose.position - _TopOfHose.position;
		xDistance = Mathf.Abs(vector.x);
		yDistance = Mathf.Abs(vector.y);
		zDistance = Mathf.Abs(vector.z);
		float num = _FollowSpeed * Time.deltaTime;
		if (xDistance >= _RangeTolerance || zDistance >= _RangeTolerance)
		{
			mSafeTargetPosition = _BottomOfHose.position + _FollowOffset;
			_TopOfHose.position = Vector3.MoveTowards(_TopOfHose.position, mSafeTargetPosition, num);
		}
		if (yDistance <= _HeightTolerance.Min)
		{
			base.transform.Translate(Vector3.up * num);
		}
		else if (yDistance >= _HeightTolerance.Max)
		{
			base.transform.Translate(Vector3.down * num);
		}
	}
}
