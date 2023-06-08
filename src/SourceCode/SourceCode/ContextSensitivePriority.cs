using System;

[Serializable]
public class ContextSensitivePriority
{
	public ContextSensitiveStateType _Type;

	public ContextSensitiveState pData { get; set; }

	public ContextSensitivePriority(ContextSensitiveStateType inType)
	{
		_Type = inType;
	}
}
