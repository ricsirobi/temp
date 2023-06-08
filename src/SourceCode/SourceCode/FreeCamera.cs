using UnityEngine;

public class FreeCamera : KAMonoBase
{
	public float zoom_speed = 4f;

	public float translation_speed = 4f;

	public float rotation_speed = 10f;

	private void LateUpdate()
	{
		if (base.camera == null || !base.camera.enabled)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		if (Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);
			num = ((touch.deltaTime > 0f) ? (touch.deltaPosition.x / (float)Screen.width / touch.deltaTime) : 0f);
			num2 = ((touch.deltaTime > 0f) ? (touch.deltaPosition.y / (float)Screen.height / touch.deltaTime) : 0f);
			if (Input.touchCount == 3)
			{
				base.transform.Translate(Vector3.forward * zoom_speed * num2);
			}
			else if (Input.touchCount == 1)
			{
				base.transform.Rotate(Vector3.right * (0f - rotation_speed) * num2, Space.Self);
				base.transform.Rotate(Vector3.up * rotation_speed * num, Space.World);
			}
			else if (Input.touchCount == 2)
			{
				base.transform.Translate(Vector3.right * (0f - translation_speed) * num, Space.Self);
				base.transform.Translate(Vector3.up * (0f - translation_speed) * num2, Space.Self);
			}
			return;
		}
		num = Input.GetAxis("Mouse X");
		num2 = Input.GetAxis("Mouse Y");
		if (Input.GetMouseButton(1))
		{
			if (Input.GetKey("left alt") || Input.GetKey("right alt"))
			{
				base.transform.Translate(Vector3.forward * zoom_speed * num2);
			}
		}
		else if (Input.GetMouseButton(0))
		{
			if (Input.GetKey("left alt") || Input.GetKey("right alt") || Input.touchCount == 1)
			{
				base.transform.Rotate(Vector3.right * (0f - rotation_speed) * num2, Space.Self);
				base.transform.Rotate(Vector3.up * rotation_speed * num, Space.World);
			}
			else
			{
				base.transform.Translate(Vector3.right * (0f - translation_speed) * num, Space.Self);
				base.transform.Translate(Vector3.up * (0f - translation_speed) * num2, Space.Self);
			}
		}
	}
}
