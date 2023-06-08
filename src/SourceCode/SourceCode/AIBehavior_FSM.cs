using System.Collections;
using UnityEngine;

public class AIBehavior_FSM : AIBehavior
{
	private int mCurrentState = -1;

	private bool mUpdateList = true;

	private AIBehavior[] mStates;

	private ArrayList mPreviousStates = new ArrayList();

	public void Awake()
	{
		if (mUpdateList)
		{
			PopulateList();
		}
	}

	public override void OnStart(AIActor Actor)
	{
		if (mUpdateList)
		{
			PopulateList();
		}
		SetState(AIBehaviorState.ACTIVE);
		if (IsCurrentStateValid())
		{
			mStates[mCurrentState].OnStart(Actor);
		}
	}

	public override void OnTerminate(AIActor Actor)
	{
		base.OnTerminate(Actor);
		if (IsCurrentStateValid())
		{
			mStates[mCurrentState].OnTerminate(Actor);
		}
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (mUpdateList)
		{
			PopulateList();
		}
		if (IsCurrentStateValid())
		{
			return SetState(mStates[mCurrentState].Think(Actor));
		}
		return SetState(AIBehaviorState.ACTIVE);
	}

	public virtual bool MoveToState(int NewState, AIActor Actor, bool bAllowGoBack = false)
	{
		if (NewState == mCurrentState)
		{
			return true;
		}
		if (!bAllowGoBack)
		{
			ResetStateHistory();
		}
		else
		{
			mPreviousStates.Add(mCurrentState);
			if (mPreviousStates.Count > 20)
			{
				mPreviousStates.RemoveAt(0);
			}
		}
		SetFSM_State(NewState, Actor);
		return true;
	}

	public virtual bool MoveToPreviousState(AIActor Actor)
	{
		if (mPreviousStates.Count <= 0)
		{
			return false;
		}
		int num = (int)mPreviousStates[mPreviousStates.Count - 1];
		mPreviousStates.RemoveAt(mPreviousStates.Count - 1);
		if (num != mCurrentState && IsStateValid(num))
		{
			SetFSM_State(num, Actor);
		}
		return true;
	}

	private void SetFSM_State(int NewState, AIActor Actor)
	{
		if (mUpdateList)
		{
			PopulateList();
		}
		if (NewState != mCurrentState)
		{
			if (IsCurrentStateValid())
			{
				mStates[mCurrentState].OnTerminate(Actor);
			}
			mCurrentState = NewState;
			if (IsCurrentStateValid())
			{
				mStates[mCurrentState].OnStart(Actor);
			}
		}
	}

	public virtual void ResetStateHistory()
	{
		mPreviousStates.Clear();
	}

	public int FindStateIndex(string StateName)
	{
		if (mStates == null)
		{
			return -1;
		}
		for (int i = 0; i < mStates.Length; i++)
		{
			if (mStates[i].name == StateName)
			{
				return i;
			}
		}
		return -1;
	}

	protected void PopulateList()
	{
		mUpdateList = false;
		ArrayList arrayList = new ArrayList();
		foreach (Transform item in base.transform)
		{
			AIBehavior component = item.GetComponent<AIBehavior>();
			if (component != null)
			{
				arrayList.Add(component);
			}
		}
		mStates = new AIBehavior[arrayList.Count];
		for (int i = 0; i < mStates.Length; i++)
		{
			mStates[i] = (AIBehavior)arrayList[i];
		}
	}

	private bool IsStateValid(int Index)
	{
		if (mStates != null && Index >= 0)
		{
			return Index < mStates.Length;
		}
		return false;
	}

	private bool IsCurrentStateValid()
	{
		if (mStates != null)
		{
			return IsStateValid(mCurrentState);
		}
		return false;
	}
}
