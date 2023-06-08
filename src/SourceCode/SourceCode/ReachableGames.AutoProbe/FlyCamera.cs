using UnityEngine;

namespace ReachableGames.AutoProbe;

public class FlyCamera : MonoBehaviour
{
	public float Speed = 1f;

	public float MouseSensitivity = 1f;

	public bool InvertMouse;

	private void Start()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void Update()
	{
		if (Cursor.visible)
		{
			if (Input.GetKeyDown(KeyCode.BackQuote))
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
			return;
		}
		Vector3 zero = Vector3.zero;
		if (Input.GetKey(KeyCode.W))
		{
			zero.z += 1f;
		}
		if (Input.GetKey(KeyCode.S))
		{
			zero.z += -1f;
		}
		if (Input.GetKey(KeyCode.A))
		{
			zero.x += -1f;
		}
		if (Input.GetKey(KeyCode.D))
		{
			zero.x += 1f;
		}
		base.transform.localPosition += base.transform.TransformVector(zero * Speed * Time.deltaTime);
		float num = Input.GetAxis("Mouse Y") * MouseSensitivity * (InvertMouse ? (-1f) : 1f);
		float num2 = Input.GetAxis("Mouse X") * MouseSensitivity;
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		localEulerAngles.x = Mathf.Clamp(MakeRelative(localEulerAngles.x) + num, -89.9999f, 89.9999f);
		localEulerAngles.y = MakeRelative(localEulerAngles.y) + num2;
		localEulerAngles.z = 0f;
		base.transform.localEulerAngles = localEulerAngles;
		if (Input.GetKeyDown(KeyCode.BackQuote))
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}
	}

	private float MakeRelative(float euler)
	{
		float num = euler - Mathf.Floor(euler / 360f) * 360f;
		return euler - ((num > 180f) ? 360f : 0f);
	}
}
