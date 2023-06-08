using UnityEngine;

namespace ShatterToolkit.Helpers;

public class MouseForce : MonoBehaviour
{
	public float impulseScale = 25f;

	protected Rigidbody grabBody;

	protected Vector3 grabPoint;

	protected float grabDistance;

	public void Update()
	{
		GrabBody();
		ReleaseBody();
	}

	public void FixedUpdate()
	{
		MoveBody();
	}

	protected void GrabBody()
	{
		if (grabBody == null && Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo) && hitInfo.rigidbody != null)
		{
			grabBody = hitInfo.rigidbody;
			grabPoint = grabBody.transform.InverseTransformPoint(hitInfo.point);
			grabDistance = hitInfo.distance;
		}
	}

	protected void ReleaseBody()
	{
		if (grabBody != null && Input.GetMouseButtonUp(0))
		{
			grabBody = null;
		}
	}

	protected void MoveBody()
	{
		if (grabBody != null)
		{
			Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, grabDistance);
			Vector3 vector = Camera.main.ScreenToWorldPoint(position);
			Vector3 vector2 = grabBody.transform.TransformPoint(grabPoint);
			Debug.DrawLine(vector, vector2, Color.red);
			Vector3 force = (vector - vector2) * (impulseScale * Time.fixedDeltaTime);
			grabBody.AddForceAtPosition(force, vector2, ForceMode.Impulse);
		}
	}
}
