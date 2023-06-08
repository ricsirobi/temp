using System;
using System.Collections.Generic;
using SWS;
using UnityEngine;
using UnityEngine.Events;

public class NPCSplineMove : splineMove
{
	[Serializable]
	public class SplineData
	{
		[Tooltip("The PathManager reference (if available).  Otherwise Name will be used.")]
		public SWS.PathManager _Spline;

		[Tooltip("The spline name.  Used if _Spline is not set.")]
		public string _Name;

		[Tooltip("The events to use for this spline.")]
		public List<UnityEvent> _Events;
	}

	public List<SplineData> _SplineData;

	private NPCAvatar mNPCAvatar;

	public NPCAvatar pNPCAvatar
	{
		get
		{
			if (mNPCAvatar == null)
			{
				mNPCAvatar = GetComponent<NPCAvatar>();
			}
			return mNPCAvatar;
		}
	}

	public override void StartMove()
	{
		if (pNPCAvatar != null)
		{
			pNPCAvatar.SetState(Character_State.idle);
		}
		if (_SplineData != null)
		{
			SplineData splineData = _SplineData.Find((SplineData s) => pathContainer == s._Spline || pathContainer.name == s._Name);
			if (splineData != null)
			{
				events = splineData._Events;
			}
		}
		base.StartMove();
	}
}
