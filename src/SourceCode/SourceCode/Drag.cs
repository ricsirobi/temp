using UnityEngine;

public class Drag : MonoBehaviour
{
	public bool forTouchScreen;

	public GameObject dragger;

	public LayerMask layerMask;

	private SpringJoint2D springJoint;

	private Rigidbody2D connectedRB;

	private bool dragging;

	private Transform hitObject;

	private bool movingTransform;

	private void Start()
	{
		if (!dragger)
		{
			dragger = new GameObject("dragger");
			dragger.AddComponent<SpringJoint2D>();
		}
		springJoint = dragger.GetComponent<SpringJoint2D>();
	}

	private void Update()
	{
		if (forTouchScreen ? (Input.touchCount > 0) : Input.GetMouseButton(1))
		{
			Vector3 vector = ((!forTouchScreen) ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position));
			vector.z = -1f;
			dragger.transform.position = vector;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.zero, float.PositiveInfinity, layerMask);
			if (!hitObject)
			{
				hitObject = raycastHit2D.transform;
			}
			if (!dragging && !movingTransform && (bool)raycastHit2D.collider && (raycastHit2D.collider.tag == "tack" || raycastHit2D.collider.tag == "candy") && (bool)raycastHit2D.collider.GetComponent<Rigidbody2D>())
			{
				springJoint.anchor = springJoint.transform.InverseTransformPoint(raycastHit2D.point);
				springJoint.connectedAnchor = raycastHit2D.transform.InverseTransformPoint(raycastHit2D.point);
				springJoint.connectedBody = raycastHit2D.collider.GetComponent<Rigidbody2D>();
				connectedRB = raycastHit2D.collider.GetComponent<Rigidbody2D>();
				connectedRB.isKinematic = false;
				dragging = true;
			}
			else if (!dragging && (bool)hitObject && hitObject.tag == "tack")
			{
				hitObject.position = vector;
				movingTransform = true;
			}
			return;
		}
		if ((bool)springJoint)
		{
			springJoint.connectedBody = null;
			if ((bool)connectedRB && connectedRB.transform.tag != "candy")
			{
				connectedRB.isKinematic = true;
			}
		}
		dragging = false;
		movingTransform = false;
		hitObject = null;
	}
}
