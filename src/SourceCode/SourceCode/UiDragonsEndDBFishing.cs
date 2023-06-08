public class UiDragonsEndDBFishing : UiDragonsEndDB
{
	public const string LAST_SHOWN_FISHFACT_INDEX = "LFFI";

	public void ShowFishFacts(Fish fish)
	{
		if (fish._FishFacts.Length == 0)
		{
			return;
		}
		int num = ((ProductData.pPairData != null) ? ProductData.pPairData.GetIntValue("LFFI", -1) : 0);
		int num2 = ((num < fish._FishFacts.Length - 1) ? (num + 1) : 0);
		KAWidget kAWidget = mResultUI.FindItem("FishFacts");
		if (kAWidget != null)
		{
			string localizedString = fish._FishFacts[num2].GetLocalizedString();
			KAWidget kAWidget2 = kAWidget.FindChildItem("TxtFact");
			if (!string.IsNullOrEmpty(localizedString) && kAWidget2 != null)
			{
				kAWidget2.SetText(localizedString);
				kAWidget.SetVisibility(inVisible: true);
			}
		}
		if (ProductData.pPairData != null)
		{
			ProductData.pPairData.SetValueAndSave("LFFI", num2.ToString());
		}
	}

	public void HideFishFacts()
	{
		KAWidget kAWidget = mResultUI.FindItem("FishFacts");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
	}

	public void EnableAdWidgets()
	{
		if (AdManager.pInstance.AdSupported(_AdEventType, AdType.REWARDED_VIDEO))
		{
			mResultUI.EnableAdWidgets(inVisible: true);
		}
	}
}
