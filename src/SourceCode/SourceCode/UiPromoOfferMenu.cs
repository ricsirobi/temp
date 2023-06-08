using UnityEngine;

public class UiPromoOfferMenu : KAUIMenu
{
	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "AniIconFront")
		{
			ShowInfo(inShowInfo: true, inWidget.GetRootItem());
		}
		else if (inWidget.name == "AniInfoBehind")
		{
			ShowInfo(inShowInfo: false, inWidget.GetRootItem());
		}
	}

	private void ShowInfo(bool inShowInfo, KAWidget inItem)
	{
		if (!(inItem == null))
		{
			Vector3 localScale = inItem.transform.localScale;
			TweenScale.Begin(inItem.gameObject, 0.5f, new Vector3(0f, localScale.y, localScale.z));
			UITweener component = inItem.gameObject.GetComponent<UITweener>();
			component.eventReceiver = inItem.gameObject;
			component.callWhenFinished = "RotatedToFrontHalf";
			if (inShowInfo)
			{
				component.callWhenFinished = "RotatedToBackHalf";
			}
			else
			{
				component.callWhenFinished = "RotatedToFrontHalf";
			}
		}
	}
}
