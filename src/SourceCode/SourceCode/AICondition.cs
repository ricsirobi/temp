using System;

[Serializable]
public class AICondition
{
	public AIEvaluationType Type;

	public float Value;

	public int Priority = 1;
}
