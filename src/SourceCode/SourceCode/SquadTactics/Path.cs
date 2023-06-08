using System;
using System.Collections.Generic;

namespace SquadTactics;

[Serializable]
public class Path
{
	public float _Cost;

	public List<Node> _Nodes;

	public Path()
	{
		_Cost = -1f;
		_Nodes = new List<Node>();
	}

	public Path(float cost, List<Node> nodes)
	{
		_Cost = cost;
		_Nodes = nodes;
	}

	public Node GetFinalPathNode()
	{
		if (_Nodes == null || _Nodes.Count == 0)
		{
			return null;
		}
		return _Nodes[_Nodes.Count - 1];
	}
}
