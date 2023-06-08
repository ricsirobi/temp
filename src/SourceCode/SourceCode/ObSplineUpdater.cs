using System;
using System.Collections;
using UnityEngine;

public class ObSplineUpdater : MonoBehaviour
{
	private SplineControl mSC;

	private Vector3 mFirstNodePos = Vector3.zero;

	private void Start()
	{
		mSC = (SplineControl)base.gameObject.GetComponent(typeof(SplineControl));
		if (mSC == null)
		{
			UtDebug.LogError("Unable to find Spline Control. Can't udpate Spline.");
		}
		else if (mSC.SplineObject == null)
		{
			mSC = null;
			UtDebug.LogError("Spline Control has null spline object. Can't udpate Spline.");
		}
		else if (mSC.SplineObject.childCount < 1)
		{
			mSC = null;
			UtDebug.LogError("Spline Control has no nodes. Can't udpate Spline.");
		}
		else
		{
			mFirstNodePos = GetFirstChildNodePosFromSplineControl(mSC);
		}
	}

	private void Update()
	{
		if (!(mSC == null))
		{
			Vector3 firstChildNodePosFromSplineControl = GetFirstChildNodePosFromSplineControl(mSC);
			if (Mathf.Abs(mFirstNodePos.y - firstChildNodePosFromSplineControl.y) > 0.01f)
			{
				mFirstNodePos = firstChildNodePosFromSplineControl;
				mSC.ResetSpline();
			}
		}
	}

	private Vector3 GetFirstChildNodePosFromSplineControl(SplineControl sc)
	{
		Transform transform = null;
		IEnumerator enumerator = sc.SplineObject.GetEnumerator();
		try
		{
			if (enumerator.MoveNext())
			{
				transform = (Transform)enumerator.Current;
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		return transform.position;
	}
}
