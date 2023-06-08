using UnityEngine;

public abstract class UserNotify : MonoBehaviour
{
	protected CoCommonLevel mCommonLevel;

	public void OnWaitBegin(GameObject parentObj)
	{
		mCommonLevel = parentObj.GetComponent<CoCommonLevel>();
		OnWaitBeginImpl();
	}

	public abstract void OnWaitBeginImpl();

	protected virtual void OnWaitEnd()
	{
		if (mCommonLevel != null)
		{
			mCommonLevel.OnWaitEnd();
			base.enabled = false;
		}
		else
		{
			UtDebug.LogError("OnWaitEnd call without parent object or Object is corrupted!!! ");
		}
	}
}
