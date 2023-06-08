using System;
using System.Collections.Generic;
using UnityEngine;

public class GameInput
{
	public int _Idx;

	public float _DefaultVal;

	public string _Name;

	public float _Value;

	private int mDisabledInputType;

	private bool mIsExclusionGroupType = true;

	private string mScenesInGroup;

	private List<InputInfo> mModifiers = new List<InputInfo>();

	private int mPlatformExclusions;

	private const uint LOG_MASK = 31u;

	public List<InputInfo> pModifiers => mModifiers;

	public GameInput(int inId, float inDefaultVal)
	{
		_Idx = inId;
		_DefaultVal = inDefaultVal;
	}

	public GameInput(string inName, float inDefaultVal)
	{
		_Idx = -1;
		_Name = inName;
		_DefaultVal = inDefaultVal;
	}

	public void AddScenes(SceneGroup inScenes)
	{
		mIsExclusionGroupType = inScenes._GroupType == SceneGroupType.EXCLUDE;
		string[] scenes = inScenes._Scenes;
		foreach (string text in scenes)
		{
			mScenesInGroup = mScenesInGroup + text.ToLower() + ",";
		}
	}

	public void AddPlatforms(PlatformGroup inPlatforms)
	{
		bool flag = inPlatforms._GroupType == SceneGroupType.EXCLUDE;
		mPlatformExclusions = ((!flag) ? 255 : 0);
		foreach (InputTypePlatformMap item in inPlatforms._InputTypePlatformMap)
		{
			bool flag2 = item._Platforms.Exists((RuntimePlatform p) => p == Application.platform);
			if (flag)
			{
				mPlatformExclusions |= (int)(flag2 ? item._InputType : InputType.NONE);
			}
			else
			{
				mPlatformExclusions &= (int)(flag2 ? (~item._InputType) : InputType.ALL);
			}
		}
	}

	public void ValidateInputsForScene(string _sceneName)
	{
		if (string.IsNullOrEmpty(mScenesInGroup))
		{
			return;
		}
		if (mScenesInGroup.Contains(_sceneName))
		{
			if (mIsExclusionGroupType)
			{
				DisableInputType(InputType.ALL);
			}
			else
			{
				EnableInputType(InputType.ALL);
			}
		}
		else if (mIsExclusionGroupType)
		{
			EnableInputType(InputType.ALL);
		}
		else
		{
			DisableInputType(InputType.ALL);
		}
	}

	public void ValidateInputsForPlatform()
	{
		foreach (int value in Enum.GetValues(typeof(InputType)))
		{
			if (value == 255)
			{
				if (mPlatformExclusions == 255)
				{
					DisableInputType((InputType)value);
				}
			}
			else if ((value & mPlatformExclusions) != 0)
			{
				DisableInputType((InputType)value);
			}
		}
	}

	public bool PlatformIncludesInputType(InputType inType)
	{
		return ((uint)inType & (uint)mPlatformExclusions) == 0;
	}

	public void AddModifier(float inMinVal, float inMaxVal, InputType inType, KeyCode inKeyCode, int inAxis, int inTouchIds, float inStep)
	{
		mModifiers.Add(new InputInfo(inMinVal, inMaxVal, inType, inKeyCode, inAxis, inTouchIds, inStep));
	}

	public void AddModifier(float inMinVal, float inMaxVal, InputType inType, string inKeyName, int inAxis, float inStep)
	{
		mModifiers.Add(new InputInfo(inMinVal, inMaxVal, inType, inKeyName, inAxis, inStep));
	}

	public void AddModifier(JoyStickPos inJoyStickPos, float inMinVal, float inMaxVal, int inAxis, float inStep, string inAssetPath, string inAssetName)
	{
		mModifiers.Add(new InputInfo(inMinVal, inMaxVal, inJoyStickPos, inAxis, inStep, inAssetPath, inAssetName));
	}

	public void AddModifier(float inMinVal, float inMaxVal, KAUIPadButtons inUI, string inButtonName, float inStep)
	{
		mModifiers.Add(new InputInfo(inMinVal, inMaxVal, inUI, inButtonName, inStep));
	}

	public void AddModifier(float inMinVal, float inMaxVal, string inUIName, string inButtonName, float inStep)
	{
		mModifiers.Add(new InputInfo(inMinVal, inMaxVal, inUIName, inButtonName, inStep));
	}

	public void AddModifier(float inMinVal, float inMaxVal, InputType inType, int inAxis, string inName, float inStep)
	{
		mModifiers.Add(new InputInfo(inMinVal, inMaxVal, inType, inAxis, inName, inStep));
	}

	public void Calibrate()
	{
		for (int i = 0; i < mModifiers.Count; i++)
		{
			InputInfo inputInfo = mModifiers[i];
			if (inputInfo.pProcessor != null && (int)((uint)mDisabledInputType & (uint)inputInfo._Type) <= 0)
			{
				inputInfo.pProcessor.Calibrate();
			}
		}
	}

	public bool IsPressed()
	{
		bool flag = false;
		for (int i = 0; i < mModifiers.Count; i++)
		{
			InputInfo inputInfo = mModifiers[i];
			if (inputInfo.pProcessor != null && (int)((uint)mDisabledInputType & (uint)inputInfo._Type) <= 0)
			{
				flag = inputInfo.pProcessor.IsPressed();
				if (flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	public bool IsUp()
	{
		bool flag = false;
		int i = 0;
		for (int count = mModifiers.Count; i < count; i++)
		{
			InputInfo inputInfo = mModifiers[i];
			if (inputInfo.pProcessor != null && (int)((uint)mDisabledInputType & (uint)inputInfo._Type) <= 0)
			{
				flag = inputInfo.pProcessor.IsUp();
				if (flag)
				{
					break;
				}
				_Value = 0f;
			}
		}
		return flag;
	}

	public bool IsDown()
	{
		bool flag = false;
		for (int i = 0; i < mModifiers.Count; i++)
		{
			InputInfo inputInfo = mModifiers[i];
			if (inputInfo.pProcessor != null && (int)((uint)mDisabledInputType & (uint)inputInfo._Type) <= 0)
			{
				flag = inputInfo.pProcessor.IsDown();
				if (flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	public void Update()
	{
		int count = mModifiers.Count;
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			InputInfo inputInfo = mModifiers[i];
			if ((int)((uint)mDisabledInputType & (uint)inputInfo._Type) <= 0 && inputInfo.pProcessor != null)
			{
				flag = inputInfo.pProcessor.Update(this);
				if (flag)
				{
					break;
				}
			}
		}
		if (!flag)
		{
			_Value = 0f;
		}
	}

	public void LateUpdate()
	{
		int count = mModifiers.Count;
		for (int i = 0; i < count; i++)
		{
			InputInfo inputInfo = mModifiers[i];
			if (inputInfo.pProcessor != null)
			{
				inputInfo.pProcessor.LateUpdate(this);
			}
		}
	}

	public float GetValue()
	{
		return _Value;
	}

	public InputInfo GetInputOfType(InputType inType)
	{
		InputInfo result = null;
		for (int i = 0; i < mModifiers.Count; i++)
		{
			InputInfo inputInfo = mModifiers[i];
			if (inputInfo._Type == inType)
			{
				result = inputInfo;
			}
		}
		return result;
	}

	public void Clear()
	{
		foreach (InputInfo mModifier in mModifiers)
		{
			mModifier.Clear();
		}
		mModifiers.Clear();
	}

	public void EnableInputType(InputType inType)
	{
		if (mIsExclusionGroupType && mScenesInGroup != null && mScenesInGroup.Contains(RsResourceManager.pCurrentLevel.ToLower()))
		{
			UtDebug.Log(" >>> " + RsResourceManager.pCurrentLevel + " excludes " + _Name + " <<<", 31u);
			return;
		}
		if (!mIsExclusionGroupType && mScenesInGroup != null && !mScenesInGroup.Contains(RsResourceManager.pCurrentLevel.ToLower()))
		{
			UtDebug.Log(" >>> " + RsResourceManager.pCurrentLevel + " doesn't include " + _Name + " <<<", 31u);
			return;
		}
		if (((uint)inType & (uint)mPlatformExclusions) != 0 && inType != InputType.ALL)
		{
			DisableInputType(inType);
			UtDebug.Log(" >>> " + Application.platform.ToString() + " platform doesn't include " + _Name + " <<<", 31u);
			return;
		}
		mDisabledInputType &= (int)(~inType);
		mDisabledInputType |= mPlatformExclusions;
		if ((0x10u & (uint)mDisabledInputType) != 0)
		{
			return;
		}
		GameObject gameObject = GameObject.Find(_Name);
		if (gameObject != null)
		{
			KAButton component = gameObject.GetComponent<KAButton>();
			if (component != null)
			{
				component.SetVisibility(inVisible: true);
			}
		}
	}

	public void DisableInputType(InputType inType)
	{
		mDisabledInputType |= (int)inType;
		mDisabledInputType |= mPlatformExclusions;
		if ((0x10 & mDisabledInputType) == 0)
		{
			return;
		}
		GameObject gameObject = GameObject.Find(_Name);
		if (gameObject != null)
		{
			KAButton component = gameObject.GetComponent<KAButton>();
			if (component != null)
			{
				component.SetVisibility(inVisible: false);
			}
		}
	}

	public bool GetInputTypeState(InputType inType)
	{
		return ((uint)mDisabledInputType & (uint)inType) == 0;
	}

	public void SetValue(InputInfo inInfo)
	{
		_Value += Time.deltaTime * inInfo._Step;
		if (inInfo._MaxVal > inInfo._MinVal)
		{
			_Value = Mathf.Clamp(_Value, inInfo._MinVal, inInfo._MaxVal);
		}
		else
		{
			_Value = Mathf.Clamp(_Value, inInfo._MaxVal, inInfo._MinVal);
		}
	}

	public void SetValueFromAxisForJoystick(Vector3 inAxisVal, InputInfo inInfo)
	{
		Vector3 vector = inAxisVal;
		vector.x = Mathf.Clamp(vector.x, inInfo._MinVal, inInfo._MaxVal);
		vector.y = Mathf.Clamp(vector.y, inInfo._MinVal, inInfo._MaxVal);
		float num = (vector.x + 1f) * 0.5f;
		vector.x = inInfo._MinVal + (inInfo._MaxVal - inInfo._MinVal) * num;
		num = (vector.y + 1f) * 0.5f;
		vector.y = inInfo._MinVal + (inInfo._MaxVal - inInfo._MinVal) * num;
		num = (vector.z + 1f) * 0.5f;
		vector.z = inInfo._MinVal + (inInfo._MaxVal - inInfo._MinVal) * num;
		if (inInfo._Axis == 1)
		{
			_Value = vector.x;
		}
		else if (inInfo._Axis == 4 && vector.x < 0f)
		{
			_Value = vector.x;
		}
		else if (inInfo._Axis == 2 && vector.x > 0f)
		{
			_Value = vector.x;
		}
		else if (inInfo._Axis == 32 && vector.y < 0f)
		{
			_Value = vector.y;
		}
		else if (inInfo._Axis == 16 && vector.y > 0f)
		{
			_Value = vector.y;
		}
		else if (inInfo._Axis == 8)
		{
			_Value = vector.y;
		}
		else if (inInfo._Axis == 64)
		{
			_Value = vector.z;
		}
	}

	public void SetValueFromAxis(Vector3 inAxisVal, InputInfo inInfo)
	{
		Vector3 vector = inAxisVal;
		float num = (vector.x + 1f) * 0.5f;
		vector.x = inInfo._MinVal + (inInfo._MaxVal - inInfo._MinVal) * num;
		num = (vector.y + 1f) * 0.5f;
		vector.y = inInfo._MinVal + (inInfo._MaxVal - inInfo._MinVal) * num;
		num = (vector.z + 1f) * 0.5f;
		vector.z = inInfo._MinVal + (inInfo._MaxVal - inInfo._MinVal) * num;
		if (inInfo._Axis == 1)
		{
			_Value = vector.x;
		}
		else if (inInfo._Axis == 4 && vector.x < 0f)
		{
			_Value = vector.x;
		}
		else if (inInfo._Axis == 2 && vector.x > 0f)
		{
			_Value = vector.x;
		}
		else if (inInfo._Axis == 32 && vector.y < 0f)
		{
			_Value = vector.y;
		}
		else if (inInfo._Axis == 16 && vector.y > 0f)
		{
			_Value = vector.y;
		}
		else if (inInfo._Axis == 8)
		{
			_Value = vector.y;
		}
		else if (inInfo._Axis == 64)
		{
			_Value = vector.z;
		}
	}

	public void ShowInput(bool inShow)
	{
		for (int i = 0; i < mModifiers.Count; i++)
		{
			InputInfo inputInfo = mModifiers[i];
			if (inputInfo._Type == InputType.UI_BUTTONS && inputInfo._PadButtonUI != null)
			{
				inputInfo._PadButtonUI.SetVisibility(inShow);
			}
		}
	}
}
