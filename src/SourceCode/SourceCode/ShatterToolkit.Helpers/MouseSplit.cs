using UnityEngine;

namespace ShatterToolkit.Helpers;

public class MouseSplit : MonoBehaviour
{
	public int raycastCount = 5;

	protected bool started;

	protected Vector3 start;

	protected Vector3 end;

	public void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			start = Input.mousePosition;
			started = true;
		}
		if (!Input.GetMouseButtonUp(0) || !started)
		{
			return;
		}
		end = Input.mousePosition;
		Camera main = Camera.main;
		float nearClipPlane = main.nearClipPlane;
		Vector3 lhs = main.ScreenToWorldPoint(new Vector3(end.x, end.y, nearClipPlane)) - main.ScreenToWorldPoint(new Vector3(start.x, start.y, nearClipPlane));
		for (int i = 0; i < raycastCount; i++)
		{
			Ray ray = main.ScreenPointToRay(Vector3.Lerp(start, end, (float)i / (float)raycastCount));
			if (Physics.Raycast(ray, out var hitInfo))
			{
				Plane plane = new Plane(Vector3.Normalize(Vector3.Cross(lhs, ray.direction)), hitInfo.point);
				hitInfo.collider.SendMessage("Split", new Plane[1] { plane }, SendMessageOptions.DontRequireReceiver);
				break;
			}
		}
		started = false;
	}
}
