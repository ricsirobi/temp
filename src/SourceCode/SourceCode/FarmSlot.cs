using System;
using System.Collections.Generic;
using UnityEngine;

public class FarmSlot : FarmItem
{
	public LocaleString _RemoveFarmSlotWithCropWarningText = new LocaleString("Are you sure you want to remove this plot ? The plot's current crop will be lost");

	public LocaleString _MoveFarmSlotWithCropWarningText = new LocaleString("Are you sure you want to move this plot ? The plot's current crop will be lost");

	public int _SeedsCategory = 393;

	public StoreLoader.Selection _StoreInfo;

	public Color _SlotHighlightColor = new Color(0.5f, 0.215f, 0.098f, 1f);

	private UserItemData mCurrentUserItemData;

	protected override void OnActivate()
	{
		if (CanActivate())
		{
			base.OnActivate();
			if (!IsCropPlaced() && InteractiveTutManager._CurrentActiveTutorialObject != null)
			{
				InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "Farmslot_Click");
			}
		}
	}

	private void UpdatePlantSubmenu()
	{
		ContextData contextData = GetContextData("Plant");
		if (contextData == null)
		{
			return;
		}
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(_SeedsCategory);
		List<string> list = new List<string>();
		if (items != null && items.Length != 0)
		{
			int i = 0;
			for (int num = items.Length; i < num; i++)
			{
				UserItemData userItemData = items[i];
				list.Add(userItemData.Item.ItemName);
				string inUseTexturePath = userItemData.Item.IconName.Replace("Crop", "Farm").Replace("Seeds", "Crop").Replace("Seed", "Crop");
				AddChildContextDataToParent(contextData, userItemData.Item, inShowInventoryCount: true, inUseTexturePath);
			}
		}
		contextData.pIsChildOpened = true;
		if (contextData._ChildrenNames.Length != list.Count)
		{
			contextData._ChildrenNames = list.ToArray();
		}
		else
		{
			int j = 0;
			for (int count = list.Count; j < count; j++)
			{
				contextData._ChildrenNames[j] = list[j];
			}
		}
		UpdateChildrenData();
	}

	protected override bool CanProcessUpdateData()
	{
		return CanActivate();
	}

	protected override void ProcessSensitiveData(ref List<string> menuItemNames)
	{
		if (CommonInventoryData.pInstance == null)
		{
			return;
		}
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(_SeedsCategory);
		if (items != null && items.Length != 0)
		{
			if (IsCropPlaced() && menuItemNames.Contains("Plant"))
			{
				menuItemNames.Remove("Plant");
			}
			UpdatePlantSubmenu();
		}
		else if (menuItemNames.Contains("Plant"))
		{
			menuItemNames.Remove("Plant");
		}
		FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
		if (InteractiveTutManager._CurrentActiveTutorialObject != null && farmManager != null && InteractiveTutManager._CurrentActiveTutorialObject == farmManager._PlantTutorial && menuItemNames.Contains("Plant"))
		{
			menuItemNames.Clear();
			menuItemNames.Add("Plant");
		}
	}

	public override void OnBuildModeChanged(bool inBuildMode)
	{
		base.OnBuildModeChanged(inBuildMode);
		GridCell gridCellfromPoint = base.pFarmManager._GridManager.GetGridCellfromPoint(base.transform.position);
		if (gridCellfromPoint._ItemOnGrids == null)
		{
			return;
		}
		foreach (GridItemData itemOnGrid in gridCellfromPoint._ItemOnGrids)
		{
			if (itemOnGrid._Object != null)
			{
				FarmItemBase component = itemOnGrid._Object.GetComponent<FarmItemBase>();
				if (component is CropFarmItem)
				{
					component.collider.enabled = !inBuildMode;
					break;
				}
			}
		}
	}

	private bool IsCropPlaced()
	{
		bool result = false;
		GridCell gridCellfromPoint = base.pFarmManager._GridManager.GetGridCellfromPoint(base.transform.position);
		if (gridCellfromPoint == null || gridCellfromPoint._ItemOnGrids == null)
		{
			result = false;
		}
		else
		{
			foreach (GridItemData itemOnGrid in gridCellfromPoint._ItemOnGrids)
			{
				if (itemOnGrid._Object != null && itemOnGrid._Object.GetComponent<FarmItemBase>() is CropFarmItem)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private GameObject GetCropPlaced()
	{
		GridCell gridCellfromPoint = base.pFarmManager._GridManager.GetGridCellfromPoint(base.transform.position);
		if (gridCellfromPoint == null || gridCellfromPoint._ItemOnGrids == null)
		{
			return null;
		}
		foreach (GridItemData itemOnGrid in gridCellfromPoint._ItemOnGrids)
		{
			if (itemOnGrid._Object != null)
			{
				FarmItemBase component = itemOnGrid._Object.GetComponent<FarmItemBase>();
				if (component is CropFarmItem)
				{
					return component.gameObject;
				}
			}
		}
		return null;
	}

	private void CheckCropSelected(string inActionName)
	{
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(_SeedsCategory);
		if (items == null || items.Length == 0)
		{
			return;
		}
		UserItemData[] array = items;
		foreach (UserItemData userItemData in array)
		{
			if (userItemData.Item.ItemName.Equals(inActionName))
			{
				mCurrentUserItemData = userItemData;
				string[] separator = new string[1] { "/" };
				string[] array2 = userItemData.Item.AssetName.Split(separator, StringSplitOptions.None);
				if (InteractiveTutManager._CurrentActiveTutorialObject != null)
				{
					InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "Plant");
				}
				CommonInventoryData.pInstance.RemoveItem(mCurrentUserItemData, 1);
				FarmItemBase.pIsBundleLoading = true;
				RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], CropEventHandler, typeof(GameObject));
				CloseMenu();
			}
		}
	}

	private void CropEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			FarmItemBase.pIsBundleLoading = false;
			GameObject gameObject = UnityEngine.Object.Instantiate(position: new Vector3(base.transform.position.x, base.transform.position.y, base.transform.position.z), original: (GameObject)inObject, rotation: Quaternion.identity);
			MyRoomsIntMain.pInstance.ObjectCreatedCallback(gameObject, mCurrentUserItemData, inSaved: false);
			UtUtilities.SetLayerRecursively(gameObject, LayerMask.NameToLayer("Furniture"));
			CreateRoomObject(gameObject, mCurrentUserItemData);
			base.pFarmManager.AddRoomObject(gameObject, mCurrentUserItemData, null, isUpdateLocalList: true);
			ObClickable component = gameObject.GetComponent<ObClickable>();
			if (component != null)
			{
				component._MessageObject = base.pFarmManager.pBuilder.gameObject;
			}
			MyRoomsIntMain.pInstance.SaveExplicit();
			break;
		}
		case RsResourceLoadEvent.ERROR:
			FarmItemBase.pIsBundleLoading = false;
			UtDebug.LogError("Item could not be downloaded.");
			break;
		}
	}

	protected override void OnContextAction(string inActionName)
	{
		CheckCropSelected(inActionName);
		switch (inActionName)
		{
		case "Store":
			if (AvAvatar.pToolbar != null && _StoreInfo != null)
			{
				StoreLoader.Load(setDefaultMenuItem: true, _StoreInfo._Category, _StoreInfo._Store, AvAvatar.pToolbar);
			}
			break;
		case "Pack Away":
			if (IsCropPlaced())
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _RemoveFarmSlotWithCropWarningText._Text, "", base.gameObject, "OnRemovePlot", "OnPressNo", "", "", inDestroyOnClick: true);
			}
			else
			{
				base.OnContextAction(inActionName);
			}
			break;
		case "Move":
			if (IsCropPlaced())
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _MoveFarmSlotWithCropWarningText._Text, "", base.gameObject, "OnMovePlot", "OnPressNo", "", "", inDestroyOnClick: true);
			}
			else
			{
				base.OnContextAction(inActionName);
			}
			break;
		default:
			base.OnContextAction(inActionName);
			break;
		}
	}

	private void OnRemovePlot()
	{
		GameObject cropPlaced = GetCropPlaced();
		base.pFarmManager.RemoveRoomObject(cropPlaced.gameObject, isDestroy: true);
		base.pFarmManager.pBuilder.AddItemToInventory(base.pUserItemData);
		base.pFarmManager.RemoveRoomObject(base.gameObject, isDestroy: true);
	}

	private void OnMovePlot()
	{
		GameObject cropPlaced = GetCropPlaced();
		base.pFarmManager.RemoveRoomObject(cropPlaced.gameObject, isDestroy: true);
		base.pFarmManager.pBuilder.GetSelectedObject(base.gameObject);
		base.OnContextAction("Move");
	}

	public override void HighlightObject(bool canShowHightlight)
	{
		FarmItemClickable component = GetComponent<FarmItemClickable>();
		if (component != null)
		{
			if (canShowHightlight && component._HighlightMaterial != null)
			{
				Color color = component._HighlightMaterial.GetColor("_RimColor");
				component._HighlightMaterial.SetColor("_RimColor", _SlotHighlightColor);
				component.Highlight();
				component._HighlightMaterial.SetColor("_RimColor", color);
			}
			else
			{
				component.UnHighlight();
			}
		}
	}
}
