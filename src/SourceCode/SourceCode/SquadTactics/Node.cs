using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class Node : MonoBehaviour
{
	public int _XPosition;

	public int _YPosition;

	public bool _Walkable;

	public bool _Occupied;

	public bool _Blocker;

	public float _MovementFactor = 1f;

	public Vector3 _WorldPosition;

	public List<Node> _Neighbors;

	public Character _CharacterOnNode;

	public Node _ParentNode;

	public float _GCost;

	public float _HCost;

	private Renderer mCachedRenderer;

	public float pFCost => _GCost + _HCost;

	public void Initialize(bool walkable, int xPosition, int yPosition, Vector3 worldPosition, float movementFactor)
	{
		mCachedRenderer = GetComponent<Renderer>();
		mCachedRenderer.enabled = false;
		_Walkable = walkable;
		_XPosition = xPosition;
		_YPosition = yPosition;
		_WorldPosition = worldPosition;
		_MovementFactor = movementFactor;
		_Blocker = (walkable ? (_Blocker = false) : (_Blocker = true));
	}

	public void ResetNodeCosts()
	{
		_GCost = 0f;
		_HCost = 0f;
		_ParentNode = null;
	}

	public void SetNodeMovementMaterial(Material movementMaterial, bool active)
	{
		mCachedRenderer.material = movementMaterial;
		mCachedRenderer.enabled = active;
	}
}
