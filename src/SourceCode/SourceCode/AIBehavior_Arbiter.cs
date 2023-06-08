using System.Collections.Generic;
using UnityEngine;

public class AIBehavior_Arbiter : AIBehavior
{
	public float _TimeBetweenArbitrariations = 0.125f;

	private float mTimeOfLastArbitrariation = -1000f;

	private bool mUpdateList = true;

	private bool mForceAbitrate;

	private List<AIEvaluator> mList = new List<AIEvaluator>();

	private AIBehavior mBestBehavior;

	private AIEvaluator mBestEvaluator;

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (mUpdateList)
		{
			PopulateList();
		}
		if (State == AIBehaviorState.INACTIVE)
		{
			OnStart(Actor);
		}
		if (mForceAbitrate || mBestBehavior == null || (_TimeBetweenArbitrariations > 0f && Time.realtimeSinceStartup > mTimeOfLastArbitrariation + _TimeBetweenArbitrariations))
		{
			Arbitrate(Actor);
		}
		if (mBestBehavior == null)
		{
			return AIBehaviorState.ACTIVE;
		}
		return mBestBehavior.Think(Actor);
	}

	public override void OnStart(AIActor Actor)
	{
		SetState(AIBehaviorState.ACTIVE);
	}

	public override void OnTerminate(AIActor Actor)
	{
		base.OnTerminate(Actor);
		if (mBestBehavior != null)
		{
			mBestBehavior.OnTerminate(Actor);
		}
		mBestBehavior = null;
		mBestEvaluator = null;
	}

	public void Arbitrate(AIActor Actor)
	{
		AIEvaluator aIEvaluator = null;
		float num = -1f;
		mForceAbitrate = false;
		if (mBestBehavior != null && mBestBehavior.IsForcingExecution() && mBestBehavior.State != AIBehaviorState.COMPLETED && mBestBehavior.State != AIBehaviorState.FAILED)
		{
			return;
		}
		GameObject gameObject = ((mBestBehavior == null) ? null : mBestBehavior.gameObject);
		int count = mList.Count;
		for (int i = 0; i < count; i++)
		{
			AIEvaluator aIEvaluator2 = mList[i];
			if (!(aIEvaluator2 == null))
			{
				float desirability = aIEvaluator2.GetDesirability(Actor, aIEvaluator2.gameObject == gameObject, null);
				if (desirability > num || aIEvaluator == null)
				{
					num = desirability;
					aIEvaluator = aIEvaluator2;
				}
			}
		}
		if (aIEvaluator != mBestEvaluator)
		{
			if (mBestBehavior != null)
			{
				mBestBehavior.OnTerminate(Actor);
			}
			mBestEvaluator = aIEvaluator;
			mBestBehavior = mBestEvaluator.GetComponent<AIBehavior>();
			if (mBestBehavior != null)
			{
				mBestBehavior.OnStart(Actor);
			}
		}
	}

	private void PopulateList()
	{
		mUpdateList = false;
		mList.Clear();
		foreach (Transform item in base.transform)
		{
			AIEvaluator component = item.GetComponent<AIEvaluator>();
			if (component != null)
			{
				component.CheckForChildEvaluators();
				mList.Add(component);
			}
		}
	}

	public void PushEvaluator(AIEvaluator Evaluator)
	{
		if (!mList.Contains(Evaluator))
		{
			mList.Add(Evaluator);
			Evaluator.transform.parent = base.transform;
			mForceAbitrate = true;
		}
	}

	public void RemoveEvaluator(AIEvaluator Evaluator, AIActor Actor)
	{
		if (!mList.Contains(Evaluator))
		{
			return;
		}
		mForceAbitrate = true;
		if (mBestEvaluator == Evaluator)
		{
			if (mBestBehavior != null)
			{
				mBestBehavior.OnTerminate(Actor);
			}
			mBestBehavior = null;
			mBestEvaluator = null;
		}
		Evaluator.transform.parent = null;
		mList.Remove(Evaluator);
	}
}
