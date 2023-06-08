using UnityEngine;

[RequireComponent(typeof(UIWidget))]
public class KAUIStretch : KAMonoBase
{
	public enum Style
	{
		None,
		Horizontal,
		Vertical,
		Both,
		BasedOnHeight
	}

	public Style _Style;

	public Vector2 _RelativeSize = Vector2.one;

	public bool _ApplyScaleToCollider;

	private BoxCollider mBoxCollider;

	private UIWidget mUIWidget;

	private void Start()
	{
		if (collider != null)
		{
			mBoxCollider = collider as BoxCollider;
		}
		if (mBoxCollider == null && base.transform.parent != null)
		{
			mBoxCollider = base.transform.parent.GetComponent<BoxCollider>();
		}
		mUIWidget = base.gameObject.GetComponent<UIWidget>();
	}

	private void OnEnable()
	{
		Update();
	}

	private void Update()
	{
		if (!(KAUIManager.pInstance != null) || _Style == Style.None || !UICamera.currentCamera || !(mUIWidget != null))
		{
			return;
		}
		Camera currentCamera = UICamera.currentCamera;
		Rect pixelRect = currentCamera.pixelRect;
		float width = pixelRect.width;
		float height = pixelRect.height;
		Vector3 vector = new Vector3(mUIWidget.width, mUIWidget.height, 0f);
		if (_Style == Style.BasedOnHeight)
		{
			vector.x = _RelativeSize.x * height;
			vector.y = _RelativeSize.y * height;
		}
		else
		{
			if (_Style == Style.Both || _Style == Style.Horizontal)
			{
				vector.x = _RelativeSize.x * width;
			}
			if (_Style == Style.Both || _Style == Style.Vertical)
			{
				vector.y = _RelativeSize.y * height;
			}
		}
		float num = currentCamera.rect.yMin * (float)Screen.height;
		float num2 = (currentCamera.rect.yMax * (float)Screen.height - num) * 0.5f;
		int num3 = KAUIManager.pInstance._MaxLandscapeOrthoSize;
		if (UtPlatform.IsMobile() && (Orientation.GetOrientation() == ScreenOrientation.Portrait || Orientation.GetOrientation() == ScreenOrientation.PortraitUpsideDown))
		{
			num3 = KAUIManager.pInstance._MaxPortraitOrthoSize;
		}
		float num4 = (float)num3 / num2;
		vector = ((!(num2 < (float)num3)) ? new Vector3(vector.x * num4, vector.y * num4, 1f) : new Vector3(vector.x * num4, vector.y * num4, 1f));
		if (_Style == Style.Horizontal)
		{
			vector.y = base.transform.localScale.y;
		}
		else if (_Style == Style.Vertical)
		{
			vector.x = base.transform.localScale.x;
		}
		mUIWidget.width = (int)vector.x;
		mUIWidget.height = (int)vector.y;
		if (_ApplyScaleToCollider && mBoxCollider != null)
		{
			Vector3 size = vector;
			size.z = mBoxCollider.size.z;
			mBoxCollider.size = size;
		}
	}
}
