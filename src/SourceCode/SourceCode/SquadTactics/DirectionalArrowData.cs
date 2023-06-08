using System;

namespace SquadTactics;

[Serializable]
public class DirectionalArrowData
{
	public int _StepIndex;

	public GridNode _StartNode;

	public GridNode _EndNode;

	public float _Scale;
}
