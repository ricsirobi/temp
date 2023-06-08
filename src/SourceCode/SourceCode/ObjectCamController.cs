using UnityEngine;

public class ObjectCamController : MonoBehaviour
{
	public Transform _LookAtTransform;

	public float _RotateSpeed = 20f;

	public float _MinZoomDist;

	public float _MaxZoomDist = 20f;

	public float _ZoomFactor = 3f;

	public float _ZoomSpeed = 14f;

	private Vector3 mMousePrev;

	private bool mFreeRotate;

	private float mZoomValue;

	private Vector3 mDefaultCamPos;

	public float pZoomValue => mZoomValue;

	private void Start()
	{
	}

	private void OnEnable()
	{
		mDefaultCamPos = GetComponent<ObjectCam>().GetWorldPosition();
		mMousePrev = Input.mousePosition;
		mFreeRotate = false;
	}

	private void Update()
	{
		if (KAInput.GetMouseButton(1) || (KAInput.pInstance.IsTouchInput() && KAInput.GetMouseButton(0) && UiJoystick.pInstance != null && !UiJoystick.pInstance.pIsPressed && KAUI.GetGlobalMouseOverItem() == null && KAInput.touchCount < 2))
		{
			if (!mFreeRotate)
			{
				mFreeRotate = true;
				mMousePrev = Input.mousePosition;
			}
			float num = Input.mousePosition.x - mMousePrev.x;
			base.transform.RotateAround(_LookAtTransform.position, Vector3.up, num * _RotateSpeed * Time.deltaTime);
			mMousePrev = Input.mousePosition;
			mDefaultCamPos = base.transform.position + base.transform.forward * mZoomValue;
			return;
		}
		mFreeRotate = false;
		float num2 = 0f;
		if (KAInput.GetAxis("CameraZoom") > 0f)
		{
			num2 = 0f - _ZoomFactor;
		}
		else if (KAInput.GetAxis("CameraZoom") < 0f)
		{
			num2 = _ZoomFactor;
		}
		mZoomValue += num2;
		mZoomValue = Mathf.Clamp(mZoomValue, _MinZoomDist, _MaxZoomDist);
		Vector3 b = mDefaultCamPos + -base.transform.forward * mZoomValue;
		base.transform.position = Vector3.Lerp(base.transform.position, b, Time.deltaTime * _ZoomSpeed);
	}
}
