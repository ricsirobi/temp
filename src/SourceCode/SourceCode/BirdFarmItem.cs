using UnityEngine;

public class BirdFarmItem : AnimalFarmItem
{
	public float _FlyingHeightThershold = 1f;

	protected override void Update()
	{
		base.Update();
		if (mCurrentState == AnimalState.Walk || mCurrentState == AnimalState.Flying)
		{
			_SplineControl.mSpline.GetSectionTimeInfo(_SplineControl.CurrentPos / _SplineControl.LinearLength, out var _, out var n, out var n2);
			if (Mathf.Abs(_SplineControl.mSpline.mNodes[n2].mPoint.y - _SplineControl.mSpline.mNodes[n].mPoint.y) > _FlyingHeightThershold)
			{
				SetInternalState(AnimalState.Flying);
			}
			else
			{
				SetInternalState(AnimalState.Walk);
			}
		}
	}

	public override void SetSpline(Spline spline)
	{
		base.SetSpline(spline);
		Vector3 position = base.transform.position;
		float groundHeight = 0f;
		UtUtilities.GetGroundHeight(position, _SplineControl.GroundCheckDist, out groundHeight);
		if (groundHeight == float.NegativeInfinity || position.y - groundHeight > _FlyingHeightThershold)
		{
			SetInternalState(AnimalState.Flying);
		}
	}
}
