using UnityEngine;

public class TutorialStepHandlerProximity : TutorialStepHandler
{
	private Transform mProximityItem1;

	private Transform mProximityItem2;

	public override void SetupTutorialStep()
	{
		base.SetupTutorialStep();
		InitializeObjects();
	}

	public void InitializeObjects()
	{
		GameObject gameObject = null;
		if (mProximityItem1 == null)
		{
			if (mTutStep != null && !string.IsNullOrEmpty(mTutStep._StepDetails._ProximityItem1))
			{
				gameObject = GameObject.Find(mTutStep._StepDetails._ProximityItem1);
			}
			if (gameObject != null)
			{
				mProximityItem1 = gameObject.GetComponent<Transform>();
			}
		}
		gameObject = null;
		if (mProximityItem2 == null)
		{
			if (mTutStep != null && !string.IsNullOrEmpty(mTutStep._StepDetails._ProximityItem2))
			{
				gameObject = GameObject.Find(mTutStep._StepDetails._ProximityItem2);
			}
			if (gameObject != null)
			{
				mProximityItem2 = gameObject.GetComponent<Transform>();
			}
		}
	}

	public override void StepUpdate()
	{
		base.StepUpdate();
		InitializeObjects();
		if (mTutStep != null)
		{
			float num = float.PositiveInfinity;
			if (mProximityItem1 != null && mProximityItem2 != null)
			{
				num = (mProximityItem1.position - mProximityItem2.position).magnitude;
			}
			if (num <= mTutStep._StepDetails._ProximityDistance && _StepProgressCallback != null)
			{
				_StepProgressCallback(0f, 0f);
			}
		}
	}

	public override void FinishTutorialStep()
	{
		base.FinishTutorialStep();
	}
}
