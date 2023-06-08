using UnityEngine;

public class InputInfo
{
	public float _MinVal;

	public float _MaxVal;

	public float _Step;

	public InputType _Type;

	public int _Axis;

	public int _TouchEvents;

	public JoyStickPos _Joystick;

	public string _JoystickPath;

	public string _JoystickPrefabName;

	public KeyCode _KeyCode;

	public string _KeyName;

	public string _KAUiPadString;

	public KAUIPadButtons _PadButtonUI;

	public string _ButtonName;

	private InputProcessor mProcessor;

	public InputProcessor pProcessor => mProcessor;

	public InputInfo(float inMinVal, float inMaxVal, InputType inType, KeyCode inKeyCode, int inAxis, int inTouchIds, float inStep)
	{
		_MinVal = inMinVal;
		_MaxVal = inMaxVal;
		_Step = inStep;
		_Type = inType;
		_Axis = inAxis;
		_KeyCode = inKeyCode;
		_TouchEvents = inTouchIds;
		CreateProcessor();
	}

	public InputInfo(float inMinVal, float inMaxVal, InputType inType, string inKeyName, int inAxis, float inStep)
	{
		_MinVal = inMinVal;
		_MaxVal = inMaxVal;
		_Step = inStep;
		_Type = inType;
		_Axis = inAxis;
		_KeyName = inKeyName;
		CreateProcessor();
	}

	public InputInfo(float inMinVal, float inMaxVal, JoyStickPos inJoyStickPos, int inAxis, float inStep, string inAssetPath, string inAssetName)
	{
		_MinVal = inMinVal;
		_MaxVal = inMaxVal;
		_Axis = inAxis;
		_Joystick = inJoyStickPos;
		_JoystickPath = inAssetPath;
		_JoystickPrefabName = inAssetName;
		_Type = InputType.JOYSTICK;
		_KeyCode = KeyCode.None;
		CreateProcessor();
	}

	public InputInfo(float inMinVal, float inMaxVal, KAUIPadButtons inUI, string inButtonName, float inStep)
	{
		_PadButtonUI = inUI;
		_MinVal = inMinVal;
		_MaxVal = inMaxVal;
		_Axis = 0;
		_Type = InputType.UI_BUTTONS;
		_KeyCode = KeyCode.None;
		_ButtonName = inButtonName;
		_KAUiPadString = "";
		if (_PadButtonUI != null)
		{
			_PadButtonUI.AddButtonForCheck(inButtonName);
		}
		CreateProcessor();
	}

	public InputInfo(float inMinVal, float inMaxVal, string inUIName, string inButtonName, float inStep)
	{
		_PadButtonUI = null;
		_MinVal = inMinVal;
		_MaxVal = inMaxVal;
		_Axis = 0;
		_Type = InputType.UI_BUTTONS;
		_KeyCode = KeyCode.None;
		_ButtonName = inButtonName;
		_KAUiPadString = inUIName;
		if (_PadButtonUI != null)
		{
			_PadButtonUI.AddButtonForCheck(inButtonName);
		}
		CreateProcessor();
	}

	public InputInfo(float inMinVal, float inMaxVal, InputType inType, int inAxis, string inName, float inStep)
	{
		_PadButtonUI = null;
		_MinVal = inMinVal;
		_MaxVal = inMaxVal;
		_Axis = inAxis;
		_Type = inType;
		_KeyCode = KeyCode.None;
		_ButtonName = inName;
		CreateProcessor();
	}

	public void Clear()
	{
		_PadButtonUI = null;
	}

	private void CreateProcessor()
	{
		if (_Type == InputType.KEYBOARD)
		{
			mProcessor = new KeyboardProcessor(this);
		}
		else if (_Type == InputType.TOUCH)
		{
			mProcessor = new TouchProcessor(this);
		}
		else if (_Type == InputType.JOYSTICK)
		{
			mProcessor = new JoystickProcessor(this);
		}
		else if (_Type == InputType.ACCELEROMETER)
		{
			mProcessor = new AccelerometerProcessor(this);
		}
		else if (_Type == InputType.UI_BUTTONS)
		{
			mProcessor = new UIButtonProcessor(this);
		}
		else if (_Type == InputType.MOUSE)
		{
			mProcessor = new AxisProcessor(this);
		}
	}
}
