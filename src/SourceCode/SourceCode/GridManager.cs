using System.Collections.Generic;
using UnityEngine;

public class GridManager : ObContextSensitive
{
	public bool _DrawGizmo = true;

	public int _AreaWidth = 10;

	public int _AreaLength = 10;

	public int _Rows = 10;

	public int _Cols = 10;

	public GameObject _GridCellHighlightObject;

	public const float ErrorOffset = 0.1f;

	private int mGridCellWidth = 5;

	private int mGridCellLength = 5;

	protected bool mIsReady;

	private List<GridCell> mAllGridCells = new List<GridCell>();

	private List<GridItemData> mItemsOnGrid = new List<GridItemData>();

	private GameObject mHighLightParent;

	public virtual bool pIsReady => mIsReady;

	public List<GridCell> pAllGridCells => mAllGridCells;

	protected override void Start()
	{
		base.Start();
		CreateGrid();
	}

	protected override void Update()
	{
		base.Update();
	}

	private void DrawGridGizmo()
	{
		if (!_DrawGizmo)
		{
			return;
		}
		foreach (GridCell mAllGridCell in mAllGridCells)
		{
			if (mAllGridCell._ItemOnGrids != null && mAllGridCell._ItemOnGrids.Count > 0)
			{
				Gizmos.color = new Color(255f, 0f, 0f);
			}
			else if (mAllGridCell._IsUnlocked)
			{
				Gizmos.color = new Color(0f, 255f, 0f);
			}
			else
			{
				Gizmos.color = new Color(0f, 0f, 255f);
			}
			Gizmos.DrawWireCube(new Vector3(mAllGridCell._GridCellRect.center.x, base.transform.position.y, mAllGridCell._GridCellRect.center.y), new Vector3(mGridCellWidth, 0f, mGridCellLength));
		}
	}

	private void OnDrawGizmos()
	{
		DrawGridGizmo();
	}

	private void CreateGrid()
	{
		if (_AreaWidth % _Cols != 0 || _AreaLength % _Rows != 0)
		{
			mIsReady = true;
			Debug.LogError("Area and Row/Col are Not Matching");
			return;
		}
		int num = 0;
		mGridCellWidth = _AreaWidth / _Cols;
		mGridCellLength = _AreaLength / _Rows;
		for (int i = 0; i < _Cols; i++)
		{
			for (int j = 0; j < _Rows; j++)
			{
				GridCell gridCell = new GridCell();
				gridCell._GridCellRect = new Rect((float)(i * mGridCellWidth) + base.transform.position.x, (float)(j * mGridCellLength) + base.transform.position.z, mGridCellWidth, mGridCellLength);
				gridCell._IsUnlocked = false;
				gridCell._GridCellIndex = num;
				mAllGridCells.Add(gridCell);
				num++;
			}
		}
		mIsReady = true;
	}

	public void AddItem(GameObject go, UserItemData item, bool inUnlockGridCells = true)
	{
		GridItemData gridItemData = AddToGridItemList(go, item);
		if (gridItemData == null)
		{
			UtDebug.LogError("ITEM CAN NOT BE CREATED");
		}
		StampGridCells(gridItemData, inUnlockGridCells);
	}

	public void UpdateItem(GameObject go, UserItemData itemData, Vector3 inPosition, int inWidth, int inLength, bool inUnlockGridCells = true)
	{
		GridItemData gridItemData = FindGridItemFromItemData(itemData);
		if (gridItemData == null)
		{
			UtDebug.LogError("FAILED TO CREATE OBJECT TO BE PLACED ON THE GRID");
			return;
		}
		ClearItemFromGridCells(gridItemData._Object);
		gridItemData._OccupiedCells.Clear();
		StampGridCells(gridItemData, inUnlockGridCells);
	}

	public void UpdateItem(GameObject go, UserItemData itemData, bool inUnlockGridCells = true)
	{
		GridItemData gridItemData = FindGridItemFromItemData(itemData);
		if (gridItemData == null)
		{
			UtDebug.LogError("FAILED TO CREATE OBJECT TO BE PLACED ON THE GRID");
			return;
		}
		ClearItemFromGridCells(gridItemData._Object);
		gridItemData._OccupiedCells.Clear();
		StampGridCells(gridItemData, inUnlockGridCells);
	}

	public List<GridCell> GetGridCellsFromArea(Vector3 inMin, Vector3 inMax)
	{
		float y = inMin.y;
		float x = Mathf.Min(inMin.x, inMax.x) + 0.1f;
		float z = Mathf.Min(inMin.z, inMax.z) + 0.1f;
		GridCell gridCellfromPoint = GetGridCellfromPoint(new Vector3(x, y, z));
		if (gridCellfromPoint == null)
		{
			return null;
		}
		float x2 = Mathf.Max(inMin.x, inMax.x) - 0.1f;
		float z2 = Mathf.Max(inMin.z, inMax.z) - 0.1f;
		GridCell gridCellfromPoint2 = GetGridCellfromPoint(new Vector3(x2, y, z2));
		if (gridCellfromPoint2 == null)
		{
			return null;
		}
		if (!GetGridCellIndex(gridCellfromPoint, out var inGridCellIndex))
		{
			return null;
		}
		if (!GetGridCellIndex(gridCellfromPoint2, out var inGridCellIndex2))
		{
			return null;
		}
		int num = (int)Mathf.Min(inGridCellIndex.x, inGridCellIndex2.x);
		int num2 = (int)Mathf.Max(inGridCellIndex.x, inGridCellIndex2.x);
		int num3 = (int)Mathf.Min(inGridCellIndex.y, inGridCellIndex2.y);
		int num4 = (int)Mathf.Max(inGridCellIndex.y, inGridCellIndex2.y);
		List<GridCell> list = new List<GridCell>();
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				list.Add(GetGridCellFromXYIndex(i, j));
			}
		}
		return list;
	}

	private void StampGridCells(GridItemData inItem, bool inUnlockGridOnStamp = true)
	{
		Collider component = inItem._Object.GetComponent<Collider>();
		List<GridCell> gridCellsFromArea = GetGridCellsFromArea(component.bounds.min, component.bounds.max);
		if (gridCellsFromArea == null || gridCellsFromArea.Count <= 0)
		{
			return;
		}
		foreach (GridCell item in gridCellsFromArea)
		{
			if (item != null && !item._ItemOnGrids.Contains(inItem))
			{
				item._ItemOnGrids.Add(inItem);
			}
			if (!inItem._OccupiedCells.Contains(item))
			{
				inItem._OccupiedCells.Add(item);
			}
		}
	}

	private GridItemData AddToGridItemList(GameObject go, UserItemData itemData)
	{
		GridItemData gridItemData = new GridItemData(go, itemData);
		mItemsOnGrid.Add(gridItemData);
		return gridItemData;
	}

	public bool RemoveItem(GameObject inObject)
	{
		GridItemData item = FindItemDataFromGameObject(inObject);
		if (inObject != null)
		{
			ClearItemFromGridCells(inObject);
			mItemsOnGrid.Remove(item);
			return true;
		}
		return false;
	}

	private GridItemData FindGridItemFromItemData(UserItemData inItemData)
	{
		if (inItemData == null)
		{
			return null;
		}
		foreach (GridItemData item in mItemsOnGrid)
		{
			if (item._ItemData.UserInventoryID == inItemData.UserInventoryID)
			{
				return item;
			}
		}
		return null;
	}

	private GridItemData FindItemDataFromGameObject(GameObject inObject)
	{
		if (inObject == null)
		{
			return null;
		}
		foreach (GridItemData item in mItemsOnGrid)
		{
			if (item._Object == inObject)
			{
				return item;
			}
		}
		return null;
	}

	public void ClearItemFromGridCells(GameObject inObject)
	{
		foreach (GridCell mAllGridCell in mAllGridCells)
		{
			mAllGridCell.ResetItem(inObject);
		}
	}

	public GridCell GetGridCellFromID(int inIndex)
	{
		if (mAllGridCells == null || mAllGridCells.Count == 0 || mAllGridCells.Count <= inIndex)
		{
			return null;
		}
		return mAllGridCells[inIndex];
	}

	public virtual GridCell GetGridCellfromPoint(Vector3 inPoint)
	{
		float x = base.transform.position.x;
		float z = base.transform.position.z;
		float num = inPoint.x - x;
		float num2 = inPoint.z - z;
		if (num < 0f || num2 < 0f)
		{
			return null;
		}
		int inIndexX = (int)num / mGridCellWidth;
		int inIndexY = (int)num2 / mGridCellLength;
		return GetGridCellFromXYIndex(inIndexX, inIndexY);
	}

	public Vector3 GetGridCellPosition(GridCell inCell)
	{
		return new Vector3(inCell._GridCellRect.center.x, base.transform.position.y, inCell._GridCellRect.center.y);
	}

	public List<GridCell> GetGridsFromUserInventoryID(int inItemID)
	{
		if (inItemID < 0)
		{
			UtDebug.LogError("USER INVENTORY ID IS NOT VALID");
		}
		foreach (GridItemData item in mItemsOnGrid)
		{
			if (item._ItemData.Item.ItemID == inItemID)
			{
				return item._OccupiedCells;
			}
		}
		return null;
	}

	public List<GridCell> GetGridsFromGameObject(GameObject inObject)
	{
		if (inObject == null)
		{
			UtDebug.LogError("NO OBJECT FOUND");
		}
		foreach (GridItemData item in mItemsOnGrid)
		{
			if (item._Object == inObject)
			{
				return item._OccupiedCells;
			}
		}
		return null;
	}

	private int GetRowfromCellIndex(int inIndex)
	{
		if (inIndex > _Rows)
		{
			return inIndex % _Rows;
		}
		return inIndex;
	}

	public bool CanPlaceItem(GameObject go, bool inCheckLock = true, bool inUnlockCheck = true)
	{
		if (go == null)
		{
			return false;
		}
		Collider component = go.GetComponent<Collider>();
		List<GridCell> gridCellsFromArea = GetGridCellsFromArea(component.bounds.min, component.bounds.max);
		if (gridCellsFromArea == null || gridCellsFromArea.Count == 0)
		{
			return false;
		}
		foreach (GridCell item in gridCellsFromArea)
		{
			if (item != null)
			{
				if (inCheckLock && (!item._IsUnlocked || (item._ItemOnGrids != null && item._ItemOnGrids.Count > 0)))
				{
					return false;
				}
				if (inUnlockCheck && item._IsUnlocked)
				{
					return false;
				}
			}
		}
		return true;
	}

	public List<GridCell> GetUnstampedCells()
	{
		if (mAllGridCells == null || mAllGridCells.Count == 0)
		{
			return null;
		}
		List<GridCell> list = new List<GridCell>();
		foreach (GridCell mAllGridCell in mAllGridCells)
		{
			if (mAllGridCell._ItemOnGrids == null || mAllGridCell._ItemOnGrids.Count == 0)
			{
				list.Add(mAllGridCell);
			}
		}
		return list;
	}

	public List<GridCell> GetUnlockedGridCells()
	{
		if (mAllGridCells == null || mAllGridCells.Count == 0)
		{
			return null;
		}
		List<GridCell> list = new List<GridCell>();
		foreach (GridCell mAllGridCell in mAllGridCells)
		{
			if (mAllGridCell != null && mAllGridCell._IsUnlocked)
			{
				list.Add(mAllGridCell);
			}
		}
		return list;
	}

	public void HighlightUnlockedCells(bool inHighlight)
	{
		if (inHighlight)
		{
			HighlightCells(GetUnlockedGridCells());
		}
		else
		{
			UnHighlightCells();
		}
	}

	protected void AddHighlightCell(GridCell inCell)
	{
		if (mHighLightParent == null)
		{
			return;
		}
		GameObject gameObject = Object.Instantiate(_GridCellHighlightObject);
		BoxCollider component = gameObject.GetComponent<BoxCollider>();
		if (component != null)
		{
			Object.Destroy(component);
		}
		gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.position = GetGridCellPosition(inCell);
		gameObject.name = "HighLight" + inCell._GridCellIndex;
		gameObject.transform.parent = mHighLightParent.transform;
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.SetActive(value: true);
		}
		gameObject.SetActive(value: true);
	}

	private void HighlightCells(List<GridCell> inGridCells)
	{
		if (inGridCells == null || inGridCells.Count <= 0 || _GridCellHighlightObject == null)
		{
			return;
		}
		mHighLightParent = new GameObject("HighlightParent");
		foreach (GridCell inGridCell in inGridCells)
		{
			AddHighlightCell(inGridCell);
		}
	}

	private void UnHighlightCells()
	{
		if (!(mHighLightParent == null))
		{
			Transform[] componentsInChildren = mHighLightParent.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.Destroy(componentsInChildren[i].gameObject);
			}
			Object.Destroy(mHighLightParent);
			mHighLightParent = null;
		}
	}

	public bool GetGridCellIndex(GridCell inGridCell, out Vector2 inGridCellIndex)
	{
		inGridCellIndex = Vector2.zero;
		if (inGridCell == null)
		{
			return false;
		}
		inGridCellIndex.x = Mathf.Floor(inGridCell._GridCellIndex / _Cols);
		inGridCellIndex.y = (float)inGridCell._GridCellIndex - inGridCellIndex.x * (float)_Cols;
		return true;
	}

	public GridCell GetGridCellFromXYIndex(int inIndexX, int inIndexY)
	{
		if (inIndexX >= _Rows || inIndexX < 0 || inIndexY >= _Cols || inIndexY < 0)
		{
			return null;
		}
		return GetGridCellFromID(inIndexX * _Cols + inIndexY);
	}

	public int GetUnlockedGridCount()
	{
		int num = 0;
		foreach (GridCell mAllGridCell in mAllGridCells)
		{
			if (mAllGridCell != null && mAllGridCell._IsUnlocked)
			{
				num++;
			}
		}
		return num;
	}
}
