using System.Collections.Generic;

public class AIBehavior_Parallel : AIBehavior
{
	private List<AIBehavior> mList = new List<AIBehavior>();

	public AIBehaviorAmount FailCondition = AIBehaviorAmount.ALL;

	public AIBehaviorAmount CompletedCondition = AIBehaviorAmount.ALL;

	public bool UpdateList = true;

	private int[] mResults = new int[4];

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (State == AIBehaviorState.INACTIVE)
		{
			OnStart(Actor);
		}
		if (UpdateList)
		{
			PopulateList();
		}
		mResults[0] = 0;
		mResults[1] = 0;
		mResults[2] = 0;
		mResults[3] = 0;
		int count = mList.Count;
		for (int i = 0; i < count; i++)
		{
			AIBehavior aIBehavior = mList[i];
			if (aIBehavior.State == AIBehaviorState.ACTIVE)
			{
				mResults[(int)aIBehavior.Think(Actor)]++;
			}
			else
			{
				mResults[(int)aIBehavior.State]++;
			}
		}
		int count2 = mList.Count;
		if (FailCondition == AIBehaviorAmount.ANY && mResults[2] > 0)
		{
			return SetState(AIBehaviorState.FAILED);
		}
		if (FailCondition == AIBehaviorAmount.ALL && mResults[2] == count2)
		{
			return SetState(AIBehaviorState.FAILED);
		}
		if (CompletedCondition == AIBehaviorAmount.ANY && mResults[3] > 0)
		{
			return SetState(AIBehaviorState.COMPLETED);
		}
		if (CompletedCondition == AIBehaviorAmount.ALL && mResults[3] == count2)
		{
			return SetState(AIBehaviorState.COMPLETED);
		}
		return AIBehaviorState.ACTIVE;
	}

	public override void OnStart(AIActor Actor)
	{
		if (UpdateList)
		{
			PopulateList();
		}
		foreach (AIBehavior m in mList)
		{
			m.OnStart(Actor);
		}
		State = AIBehaviorState.ACTIVE;
	}

	public override void OnTerminate(AIActor Actor)
	{
		foreach (AIBehavior m in mList)
		{
			m.OnTerminate(Actor);
		}
		State = AIBehaviorState.INACTIVE;
	}

	private void PopulateList()
	{
		UpdateList = false;
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

	public override bool IsForcingExecution()
	{
		if (base.IsForcingExecution())
		{
			return true;
		}
		int i = 0;
		for (int count = mList.Count; i < count; i++)
		{
			if (mList[i].IsForcingExecution())
			{
				return true;
			}
		}
		return false;
	}
}
