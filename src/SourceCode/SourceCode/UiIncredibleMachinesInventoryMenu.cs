using UnityEngine;

public class UiIncredibleMachinesInventoryMenu : KAUIMenu
{
	private bool mItemLoading;

	public bool pItemLoading => mItemLoading;

	public void AddItemBack(GameObject obj)
	{
		foreach (KAWidget item in GetItems())
		{
			IncredibleMachineUserData incredibleMachineUserData = (IncredibleMachineUserData)item.GetUserData();
			string value = obj.name.Replace("(Clone)", "");
			if (incredibleMachineUserData.Asset.Contains(value))
			{
				incredibleMachineUserData.Quantity++;
				item.FindChildItem("ItemCount").SetText(incredibleMachineUserData.Quantity.ToString());
			}
		}
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta2)
	{
		base.OnDrag(inWidget, inDelta2);
		if (!mItemLoading && InputManager.pInstance.pSelectedObject == null)
		{
			CreateDragObject(inWidget);
		}
	}

	private void CreateDragObject(KAWidget item)
	{
		IncredibleMachineUserData incredibleMachineUserData = (IncredibleMachineUserData)item.GetUserData();
		if (incredibleMachineUserData.Quantity > 0)
		{
			if (!string.IsNullOrEmpty(incredibleMachineUserData.Asset))
			{
				mItemLoading = true;
				KAUICursorManager.SetDefaultCursor("Loading");
				RsResourceManager.LoadAssetFromBundle(incredibleMachineUserData.Asset, OnInventoryItemLoaded, typeof(GameObject), inDontDestroy: false, item);
			}
			else
			{
				UtDebug.Log("Asset URL not found");
			}
		}
	}

	private void OnInventoryItemLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAWidget obj = (KAWidget)inUserData;
			IncredibleMachineUserData incredibleMachineUserData = (IncredibleMachineUserData)obj.GetUserData();
			incredibleMachineUserData.Quantity--;
			GameObject gameObject = Object.Instantiate((GameObject)inObject);
			IMGLevelManager.pInstance.AddItem(gameObject.GetComponent<ObjectBase>());
			InputManager.pInstance.ProcessSelectedObject(gameObject.transform);
			obj.FindChildItem("ItemCount").SetText(incredibleMachineUserData.Quantity.ToString());
			mItemLoading = false;
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
		case RsResourceLoadEvent.ERROR:
			mItemLoading = false;
			KAUICursorManager.SetDefaultCursor("Arrow");
			UtDebug.LogError("Failed to load tool");
			break;
		}
	}
}
