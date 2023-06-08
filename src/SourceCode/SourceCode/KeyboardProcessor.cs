using UnityEngine;

public class KeyboardProcessor : InputProcessor
{
	private InputInfo mInfo;

	public KeyboardProcessor(InputInfo inInfo)
	{
		mInfo = inInfo;
	}

	public bool Update(GameInput inInput)
	{
		bool result = false;
		if ((mInfo._KeyCode != 0 && Input.GetKey(mInfo._KeyCode)) || (!string.IsNullOrEmpty(mInfo._KeyName) && Input.GetKey(mInfo._KeyName)))
		{
			inInput.SetValue(mInfo);
			result = true;
		}
		return result;
	}

	public void LateUpdate(GameInput inInput)
	{
	}

	public bool IsPressed()
	{
		bool flag = false;
		if (!string.IsNullOrEmpty(mInfo._KeyName))
		{
			flag = Input.GetKey(mInfo._KeyName);
		}
		if (!flag && mInfo._KeyCode != 0)
		{
			flag = Input.GetKey(mInfo._KeyCode);
		}
		return flag;
	}

	public bool IsUp()
	{
		bool flag = false;
		if (!string.IsNullOrEmpty(mInfo._KeyName))
		{
			flag = Input.GetKeyUp(mInfo._KeyName);
		}
		if (!flag && mInfo._KeyCode != 0)
		{
			flag = Input.GetKeyUp(mInfo._KeyCode);
		}
		return flag;
	}

	public bool IsDown()
	{
		bool flag = false;
		if (!string.IsNullOrEmpty(mInfo._KeyName))
		{
			flag = Input.GetKeyDown(mInfo._KeyName);
		}
		if (!flag && mInfo._KeyCode != 0)
		{
			flag = Input.GetKeyDown(mInfo._KeyCode);
		}
		return flag;
	}

	public void Calibrate()
	{
	}

	public void SetCalibration(float x, float y, float z)
	{
	}
}
