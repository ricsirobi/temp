using System;
using UnityEngine;

[Serializable]
public class ContextSensitiveState : ICloneable
{
	public ContextSensitiveStateType _MenuType;

	public string[] _CurrentContextNamesList;

	public bool _Enable3DUI;

	public Vector3 _OffsetPos;

	public Vector3 _UIScale = Vector3.one;

	public bool _ShowCloseButton;

	public object Clone()
	{
		return (ContextSensitiveState)MemberwiseClone();
	}
}
