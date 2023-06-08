using System;

namespace SquadTactics;

[Serializable]
public class StepTargetCoordinate
{
	public int _StepIndex;

	public GridNode _Node;

	public bool _DisableMove;
}
