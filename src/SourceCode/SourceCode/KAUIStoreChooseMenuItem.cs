using UnityEngine;

public class KAUIStoreChooseMenuItem : MonoBehaviour
{
	private Vector3 mOriginalScale = Vector3.one;

	private void Start()
	{
		mOriginalScale = base.transform.localScale;
	}

	public void RotatedToFrontHalf()
	{
		UpdatePreview(show: false);
	}

	public void RotatedToBackHalf()
	{
		UpdatePreview(show: true);
	}

	private void UpdatePreview(bool show)
	{
		KAWidget component = GetComponent<KAWidget>();
		KAWidget kAWidget = component.FindChildItem("BtnInfo");
		KAWidget kAWidget2 = component.FindChildItem("AniCreditsInfo");
		KAWidget kAWidget3 = component.FindChildItem("AniIconInfo");
		KAWidget kAWidget4 = null;
		KAWidget kAWidget5 = null;
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(!show);
		}
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(!show);
		}
		if (kAWidget3 != null)
		{
			kAWidget4 = kAWidget3.FindChildItem("AniIconFront");
			kAWidget5 = kAWidget3.FindChildItem("AniInfoBehind");
		}
		else
		{
			kAWidget4 = component.FindChildItem("AniIconFront");
			kAWidget5 = component.FindChildItem("AniInfoBehind");
		}
		if (kAWidget4 != null)
		{
			kAWidget4.SetVisibility(!show);
		}
		if (kAWidget5 != null)
		{
			kAWidget5.SetVisibility(show);
		}
		TweenScale.Begin(base.gameObject, 0.5f, mOriginalScale);
	}
}
