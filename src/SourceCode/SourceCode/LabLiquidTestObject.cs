using UnityEngine;

public class LabLiquidTestObject : LabTestObject
{
	private Transform mSurfaceMarker;

	public bool pDestroyOnScaleEnd;

	public float pCurrentScaleTime { get; set; }

	public override Vector3 pTopPosition => pBottomPosition;

	public override Vector3 pBottomPosition
	{
		get
		{
			if (_Renderer == null)
			{
				return base.pTopPosition;
			}
			if (mSurfaceMarker == null)
			{
				mSurfaceMarker = base.transform.Find("Surface");
			}
			if (mSurfaceMarker != null)
			{
				return mSurfaceMarker.transform.position;
			}
			return base.pBottomPosition;
		}
	}

	public override void OnScaleStopped()
	{
		base.OnScaleStopped();
		if (mManager != null && mManager._WaterStream != null)
		{
			mManager._WaterStream.Stop();
			if (mManager._AddWaterSFX != null)
			{
				SnChannel.StopPool("Default_Pool3");
			}
			if (mManager._WaterSplashSteam != null)
			{
				mManager._WaterSplashSteam.Stop();
			}
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (base.pCrucible != null)
		{
			base.pCrucible.UpdateLiquidPosition();
		}
	}
}
