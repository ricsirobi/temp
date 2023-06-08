using System.Collections;
using UnityEngine;

public abstract class AIBehavior : MonoBehaviour
{
	public AIBehaviorState State;

	protected bool _ForceExecution;

	public abstract AIBehaviorState Think(AIActor Actor);

	public virtual bool CanBeExecuted(AIActor Actor)
	{
		return true;
	}

	public virtual void OnStart(AIActor Actor)
	{
		SetState(AIBehaviorState.ACTIVE);
	}

	public virtual void OnTerminate(AIActor Actor)
	{
		SetState(AIBehaviorState.INACTIVE);
	}

	public AIBehaviorState SetState(AIBehaviorState NewState)
	{
		State = NewState;
		return State;
	}

	public virtual bool IsForcingExecution()
	{
		return _ForceExecution;
	}

	private IEnumerator Action_NavigateTo(AITarget Target, float FinishWhenCloserThan = 0.5f, AITarget LookAt = null)
	{
		return null;
	}

	private IEnumerator Action_MoveTo(AITarget Target, float FinishWhenCloserThan = 0.5f, AITarget LookAt = null)
	{
		return null;
	}

	private IEnumerator Action_RotateToward(AITarget LookAt)
	{
		return null;
	}

	private IEnumerator Action_LookAt(AITarget LookAt)
	{
		return null;
	}

	private IEnumerator Action_LookMouse()
	{
		return null;
	}

	public static Coroutine WaitFor_AnimOver(MonoBehaviour obj, AnimationState Anim, float PreFinishTime = 0f)
	{
		return CoroutineWrapper.Start(obj, pWaitFor_AnimOver(Anim, PreFinishTime));
	}

	private static IEnumerator pWaitFor_AnimOver(AnimationState Anim, float PreFinishTime = 0f)
	{
		if (!(Anim == null))
		{
			while (Anim.weight > 0f && Anim.enabled && (Anim.time < Anim.length - PreFinishTime || Anim.wrapMode == WrapMode.Loop || Anim.wrapMode == WrapMode.ClampForever || Anim.wrapMode == WrapMode.PingPong))
			{
				yield return null;
			}
		}
	}

	public static Coroutine WaitFor_Any(MonoBehaviour obj, params IEnumerator[] Actions)
	{
		return CoroutineWrapper.Start(obj, pWaitFor_Any(obj, Actions));
	}

	private static IEnumerator pWaitFor_Any(MonoBehaviour obj, params IEnumerator[] Actions)
	{
		ArrayList Wrappers = new ArrayList();
		foreach (IEnumerator func in Actions)
		{
			Wrappers.Add(CoroutineWrapper.StartWrapper(obj, func));
		}
		while (true)
		{
			foreach (CoroutineWrapper item in Wrappers)
			{
				if (!item.Finished)
				{
					continue;
				}
				{
					foreach (CoroutineWrapper item2 in Wrappers)
					{
						item2.Stop();
					}
					yield break;
				}
			}
			yield return null;
		}
	}

	public static Coroutine WaitFor_All(MonoBehaviour obj, params IEnumerator[] Actions)
	{
		return CoroutineWrapper.Start(obj, pWaitFor_All(obj, Actions));
	}

	private static IEnumerator pWaitFor_All(MonoBehaviour obj, params IEnumerator[] Actions)
	{
		ArrayList Wrappers = new ArrayList();
		foreach (IEnumerator func in Actions)
		{
			Wrappers.Add(CoroutineWrapper.StartWrapper(obj, func));
		}
		bool IsAnyActionRunning = true;
		while (IsAnyActionRunning)
		{
			IsAnyActionRunning = false;
			foreach (CoroutineWrapper item in Wrappers)
			{
				if (!item.Finished)
				{
					IsAnyActionRunning = true;
					break;
				}
			}
			if (IsAnyActionRunning)
			{
				yield return null;
			}
		}
		foreach (CoroutineWrapper item2 in Wrappers)
		{
			item2.Stop();
		}
	}
}
