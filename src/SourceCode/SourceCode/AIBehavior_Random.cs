using System.Collections;
using UnityEngine;

public class AIBehavior_Random : AIBehavior
{
	private bool mUpdateList = true;

	private ArrayList mList = new ArrayList();

	private int mCurrentBehavior = -1;

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (mCurrentBehavior < 0 || mCurrentBehavior >= mList.Count)
		{
			return AIBehaviorState.COMPLETED;
		}
		AIBehavior aIBehavior = (AIBehavior)mList[mCurrentBehavior];
		if (aIBehavior.State == AIBehaviorState.ACTIVE)
		{
			return aIBehavior.Think(Actor);
		}
		return aIBehavior.State;
	}

	public override void OnStart(AIActor Actor)
	{
		if (mUpdateList)
		{
			PopulateList();
		}
		base.OnStart(Actor);
		mCurrentBehavior = Random.Range(0, mList.Count - 1);
		((AIBehavior)mList[mCurrentBehavior]).OnStart(Actor);
	}

	public override void OnTerminate(AIActor Actor)
	{
		base.OnTerminate(Actor);
		if (mCurrentBehavior >= 0 && mCurrentBehavior < mList.Count)
		{
			((AIBehavior)mList[mCurrentBehavior]).OnTerminate(Actor);
		}
		mCurrentBehavior = -1;
	}

	private void PopulateList()
	{
		mUpdateList = false;
		mList.Clear();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			AIBehavior component = base.transform.GetChild(i).GetComponent<AIBehavior>();
			if (component != null)
			{
				mList.Add(component);
			}
		}
	}
}
