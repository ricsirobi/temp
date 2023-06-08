using System.Collections;
using System.Linq;
using UnityEngine;

public class AIBehavior_Sequence : AIBehavior
{
	public AIBehaviorAmount FailCondition = AIBehaviorAmount.ALL;

	public AIBehaviorAmount CompletedCondition = AIBehaviorAmount.ALL;

	public bool Loop;

	public bool YieldAfterEveryAction;

	public bool _DestroyBehaviorsAfterCompleted;

	public bool _RemoveBehaviorsAfterCompleted;

	private ArrayList mList = new ArrayList();

	private int mCurrentIndex;

	private bool mUpdateList = true;

	private int[] mResults = new int[4];

	private ArrayList pList
	{
		get
		{
			if (mList == null)
			{
				mList = new ArrayList();
			}
			return mList;
		}
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (State == AIBehaviorState.INACTIVE || State == AIBehaviorState.COMPLETED || mCurrentIndex < 0 || mCurrentIndex >= pList.Count)
		{
			OnStart(Actor);
		}
		if (mUpdateList)
		{
			PopulateList();
		}
		AIBehaviorState aIBehaviorState = AIBehaviorState.COMPLETED;
		int count = pList.Count;
		while (mCurrentIndex < pList.Count)
		{
			AIBehavior aIBehavior = (AIBehavior)pList[mCurrentIndex];
			aIBehaviorState = ((!aIBehavior.enabled) ? AIBehaviorState.COMPLETED : aIBehavior.Think(Actor));
			if (aIBehaviorState == AIBehaviorState.ACTIVE)
			{
				return AIBehaviorState.ACTIVE;
			}
			mResults[(int)aIBehaviorState]++;
			aIBehavior.OnTerminate(Actor);
			if (!_RemoveBehaviorsAfterCompleted)
			{
				mCurrentIndex++;
			}
			else
			{
				pList.RemoveAt(mCurrentIndex);
				if (_DestroyBehaviorsAfterCompleted)
				{
					Object.Destroy(aIBehavior);
				}
			}
			if (FailCondition == AIBehaviorAmount.ANY && aIBehaviorState == AIBehaviorState.FAILED)
			{
				return SetState(AIBehaviorState.FAILED);
			}
			if (FailCondition == AIBehaviorAmount.ALL && mResults[2] == count)
			{
				return SetState(AIBehaviorState.FAILED);
			}
			if (CompletedCondition == AIBehaviorAmount.ANY && mResults[3] > 0)
			{
				return SetState(AIBehaviorState.COMPLETED);
			}
			if (CompletedCondition == AIBehaviorAmount.ALL && mResults[3] == count)
			{
				return SetState(AIBehaviorState.COMPLETED);
			}
			if (mCurrentIndex >= pList.Count)
			{
				if (!Loop && CompletedCondition != AIBehaviorAmount.NEVER)
				{
					return SetState(AIBehaviorState.COMPLETED);
				}
				OnStart(Actor);
			}
			if (((AIBehavior)pList[mCurrentIndex]).enabled)
			{
				((AIBehavior)pList[mCurrentIndex]).OnStart(Actor);
			}
			if (YieldAfterEveryAction || mCurrentIndex == 0)
			{
				return SetState(AIBehaviorState.ACTIVE);
			}
		}
		return aIBehaviorState;
	}

	public override void OnStart(AIActor Actor)
	{
		if (mUpdateList)
		{
			PopulateList();
		}
		for (int i = 0; i < pList.Count; i++)
		{
			((AIBehavior)pList[i]).SetState(AIBehaviorState.INACTIVE);
		}
		mCurrentIndex = 0;
		if (pList.Count > 0)
		{
			((AIBehavior)pList[mCurrentIndex]).OnStart(Actor);
		}
		State = AIBehaviorState.ACTIVE;
		mResults[0] = (mResults[1] = (mResults[2] = (mResults[3] = 0)));
	}

	public override void OnTerminate(AIActor Actor)
	{
		if (mCurrentIndex < pList.Count && mCurrentIndex >= 0)
		{
			((AIBehavior)pList[mCurrentIndex]).OnTerminate(Actor);
		}
		State = AIBehaviorState.INACTIVE;
	}

	private void PopulateList()
	{
		mUpdateList = false;
		pList.Clear();
		foreach (Transform item in from Transform t in base.transform
			orderby t.name
			select t)
		{
			AIBehavior component = item.GetComponent<AIBehavior>();
			if (component != null)
			{
				pList.Add(component);
			}
		}
	}

	public void PushBehavior(AIBehavior pBehavior, AIActor Actor)
	{
		pList.Remove(pBehavior);
		pList.Insert(0, pBehavior);
		pBehavior.OnStart(Actor);
		pBehavior.transform.parent = base.transform;
	}

	public void RemoveBehavior(AIBehavior pBehavior, AIActor Actor)
	{
		if (mCurrentIndex >= 0 && mCurrentIndex < pList.Count && (AIBehavior)pList[mCurrentIndex] == pBehavior)
		{
			pBehavior.transform.parent = null;
			pBehavior.OnTerminate(Actor);
			pList.Remove(pBehavior);
			if (mCurrentIndex < pList.Count)
			{
				((AIBehavior)pList[mCurrentIndex]).OnStart(Actor);
			}
		}
		else
		{
			pList.Remove(pBehavior);
		}
	}

	public override bool IsForcingExecution()
	{
		if (base.IsForcingExecution())
		{
			return true;
		}
		if (mCurrentIndex >= 0 && mCurrentIndex < pList.Count)
		{
			return ((AIBehavior)pList[mCurrentIndex]).IsForcingExecution();
		}
		return false;
	}
}
