using UnityEngine;

public class UIButtonProcessor : InputProcessor
{
	private InputInfo mInfo;

	public UIButtonProcessor(InputInfo inInfo)
	{
		mInfo = inInfo;
	}

	public bool Update(GameInput inInput)
	{
		if (mInfo._PadButtonUI == null)
		{
			GameObject gameObject = GameObject.Find(mInfo._KAUiPadString);
			if (gameObject != null)
			{
				mInfo._PadButtonUI = gameObject.GetComponent(typeof(KAUIPadButtons)) as KAUIPadButtons;
				if (mInfo._PadButtonUI != null)
				{
					mInfo._PadButtonUI.AddButtonForCheck(mInfo._ButtonName);
				}
				else
				{
					UtDebug.LogError(" @@ KAUIPadButtons not defined for: " + mInfo._KAUiPadString);
				}
			}
		}
		inInput.SetValue(mInfo);
		return inInput._Value != 0f;
	}

	public void LateUpdate(GameInput inInput)
	{
	}

	public bool IsPressed()
	{
		return IsButtonPressed();
	}

	public bool IsUp()
	{
		return IsButtonReleased();
	}

	public bool IsDown()
	{
		return IsButtonClicked();
	}

	private bool IsButtonReleased()
	{
		if (mInfo._PadButtonUI != null && mInfo._PadButtonUI.IsWidgetReleased(mInfo._ButtonName))
		{
			return true;
		}
		return false;
	}

	private bool IsButtonClicked()
	{
		if (mInfo._PadButtonUI != null && mInfo._PadButtonUI.IsWidgetClicked(mInfo._ButtonName))
		{
			return true;
		}
		return false;
	}

	private bool IsButtonPressed()
	{
		if (mInfo._PadButtonUI != null && mInfo._PadButtonUI.IsWidgetPressed(mInfo._ButtonName))
		{
			return true;
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
