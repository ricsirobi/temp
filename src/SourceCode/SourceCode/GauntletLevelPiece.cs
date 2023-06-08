using System;
using System.Collections.Generic;
using UnityEngine;

public class GauntletLevelPiece : MonoBehaviour
{
	public GauntletPieceDirection _ExitDirection;

	public GameObject _StartSpline;

	public GameObject[] _AlternateSplines;

	[NonSerialized]
	public bool _ReverseSplineDirection;

	private GameObject mPathTriggerParentObject;

	private List<GameObject> mSplinePaths = new List<GameObject>();

	private List<SplineControl> mSplineControl = new List<SplineControl>();

	private List<GameObject> mPathTriggers = new List<GameObject>();

	private void Start()
	{
		mPathTriggerParentObject = new GameObject("MultiPathTriggers");
		mPathTriggerParentObject.transform.parent = base.transform;
		GenerateMultiPathTriggers();
	}

	private void GenerateMultiPathTriggers()
	{
		if (!(_StartSpline != null) || _AlternateSplines.Length == 0)
		{
			return;
		}
		mSplinePaths = new List<GameObject>(_AlternateSplines);
		mSplinePaths.Insert(0, _StartSpline);
		foreach (GameObject mSplinePath in mSplinePaths)
		{
			SplineControl splineControl = new GameObject(mSplinePath.name).AddComponent<SplineControl>();
			splineControl.SplineObject = mSplinePath.transform;
			splineControl.ResetSpline();
			mSplineControl.Add(splineControl);
		}
		GenerateTriggerPoints(0);
		foreach (SplineControl item in mSplineControl)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		mSplinePaths.Clear();
		mSplineControl.Clear();
		mPathTriggers.Clear();
	}

	private void GenerateTriggerPoints(int index)
	{
		SplineNode[] mNodes = mSplineControl[index].mSpline.mNodes;
		if (mNodes == null || mNodes.Length == 0)
		{
			return;
		}
		for (int i = 0; i < mNodes.Length; i++)
		{
			for (int j = 0; j < mSplineControl.Count; j++)
			{
				if (mSplineControl[j].mSpline.mNodes == null)
				{
					continue;
				}
				int num = mSplineControl[j].mSpline.mNodes.Length;
				if (j == index || num == 0)
				{
					continue;
				}
				Vector3 mPoint = mSplineControl[index].mSpline.mNodes[i].mPoint;
				if ((mPoint - mSplineControl[j].mSpline.mNodes[0].mPoint).magnitude < 0.1f)
				{
					AddPath(mPoint, mSplinePaths[j], 0);
					if (i < mNodes.Length - 1)
					{
						AddPath(mPoint, mSplinePaths[index], i);
					}
					if (j > index)
					{
						GenerateTriggerPoints(j);
					}
				}
				if ((mPoint - mSplineControl[j].mSpline.mNodes[num - 1].mPoint).magnitude < 0.1f && i < mNodes.Length - 1)
				{
					AddPath(mPoint, mSplinePaths[index], i);
				}
			}
		}
	}

	private void AddPath(Vector3 pos, GameObject splineObj, int startNodeIndex)
	{
		GauntletPathModTrigger gauntletPathModTrigger = null;
		foreach (GameObject mPathTrigger in mPathTriggers)
		{
			if ((pos - mPathTrigger.transform.position).magnitude < 0.1f)
			{
				gauntletPathModTrigger = mPathTrigger.GetComponent<GauntletPathModTrigger>();
				break;
			}
		}
		if (gauntletPathModTrigger == null)
		{
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			gameObject.GetComponent<Renderer>().enabled = false;
			gameObject.GetComponent<Collider>().isTrigger = true;
			gameObject.transform.position = pos;
			gameObject.transform.parent = mPathTriggerParentObject.transform;
			gauntletPathModTrigger = gameObject.AddComponent<GauntletPathModTrigger>();
			mPathTriggers.Add(gameObject);
			gameObject.name = "PathTrigger" + mPathTriggers.Count;
		}
		int num = 0;
		if (gauntletPathModTrigger._AlternatePaths != null)
		{
			num = gauntletPathModTrigger._AlternatePaths.Length;
		}
		bool flag = false;
		if (num > 0)
		{
			GauntletPathModData[] alternatePaths = gauntletPathModTrigger._AlternatePaths;
			for (int i = 0; i < alternatePaths.Length; i++)
			{
				if (alternatePaths[i]._SplineObject == splineObj.transform)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag || num == 0)
		{
			GauntletPathModData gauntletPathModData = new GauntletPathModData();
			gauntletPathModData._SplineObject = splineObj.transform;
			gauntletPathModData._NodeIndex = startNodeIndex;
			Array.Resize(ref gauntletPathModTrigger._AlternatePaths, num + 1);
			gauntletPathModTrigger._AlternatePaths[num] = gauntletPathModData;
		}
	}
}
