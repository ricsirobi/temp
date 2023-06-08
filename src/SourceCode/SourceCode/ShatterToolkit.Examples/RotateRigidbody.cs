using UnityEngine;

namespace ShatterToolkit.Examples;

[RequireComponent(typeof(Rigidbody))]
public class RotateRigidbody : MonoBehaviour
{
	public Vector3 axis = Vector3.up;

	public float angularVelocity = 7f;

	protected Rigidbody rb;

	public void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	public void FixedUpdate()
	{
		Quaternion quaternion = Quaternion.AngleAxis(angularVelocity * Time.fixedDeltaTime, axis);
		rb.MoveRotation(rb.rotation * quaternion);
	}
}
