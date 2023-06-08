using System;
using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class Grid : MonoBehaviour
{
	public GameObject NodePrefab;

	public LayerMask UnwalkableMask;

	public Vector2 GridWorldSize;

	public float NodeSize;

	public float _LineRendererOffset = 1f;

	public LineRenderer _LineRenderer;

	public LineRenderer _PathLineRenderer;

	public GameObject _PathEnd;

	public GameObject _Attack;

	public Node[,] CurrentGrid;

	private BoxCollider gridCollider;

	private float nodeDiameter;

	private int gridSizeX;

	private int gridSizeY;

	public float _NodeYOffset;

	public LayerMask NodeLayer;

	private List<LineRenderer> mMovesLineRenderers = new List<LineRenderer>();

	public void CreateGrid()
	{
		nodeDiameter = NodeSize * 2f;
		gridSizeX = Mathf.RoundToInt(GridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(GridWorldSize.y / nodeDiameter);
		CurrentGrid = new Node[gridSizeX, gridSizeY];
		Vector3 vector = base.transform.position - Vector3.left * GridWorldSize.x / 2f - Vector3.back * GridWorldSize.y / 2f;
		for (int i = 0; i < gridSizeX; i++)
		{
			for (int j = 0; j < gridSizeY; j++)
			{
				Vector3 vector2 = vector + Vector3.left * ((float)i * nodeDiameter + NodeSize) + Vector3.back * ((float)j * nodeDiameter + NodeSize);
				GameObject obj = UnityEngine.Object.Instantiate(NodePrefab, vector2, Quaternion.identity, base.transform);
				obj.transform.position += new Vector3(0f, _NodeYOffset, 0f);
				obj.transform.localScale = new Vector3(1f * (nodeDiameter - 0.1f), 0.1f, 1f * (nodeDiameter - 0.1f));
				obj.name = $"Node_{i}, {j}";
				bool flag = !Physics.CheckSphere(vector2, NodeSize, UnwalkableMask);
				float movementFactor = 1f;
				if (!flag)
				{
					Collider[] array = Physics.OverlapSphere(vector2, NodeSize, UnwalkableMask);
					if (array.Length != 0 && array[0] != null)
					{
						MoveObstacle component = array[0].GetComponent<MoveObstacle>();
						if (component != null)
						{
							movementFactor = component._MovementFactor;
							flag = true;
							Debug.Log($"Node_{i}, {j}" + "had movement factor set to: " + movementFactor);
						}
					}
				}
				Node component2 = obj.GetComponent<Node>();
				component2.Initialize(flag, i, j, vector2, movementFactor);
				CurrentGrid[i, j] = component2;
			}
		}
		SetNodeNeighbors();
		gridCollider = base.gameObject.GetComponent<BoxCollider>();
		if (gridCollider != null)
		{
			gridCollider.size = new Vector3(GridWorldSize.x, 1f, GridWorldSize.y);
		}
	}

	private void SetNodeNeighbors()
	{
		for (int i = 0; i < gridSizeX; i++)
		{
			for (int j = 0; j < gridSizeY; j++)
			{
				CurrentGrid[i, j]._Neighbors = GetNodeNeighbors(CurrentGrid[i, j]);
			}
		}
	}

	public Node GetNodeByPosition(int xPosition, int yPosition)
	{
		if (xPosition >= gridSizeX || yPosition >= gridSizeY)
		{
			Debug.LogErrorFormat("Attempting to get NodeFromGrid with bounds that are outside of the grid!  Grid size is [{0}x{1}] and passed position was [{2},{3}]", gridSizeX, gridSizeY, xPosition, yPosition);
			return null;
		}
		return CurrentGrid[xPosition, yPosition];
	}

	public Node GetNodeFromHitPoint(Vector3 point)
	{
		Vector3 vector = base.transform.InverseTransformPoint(point);
		float value = (0f - vector.x + GridWorldSize.x / 2f) / GridWorldSize.x;
		float value2 = (0f - vector.z + GridWorldSize.y / 2f) / GridWorldSize.y;
		value = Mathf.Clamp01(value);
		value2 = Mathf.Clamp01(value2);
		int num = (int)((float)gridSizeX * value);
		int num2 = (int)((float)gridSizeY * value2);
		return CurrentGrid[num, num2];
	}

	public List<Node> GetNodeNeighbors(Node node)
	{
		List<Node> list = new List<Node>();
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (i != 0 || j != 0)
				{
					int num = node._XPosition + i;
					int num2 = node._YPosition + j;
					if (num >= 0 && num < gridSizeX && num2 >= 0 && num2 < gridSizeY)
					{
						list.Add(CurrentGrid[num, num2]);
					}
				}
			}
		}
		return list;
	}

	public float GetDistanceBetweenNodes(Node nodeA, Node nodeB)
	{
		int num = Mathf.Abs(nodeA._XPosition - nodeB._XPosition);
		int num2 = Mathf.Abs(nodeA._YPosition - nodeB._YPosition);
		if (num > num2)
		{
			return GameManager.pInstance._DiagonalMoveCost * (float)num2 + (float)(GameManager.pInstance._NormalMoveCost * (num - num2));
		}
		return GameManager.pInstance._DiagonalMoveCost * (float)num + (float)(GameManager.pInstance._NormalMoveCost * (num2 - num));
	}

	public void UpdateCurrentValidMoves(ref List<Node> validMoves, Node startNode, Character character)
	{
		if (CurrentGrid == null)
		{
			return;
		}
		if (_PathLineRenderer != null)
		{
			_PathLineRenderer.enabled = false;
		}
		if (_PathEnd != null)
		{
			_PathEnd.SetActive(value: false);
		}
		if (_Attack != null)
		{
			_Attack.SetActive(value: false);
		}
		for (int i = 0; i < gridSizeX; i++)
		{
			for (int j = 0; j < gridSizeY; j++)
			{
				if (startNode != CurrentGrid[i, j])
				{
					Path path = GetPath(startNode, CurrentGrid[i, j], character);
					if (path != null && path._Cost <= character.pCharacterData._Stats._Movement.pCurrentValue)
					{
						validMoves.Add(CurrentGrid[i, j]);
					}
				}
			}
		}
	}

	public void ShowCurrentValidMoves(bool show, Character character = null, Material material = null)
	{
		if (!(_LineRenderer != null) || mMovesLineRenderers == null)
		{
			return;
		}
		if (!show)
		{
			foreach (LineRenderer mMovesLineRenderer in mMovesLineRenderers)
			{
				mMovesLineRenderer.enabled = false;
			}
			if (_PathLineRenderer != null)
			{
				_PathLineRenderer.enabled = false;
			}
			if (_PathEnd != null)
			{
				_PathEnd.SetActive(value: false);
			}
			if (_Attack != null)
			{
				_Attack.SetActive(value: false);
			}
		}
		else
		{
			if (!(character != null))
			{
				return;
			}
			List<Node> list = new List<Node>(character.pCurrentDisplayMoveNodes);
			list.Add(character._CurrentNode);
			if (list.Count <= 0)
			{
				return;
			}
			List<LineSegment> list2 = new List<LineSegment>();
			foreach (Node item2 in list)
			{
				Vector3 worldPosition = item2._WorldPosition;
				worldPosition.x -= NodeSize;
				worldPosition.y += _LineRendererOffset;
				worldPosition.z += NodeSize;
				Vector3 worldPosition2 = item2._WorldPosition;
				worldPosition2.x += NodeSize;
				worldPosition2.y += _LineRendererOffset;
				worldPosition2.z += NodeSize;
				Vector3 worldPosition3 = item2._WorldPosition;
				worldPosition3.x -= NodeSize;
				worldPosition3.y += _LineRendererOffset;
				worldPosition3.z -= NodeSize;
				Vector3 worldPosition4 = item2._WorldPosition;
				worldPosition4.x += NodeSize;
				worldPosition4.y += _LineRendererOffset;
				worldPosition4.z -= NodeSize;
				LineSegment item = new LineSegment(worldPosition3, worldPosition);
				if (list2.Contains(item))
				{
					list2.Remove(item);
				}
				else
				{
					list2.Add(item);
				}
				item = new LineSegment(worldPosition3, worldPosition4);
				if (list2.Contains(item))
				{
					list2.Remove(item);
				}
				else
				{
					list2.Add(item);
				}
				item = new LineSegment(worldPosition4, worldPosition2);
				if (list2.Contains(item))
				{
					list2.Remove(item);
				}
				else
				{
					list2.Add(item);
				}
				item = new LineSegment(worldPosition, worldPosition2);
				if (list2.Contains(item))
				{
					list2.Remove(item);
				}
				else
				{
					list2.Add(item);
				}
			}
			int num = 0;
			while (list2.Count > 0)
			{
				List<Vector3> list3 = SortPoints2(list2);
				if (mMovesLineRenderers.Count <= num)
				{
					mMovesLineRenderers.Add(UnityEngine.Object.Instantiate(_LineRenderer.gameObject).GetComponent<LineRenderer>());
				}
				mMovesLineRenderers[num].positionCount = list3.Count;
				mMovesLineRenderers[num].SetPositions(list3.ToArray());
				mMovesLineRenderers[num].enabled = true;
				if (material != null)
				{
					mMovesLineRenderers[num].material = material;
				}
				num++;
			}
		}
	}

	private List<Vector3> SortPoints(List<LineSegment> lines)
	{
		List<Vector3> list = new List<Vector3>();
		list.Add(lines[0]._Start);
		list.Add(lines[0]._End);
		Vector3 currentPoint = lines[0]._End;
		lines.RemoveAt(0);
		bool flag = false;
		while (!flag)
		{
			LineSegment lineSegment = lines.Find((LineSegment x) => x._Start == currentPoint);
			if (lineSegment != null)
			{
				currentPoint = lineSegment._End;
				list.Add(lineSegment._End);
				lines.Remove(lineSegment);
				if (currentPoint == list[0])
				{
					flag = true;
				}
				continue;
			}
			lineSegment = lines.Find((LineSegment x) => x._End == currentPoint);
			if (lineSegment != null)
			{
				currentPoint = lineSegment._Start;
				list.Add(lineSegment._Start);
				lines.Remove(lineSegment);
				if (currentPoint == list[0])
				{
					flag = true;
				}
			}
		}
		return list;
	}

	private List<Vector3> SortPoints2(List<LineSegment> lines)
	{
		List<Vector3> list = new List<Vector3>();
		list.Add(lines[0]._Start);
		list.Add(lines[0].Mid());
		list.Add(lines[0]._End);
		Vector3 currentPoint = lines[0]._End;
		lines.RemoveAt(0);
		bool flag = false;
		while (!flag)
		{
			LineSegment lineSegment = lines.Find((LineSegment x) => x._Start == currentPoint);
			if (lineSegment != null)
			{
				list.Add(lineSegment.Mid());
				currentPoint = lineSegment._End;
				list.Add(lineSegment._End);
				lines.Remove(lineSegment);
				if (currentPoint == list[0])
				{
					flag = true;
				}
				continue;
			}
			lineSegment = lines.Find((LineSegment x) => x._End == currentPoint);
			if (lineSegment != null)
			{
				list.Add(lineSegment.Mid());
				currentPoint = lineSegment._Start;
				list.Add(lineSegment._Start);
				lines.Remove(lineSegment);
				if (currentPoint == list[0])
				{
					flag = true;
				}
			}
		}
		for (int i = 0; i < list.Count - 1; i++)
		{
			Vector3 vector = list[i];
			Vector3 vector2 = list[i + 1];
			Vector3 vector3 = ((i + 2 == list.Count) ? list[1] : list[i + 2]);
			if ((vector.x != vector2.x || vector2.x != vector3.x) && (vector.z != vector2.z || vector2.z != vector3.z))
			{
				List<Vector3> list2 = new List<Vector3>();
				Vector3 vector4 = vector3 - (vector2 - vector);
				for (int j = 0; j <= 90; j += 15)
				{
					Vector3 item = Vector3.Slerp(vector - vector4, vector3 - vector4, (float)j / 90f) + vector4;
					list2.Add(item);
				}
				list.RemoveAt(i + 1);
				list.InsertRange(i + 1, list2);
				i += list2.Count;
				if (vector2 == list[0])
				{
					list.RemoveAt(0);
				}
			}
		}
		return list;
	}

	public void GetCurrentValidAttackPaths(ref List<Path> validMoves, Node targetNode, Character character)
	{
		for (int i = 0; i < gridSizeX; i++)
		{
			for (int j = 0; j < gridSizeY; j++)
			{
				if (!(GetDistanceBetweenNodes(CurrentGrid[i, j], targetNode) <= character.pCurrentAbility._Range) || !HasLineOfSight(CurrentGrid[i, j], targetNode))
				{
					continue;
				}
				Path path = GetPath(character._CurrentNode, CurrentGrid[i, j], character);
				try
				{
					if (path != null && path._Nodes.Count > 0 && (path.GetFinalPathNode()._CharacterOnNode == null || (path.GetFinalPathNode()._CharacterOnNode != null && path.GetFinalPathNode()._CharacterOnNode.pCharacterData._Team == Character.Team.INANIMATE && !path.GetFinalPathNode()._CharacterOnNode.pBlockMoveNodes)) && path._Cost <= character.pCharacterData._Stats._Movement.pCurrentValue)
					{
						validMoves.Add(path);
					}
				}
				catch (Exception message)
				{
					UtDebug.LogError(message);
				}
			}
		}
	}

	public bool HasLineOfSight(Node currentNode, Node targetNode)
	{
		RaycastHit[] array = Physics.RaycastAll(new Ray(currentNode._WorldPosition, targetNode._WorldPosition - currentNode._WorldPosition), Vector3.Distance(currentNode._WorldPosition, targetNode._WorldPosition), NodeLayer);
		for (int i = 0; i < array.Length; i++)
		{
			Node component = array[i].collider.gameObject.GetComponent<Node>();
			if (!(component == currentNode) && !(component == targetNode) && (component._Blocker || ((bool)component._CharacterOnNode && component._CharacterOnNode.pBlockRangedAttacks)))
			{
				return false;
			}
		}
		return true;
	}

	public bool FindPath(Node startNode, Node targetNode, Character character, bool forcePath)
	{
		ResetAllNodesCosts();
		List<Node> list = new List<Node>();
		HashSet<Node> hashSet = new HashSet<Node>();
		list.Add(startNode);
		while (list.Count > 0)
		{
			Node node = list[0];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].pFCost < node.pFCost || (list[i].pFCost == node.pFCost && list[i]._HCost < node._HCost))
				{
					node = list[i];
				}
			}
			list.Remove(node);
			hashSet.Add(node);
			if (node == targetNode)
			{
				return true;
			}
			for (int j = 0; j < node._Neighbors.Count; j++)
			{
				if (!node._Neighbors[j]._Walkable || hashSet.Contains(node._Neighbors[j]))
				{
					continue;
				}
				if (character != null)
				{
					if (GetDistanceBetweenNodes(node, node._Neighbors[j]) * node._Neighbors[j]._MovementFactor > character.pCharacterData._Stats._Movement.pCurrentValue)
					{
						continue;
					}
					bool flag = false;
					bool flag2 = false;
					bool flag3 = false;
					if (node._Neighbors[j]._CharacterOnNode != null)
					{
						flag = (character.pCharacterData._Team == Character.Team.PLAYER && node._Neighbors[j]._CharacterOnNode.pCharacterData._Team == Character.Team.ENEMY) || (character.pCharacterData._Team == Character.Team.ENEMY && node._Neighbors[j]._CharacterOnNode.pCharacterData._Team == Character.Team.PLAYER);
						flag2 = flag && !node._Neighbors[j]._CharacterOnNode.pIsIncapacitated;
						flag3 = node._Neighbors[j]._CharacterOnNode.pCharacterData._Team == Character.Team.INANIMATE && node._Neighbors[j]._CharacterOnNode.pBlockMoveNodes;
					}
					if (node._Neighbors[j]._CharacterOnNode != null && (flag || flag3 || flag2) && !forcePath)
					{
						continue;
					}
				}
				float num = node._GCost + GetDistanceBetweenNodes(node, node._Neighbors[j]) * node._Neighbors[j]._MovementFactor;
				if (num < node._Neighbors[j]._GCost || !list.Contains(node._Neighbors[j]))
				{
					node._Neighbors[j]._GCost = num;
					node._Neighbors[j]._HCost = GetDistanceBetweenNodes(node._Neighbors[j], targetNode);
					node._Neighbors[j]._ParentNode = node;
					if (!list.Contains(node._Neighbors[j]))
					{
						list.Add(node._Neighbors[j]);
					}
				}
			}
		}
		return false;
	}

	public Node GetCheapestNeighbor(Node currentNode, Node targetNode, Character character)
	{
		Path path = null;
		foreach (Node neighbor in targetNode._Neighbors)
		{
			Path path2 = GetPath(currentNode, neighbor, character);
			if (path2 != null && (path == null || (path != null && path2._Cost < path._Cost)))
			{
				path = path2;
			}
		}
		return path?.GetFinalPathNode();
	}

	public Path GetPath(Node startNode, Node endNode, Character character, bool forcePath = false)
	{
		if (!FindPath(startNode, endNode, character, forcePath))
		{
			return null;
		}
		Path path = new Path();
		Node node = endNode;
		while (node != startNode)
		{
			path._Nodes.Add(node);
			node = node._ParentNode;
		}
		path._Nodes.Reverse();
		path._Cost = endNode.pFCost;
		return path;
	}

	public void DrawPath(Node currentNode, Path path, Material material)
	{
		if (path != null && _PathLineRenderer != null)
		{
			List<Vector3> list = new List<Vector3>();
			list.Add(currentNode._WorldPosition + new Vector3(0f, _LineRendererOffset, 0f));
			for (int i = 0; i < path._Nodes.Count; i++)
			{
				list.Add(path._Nodes[i]._WorldPosition + new Vector3(0f, _LineRendererOffset, 0f));
			}
			_PathLineRenderer.positionCount = list.Count;
			_PathLineRenderer.SetPositions(list.ToArray());
			_PathLineRenderer.enabled = true;
			_PathLineRenderer.material = material;
			if (_PathEnd != null)
			{
				_PathEnd.transform.position = list[list.Count - 1] + new Vector3(0f, 0.1f, 0f);
				_PathEnd.SetActive(value: true);
				_PathEnd.GetComponent<Renderer>().material = material;
			}
		}
	}

	public void DrawAttackPath(Node currentNode, Node attackNode, Material material)
	{
		if (_PathLineRenderer != null)
		{
			if (currentNode == null)
			{
				_PathLineRenderer.positionCount++;
				_PathLineRenderer.SetPosition(_PathLineRenderer.positionCount - 1, attackNode._WorldPosition + new Vector3(0f, _LineRendererOffset, 0f));
			}
			else
			{
				_PathLineRenderer.positionCount = 2;
				_PathLineRenderer.SetPosition(0, currentNode._WorldPosition + new Vector3(0f, _LineRendererOffset, 0f));
				_PathLineRenderer.SetPosition(1, attackNode._WorldPosition + new Vector3(0f, _LineRendererOffset, 0f));
				_PathLineRenderer.enabled = true;
				_PathLineRenderer.material = material;
			}
			if (_Attack != null)
			{
				_Attack.transform.position = attackNode._WorldPosition + new Vector3(0f, 0.1f, 0f);
				_Attack.SetActive(value: true);
			}
		}
	}

	public void HidePath()
	{
		if (_PathLineRenderer != null)
		{
			_PathLineRenderer.enabled = false;
		}
		if (_PathEnd != null)
		{
			_PathEnd.SetActive(value: false);
		}
		if (_Attack != null)
		{
			_Attack.SetActive(value: false);
		}
	}

	public void ResetAllNodesCosts()
	{
		for (int i = 0; i < gridSizeX; i++)
		{
			for (int j = 0; j < gridSizeY; j++)
			{
				CurrentGrid[i, j].ResetNodeCosts();
			}
		}
	}
}
