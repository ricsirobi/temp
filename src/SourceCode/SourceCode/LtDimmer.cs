using UnityEngine;

public class LtDimmer : LtAnimBase
{
	public float _StartIntensity;

	public float _EndIntensity = 1f;

	public override void UpdateValue(float f)
	{
		base.light.intensity = Mathf.Lerp(_StartIntensity, _EndIntensity, f);
	}
}
