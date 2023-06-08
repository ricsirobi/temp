using System;
using System.Collections.Generic;
using UnityEngine;

public class PathManager
{
	private static PathManager mInstance;

	private static bool mInitialized;

	private List<NodeInfo>[] mPathData;

	private byte mPathCount;

	public bool _Log;

	public static PathManager pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new PathManager();
			}
			return mInstance;
		}
	}

	public static bool pInitialized
	{
		get
		{
			return mInitialized;
		}
		set
		{
			mInitialized = value;
		}
	}

	public static void Destroy()
	{
		if (mInstance != null)
		{
			UtDebug.LogError("Destroy Path Manger ");
			Array.Clear(mInstance.mPathData, 0, mInstance.mPathData.Length);
			mInstance.mPathData = null;
			mInstance.mPathCount = 0;
			mInitialized = false;
			mInstance = null;
		}
	}

	private PathManager()
	{
		mPathData = new List<NodeInfo>[ShortcutPath.pShortcutCount + 1];
	}

	public static Vector3 GetNodePosition(int inId, int inIndex)
	{
		if (inIndex >= GetNodeCount(inId))
		{
			inIndex = GetNodeCount(inId) - 1;
		}
		if (!mInitialized)
		{
			return Vector3.zero;
		}
		UtDebug.Assert(inId < pInstance.mPathCount, "XXXXXXXX GetNodePosition() :: Path ID Incorrect");
		return pInstance.mPathData[inId][inIndex].mPosition;
	}

	public static Quaternion GetNodeRot(int inId, int inIndex)
	{
		if (inIndex >= GetNodeCount(inId))
		{
			inIndex = GetNodeCount(inId) - 1;
		}
		if (!mInitialized)
		{
			return Quaternion.identity;
		}
		UtDebug.Assert(inId < pInstance.mPathCount, "XXXXXXXX GetNodeRot() :: Path ID Incorrect");
		return pInstance.mPathData[inId][inIndex].mRotation;
	}

	public static float GetNodeDistance(int inId, int inIndex)
	{
		if (inIndex >= GetNodeCount(inId))
		{
			inIndex = GetNodeCount(inId) - 1;
		}
		if (!mInitialized)
		{
			return 0f;
		}
		UtDebug.Assert(inId < pInstance.mPathCount, "XXXXXXXX GetNodeDistance() :: Path ID Incorrect");
		return pInstance.mPathData[inId][inIndex].mDistance;
	}

	public static float GetSplineLength(int inId)
	{
		if (!mInitialized)
		{
			return 0f;
		}
		UtDebug.Assert(inId < pInstance.mPathCount, "XXXXXXXX GetSplineLength() :: Path ID Incorrect");
		return pInstance.mPathData[inId][pInstance.mPathData[inId].Count - 1].mDistance;
	}

	public static int GetNodeCount(int inId)
	{
		if (!mInitialized)
		{
			return 0;
		}
		UtDebug.Assert(inId < pInstance.mPathCount, "XXXXXXXX GetNodeCount() :: Path ID Incorrect");
		return pInstance.mPathData[inId].Count;
	}

	public void PushNodeInList(GameObject inNodeParent)
	{
		if (inNodeParent == null)
		{
			return;
		}
		Component[] t = inNodeParent.GetComponentsInChildren(typeof(Transform));
		GameUtilities.SortByName(ref t);
		SplineControl splineControl = inNodeParent.GetComponent(typeof(SplineControl)) as SplineControl;
		if (splineControl == null)
		{
			UtDebug.LogError(" RaceSPline is not attached.........");
			return;
		}
		mPathData[mPathCount] = new List<NodeInfo>();
		splineControl.ResetSpline();
		int num = 0;
		Component[] array = t;
		for (int i = 0; i < array.Length; i++)
		{
			Transform transform = (Transform)array[i];
			if (transform != inNodeParent.transform)
			{
				NodeInfo item = default(NodeInfo);
				item.mPosition = transform.position;
				item.mDistance = splineControl.mSpline.mNodes[num++].mDistance;
				item.mRotation = transform.rotation;
				mPathData[mPathCount].Add(item);
			}
		}
		mPathCount++;
	}
}
