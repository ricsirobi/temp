using System.Collections;
using UnityEngine;

public class CoroutineWrapper
{
	public static CoroutineWrapper CurrentContext;

	public bool Finished;

	public CoroutineWrapper Parent;

	public void Stop()
	{
		Finished = true;
	}

	private bool IsFinished()
	{
		if (Finished)
		{
			return true;
		}
		if (Parent == null)
		{
			return false;
		}
		return Parent.IsFinished();
	}

	public static CoroutineWrapper StartWrapper(MonoBehaviour Obj, IEnumerator Func)
	{
		CoroutineWrapper coroutineWrapper = new CoroutineWrapper();
		coroutineWrapper.Parent = CurrentContext;
		CurrentContext = coroutineWrapper;
		Obj.StartCoroutine(coroutineWrapper.MyFunc(Func));
		CurrentContext = coroutineWrapper.Parent;
		return coroutineWrapper;
	}

	public static Coroutine Start(MonoBehaviour Obj, IEnumerator Func)
	{
		CoroutineWrapper coroutineWrapper = new CoroutineWrapper();
		coroutineWrapper.Parent = CurrentContext;
		CurrentContext = coroutineWrapper;
		Coroutine result = Obj.StartCoroutine(coroutineWrapper.MyFunc(Func));
		CurrentContext = coroutineWrapper.Parent;
		return result;
	}

	public IEnumerator MyFunc(IEnumerator Func)
	{
		Finished = false;
		Parent = CurrentContext;
		CurrentContext = this;
		while (!Finished && !IsFinished() && Func.MoveNext())
		{
			CurrentContext = null;
			yield return Func.Current;
			CurrentContext = this;
		}
		Finished = true;
		CurrentContext = null;
	}
}
