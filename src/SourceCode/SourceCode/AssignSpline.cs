using SWS;
using UnityEngine;

public class AssignSpline : MonoBehaviour
{
	[Tooltip("Set the moveToPath variable of splineMove or navMove component when setting the path.")]
	public bool _MoveToPath = true;

	public bool _StartMovement = true;

	public void SetSplineRoot(GameObject splineRoot)
	{
		SWS.PathManager pathManager = ((splineRoot != null) ? splineRoot.GetComponent<SWS.PathManager>() : null);
		navMove component = GetComponent<navMove>();
		if (component != null)
		{
			if (pathManager != null)
			{
				component.moveToPath = _MoveToPath;
				if (_StartMovement)
				{
					component.SetPath(pathManager);
					return;
				}
				component.Stop();
				component.pathContainer = pathManager;
			}
			else
			{
				component.Stop();
			}
			return;
		}
		splineMove component2 = GetComponent<splineMove>();
		if (component2 != null)
		{
			if (pathManager != null)
			{
				component2.moveToPath = _MoveToPath;
				if (_StartMovement)
				{
					component2.SetPath(pathManager);
					return;
				}
				component2.Stop();
				component2.pathContainer = pathManager;
			}
			else
			{
				component2.Stop();
			}
			return;
		}
		SplineControl component3 = GetComponent<SplineControl>();
		if (!(component3 != null))
		{
			return;
		}
		if (splineRoot != null)
		{
			component3.SplineObject = splineRoot.transform;
			if (_StartMovement)
			{
				component3.ResetSpline();
			}
		}
		else
		{
			component3.SetSpline(null);
		}
	}

	public void SetEmptySplineRoot()
	{
		SetSplineRoot(null);
	}
}
