using System;
using UnityEngine;

[Serializable]
public class AnimalPenSpline
{
	private FarmItem mFarmItem;

	public int _ID;

	public Transform[] _SplinePoints;

	public bool _FlipDirOnBackward;

	public Transform _AvatarMarker;

	public FarmItem pFarmItem
	{
		get
		{
			return mFarmItem;
		}
		set
		{
			mFarmItem = value;
		}
	}

	public bool IsInUse()
	{
		return mFarmItem != null;
	}

	public Spline CreateSpline()
	{
		if (_SplinePoints == null || _SplinePoints.Length <= 1)
		{
			return null;
		}
		Spline spline = new Spline(_SplinePoints.Length, looping: true, constSpeed: true, alignTangent: true, hasQ: true);
		for (int i = 0; i < _SplinePoints.Length; i++)
		{
			if (_SplinePoints[i] == null)
			{
				return null;
			}
			spline.SetControlPoint(i, _SplinePoints[i].position, _SplinePoints[i].rotation, 0f);
		}
		spline.RecalculateSpline();
		return spline;
	}

	public Spline GetSpline()
	{
		return CreateSpline();
	}
}
