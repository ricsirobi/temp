using System;
using UnityEngine;

public class KAUIStoreAdvSyncPopUp : KAUIStoreSyncPopUp
{
	[Serializable]
	public class PurchaseTutorials
	{
		public int _ItemCategory;

		public string _BundleName;
	}

	private ItemData mCurrentPurchasedItem;

	public PurchaseTutorials[] _PurchaseTutorials;

	protected override void ProcessItem(PurchaseItemData currentPurchaseItem)
	{
		base.ProcessItem(currentPurchaseItem);
		if (currentPurchaseItem != null)
		{
			mCurrentPurchasedItem = currentPurchaseItem._ItemData;
		}
	}

	private void LoadTutObj(string AsstePath)
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = AsstePath.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnAssetLoadingEvent, typeof(GameObject));
	}

	public void OnAssetLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			UnityEngine.Object.Instantiate((GameObject)inObject).name = "FirstTimeTutorial_" + mCurrentPurchasedItem.ItemName;
			RsResourceManager.ReleaseBundleData(inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			mCurrentPurchasedItem = null;
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			UtDebug.LogError("Error while loading " + inURL, 0);
			break;
		}
	}

	protected override void CloseSyncUI(bool isPurchaseSuccess)
	{
		base.CloseSyncUI(isPurchaseSuccess);
		if (!isPurchaseSuccess || mCurrentPurchasedItem == null)
		{
			return;
		}
		PurchaseTutorials[] purchaseTutorials = _PurchaseTutorials;
		foreach (PurchaseTutorials purchaseTutorials2 in purchaseTutorials)
		{
			if (mCurrentPurchasedItem.HasCategory(purchaseTutorials2._ItemCategory))
			{
				LoadTutObj(purchaseTutorials2._BundleName);
				purchaseTutorials2._ItemCategory = -1;
				break;
			}
		}
	}
}
