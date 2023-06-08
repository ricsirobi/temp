using UnityEngine;

public class EquipmentBeltData : KAWidgetUserData
{
	public ItemData _ItemData;

	public int _CategoryID;

	public void LoadIcon(ItemData dataItem)
	{
		if (dataItem != null)
		{
			_ItemData = dataItem;
			string[] array = _ItemData.IconName.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnIconLoaded, typeof(Texture));
		}
	}

	public void OnIconLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			KAWidget kAWidget = _Item.FindChildItem("Data");
			if (kAWidget != null)
			{
				kAWidget.SetTexture((Texture)inObject, inPixelPerfect: true, inURL);
				kAWidget.SetState(KAUIState.NOT_INTERACTIVE);
				kAWidget.SetVisibility(inVisible: true);
			}
		}
		if (inEvent == RsResourceLoadEvent.ERROR)
		{
			Debug.LogError("Error !!! Icon not available");
		}
	}
}
