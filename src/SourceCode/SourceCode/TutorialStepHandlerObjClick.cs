using System;
using UnityEngine;

public class TutorialStepHandlerObjClick : TutorialStepHandler
{
	public class ObjectData
	{
		public GameObject Object;

		public string CursorName;
	}

	private ObjectData[] mWaitForGameObjects;

	private int mObjectsClicked;

	public override void SetupTutorialStep()
	{
		base.SetupTutorialStep();
		GameObject gameObject = null;
		if (mTutStep != null && mTutStep._StepDetails._WaitForClickableObject != null)
		{
			mWaitForGameObjects = new ObjectData[mTutStep._StepDetails._WaitForClickableObject.Length];
			int num = 0;
			string[] waitForClickableObject = mTutStep._StepDetails._WaitForClickableObject;
			foreach (string text in waitForClickableObject)
			{
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				gameObject = GameObject.Find(text);
				mWaitForGameObjects[num] = new ObjectData();
				mWaitForGameObjects[num].Object = gameObject;
				if (gameObject != null)
				{
					ObClickable obClickable = gameObject.GetComponent<ObClickable>();
					if (obClickable == null)
					{
						obClickable = (ObClickable)gameObject.AddComponent(typeof(ObClickable));
					}
					mWaitForGameObjects[num].CursorName = obClickable._RollOverCursorName;
					obClickable._Range = mTutStep._StepDetails._ProximityDistance;
					ObClickable obClickable2 = obClickable;
					obClickable2._ObjectClickedCallback = (ObClickable.ObjectClickedDelegate)Delegate.Combine(obClickable2._ObjectClickedCallback, new ObClickable.ObjectClickedDelegate(ObjectClicked));
					obClickable._UseGlobalActive = false;
					obClickable._RollOverCursorName = mTutStep._StepDetails._RollOverCursorName;
				}
				num++;
			}
		}
		gameObject = null;
	}

	public override void StepUpdate()
	{
		base.StepUpdate();
		if (mObjectsClicked >= 1 && _StepProgressCallback != null)
		{
			_StepProgressCallback(0f, 0f);
		}
	}

	public override void FinishTutorialStep()
	{
		base.FinishTutorialStep();
	}

	public virtual void ObjectClicked(GameObject go)
	{
		int num = 0;
		ObjectData[] array = mWaitForGameObjects;
		foreach (ObjectData objectData in array)
		{
			if (objectData != null && objectData.Object != null && objectData.Object == go)
			{
				mObjectsClicked++;
				ObClickable component = mWaitForGameObjects[num].Object.GetComponent<ObClickable>();
				if (component != null)
				{
					component._RollOverCursorName = mWaitForGameObjects[num].CursorName;
				}
				mWaitForGameObjects[num] = null;
			}
			num++;
		}
	}
}
