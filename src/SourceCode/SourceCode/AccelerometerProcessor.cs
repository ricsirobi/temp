using UnityEngine;

public class AccelerometerProcessor : InputProcessor
{
	private InputInfo mInfo;

	private Vector3 mInitialAccl = Vector3.zero;

	public void Calibrate()
	{
		mInitialAccl = Input.acceleration;
	}

	public void SetCalibration(float x, float y, float z)
	{
		mInitialAccl = new Vector3(x, y, z);
	}

	public AccelerometerProcessor(InputInfo inInfo)
	{
		mInfo = inInfo;
		Calibrate();
	}

	public bool Update(GameInput inInput)
	{
		Vector3 acceleration = Input.acceleration;
		acceleration.x = 0f;
		acceleration.Normalize();
		Vector3 lhs = mInitialAccl;
		lhs.x = 0f;
		lhs.Normalize();
		Vector3 vector = Vector3.Cross(lhs, acceleration);
		if (vector.sqrMagnitude > 1f)
		{
			vector.Normalize();
		}
		float num = vector.magnitude;
		if (vector.x < 0f)
		{
			num = 0f - num;
		}
		float x = Input.acceleration.x - mInitialAccl.x;
		float z = Input.acceleration.z - mInitialAccl.z;
		inInput.SetValueFromAxis(new Vector3(x, num, z), mInfo);
		return Mathf.Abs(inInput._Value) > 0.01f;
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
}
