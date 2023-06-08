using System.Collections.Generic;
using UnityEngine;

public class GridItemData
{
	public GameObject _Object;

	public UserItemData _ItemData;

	public List<GridCell> _OccupiedCells = new List<GridCell>();

	public GridItemData(GameObject go, UserItemData item)
	{
		_Object = go;
		_ItemData = new UserItemData();
		if (item != null)
		{
			_ItemData.Item = item.Item;
			_ItemData.UserInventoryID = item.UserInventoryID;
			_ItemData.Quantity = item.Quantity;
			_ItemData.Uses = item.Uses;
		}
	}
}
