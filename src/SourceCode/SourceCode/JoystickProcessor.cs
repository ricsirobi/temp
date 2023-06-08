using UnityEngine;

public class JoystickProcessor : InputProcessor
{
	private InputInfo mInfo;

	public JoystickProcessor(InputInfo inInfo)
	{
		mInfo = inInfo;
	}

	public bool Update(GameInput inInput)
	{
		Vector2 joyStickVal = KAInput.GetJoyStickVal(mInfo._Joystick);
		inInput.SetValueFromAxis(joyStickVal, mInfo);
		return inInput._Value != 0f;
	}

	public void LateUpdate(GameInput inInput)
	{
	}

	public bool IsPressed()
	{
		return IsJoystickMoved();
	}

	public bool IsUp()
	{
		return IsJoystickMoved();
	}

	public bool IsDown()
	{
		return IsJoystickMoved();
	}

	private bool IsJoystickMoved()
	{
		Vector2 joyStickVal = KAInput.GetJoyStickVal(mInfo._Joystick);
		bool result = false;
		if (joyStickVal.sqrMagnitude > 0f)
		{
			if (mInfo._Axis == 1 && joyStickVal.x != 0f)
			{
				result = true;
			}
			else if (mInfo._Axis == 4 && joyStickVal.x < 0f)
			{
				result = true;
			}
			else if (mInfo._Axis == 2 && joyStickVal.x > 0f)
			{
				result = true;
			}
			else if (mInfo._Axis == 32 && joyStickVal.y < 0f)
			{
				result = true;
			}
			else if (mInfo._Axis == 16 && joyStickVal.y > 0f)
			{
				result = true;
			}
			else if (mInfo._Axis == 8 && joyStickVal.y != 0f)
			{
				result = true;
			}
			return result;
		}
		return false;
	}

	public void Calibrate()
	{
	}

	public void SetCalibration(float x, float y, float z)
	{
	}
}
