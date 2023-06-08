using UnityEngine;

public static class MobileInput
{
	public static Vector3 pAcceleration => OrientedAccleration(Input.acceleration);

	public static Vector3 OrientedAccleration(Vector3 inAccl)
	{
		Vector3 result = inAccl;
		if (Orientation.GetOrientation() != ScreenOrientation.Landscape)
		{
			if (Orientation.GetOrientation() == ScreenOrientation.LandscapeRight)
			{
				result.x = 0f - inAccl.x;
				result.y = 0f - inAccl.y;
			}
			else if (Orientation.GetOrientation() == ScreenOrientation.Portrait)
			{
				result.x = 0f - inAccl.y;
				result.y = 0f - inAccl.x;
			}
			else if (Orientation.GetOrientation() == ScreenOrientation.PortraitUpsideDown)
			{
				result.x = inAccl.y;
				result.y = inAccl.x;
			}
		}
		return result;
	}
}
