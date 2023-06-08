using System.Collections.Generic;
using UnityEngine;

public class KAInputManager : MonoBehaviour
{
	[SerializeField]
	public List<KAInputInfo> _Axes;

	private Vector2 mScrollPos = Vector2.zero;

	private Rect mWindowRect;

	private void Awake()
	{
		base.enabled = false;
		mWindowRect = new Rect(50f, 20f, Screen.width - 200, Screen.height - 100);
		if (!KAInput.pInstance.pIsReady)
		{
			UpdateInputs();
		}
	}

	private void UpdateInputs()
	{
		foreach (KAInputInfo axis in _Axes)
		{
			Axis inAxis = axis._Config._Axis;
			KAInput.AddInput(axis._Name, 0f);
			KAInput.AddScenes(axis._Config._Scenes);
			KAInput.AddPlatforms(axis._Config._Platforms);
			UpdateKeyConfig(axis._Config._Keys, ref inAxis);
			UpdateJoystickConfig(axis._Config._Joystick, ref inAxis);
			UpdateTouchConfig(axis._Config._Touches, ref inAxis);
			UpdateMouseAndAccleConfig(axis._Config._AxisConf, ref inAxis);
			UpdateUIButton(axis._Config._UIConf, ref inAxis);
		}
		KAInput.pInstance.SetState(KAInputState.READY);
	}

	private void UpdateKeyConfig(KeyConfig[] inConfig, ref Axis inAxis)
	{
		foreach (KeyConfig keyConfig in inConfig)
		{
			float minVal = keyConfig._Values._MinVal;
			float maxVal = keyConfig._Values._MaxVal;
			float step = keyConfig._Values._Step;
			if (keyConfig._PrimKey != 0)
			{
				KAInput.AddModifier(minVal, maxVal, InputType.KEYBOARD, keyConfig._PrimKey, (int)inAxis, 0, step);
			}
			if (!string.IsNullOrEmpty(keyConfig._PrimKeyName))
			{
				KAInput.AddModifier(minVal, maxVal, keyConfig._PrimKeyName, (int)inAxis, step);
			}
			if (keyConfig._AltKey != 0)
			{
				KAInput.AddModifier(minVal, maxVal, InputType.KEYBOARD, keyConfig._AltKey, (int)inAxis, 0, step);
			}
			if (!string.IsNullOrEmpty(keyConfig._AltKeyName))
			{
				KAInput.AddModifier(minVal, maxVal, keyConfig._AltKeyName, (int)inAxis, step);
			}
		}
	}

	private void UpdateJoystickConfig(JoystickConfig inConfig, ref Axis inAxis)
	{
		if (inConfig._StickPos != 0)
		{
			KAInput.AddModifier(inConfig._Values._MinVal, inConfig._Values._MaxVal, (int)inAxis, inConfig._StickPos, inConfig._Values._Step, inConfig._ResourcesPath, inConfig._PrefabName);
		}
	}

	private void UpdateTouchConfig(TouchConfig[] inConfig, ref Axis inAxis)
	{
		for (int i = 0; i < inConfig.Length; i++)
		{
			if (inConfig[i]._Touches.Length != 0)
			{
				TouchConfig touchConfig = inConfig[i];
				KAInput.AddModifier(touchConfig._Values._MinVal, touchConfig._Values._MaxVal, inStep: touchConfig._Values._Step, inType: InputType.TOUCH, inKeyCode: KeyCode.None, inAxis: 0, inTouchIds: KAInput.CreateTouchIdFromEnum(touchConfig._Touches));
			}
		}
	}

	private void UpdateMouseAndAccleConfig(AxisConfig[] inConfig, ref Axis inAxis)
	{
		for (int i = 0; i < inConfig.Length; i++)
		{
			if ((!UtPlatform.IsStandAlone() || inConfig[i]._Type != InputType.ACCELEROMETER) && (inConfig[i]._Type == InputType.MOUSE || inConfig[i]._Type == InputType.ACCELEROMETER))
			{
				float minVal = inConfig[i]._Values._MinVal;
				float maxVal = inConfig[i]._Values._MaxVal;
				float step = inConfig[i]._Values._Step;
				KAInput.AddModifier(inConfig[i]._Name, minVal, maxVal, inConfig[i]._Type, (int)inAxis, step);
			}
		}
	}

	private void UpdateUIButton(UIButtonConfig[] inConfig, ref Axis inAxis)
	{
		for (int i = 0; i < inConfig.Length; i++)
		{
			UIButtonConfig uIButtonConfig = inConfig[i];
			if (uIButtonConfig._Interface != null)
			{
				for (int j = 0; j < uIButtonConfig._ButtonName.Length; j++)
				{
					KAInput.AddModifier(uIButtonConfig._Values._MinVal, uIButtonConfig._Values._MaxVal, uIButtonConfig._Interface, uIButtonConfig._ButtonName[i], uIButtonConfig._Values._Step);
				}
			}
			if (!string.IsNullOrEmpty(uIButtonConfig._InterfaceName))
			{
				for (int k = 0; k < uIButtonConfig._ButtonName.Length; k++)
				{
					KAInput.AddModifier(uIButtonConfig._Values._MinVal, uIButtonConfig._Values._MaxVal, uIButtonConfig._InterfaceName, uIButtonConfig._ButtonName[i], uIButtonConfig._Values._Step);
				}
			}
		}
	}

	private void ShowConf(int inWindowId)
	{
		Rect position = new Rect(100f, 50f, 80f, 20f);
		position.x = mWindowRect.x + 100f;
		position.y = mWindowRect.y - 20f;
		if (GUI.Button(position, "Update"))
		{
			UpdateInputs();
		}
		position.x += 400f;
		if (GUI.Button(position, "X"))
		{
			base.enabled = false;
		}
		mScrollPos = GUILayout.BeginScrollView(mScrollPos);
		int num = 0;
		foreach (KAInputInfo axis in _Axes)
		{
			if (axis._Draw = GUILayout.Toggle(axis._Draw, axis._Name))
			{
				DrawKeyConfig(axis._Config._Keys);
				DrawJoystickConfig(axis._Config._Joystick);
				DrawTouchConfig(axis._Config._Touches);
				DrawMouseAndAccleConfig(axis._Config._AxisConf);
				num += 100;
			}
		}
		GUILayout.EndScrollView();
	}

	private void DrawKeyConfig(KeyConfig[] inConfig)
	{
		for (int i = 0; i < inConfig.Length; i++)
		{
			string text = "KeyConf : ";
			if (inConfig[i]._PrimKey != 0)
			{
				text += inConfig[i]._PrimKey;
			}
			if (!string.IsNullOrEmpty(inConfig[i]._PrimKeyName))
			{
				text = text + " | " + inConfig[i]._PrimKeyName;
			}
			if (inConfig[i]._AltKey != 0)
			{
				text = text + " | " + inConfig[i]._AltKey;
			}
			if (!string.IsNullOrEmpty(inConfig[i]._AltKeyName))
			{
				text = text + " | " + inConfig[i]._AltKeyName;
			}
			GUILayout.Box(text);
			UtUtilities.DrawPublicProperties(inConfig[i]._Values);
		}
	}

	private void DrawJoystickConfig(JoystickConfig inConfig)
	{
		if (inConfig._StickPos != 0)
		{
			GUILayout.Box("Joystick : " + inConfig._StickPos);
			UtUtilities.DrawPublicProperties(inConfig._Values);
		}
	}

	private void DrawTouchConfig(TouchConfig[] inConfig)
	{
		for (int i = 0; i < inConfig.Length; i++)
		{
			if (inConfig[i]._Touches.Length != 0)
			{
				string text = " Touch: ";
				for (int j = 0; j < inConfig[i]._Touches.Length; j++)
				{
					text += inConfig[i]._Touches[j];
				}
				GUILayout.Box(text);
				UtUtilities.DrawPublicProperties(inConfig[i]._Values);
			}
		}
	}

	private void DrawMouseAndAccleConfig(AxisConfig[] inConfig)
	{
		for (int i = 0; i < inConfig.Length; i++)
		{
			if ((!UtPlatform.IsStandAlone() || inConfig[i]._Type != InputType.ACCELEROMETER) && (inConfig[i]._Type == InputType.MOUSE || inConfig[i]._Type == InputType.ACCELEROMETER))
			{
				GUILayout.Box("MouseAndAccl: " + inConfig[i]._Type);
				UtUtilities.DrawPublicProperties(inConfig[i]._Values);
			}
		}
	}
}
