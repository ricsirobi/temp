using System;
using UnityEngine;

public class KAUICameraToWidget : MonoBehaviour
{
	public Camera _Camera;

	private UIWidget mWidgetToDrawTo;

	private Camera mUICamera;

	private Vector3 mLowerLeftPoint;

	private Vector3 mUpperRightPoint;

	private void Start()
	{
		SetCameraRect();
	}

	private void OnEnable()
	{
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Combine(UICamera.onScreenResize, new UICamera.OnScreenResize(ScreenSizeChanged));
		mUICamera = KAUIManager.pInstance.cachedCamera;
		if (mWidgetToDrawTo == null)
		{
			mWidgetToDrawTo = base.gameObject.GetComponent<UIWidget>();
		}
		SetCameraRect();
	}

	private void OnDisable()
	{
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Remove(UICamera.onScreenResize, new UICamera.OnScreenResize(ScreenSizeChanged));
	}

	public void SetCameraRect()
	{
		if (mUICamera != null && mWidgetToDrawTo != null && _Camera != null)
		{
			mLowerLeftPoint = mUICamera.WorldToViewportPoint(mWidgetToDrawTo.worldCorners[0]);
			mUpperRightPoint = mUICamera.WorldToViewportPoint(mWidgetToDrawTo.worldCorners[2]);
			_Camera.rect = new Rect(mLowerLeftPoint.x, mLowerLeftPoint.y, mUpperRightPoint.x - mLowerLeftPoint.x, mUpperRightPoint.y - mLowerLeftPoint.y);
		}
	}

	private void ScreenSizeChanged()
	{
		SetCameraRect();
	}
}
