using UnityEngine;

public class AxisProcessor : InputProcessor
{
	private InputInfo mInfo;

	public AxisProcessor(InputInfo inInfo)
	{
		mInfo = inInfo;
	}

	public bool Update(GameInput inInput)
	{
		if (!string.IsNullOrEmpty(mInfo._ButtonName))
		{
			Vector3 zero = Vector3.zero;
			float axis = Input.GetAxis(mInfo._ButtonName);
			if (mInfo._Axis >= 1 && mInfo._Axis <= 4)
			{
				zero.x = axis;
			}
			else if (mInfo._Axis >= 8 && mInfo._Axis <= 32)
			{
				zero.y = axis;
			}
			else if (mInfo._Axis >= 64 && mInfo._Axis <= 256)
			{
				zero.z = axis;
			}
			inInput.SetValueFromAxis(zero, mInfo);
		}
		return inInput._Value != 0f;
	}

	public void LateUpdate(GameInput inInput)
	{
	}

	public bool IsPressed()
	{
		return false;
	}

	public bool IsUp()
	{
		return false;
	}

	public bool IsDown()
	{
		return false;
	}

	public void Calibrate()
	{
	}

	public void SetCalibration(float x, float y, float z)
	{
	}
}
