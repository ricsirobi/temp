using UnityEngine;

public class ScaleOnZoom : MonoBehaviour
{
	private bool mIsAnimating;

	private Vector3 mZoomTo = Vector3.zero;

	public bool _EnableSmoothZoom;

	public GameObject _TargetForScaling;

	public ScaleConfig _ScaleForSmallScreen;

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	private void Update()
	{
		if (mIsAnimating)
		{
			if (Vector3.Distance(_TargetForScaling.transform.localScale, mZoomTo) < 0.02f)
			{
				_TargetForScaling.transform.localScale = mZoomTo;
				mIsAnimating = false;
			}
			else
			{
				_TargetForScaling.transform.localScale = Vector3.Lerp(_TargetForScaling.transform.localScale, mZoomTo, Time.deltaTime * 10f);
			}
		}
	}
}
