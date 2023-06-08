using System;

[Serializable]
public class FarmItemContextData
{
	public bool _IsBuildMode;

	public ContextSensitiveState _ContextSensitiveState;

	public void CopyTo(ref FarmItemContextData farmItemContextData)
	{
		farmItemContextData = new FarmItemContextData();
		farmItemContextData._IsBuildMode = _IsBuildMode;
		farmItemContextData._ContextSensitiveState = new ContextSensitiveState();
		if (_ContextSensitiveState == null)
		{
			return;
		}
		farmItemContextData._ContextSensitiveState._MenuType = _ContextSensitiveState._MenuType;
		farmItemContextData._ContextSensitiveState._Enable3DUI = _ContextSensitiveState._Enable3DUI;
		farmItemContextData._ContextSensitiveState._OffsetPos = _ContextSensitiveState._OffsetPos;
		farmItemContextData._ContextSensitiveState._ShowCloseButton = _ContextSensitiveState._ShowCloseButton;
		farmItemContextData._ContextSensitiveState._UIScale = _ContextSensitiveState._UIScale;
		int num = 0;
		if (_ContextSensitiveState._CurrentContextNamesList != null && _ContextSensitiveState._CurrentContextNamesList.Length != 0)
		{
			num = _ContextSensitiveState._CurrentContextNamesList.Length;
		}
		farmItemContextData._ContextSensitiveState._CurrentContextNamesList = new string[num];
		int num2 = 0;
		if (_ContextSensitiveState._CurrentContextNamesList != null && _ContextSensitiveState._CurrentContextNamesList.Length != 0)
		{
			string[] currentContextNamesList = _ContextSensitiveState._CurrentContextNamesList;
			foreach (string text in currentContextNamesList)
			{
				farmItemContextData._ContextSensitiveState._CurrentContextNamesList[num2] = text;
				num2++;
			}
		}
	}

	public FarmItemContextData()
	{
		_ContextSensitiveState = new ContextSensitiveState();
	}
}
